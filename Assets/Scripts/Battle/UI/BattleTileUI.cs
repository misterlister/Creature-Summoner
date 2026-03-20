using DG.Tweening;
using TMPro;
using UnityEngine;
using System;
using UnityEngine.UI;
using static BattleUIConstants;

/// <summary>
/// Handles all visual presentation for a single battle tile.
/// Pure UI - no game logic, just responds to events from BattleTile.
/// </summary>
public class BattleTileUI : MonoBehaviour
{
    [Header("Layout")]
    [SerializeField] private RectTransform scalableSection;
    [SerializeField] private RectTransform displayColumn;

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

    [Header("Info Display")]
    [SerializeField] private GameObject namePanel;
    [SerializeField] private TextMeshProUGUI nameField;
    [SerializeField] private GameObject levelPanel;
    [SerializeField] private TextMeshProUGUI levelField;

    [Header("Terrain Display")]
    [SerializeField] private Image terrainBackgroundImage;
    [SerializeField] private Image terrainForegroundImage;

    [Header("Highlight Colours")]
    [SerializeField] private Color activeColour = new Color(0.42f, 0.65f, 0.96f, 1f);
    [SerializeField] private Color moveColour = new Color(0.98f, 0.78f, 0.46f, 1f);
    [SerializeField] private Color supportColour = new Color(0.59f, 0.75f, 0.35f, 1f);
    [SerializeField] private Color offensiveColour = new Color(0.88f, 0.29f, 0.29f, 1f);

    [Header("Arrow Colours")]
    [SerializeField] private Color arrowValidColour = new Color(0.96f, 0.92f, 0.80f);
    [SerializeField] private Color arrowInvalidColour = new Color(0.55f, 0.55f, 0.60f);

    private Color Hovered(Color c)
    {
        Color.RGBToHSV(c, out float h, out float s, out float v);
        return Color.HSVToRGB(h, s * 0.9f, v);
    }

    private Color Secondary(Color c)
    {
        Color.RGBToHSV(c, out float h, out float s, out float v);
        return Color.HSVToRGB(h, s * 0.8f, v);
    }

    private Color Tertiary(Color c)
    {
        Color.RGBToHSV(c, out float h, out float s, out float v);
        return Color.HSVToRGB(h, s * 0.7f, v);
    }
    private Color Valid(Color c)
    {
        Color.RGBToHSV(c, out float h, out float s, out float v);
        return Color.HSVToRGB(h, s * 0.5f, v);
    }

    // References
    public BattleTile DataTile { get; private set; }
    private bool isPlayerSide;
    private Creature boundCreature;
    public float TileScale { get; private set; }

    // Animation state
    private Vector2 originalSpriteAnchoredPos;
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
        selectionArrow.gameObject.SetActive(true);
        SetArrow(SelectionArrowState.Hidden);
    }

    private void SubscribeToTile(BattleTile tile)
    {
        if (tile == null) return;
        tile.OnCreaturePlaced += OnCreaturePlaced;
        tile.OnCreatureRemoved += OnCreatureRemoved;
        tile.OnTileTerrainChanged += OnTileTerrainChanged;
        tile.OnSurfaceApplied += OnSurfaceApplied;
        tile.OnSurfaceRemoved += OnSurfaceRemoved;
        tile.OnCreatureDefeated += OnCreatureDefeated;

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
        tile.OnTileTerrainChanged -= OnTileTerrainChanged;
        tile.OnSurfaceApplied -= OnSurfaceApplied;
        tile.OnSurfaceRemoved -= OnSurfaceRemoved;
        tile.OnCreatureDefeated -= OnCreatureDefeated;

        tile.OnCreatureHPChanged -= OnBoundCreatureHPChanged;
        tile.OnCreatureDamaged -= OnBoundCreatureDamaged;
        tile.OnCreatureHealed -= OnBoundCreatureHealed;
    }

    private void SetupLayout()
    {
        var hlg = GetComponent<HorizontalLayoutGroup>();
        hlg.reverseArrangement = !isPlayerSide;
        hlg.childAlignment = isPlayerSide ? TextAnchor.LowerRight : TextAnchor.LowerLeft;

        RectTransform scalableRect = scalableSection.GetComponent<RectTransform>();
        if (isPlayerSide)
        {
            scalableRect.anchorMin = new Vector2(0, 0);
            scalableRect.anchorMax = new Vector2(0, 0);
            scalableRect.pivot = new Vector2(0, 0);
        }
        else
        {
            scalableRect.anchorMin = new Vector2(1, 0);
            scalableRect.anchorMax = new Vector2(1, 0);
            scalableRect.pivot = new Vector2(1, 0);
        }
        scalableRect.anchoredPosition = new Vector2(0, NAME_PANEL_HEIGHT);

        originalSpriteAnchoredPos = spriteHolder.GetComponent<RectTransform>().anchoredPosition;
    }
    public void SetTileSize(int row)
    {
        TileScale = row switch
        {
            1 => TILE_SCALE_ROW1,
            2 => TILE_SCALE_ROW2,
            _ => 1f
        };

        float scalableSize = BASE_SCALABLE_SIZE * TileScale;

        GetComponent<RectTransform>().sizeDelta = new Vector2(TILE_WIDTH_ROW0 * TileScale, TILE_HEIGHT_ROW0 * TileScale);
        scalableSection.sizeDelta = Vector2.one * scalableSize;
        displayColumn.sizeDelta = new Vector2(scalableSize, scalableSize + NAME_PANEL_HEIGHT);
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

        boundCreature = creature;
        SetupCreatureVisuals(creature);
        UpdateBars(creature, instant: true);
        PlaySummonAnimation();
    }

    public void OnCreatureRemoved(Creature creature)
    {
        Clear();
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

    public void OnTileTerrainChanged(TerrainType oldTerrain, TerrainType newTerrain, TerrainVisuals visuals)
    {
        if (terrainBackgroundImage == null) { 
            Debug.LogWarning("BattleTileUI: terrainBackgroundImage is not assigned!");
            return;
        }

        Debug.Log($"BattleTileUI: Row {DataTile.BattlefieldPosition.Row}, Col {DataTile.BattlefieldPosition.GlobalCol} visuals: {(visuals != null ? visuals.VariantName : "null")}");

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

        // If no visuals or no sprite, hide terrain foreground
        if (visuals == null || visuals.ForegroundSprite == null)
        {
            terrainForegroundImage.gameObject.SetActive(false);
            return;
        }

        terrainForegroundImage.sprite = visuals.ForegroundSprite;
        terrainForegroundImage.color = visuals.TintColor;
        terrainForegroundImage.gameObject.SetActive(true);

    }

    private void OnSurfaceApplied(SurfaceEffect surface)
    {
        // TODO: Add surface visual feedback (particle effects, etc.)
    }

    private void OnSurfaceRemoved()
    {
        // TODO: Remove surface visual feedback
    }

    private void OnCreatureDefeated(Creature creature)
    {
        PlayDefeatAnimation(onComplete: () => DataTile.RemoveCreature());
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
        levelField.text = $"Lv. {creature.Level.ToString()}";

        UpdateSpriteAnchor(creature);
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

        float scale = (baseSprite / BASE_SCALABLE_SIZE) * TileScale;
        float flipX = isPlayerSide ? -1f : 1f;
        spriteHolder.transform.localScale = new Vector3(scale * flipX, scale, 1f);
        
    }
    private void UpdateSpriteAnchor(Creature creature = null)
    {
        
        RectTransform spriteRect = spriteHolder.GetComponent<RectTransform>();
        Debug.Log($"Updating sprite anchor for {(creature != null ? creature.Nickname : "null creature")}: AnchorMin={spriteRect.anchorMin}, AnchorMax={spriteRect.anchorMax}, Pivot={spriteRect.pivot}");
        bool isAir = creature != null && creature.IsElement(CreatureElement.Air);

        spriteRect.anchorMin = new Vector2(0.5f, isAir ? 1f : 0f);
        spriteRect.anchorMax = new Vector2(0.5f, isAir ? 1f : 0f);
        spriteRect.pivot = new Vector2(0.5f, isAir ? 1f : 0f);
        spriteRect.anchoredPosition = Vector2.zero;

        originalSpriteAnchoredPos = spriteRect.anchoredPosition;
        Debug.Log($"Updated sprite anchor for {(creature != null ? creature.Nickname : "null creature")}: AnchorMin={spriteRect.anchorMin}, AnchorMax={spriteRect.anchorMax}, Pivot={spriteRect.pivot}");
        
    }

    private void Clear()
    {
        // Kill any leftover tweens on this tile's components
        creatureSprite.DOKill();
        spriteHolder.GetComponent<RectTransform>().DOKill();
        isAnimating = false;

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

        UpdateSpriteAnchor();
        UpdateScale(CreatureSize.Medium);
        ClearAllHighlights();
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
        RectTransform spriteRect = spriteHolder.GetComponent<RectTransform>();
        float direction = isPlayerSide ? 1f : -1f;
        Vector2 attackPos = originalSpriteAnchoredPos + Vector2.right * (ATTACK_DISTANCE * direction);

        var sequence = DOTween.Sequence();
        sequence.Append(spriteRect.DOAnchorPos(attackPos, ATTACK_SPEED));
        sequence.Append(spriteRect.DOAnchorPos(originalSpriteAnchoredPos, ATTACK_SPEED));
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

    public void PlayDefeatAnimation(Action onComplete = null)
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
            onComplete?.Invoke();
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
        SetHighlight(HighlightType.None);
    }

    public void SetPinnedHighlight(HighlightType type)
    {
        pinnedHighlight = type;
        SetHighlight(type);
    }
    public void ClearPinnedHighlight()
    {
        pinnedHighlight = null;
        SetHighlight(HighlightType.None);
    }

    public void SetHighlight(HighlightType type)
    {
        if (type == HighlightType.None)
        {
            if (persistentHighlight.HasValue)
            {
                SetHighlight(persistentHighlight.Value);
                return;
            }
            if (pinnedHighlight.HasValue)
            {
                SetHighlight(pinnedHighlight.Value);
                return;
            }
            highlightAura.gameObject.SetActive(false);
            return;
        }

        highlightAura.color = type switch
        {
            HighlightType.ActiveCreature => Hovered(activeColour),
            HighlightType.ValidMove => Valid(moveColour),
            HighlightType.HoveredMove => Hovered(moveColour),
            HighlightType.SupportTarget => Valid(supportColour),
            HighlightType.HoveredSupportTarget => Hovered(supportColour),
            HighlightType.SelectedSupportTarget => supportColour,
            HighlightType.SupportSplashSecondary => Secondary(supportColour),
            HighlightType.SupportSplashTertiary => Tertiary(supportColour),
            HighlightType.OffensiveTarget => Valid(offensiveColour),
            HighlightType.HoveredOffensiveTarget => Hovered(offensiveColour),
            HighlightType.SelectedOffensiveTarget => offensiveColour,
            HighlightType.OffensiveSplashSecondary => Secondary(offensiveColour),
            HighlightType.OffensiveSplashTertiary => Tertiary(offensiveColour),
            _ => Color.clear
        };
        highlightAura.gameObject.SetActive(true);
        Debug.Log($"highlightAura Set to {type}");
    }

    public void SetArrow(SelectionArrowState state)
    {
        selectionArrow.color = state switch
        {
            SelectionArrowState.HoveredValid => arrowValidColour,
            SelectionArrowState.HoveredInvalid => arrowInvalidColour,
            SelectionArrowState.Hidden => Color.clear,
            _ => Color.clear
        };
    }

    public void ClearAllHighlights()
    {
        pinnedHighlight = null;
        persistentHighlight = null;
        highlightAura.gameObject.SetActive(false);
        SetArrow(SelectionArrowState.Hidden);
    }

    #endregion

    public bool IsAnimating => isAnimating;

    private void OnDestroy()
    {
        // Clean up subscriptions when UI is destroyed
        UnsubscribeFromTile(DataTile);
    }
}