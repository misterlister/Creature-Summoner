using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class MenuPanel : MonoBehaviour
{
    [SerializeField] private List<TextMeshProUGUI> slots;
    [SerializeField] private Color normalColor = Color.black;
    [SerializeField] private Color highlightColor = Color.blue;
    [SerializeField] private Color disabledColor = new Color(0.35f, 0.35f, 0.35f);
    [SerializeField] private Color highlightDisabledColor = new Color(0.6f, 0.6f, 0.6f);

    private int activeCount = 0;
    private HashSet<int> disabledIndices = new();

    public int ActiveCount => activeCount;
    public bool IsDisabled(int index) => disabledIndices.Contains(index);
    public string GetLabel(int index) =>
        index >= 0 && index < activeCount ? slots[index].text : "";

    public void Build(List<string> labels, HashSet<int> disabled = null)
    {
        disabledIndices = disabled ?? new HashSet<int>();
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
    }

    public void SetSelection(int index)
    {
        for (int i = 0; i < activeCount; i++)
        {
            if (disabledIndices.Contains(i))
            {
                slots[i].color = (i == index) ? highlightDisabledColor : disabledColor;
                continue;
            }
            slots[i].color = (i == index) ? highlightColor : normalColor;
        }
    }
    public void SetDisabled(int index, bool disabled)
    {
        if (index < 0 || index >= activeCount) return;

        if (disabled)
            disabledIndices.Add(index);
        else
            disabledIndices.Remove(index);

        slots[index].color = disabled ? disabledColor : normalColor;
    }
    public void Clear()
    {
        foreach (var slot in slots)
            slot.gameObject.SetActive(false);

        disabledIndices.Clear();
        activeCount = 0;
    }
}