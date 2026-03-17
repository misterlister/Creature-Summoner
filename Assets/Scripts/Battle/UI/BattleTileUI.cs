using DG.Tweening;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using static BattleUIConstants;

/// <summary>
/// Handles all visual presentation for a single battle tile.
/// Pure UI - no game logic, just responds to events from BattleTile.
/// </summary>
public class BattleTileUI : MonoBehaviour
{
    [Header("Creature Display")]
    [SerializeField] private Image creatureSprite;
    [SerializeField] private GameObject spriteHolder;

    [Header("Status Bars")]
    [SerializeField] private GameObject barsContainer;
    [SerializeField] private ResourceBar shieldBar;
    [SerializeField] private ResourceBar hpBar;
    [SerializeField] private ResourceBar energyBar;

    [Header("Highlights")]
    [SerializeField] private Image highlightAura;
    [SerializeField] private Image selectionArrow;

    [Header("Highlight Sprites")]
    [SerializeField] private Sprite activeCreatureAura;
    [SerializeField] private Sprite negativeTargetAura;
    [SerializeField] private Sprite positiveTargetAura;
    [SerializeField] private Sprite moveTargetAura;
    [SerializeField] private Sprite validArrowSprite;
    [SerializeField] private Sprite invalidArrowSprite;
    [SerializeField] private Sprite selectedArrowSprite;

    [Header("Info Display")]
    [SerializeField] private GameObject namePanel;
    [SerializeField] private TextMeshProUGUI nameField;
    [SerializeField] private GameObject levelPanel;
    [SerializeField] private TextMeshProUGUI levelField;

    [Header("Terrain Display")]
    [SerializeField] private Image terrainBackgroundImage;
    [SerializeField] private Image terrainForegroundImage;

    // References
    public BattleTile DataTile { get; private set; }
    private bool isPlayerSide;
    private Creature boundCreature;

    // Animation state
    private Vector3 originalSpritePos;
    private Color originalSpriteColor;
    private bool isAnimating;

    // Highlight state
    private HighlightType? persistentHighlight = null;
    private HighlightType? pinnedHighlight = null;

    public void Initialize(BattleTile tile, bool playerSide)
    {
        // Unsubscribe previous tile if any
        if (DataTile != null) UnsubscribeFromTile(DataTile);

        DataTile = tile;
        isPlayerSide = playerSide;

        // Subscribe to tile-level events (tile forwards creature events)
        SubscribeToTile(DataTile);

        SetupLayout();

        if (DataTile.OccupyingCreature != null)
        {
            OnCreaturePlaced(DataTile.OccupyingCreature);
        } 
        else
        {
            Clear();
        }
    }

    private void SubscribeToTile(BattleTile tile)
    {
        if (tile == null) return;
        tile.OnCreaturePlaced += OnCreaturePlaced;
        tile.OnCreatureRemoved += OnCreatureRemoved;
        tile.OnTerrainChanged += OnTerrainChanged;
        tile.OnSurfaceApplied += OnSurfaceApplied;
        tile.OnSurfaceRemoved += OnSurfaceRemoved;

        // Forwarded creature events
        tile.OnCreatureHPChanged += OnBoundCreatureHPChanged;
        tile.OnCreatureDamaged += OnBoundCreatureDamaged;
        tile.OnCreatureHealed += OnBoundCreatureHealed;
    }

    private void UnsubscribeFromTile(BattleTile tile)
    {
        if (tile == null) return;
        tile.OnCreaturePlaced -= OnCreaturePlaced;
        tile.OnCreatureRemoved -= OnCreatureRemoved;
        tile.OnTerrainChanged -= OnTerrainChanged;
        tile.OnSurfaceApplied -= OnSurfaceApplied;
        tile.OnSurfaceRemoved -= OnSurfaceRemoved;

        tile.OnCreatureHPChanged -= OnBoundCreatureHPChanged;
        tile.OnCreatureDamaged -= OnBoundCreatureDamaged;
        tile.OnCreatureHealed -= OnBoundCreatureHealed;
    }

    private void SetupLayout()
    {
        var layoutGroup = GetComponent<HorizontalLayoutGroup>();
        layoutGroup.reverseArrangement = !isPlayerSide;
        originalSpritePos = spriteHolder.transform.localPosition;
    }

    #region Event Handlers from BattleTile

    public void OnCreaturePlaced(Creature creature)
    {
        if (creature == null)
        {
            Clear();
            Debug.Log("BattleTileUI: OnCreaturePlaced called with null creature.");
            return;
        }

        // Unsubscribe from any previously-bound creature
        if (boundCreature != null)
        {
            UnbindCreatureEvents();
        }

        boundCreature = creature;
        SetupCreatureVisuals(creature);
        UpdateBars(creature, instant: true);
        PlaySummonAnimation();

        // Subscribe to creature events
        boundCreature.OnHPChanged += OnBoundCreatureHPChanged;
        boundCreature.OnTakeDamage += OnBoundCreatureDamaged;
        boundCreature.OnHealed += OnBoundCreatureHealed;
        boundCreature.OnShieldChanged += OnBoundCreatureShieldingChanged;
        boundCreature.OnEnergyChanged += OnBoundCreatureEnergyChanged;
    }

    public void OnCreatureRemoved(Creature creature)
    {
        Clear();
    }

    private void UnbindCreatureEvents()
    {
        if (boundCreature == null) return;
        boundCreature.OnHPChanged -= OnBoundCreatureHPChanged;
        boundCreature.OnTakeDamage -= OnBoundCreatureDamaged;
        boundCreature.OnHealed -= OnBoundCreatureHealed;
        boundCreature.OnShieldChanged -= OnBoundCreatureShieldingChanged;
        boundCreature.OnEnergyChanged -= OnBoundCreatureEnergyChanged;
    }

    private void OnBoundCreatureHPChanged(int currentHP, int maxHP)
    {
        // Update bars when HP or related stats change
        UpdateBars(boundCreature, instant: false);
    }

    private void OnBoundCreatureEnergyChanged(int currentEnergy, int maxEnergy)
    {
        // Update bars when Energy or related stats change
        UpdateBars(boundCreature, instant: false);
    }

    private void OnBoundCreatureShieldingChanged(int currentShielding, int maxHP)
    {
        // Update bars when Shielding or related stats change
        UpdateBars(boundCreature, instant: false);
    }

    private void OnBoundCreatureDamaged(int damage)
    {
        PlayHitAnimation();
    }

    private void OnBoundCreatureHealed(int amount)
    {
        PlayHealAnimation();
    }

    public void OnTerrainChanged(TerrainType oldTerrain, TerrainType newTerrain, TerrainVisuals visuals)
    {
        if (terrainBackgroundImage == null) { 
            Debug.LogWarning("BattleTileUI: terrainBackgroundImage is not assigned!");
            return;
        }

        // If no visuals or no sprite, hide terrain background
        if (visuals == null || visuals.Sprite == null)
        {
            terrainBackgroundImage.gameObject.SetActive(false);
        }
        else
        {
            terrainBackgroundImage.sprite = visuals.Sprite;
            terrainBackgroundImage.color = visuals.TintColor;
            terrainBackgroundImage.gameObject.SetActive(true);
        }

        // If no visuals or no sprite, hide terrain
        if (visuals == null || visuals.ForegroundSprite == null)
        {
            terrainForegroundImage.gameObject.SetActive(false);
            return;
        }

        terrainForegroundImage.sprite = visuals.ForegroundSprite;
        terrainForegroundImage.color = visuals.TintColor;
        terrainForegroundImage.gameObject.SetActive(true);
    }

    public void OnSurfaceApplied(SurfaceEffect surface)
    {
        // TODO: Add surface visual feedback (particle effects, etc.)
    }

    public void OnSurfaceRemoved()
    {
        // TODO: Remove surface visual feedback
    }

    #endregion

    #region Creature Visuals

    private void SetupCreatureVisuals(Creature creature)
    {
        creatureSprite.sprite = creature.Species.FrontSprite;
        originalSpriteColor = creatureSprite.color;
        spriteHolder.SetActive(true);

        namePanel.SetActive(true);
        levelPanel.SetActive(true);
        barsContainer.SetActive(true);

        nameField.text = creature.Nickname;
        levelField.text = creature.Level.ToString();

        UpdateScale(creature.Species.Size);

        hpBar.SetVisibility(true);
        energyBar.SetVisibility(true);

        UpdateBars(creature, instant: true);
    }

    private void UpdateScale(CreatureSize size)
    {
        float baseSprite = size switch
        {
            CreatureSize.Tiny => TINY_SPRITE_SIZE,
            CreatureSize.Small => SMALL_SPRITE_SIZE,
            CreatureSize.Medium => MEDIUM_SPRITE_SIZE,
            CreatureSize.Large => LARGE_SPRITE_SIZE,
            CreatureSize.Huge => HUGE_SPRITE_SIZE,
            _ => MEDIUM_SPRITE_SIZE
        };

        float rowScale = DataTile.BattlefieldPosition.Row switch
        {
            0 => 1.0f,
            1 => TILE_SCALE_ROW1,
            2 => TILE_SCALE_ROW2,
            _ => 1.0f
        };

        float scale = (baseSprite / TILE_HEIGHT_ROW0) * rowScale;
        float flipX = isPlayerSide ? -1f : 1f;
        spriteHolder.transform.localScale = new Vector3(scale * flipX, scale, 1f);
    }

    private void Clear()
    {
        // Kill any leftover tweens on this tile's components
        creatureSprite.DOKill();
        spriteHolder.transform.DOKill();
        isAnimating = false;

        UnbindCreatureEvents();
        boundCreature = null;
        
        creatureSprite.sprite = null;
        spriteHolder.SetActive(false);

        ZeroBars();
        hpBar.SetVisibility(false, 0);
        energyBar.SetVisibility(false, 0);
        shieldBar.SetVisibility(false, 0);

        barsContainer.SetActive(false);

        if (nameField != null) nameField.text = "";
        if (namePanel != null) namePanel.SetActive(false);
        if (levelPanel != null) levelPanel.SetActive(false);

        UpdateScale(CreatureSize.Medium);
        ClearHighlight();
    }

    #endregion

    #region Bar Updates

    private void UpdateBars(Creature creature, bool instant)
    {
        if (creature == null) return;

        float hpPercent = (float)creature.HP / creature.MaxHP;
        float energyPercent = (float)creature.Energy / creature.MaxEnergy;
        float shieldPercent = (float)creature.Shielding / creature.MaxHP;

        hpBar.SetValue(hpPercent, instant);
        energyBar.SetValue(energyPercent, instant);

        // Shield logic: Hide if 0, show if active
        shieldBar.SetVisibility(creature.Shielding > 0);
        shieldBar.SetValue(shieldPercent, instant);
    }

    private void ZeroBars()
    {
        hpBar.SetValue(0, true);
        energyBar.SetValue(0, true);
        shieldBar.SetValue(0, true);
    }

    #endregion

    #region Animations

    public void PlaySummonAnimation()
    {
        isAnimating = true;
        creatureSprite.color = new Color(originalSpriteColor.r, originalSpriteColor.g, originalSpriteColor.b, 0f);

        creatureSprite.DOColor(originalSpriteColor, SUMMON_DURATION)
            .OnComplete(() => isAnimating = false);
    }

    public void PlayAttackAnimation()
    {
        if (isAnimating) return;

        isAnimating = true;
        float direction = isPlayerSide ? 1f : -1f;
        Vector3 attackPos = originalSpritePos + Vector3.right * (ATTACK_DISTANCE * direction);

        var sequence = DOTween.Sequence();
        sequence.Append(creatureSprite.transform.DOLocalMove(attackPos, ATTACK_SPEED));
        sequence.Append(creatureSprite.transform.DOLocalMove(originalSpritePos, ATTACK_SPEED));
        sequence.OnComplete(() => isAnimating = false);
    }

    public void PlayHitAnimation()
    {
        if (isAnimating) return;

        isAnimating = true;
        var sequence = DOTween.Sequence();

        // Flash white on hit
        sequence.Append(creatureSprite.DOColor(Color.white, HIT_DURATION));
        sequence.Append(creatureSprite.DOColor(originalSpriteColor, HIT_DURATION));

        // Shake for impact
        spriteHolder.transform.DOShakePosition(
            HIT_DURATION * 2, strength: 15f, vibrato: 10, randomness: 90, snapping: false, fadeOut: true);

        sequence.OnComplete(() => isAnimating = false);
    }

    public void PlayHealAnimation()
    {
        if (isAnimating) return;

        isAnimating = true;
        Vector3 originalScale = creatureSprite.transform.localScale;

        var sequence = DOTween.Sequence();
        sequence.Append(creatureSprite.DOColor(Color.green, HEAL_DURATION / 2));
        sequence.Join(creatureSprite.transform.DOScale(originalScale * 1.05f, HEAL_DURATION / 2));
        sequence.Append(creatureSprite.DOColor(originalSpriteColor, HEAL_DURATION / 2));
        sequence.Join(creatureSprite.transform.DOScale(originalScale, HEAL_DURATION / 2));
        sequence.OnComplete(() => isAnimating = false);
    }

    public void PlayDefeatAnimation()
    {
        if (isAnimating) return;
        isAnimating = true;

        var sequence = DOTween.Sequence();
        sequence.Append(creatureSprite.DOFade(0f, DEFEAT_DURATION));

        // Use your clean method instead of GetComponent
        hpBar.SetVisibility(false, DEFEAT_DURATION);
        energyBar.SetVisibility(false, DEFEAT_DURATION);
        shieldBar.SetVisibility(false, DEFEAT_DURATION);

        sequence.SetEase(Ease.InOutQuad).OnComplete(() =>
        {
            isAnimating = false;
            Clear();
        });
    }

    #endregion

    #region Highlight Management

    public void SetPersistentHighlight(HighlightType type)
    {
        persistentHighlight = type;
        SetHighlight(type);
    }
    public void ClearPersistentHighlight()
    {
        persistentHighlight = null;
        if (pinnedHighlight.HasValue)
            SetHighlight(pinnedHighlight.Value);
        else
            ClearHighlight();
    }

    public void SetPinnedHighlight(HighlightType type)
    {
        pinnedHighlight = type;
        SetHighlight(type);
    }
    public void ClearPinnedHighlight()
    {
        pinnedHighlight = null;
        ClearHighlight();
    }

    public void SetHoverHighlight(HighlightType type)
    {
        SetHighlight(type); // visually override
    }
    public void ClearHoverHighlight()
    {
        if (persistentHighlight.HasValue)
            SetHighlight(persistentHighlight.Value);
        else if (pinnedHighlight.HasValue)
            SetHighlight(pinnedHighlight.Value);
        else
            ClearHighlight();
    }

    public void SetHighlight(HighlightType type)
    {
        switch (type)
        {
            case HighlightType.ActiveCreature:
                highlightAura.sprite = activeCreatureAura;
                highlightAura.gameObject.SetActive(true);
                selectionArrow.gameObject.SetActive(false);
                break;

            case HighlightType.NegativeTarget:
                highlightAura.sprite = negativeTargetAura;
                highlightAura.gameObject.SetActive(true);
                selectionArrow.sprite = validArrowSprite;
                selectionArrow.gameObject.SetActive(true);
                break;

            case HighlightType.PositiveTarget:
                highlightAura.sprite = positiveTargetAura;
                highlightAura.gameObject.SetActive(true);
                selectionArrow.sprite = validArrowSprite;
                selectionArrow.gameObject.SetActive(true);
                break;

            case HighlightType.MoveTarget:
                highlightAura.sprite = moveTargetAura;
                highlightAura.gameObject.SetActive(true);
                selectionArrow.sprite = validArrowSprite;
                selectionArrow.gameObject.SetActive(true);
                break;

            case HighlightType.InvalidTarget:
                highlightAura.gameObject.SetActive(false);
                selectionArrow.sprite = invalidArrowSprite;
                selectionArrow.gameObject.SetActive(true);
                break;

            case HighlightType.ValidTarget:
                highlightAura.gameObject.SetActive(false);
                selectionArrow.sprite = validArrowSprite;
                selectionArrow.gameObject.SetActive(true);
                break;

            default:
                ClearHighlight();
                break;
        }
    }

    public void ClearHighlight()
    {
        highlightAura.gameObject.SetActive(false);
        selectionArrow.gameObject.SetActive(false);
    }

    #endregion

    public bool IsAnimating => isAnimating;

    private void OnDestroy()
    {
        // Clean up subscriptions when UI is destroyed
        UnsubscribeFromTile(DataTile);
    }
}