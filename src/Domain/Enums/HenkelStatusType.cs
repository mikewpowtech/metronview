using System.Text.Json.Serialization;

namespace Domain.Enums
{
    [JsonConverter(typeof(JsonStringEnumConverter))]
    public enum HenkelStatusType
    {
        Ok = 0,
        BreachedUpper = 1,
        BreachedLower = 2, 
        Unknown = 3
    }
}