namespace SF.DataGeneration.Models.StudioApiModels.Other
{
    public class ShortkeysBaseDto
    {
        public IList<ShortKeyConfig> EntityShortkeys { get; set; } = new List<ShortKeyConfig>();
        public IList<ShortKeyConfig> IntentShortkeys { get; set; } = new List<ShortKeyConfig>();
    }
}
