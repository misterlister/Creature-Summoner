using UnityEngine;

[System.Serializable]
public class LearnableTrait
{
    [SerializeField] TraitBase actionBase;
    [SerializeField] int level;

    public ActionBase Action => actionBase;
    public int Level => level;
}
