using UnityEngine;
using UnityEngine.UI;
using TMPro;
using DG.Tweening;
using System.Collections.Generic;
using static UIConstants;

/// <summary>
/// Handles all visual presentation for a single battle tile.
/// Pure UI - no game logic, just responds to events from BattleTile.
/// </summary>
public class BattleTileUI : MonoBehaviour
{
    [Header("Creature Display")]
    [SerializeField] private Image creatureSprite;
    [SerializeField] private GameObject creatureContainer;

    [Header("Status Bars")]
    [SerializeField] private Slider hpBar;
    [SerializeField] private Slider energyBar;
    [SerializeField] private Slider xpBar;

    [Header("Highlights")]
    [SerializeField] private Image highlightAura;
    [SerializeField] private Image selectionArrow;

    [Header("Highlight Sprites")]
    [SerializeField] private Sprite activeCreatureSprite;
    [SerializeField] private Sprite negativeTargetSprite;
    [SerializeField] private Sprite positiveTargetSprite;
    [SerializeField] private Sprite moveTargetSprite;
    [SerializeField] private Sprite validArrowSprite;
    [SerializeField] private Sprite invalidArrowSprite;
    [SerializeField] private Sprite selectedArrowSprite;

    [Header("Info Display")]
    [SerializeField] private TextMeshProUGUI nameText;
    [SerializeField] private TextMeshProUGUI levelText;
    [SerializeField] private GameObject infoPanel;

    [Header("Status Window")]
    [SerializeField] private CreatureStatusWindow statusWindow;

    [Header("Terrain Display")]
    [SerializeField] private Image terrainImage;

    // References
    public BattleTile DataTile { get; private set; }
    private bool isPlayerSide;

    // Currently bound creature for event subscriptions
    private Creature boundCreature;

    // Animation state
    private Vector3 originalSpritePos;
    private Color originalSpriteColor;
    private bool isAnimating;

    // Queued updates for smooth animations
    private Queue<float> hpQueue = new Queue<float>();
    private Queue<float> energyQueue = new Queue<float>();
    private Queue<float> xpQueue = new Queue<float>();
    private bool isUpdatingHP;
    private bool isUpdatingEnergy;
    private bool isUpdatingXP;

    public void Initialize(BattleTile tile, bool playerSide)
    {
        // Unsubscribe previous tile if any
        if (DataTile != null)
            UnsubscribeFromTile(DataTile);

        DataTile = tile;
        isPlayerSide = playerSide;

        // Subscribe to tile-level events (tile forwards creature events)
        SubscribeToTile(DataTile);

        SetupLayout();
        Clear();
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
        if (!isPlayerSide)
        {
            // Flip sprite direction for enemy
            Vector3 scale = creatureSprite.transform.localScale;
            scale.x *= -1;
            creatureSprite.transform.localScale = scale;
        }

        // Store original values
        originalSpritePos = creatureSprite.transform.localPosition;
        originalSpriteColor = creatureSprite.color;
    }

    #region Event Handlers from BattleTile

    public void OnCreaturePlaced(Creature creature)
    {
        if (creature == null)
        {
            Clear();
            return;
        }

        // Unsubscribe from any previously-bound creature just in case
        if (boundCreature != null)
        {
            UnbindCreatureEvents();
            boundCreature = null;
        }

        boundCreature = creature;

        SetupCreatureVisuals(creature);
        UpdateBars(creature, instant: true);
        PlaySummonAnimation();

        // Subscribe to creature events (use actual event names)
        boundCreature.OnHPChanged += OnBoundCreatureHPChanged;
        boundCreature.OnTakeDamage += OnBoundCreatureDamaged;
        boundCreature.OnHealed += OnBoundCreatureHealed;
    }

    public void OnCreatureRemoved(Creature creature)
    {
        // Unsubscribe to avoid memory leaks
        if (boundCreature != null)
        {
            UnbindCreatureEvents();
            boundCreature = null;
        }

        Clear();
    }

    private void UnbindCreatureEvents()
    {
        if (boundCreature == null) return;
        boundCreature.OnHPChanged -= OnBoundCreatureHPChanged;
        boundCreature.OnTakeDamage -= OnBoundCreatureDamaged;
        boundCreature.OnHealed -= OnBoundCreatureHealed;
    }

    private void OnBoundCreatureHPChanged(int currentHP, int maxHP)
    {
        // Update bars when HP or related stats change
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
        if (newTerrain == null || visuals == null)
        {
            terrainImage.gameObject.SetActive(false);
            return;
        }

        terrainImage.sprite = visuals.Sprite;
        terrainImage.color = visuals.TintColor;
        terrainImage.gameObject.SetActive(true);
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
        creatureSprite.gameObject.SetActive(true);
        creatureContainer.SetActive(true);

        SetSpriteSize(creature.Species.Size);

        nameText.text = creature.Nickname;
        levelText.text = creature.Level.ToString();
        infoPanel.SetActive(true);

        // Setup bars
        hpBar.gameObject.SetActive(true);
        energyBar.gameObject.SetActive(isPlayerSide);
        xpBar.gameObject.SetActive(isPlayerSide);

        // Update status window if present
        statusWindow?.UpdateCreature(creature);
    }

    private void SetSpriteSize(CreatureSize size)
    {
        int dimension = BASE_SPRITE_SIZE;

        switch (size)
        {
            case CreatureSize.VerySmall:
                dimension -= 2 * SIZE_INCREMENT;
                break;
            case CreatureSize.Small:
                dimension -= SIZE_INCREMENT;
                break;
            case CreatureSize.Large:
                dimension += SIZE_INCREMENT;
                break;
            case CreatureSize.ExtraLarge:
                dimension += 2 * SIZE_INCREMENT;
                break;
        }

        RectTransform spriteRect = creatureSprite.GetComponent<RectTransform>();
        spriteRect.sizeDelta = new Vector2(dimension, dimension);

        // Scale aura accordingly
        if (highlightAura != null)
        {
            RectTransform auraRect = highlightAura.GetComponent<RectTransform>();
            float auraDimension = dimension * AURA_SIZE_MULTIPLIER;
            auraRect.sizeDelta = new Vector2(auraDimension, auraDimension);
        }
    }

    private void Clear()
    {
        creatureSprite.sprite = null;
        creatureSprite.gameObject.SetActive(false);
        creatureContainer.SetActive(false);

        hpBar.value = 0;
        energyBar.value = 0;
        xpBar.value = 0;
        hpBar.gameObject.SetActive(false);
        energyBar.gameObject.SetActive(false);
        xpBar.gameObject.SetActive(false);

        infoPanel.SetActive(false);
        ClearHighlight();

        statusWindow?.Clear();
    }

    #endregion

    #region Bar Updates

    private void UpdateBars(Creature creature, bool instant)
    {
        float hpPercent = (float)creature.HP / creature.MaxHP;
        float energyPercent = (float)creature.Energy / creature.MaxEnergy;
        float xpPercent = creature.XP / 100f; // TODO: Use actual XP requirement

        if (instant)
        {
            hpBar.value = hpPercent;
            energyBar.value = energyPercent;
            xpBar.value = xpPercent;
        }
        else
        {
            QueueBarUpdate(hpQueue, hpPercent, hpBar, () => isUpdatingHP = true, () => isUpdatingHP = false);
            QueueBarUpdate(energyQueue, energyPercent, energyBar, () => isUpdatingEnergy = true, () => isUpdatingEnergy = false);
            QueueBarUpdate(xpQueue, xpPercent, xpBar, () => isUpdatingXP = true, () => isUpdatingXP = false);
        }
    }

    private void QueueBarUpdate(Queue<float> queue, float targetValue, Slider slider, System.Action onStart, System.Action onComplete)
    {
        queue.Enqueue(Mathf.Clamp01(targetValue));

        if (queue == hpQueue)
        {
            if (!isUpdatingHP)
                StartCoroutine(ProcessBarUpdates(queue, slider, onStart, onComplete));
        }
        else if (queue == energyQueue)
        {
            if (!isUpdatingEnergy)
                StartCoroutine(ProcessBarUpdates(queue, slider, onStart, onComplete));
        }
        else if (queue == xpQueue)
        {
            if (!isUpdatingXP)
                StartCoroutine(ProcessBarUpdates(queue, slider, onStart, onComplete));
        }
    }

    private System.Collections.IEnumerator ProcessBarUpdates(
        Queue<float> queue,
        Slider slider,
        System.Action onStart,
        System.Action onComplete)
    {
        onStart?.Invoke();

        while (queue.Count > 0)
        {
            float targetValue = queue.Dequeue();
            float startValue = slider.value;
            float elapsed = 0f;

            while (elapsed < SLIDER_DURATION)
            {
                slider.value = Mathf.Lerp(startValue, targetValue, elapsed / SLIDER_DURATION);
                elapsed += Time.deltaTime;
                yield return null;
            }

            slider.value = targetValue;
        }

        onComplete?.Invoke();
    }

    #endregion

    #region Animations

    public void PlaySummonAnimation()
    {
        if (isAnimating) return;

        isAnimating = true;
        Color fadeColor = originalSpriteColor;
        fadeColor.a = 0;
        creatureSprite.color = fadeColor;

        creatureSprite.DOFade(1f, SUMMON_DURATION)
            .SetEase(Ease.InOutQuad)
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
        sequence.Append(creatureSprite.DOColor(Color.gray, HIT_DURATION));
        sequence.Append(creatureSprite.DOColor(originalSpriteColor, HIT_DURATION));
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
        creatureSprite.DOFade(0f, DEFEAT_DURATION)
            .SetEase(Ease.InOutQuad)
            .OnComplete(() =>
            {
                isAnimating = false;
                Clear();
            });
    }

    #endregion

    #region Highlight Management

    public void SetHighlight(HighlightType type)
    {
        switch (type)
        {
            case HighlightType.ActiveCreature:
                highlightAura.sprite = activeCreatureSprite;
                highlightAura.gameObject.SetActive(true);
                selectionArrow.gameObject.SetActive(false);
                break;

            case HighlightType.NegativeTarget:
                highlightAura.sprite = negativeTargetSprite;
                highlightAura.gameObject.SetActive(true);
                selectionArrow.sprite = validArrowSprite;
                selectionArrow.gameObject.SetActive(true);
                break;

            case HighlightType.PositiveTarget:
                highlightAura.sprite = positiveTargetSprite;
                highlightAura.gameObject.SetActive(true);
                selectionArrow.sprite = validArrowSprite;
                selectionArrow.gameObject.SetActive(true);
                break;

            case HighlightType.MoveTarget:
                highlightAura.sprite = moveTargetSprite;
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

    #region Status Window

    public void ShowStatusWindow()
    {
        if (DataTile.IsOccupied)
        {
            statusWindow?.Show(DataTile.OccupyingCreature);
        }
    }

    public void HideStatusWindow()
    {
        statusWindow?.Hide();
    }

    #endregion

    public bool IsAnimating => isAnimating || isUpdatingHP || isUpdatingEnergy || isUpdatingXP;

    private void OnDestroy()
    {
        // Clean up subscriptions when UI is destroyed
        UnsubscribeFromTile(DataTile);
    }
}