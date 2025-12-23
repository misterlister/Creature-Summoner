namespace Game.Traits.Triggers.Results
{
    [System.Serializable]
    public abstract class TraitResult
    {
        public abstract void Execute(BattleEventData eventData);
        public abstract string GetDescription();
    }
}