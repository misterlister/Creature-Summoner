using System.Collections.Generic;
using TMPro;
using UnityEngine;
using static BattleUIConstants;

public class MenuPanel : MonoBehaviour
{
    [SerializeField] private List<TextMeshProUGUI> slots;
    [SerializeField] private Color normalColor = Color.black;
    [SerializeField] private Color highlightColor = Color.blue;
    [SerializeField] private Color disabledColor = new Color(0.35f, 0.35f, 0.35f);
    [SerializeField] private Color highlightDisabledColor = new Color(0.6f, 0.6f, 0.6f);

    private int activeCount = 0;
    private HashSet<int> disabledIndices = new();
    private int currentIndex = 0;
    private int backIndex = -1;

    public int ActiveCount => activeCount;
    public int CurrentIndex => currentIndex;
    public bool IsDisabled(int index) => disabledIndices.Contains(index);

    public bool IsBackButton(int index) => index == backIndex;
    public string GetLabel(int index) =>
        index >= 0 && index < activeCount ? slots[index].text : "";

    public void Build(List<string> labels, HashSet<int> disabled = null, int backIndex = -1)
    {
        disabledIndices = disabled ?? new HashSet<int>();
        this.backIndex = backIndex;
        activeCount = labels.Count;

        for (int i = 0; i < slots.Count; i++)
        {
            if (i < labels.Count)
            {
                slots[i].text = labels[i];
                slots[i].gameObject.SetActive(true);
                slots[i].color = disabledIndices.Contains(i) ? disabledColor : normalColor;
            }
            else
            {
                slots[i].gameObject.SetActive(false);
            }
        }

        currentIndex = 0;
        SetSelection(0);
    }

    public void SetSelection(int index)
    {
        if (index < 0 || index >= activeCount) return;

        // Clear previous selection highlight
        slots[currentIndex].color = disabledIndices.Contains(currentIndex) ? disabledColor : normalColor;

        // Apply new selection highlight
        currentIndex = index;
        slots[currentIndex].color = disabledIndices.Contains(currentIndex) ? highlightDisabledColor : highlightColor;
    }

    public void MoveSelection(Direction direction)
    {
        if (activeCount == 0) return;
        int newIndex = currentIndex;
        int col = newIndex % MENU_COLS;

        switch (direction)
        {
            case Direction.Down:
                int down = newIndex + MENU_COLS;
                if (down < activeCount) newIndex = down;
                break;
            case Direction.Up:
                int up = newIndex - MENU_COLS;
                if (up >= 0) newIndex = up;
                break;
            case Direction.Right:
                int right = newIndex + 1;
                if (col < MENU_COLS - 1 && right < activeCount) newIndex = right;
                break;
            case Direction.Left:
                int left = newIndex - 1;
                if (col > 0) newIndex = left;
                break;
        }
        SetSelection(newIndex);
    }


    public void SetDisabled(int index, bool disabled)
    {
        if (index < 0 || index >= activeCount) return;

        if (disabled) disabledIndices.Add(index);
        else disabledIndices.Remove(index);

        bool isSelected = index == currentIndex;
        slots[index].color = disabled
            ? (isSelected ? highlightDisabledColor : disabledColor)
            : (isSelected ? highlightColor : normalColor);
    }
    public void Clear()
    {
        foreach (var slot in slots)
            slot.gameObject.SetActive(false);

        disabledIndices.Clear();
        activeCount = 0;
    }

    public bool IsBackButtonSelected() => backIndex >= 0 && currentIndex == backIndex;
}