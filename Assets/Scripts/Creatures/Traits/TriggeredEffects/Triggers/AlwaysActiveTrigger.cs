using System;

[Serializable]
public class AlwaysActiveTrigger : TraitTrigger
{
    public override bool CheckTrigger(TraitEventData eventData)
    {
        return true;
    }
    public override string GetDescription()
    {
        return "";
    }
}
