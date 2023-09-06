using SF.DataGeneration.Models.StudioApiModels.Other;

namespace SF.DataGeneration.Models.StudioApiModels.RequestDto
{
    public class DocumentbotShortkeys : ShortkeysBaseDto
    {
        public IList<ShortKeyConfig> DocumentTypeShortkeys { get; set; } = new List<ShortKeyConfig>();
    }
}
