using UnityEngine;

public class CreatureHudManager : MonoBehaviour
{
    public static CreatureHudManager Instance { get; private set; }

    [SerializeField] private Sprite highlightArrow;
    [SerializeField] private Sprite selectionArrow;

    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }

    public Sprite GetHighlightArrow()  => highlightArrow;
    public Sprite GetSelectionArrow() => selectionArrow;
}
