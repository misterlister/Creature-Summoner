using UnityEngine;

[System.Serializable]
public class LearnableTrait
{
    [SerializeField] TraitBase trait;
    [SerializeField] int level;

    public TraitBase Trait => trait;
    public int Level => level;
}
