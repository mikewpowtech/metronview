using Domain.Enums;
using System;
using System.Text.Json.Serialization;

namespace Presentation.AlarmServer.Models;

public class RtuResponse
{
    [JsonPropertyName("id")]
    public string Id { get; set; }

    [JsonPropertyName("status"), JsonConverter(typeof(JsonStringEnumConverter))]
    public HenkelStatusType Status { get; set; }

    [JsonPropertyName("lastReadingOn")]
    public DateTime LastReadingOn { get; set; }
}
