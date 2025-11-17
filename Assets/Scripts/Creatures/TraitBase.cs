using UnityEngine;

[CreateAssetMenu(fileName = "Trait", menuName = "Scriptable Objects/Trait")]
public class TraitBase : ScriptableObject
{
    [SerializeField] string traitName;
    [TextArea]
    [SerializeField] string description;

    public string TraitName => traitName;
    public string Description => description;
}
