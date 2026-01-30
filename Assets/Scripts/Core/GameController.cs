using UnityEngine;

public enum GameState
{
    FreeRoam,
    Battle
}

public class GameController : MonoBehaviour
{
    [SerializeField] PlayerController playerController;
    [SerializeField] BattleManager battleManager;
    [SerializeField] Camera worldCamera;

    GameState state;

    private void Start()
    {
        ToFreeRoamState();
        playerController.OnEncountered += StartBattle;
        battleManager.OnBattleEnd += EndBattle;
    }

    void ToFreeRoamState()
    {
        state = GameState.FreeRoam;
        battleManager.gameObject.SetActive(false);
        worldCamera.gameObject.SetActive(true);
    }

    void StartBattle()
    {
        state = GameState.Battle;
        battleManager.gameObject.SetActive(true);
        worldCamera.gameObject.SetActive(false);

        var playerTeam = playerController.GetComponent<CreatureTeam>();

        // Get the map area the player is currently in
        var currentMapArea = playerController.GetCurrentMapArea();
        if (currentMapArea == null)
        {
            Debug.LogError("Cannot start battle: player is not in a map area!");
            ToFreeRoamState();
            return;
        }

        var enemyTeam = currentMapArea.GenerateWildCreatureTeam();

        battleManager.StartBattle(playerTeam, enemyTeam);
    }

    void EndBattle(TeamSide victor)
    {
        if (victor == TeamSide.Player)
        {
            Debug.Log("Player won the battle!");
        }
        else
        {
            Debug.Log("Player lost the battle...");
        }
        ToFreeRoamState();
    }

    private void Update()
    {
        if (state == GameState.FreeRoam)
        {
            playerController.HandleUpdate();
        }
        else if (state == GameState.Battle)
        {
            battleManager.HandleUpdate();
        }
    }
}