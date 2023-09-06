using Newtonsoft.Json;

namespace SF.DataGeneration.Models.StudioApiModels.ResponseDto
{
    public class IntentHelperDto //intent
    {
        public Guid Id { get; set; }
        public string Name { get; set; }
    }

    public class IntentResponseDto
    {
        [JsonProperty("result")]
        public List<IntentResult> Result { get; set; }

    }

    public class IntentResult
    {
        [JsonProperty("intents")]
        public List<IntentHelperDto> Intents { get; set; }
    }
}
