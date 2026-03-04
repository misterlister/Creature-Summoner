using TMPro;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Represents a single row in a stat display using Layout Groups for automatic spacing.
/// </summary>
public class StatRow : MonoBehaviour
{
    [Header("Text Components")]
    [SerializeField] private TextMeshProUGUI statNameText;
    [SerializeField] private TextMeshProUGUI baseValueText;
    [SerializeField] private TextMeshProUGUI modifiedValueText;

    [Header("Colors")]
    [SerializeField] private Color defaultColor = Color.black;
    [SerializeField] private Color increasedColor = Color.green;
    [SerializeField] private Color decreasedColor = Color.red;

    [Header("Layout Settings")]
    [SerializeField] private bool autoManageLayout = true;

    private DisplayMode currentMode = DisplayMode.Uninitialized;

    private enum DisplayMode
    {
        Uninitialized,
        Single,
        Double
    }

    #region Setup

    /// <summary>
    /// Initialize the row for single or double value display.
    /// Layout groups will automatically handle spacing.
    /// </summary>
    public void SetupRow(string name, bool singleField)
    {
        if (statNameText != null)
            statNameText.text = name;

        currentMode = singleField ? DisplayMode.Single : DisplayMode.Double;
        UpdateFieldVisibility();
    }

    private void UpdateFieldVisibility()
    {
        // In single mode, hide the modified value text
        // Layout group will automatically expand baseValue to fill space
        if (currentMode == DisplayMode.Single)
        {
            SetActive(baseValueText, true);
            SetActive(modifiedValueText, false);
        }
        else if (currentMode == DisplayMode.Double)
        {
            SetActive(baseValueText, true);
            SetActive(modifiedValueText, true);
        }

        // Force layout rebuild if auto-manage is enabled
        if (autoManageLayout)
        {
            LayoutRebuilder.ForceRebuildLayoutImmediate(GetComponent<RectTransform>());
        }
    }

    #endregion

    #region Update Methods

    /// <summary>
    /// Update stats display with base and modified values.
    /// Automatically color-codes and hides modified value if unchanged.
    /// </summary>
    public void UpdateStats(int baseValue, int modifiedValue)
    {
        ValidateMode(DisplayMode.Double, "UpdateStats");

        if (baseValueText != null)
        {
            baseValueText.text = baseValue.ToString();
            baseValueText.color = defaultColor;
        }

        // Only show modified value if different from base
        if (modifiedValue == baseValue)
        {
            SetActive(modifiedValueText, false);
        }
        else
        {
            SetActive(modifiedValueText, true);
            if (modifiedValueText != null)
            {
                modifiedValueText.text = modifiedValue.ToString();
                modifiedValueText.color = GetComparisonColor(modifiedValue, baseValue);
            }
        }
    }

    /// <summary>
    /// Update with a single text value.
    /// When in single mode, baseValueText will automatically expand to fill available space.
    /// </summary>
    public void UpdateSingleText(string text, Color? color = null)
    {
        ValidateMode(DisplayMode.Single, "UpdateSingleText");

        SetActive(modifiedValueText, false);

        if (baseValueText != null)
        {
            baseValueText.text = text;
            baseValueText.color = color ?? defaultColor;
        }
    }

    /// <summary>
    /// Update with two text values (both the same color).
    /// </summary>
    public void UpdateDoubleText(string text1, string text2, Color? color = null)
    {
        ValidateMode(DisplayMode.Double, "UpdateDoubleText");

        Color useColor = color ?? defaultColor;

        if (baseValueText != null)
        {
            baseValueText.text = text1;
            baseValueText.color = useColor;
        }

        if (modifiedValueText != null)
        {
            modifiedValueText.text = text2;
            modifiedValueText.color = useColor;
            SetActive(modifiedValueText, true);
        }
    }

    /// <summary>
    /// Update with current/max resource values (HP, Energy, etc.).
    /// </summary>
    public void UpdateResource(int current, int max, Color? color = null)
    {
        ValidateMode(DisplayMode.Single, "UpdateResource");

        SetActive(modifiedValueText, false);

        if (baseValueText != null)
        {
            baseValueText.text = $"{current}/{max}";
            baseValueText.color = color ?? defaultColor;
        }
    }
    public void UpdateModifier(float value)
    {
        ValidateMode(DisplayMode.Single, "UpdateModifier");
        SetActive(modifiedValueText, false);

        if (baseValueText != null)
        {
            string sign = value > 0 ? "+" : "";
            baseValueText.text = $"{sign}{value:P0}";
            baseValueText.color = value > 0 ? increasedColor
                                : value < 0 ? decreasedColor
                                : defaultColor;
        }
    }

    #endregion

    #region Helper Methods

    private Color GetComparisonColor(int modifiedValue, int baseValue)
    {
        if (modifiedValue < baseValue)
            return decreasedColor;
        else if (modifiedValue > baseValue)
            return increasedColor;
        else
            return defaultColor;
    }

    private void ValidateMode(DisplayMode expectedMode, string methodName)
    {
        if (currentMode == DisplayMode.Uninitialized)
        {
            Debug.LogWarning($"StatRow: {methodName} called before SetupRow. Call SetupRow first.");
        }
        else if (currentMode != expectedMode)
        {
            Debug.LogWarning($"StatRow: {methodName} called but row is in {currentMode} mode (expected {expectedMode}).");
        }
    }

    private void SetActive(Component component, bool active)
    {
        if (component != null)
        {
            component.gameObject.SetActive(active);
        }
    }

    #endregion

    #region Public Accessors

    public void SetStatName(string name)
    {
        if (statNameText != null)
            statNameText.text = name;
    }

    public void Clear()
    {
        if (statNameText != null) statNameText.text = "";
        if (baseValueText != null) baseValueText.text = "";
        if (modifiedValueText != null) modifiedValueText.text = "";
    }

    /// <summary>
    /// Force the layout to rebuild (useful if you've changed sizes manually).
    /// </summary>
    public void RebuildLayout()
    {
        LayoutRebuilder.ForceRebuildLayoutImmediate(GetComponent<RectTransform>());
    }

    #endregion
}
