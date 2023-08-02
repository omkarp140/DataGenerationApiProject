using System.Text.Json.Serialization;

namespace SF.DataGeneration.Models.Enum
{
    [JsonConverter(typeof(JsonStringEnumConverter))]
    public enum StudioEnvironment
    {
        Dev = 1,
        QA = 2,
        Staging = 3,
    }
}
