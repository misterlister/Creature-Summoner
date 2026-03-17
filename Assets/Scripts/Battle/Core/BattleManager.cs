using DG.Tweening;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using static BattleSystemConstants;



/// <summary>
/// Main controller for battle flow and turn management.
/// Orchestrates between battlefield, turn system, action execution, and UI.
/// Player turn input is delegated to PlayerTurnController.
/// </summary>
public class BattleManager : MonoBehaviour
{
    // Core references
    [SerializeField] private UnifiedBattlefield battlefield;
    [SerializeField] private BattleUI battleUI;
    [SerializeField] private Biome defaultBiome;

    // Systems
    public BattleContext Context { get; private set; }
    private ActionExecutor actionExecutor;
    private TurnSystem turnSystem;
    private PlayerTurnController playerTurnController;

    // High-level battle state
    private BattleState currentState;
    public BattleState CurrentState => currentState;

    private Creature activeCreature;
    public Creature ActiveCreature => activeCreature;

    // Player turn completion flag
    private bool isPlayerTurnComplete = false;

    // Events
    public event System.Action<TeamSide> OnBattleEnd;

    private void Awake()
    {
        DOTween.Init(recycleAllByDefault: true, useSafeMode: true, logBehaviour: LogBehaviour.ErrorsOnly);
        InitializeBattle();
    }

    private void InitializeBattle()
    {
        if (defaultBiome == null)
        {
            Debug.LogWarning("No default biome assigned to BattleManager!");
        }

        // Subscribe to events
        battlefield.OnCreatureDefeated += HandleCreatureDefeated;
    }

    #region Battle Setup

    public void StartBattle(CreatureTeam playerTeam, CreatureTeam enemyTeam, TerrainLayout terrainLayout = null, Biome biome = null)
    {
        if (terrainLayout == null)
        {
            Debug.LogWarning("No terrain layout provided - battlefield will use default terrain");
        }

        if (biome == null)
        {
            Debug.LogWarning("No biome provided - using default biome");
        }

        currentState = BattleState.NotStarted;
        Context = new BattleContext(battlefield, this, biome);
        activeCreature = null;
        isPlayerTurnComplete = false;


        // Initialize systems
        actionExecutor = new ActionExecutor(Context);
        turnSystem = new TurnSystem();
        playerTurnController = new PlayerTurnController(this, battlefield, battleUI);

        if (biome != null)
        {
            defaultBiome = biome;
            Context.CurrentBiome = biome;
        }
        battlefield.SetBiome(defaultBiome);

        battlefield.ApplyTerrainLayout(terrainLayout);

        // Initialize creatures with event manager
        foreach (var creature in playerTeam.Creatures)
        {
            creature.InitializeBattle(Context.EventManager);
        }
        foreach (var creature in enemyTeam.Creatures)
        {
            creature.InitializeBattle(Context.EventManager);
        }

        StartCoroutine(SetupBattleRoutine(playerTeam, enemyTeam));
    }

    private IEnumerator SetupBattleRoutine(CreatureTeam playerTeam, CreatureTeam enemyTeam)
    {
        currentState = BattleState.Start;

        // Place creatures on battlefield
        PlaceTeam(playerTeam, TeamSide.Player);
        PlaceTeam(enemyTeam, TeamSide.Enemy);

        // Show intro message
        int enemyCount = battlefield.GetEnemyCount();
        yield return battleUI.TypeMessage($"You have been attacked by {enemyCount} creatures!");

        // Start combat rounds
        yield return StartCoroutine(NewRound());
    }

    private void PlaceTeam(CreatureTeam team, TeamSide side)
    {
        var grid = battlefield.GetGrid(side);

        foreach (var creature in team.Creatures)
        {
            if (creature == null) continue;

            var tile = FindPlacementTile(grid, creature);

            if (tile == null)
            {
                Debug.LogError($"No valid placement tile found for {creature.Nickname} on {side} side");
                continue;
            }

            var battlePos = BattlePosition.FromGridPosition(tile.LocalPosition, side);
            battlefield.PlaceCreature(creature, battlePos);
        }
    }

    private BattleTile FindPlacementTile(BattleGrid grid, Creature creature)
    {
        // Try preferred column first, then fallback order
        var colPriority = GetColumnPriority(creature.PreferredPositionRole, creature.TeamSide);

        foreach (int col in colPriority)
        {
            var tile = grid.GetColumn(col)
                .FirstOrDefault(t => t.IsValidSpawnTile());

            if (tile != null) return tile;
        }

        return null;
    }

    private int[] GetColumnPriority(PositionRole role, TeamSide team)
    {
        if (team == TeamSide.Player)
        {
            return role switch
            {
                PositionRole.Frontline => new[] { 2, 1, 0 },
                PositionRole.Midline => new[] { 1, 2, 0 },
                PositionRole.Backline => new[] { 0, 1, 2 },
                _ => new[] { 2, 1, 0 }
            };
        }
        else
        {
            return role switch
            {
                PositionRole.Frontline => new[] { 0, 1, 2 },
                PositionRole.Midline => new[] { 1, 0, 2 },
                PositionRole.Backline => new[] { 2, 1, 0 },
                _ => new[] { 0, 1, 2 }
            };
        }
    }

    #endregion

    #region Round and Turn Flow

    private IEnumerator NewRound()
    {
        currentState = BattleState.NewRound;
        Context.IncrementTurn();

        // Roll initiative for all creatures
        var allCreatures = battlefield.GetAllCreatures();
        turnSystem.RollInitiative(allCreatures);

        // Start turn loop
        yield return StartCoroutine(TurnLoop());
    }

    private IEnumerator TurnLoop()
    {
        while (!turnSystem.IsRoundOver())
        {
            activeCreature = turnSystem.GetNextCreature();

            if (activeCreature == null)
                break;

            // Highlight active creature
            SetActiveCreatureHighlight();

            // Execute turn
            yield return StartCoroutine(ExecuteTurn(activeCreature));

            // Clear highlight
            ClearActiveCreatureHighlight();

            // Check battle end
            if (CheckBattleEnd())
                yield break;

            yield return new WaitForSeconds(0.5f);
        }

        // Round is over, start new round
        yield return StartCoroutine(NewRound());
    }

    private IEnumerator ExecuteTurn(Creature creature)
    {
        Context.CurrentActingCreature = creature;

        // Trigger turn start events
        var turnStartEvent = new TurnStartEventData(creature, Context)
        {
            TurnNumber = Context.TurnNumber
        };
        Context.EventManager.TriggerTurnStart(turnStartEvent);

        // Surface effects at turn start
        creature.CurrentTile?.TriggerSurfaceEffect(
            creature,
            SurfaceTriggerTiming.OnTurnStart,
            Context);

        yield return new WaitForSeconds(0.3f);

        // Execute based on team
        if (creature.TeamSide == TeamSide.Player)
        {
            yield return StartCoroutine(ExecutePlayerTurn(creature));
        }
        else
        {
            yield return StartCoroutine(ExecuteEnemyTurn(creature));
        }

        // Turn end
        var turnEndEvent = new TurnEndEventData(creature, Context)
        {
            TurnNumber = Context.TurnNumber
        };
        Context.EventManager.TriggerTurnEnd(turnEndEvent);

        battlefield.PlayerGrid.TickAllSurfaces();
        battlefield.EnemyGrid.TickAllSurfaces();

        Context.TurnNumber++;
    }

    public void SetActiveCreatureHighlight()
    {
        if (activeCreature != null)
        {
            var pos = battlefield.GetBattlePosition(activeCreature);
            battlefield.GetTileUI(pos)?.SetPinnedHighlight(HighlightType.ActiveCreature);
        }
    }

    public void ClearActiveCreatureHighlight()
    {
        if (activeCreature != null)
        {
            var pos = battlefield.GetBattlePosition(activeCreature);
            battlefield.GetTileUI(pos)?.ClearPinnedHighlight();
        }
    }

    #endregion

    #region Player Turn

    private IEnumerator ExecutePlayerTurn(Creature creature)
    {
        currentState = BattleState.PlayerTurn;

        // Delegate to player turn controller
        playerTurnController.StartTurn(creature);

        isPlayerTurnComplete = false;

        // Wait for player to complete their turn
        yield return new WaitUntil(() => isPlayerTurnComplete);
    }

    // Called by PlayerTurnController when player confirms action
    public void ExecutePlayerAction(ActionBase action, TargetList targets)
    {
        StartCoroutine(ExecutePlayerActionCoroutine(action, targets));
    }

    private IEnumerator ExecutePlayerActionCoroutine(ActionBase action, TargetList targets)
    {
        yield return StartCoroutine(ExecuteAction(activeCreature, action, targets));
        isPlayerTurnComplete = true;
    }

    #endregion

    #region Enemy Turn

    private IEnumerator ExecuteEnemyTurn(Creature creature)
    {
        currentState = BattleState.EnemyTurn;

        // AI decides action (placeholder - implement AIController)
        var aiDecision = DecideAIAction(creature);

        yield return new WaitForSeconds(0.5f);

        yield return StartCoroutine(ExecuteAction(
            creature,
            aiDecision.action,
            aiDecision.targets
        ));
    }

    // Placeholder AI decision - replace with actual AIController
    private (ActionBase action, TargetList targets) DecideAIAction(Creature creature)
    {
        // Simple AI: pick first core action and first valid target
        var action = creature.Actions.EquippedCoreActions[0]?.Action;
        if (action == null)
        {
            Debug.LogError($"AI creature {creature.Nickname} has no actions!");
            return (null, new TargetList());
        }

        var validTargets = action.GetValidTargets(creature, battlefield);
        if (validTargets.Count == 0)
            return (action, TargetList.Empty);

        var primaryTarget = validTargets[0];
        var grid = battlefield.GetGrid(creature.TeamSide);
        var targetResult = AOETargetCalculator.GetTargets(
            primaryTarget,
            action.AreaOfEffect,
            grid,
            creature.TeamSide);

        return (action, targetResult);
    }

    #endregion

    #region Action Execution

    private IEnumerator ExecuteAction(Creature attacker, ActionBase action, TargetList targets)
    {
        if (action == null || targets.PrimaryTarget == null)
        {
            Debug.LogError("Cannot execute action: invalid parameters");
            yield break;
        }

        currentState = BattleState.Busy;

        // Show action message
        yield return battleUI.TypeMessage($"{attacker.Nickname} used {action.ActionName}!");

        // Highlight targets
        battlefield.HighlightTiles(targets.AllTargets(),
            action.Role == ActionRole.Offensive ? HighlightType.NegativeTarget : HighlightType.PositiveTarget);

        yield return new WaitForSeconds(0.3f);

        // Play attack animation
        var attackerPos = battlefield.GetBattlePosition(attacker);
        battlefield.GetTileUI(attackerPos)?.PlayAttackAnimation();

        yield return new WaitForSeconds(0.4f);

        // Execute action via ActionExecutor
        ActionResult result = actionExecutor.Execute(action, attacker, targets);

        // Display messages
        foreach (var message in result.GetMessages())
        {
            yield return battleUI.TypeMessage(message);
        }

        // Wait for animations to complete
        yield return new WaitUntil(() => !AnyAnimationsPlaying());

        battlefield.ClearAllHighlights();
        yield return new WaitForSeconds(0.3f);
    }

    private bool AnyAnimationsPlaying()
    {
        foreach (var creature in battlefield.GetAllCreatures())
        {
            var pos = battlefield.GetBattlePosition(creature);
            var tileUI = battlefield.GetTileUI(pos);
            if (tileUI != null && tileUI.IsAnimating)
                return true;
        }
        return false;
    }

    #endregion

    #region Battle End Conditions

    private bool CheckBattleEnd()
    {
        int playerCount = battlefield.GetPlayerCount();
        int enemyCount = battlefield.GetEnemyCount();

        if (playerCount == 0)
        {
            EndBattle(TeamSide.Enemy);
            return true;
        }

        if (enemyCount == 0)
        {
            EndBattle(TeamSide.Player);
            return true;
        }

        return false;
    }

    private void EndBattle(TeamSide winner)
    {
        currentState = BattleState.Ended;

        // Cleanup creatures
        var allCreatures = battlefield.GetAllCreatures();
        foreach (var creature in allCreatures)
        {
            creature.CleanupBattle();
        }

        Context.EventManager?.ClearAllSubscriptions();

        OnBattleEnd?.Invoke(winner);
        battleUI.ShowBattleEndScreen(winner);
    }

    #endregion

    #region Event Handlers

    private void HandleCreatureDefeated(Creature creature)
    {
        // Remove from turn order
        turnSystem.RemoveCreature(creature);

        // Award XP to victorious team
        AwardExperience(creature);
    }

    private void AwardExperience(Creature defeated)
    {
        var victoriousTeam = defeated.TeamSide == TeamSide.Player
            ? TeamSide.Enemy
            : TeamSide.Player;

        var survivors = battlefield.GetGrid(victoriousTeam).GetAllCreatures();

        int xpPerCreature = CalculateXPReward(defeated) / Mathf.Max(survivors.Count, 1);

        foreach (var survivor in survivors)
        {
            survivor.AddXP(xpPerCreature);
        }
    }

    private int CalculateXPReward(Creature defeated)
    {
        // XP calculation formula
        return defeated.Level * 10; // Placeholder
    }

    #endregion

    #region Public API for PlayerTurnController

    public List<BattleTile> GetMoveTargets(Creature creature)
    {
        var grid = battlefield.GetGrid(creature.TeamSide);
        var validPositions = grid.GetValidMovePositions(creature, creature.Energy);

        return validPositions.ConvertAll(pos => grid.GetTile(pos));
    }

    public void ClearTargetHighlights()
    {
        battlefield.ClearAllHighlights();
    }

    #endregion

    #region Unity Lifecycle

    // Called every frame by GameController
    public void HandleUpdate()
    {
        if (currentState == BattleState.PlayerTurn)
        {
            playerTurnController.HandleInput();
        }
    }

    private void OnDestroy()
    {
        // Cleanup
        if (battlefield != null)
        {
            battlefield.OnCreatureDefeated -= HandleCreatureDefeated;
        }

        Context?.EventManager?.ClearAllSubscriptions();
    }

    #endregion
}
