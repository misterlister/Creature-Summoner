using System;

[Serializable]
public class CombatModifier
{
    public CombatModifierType modifierType;
    public float value;
    public object source;

    public CombatModifier(CombatModifierType modifierType, float value, object source = null)
    {
        this.modifierType = modifierType;
        this.value = value;
        this.source = source;
    }
}
