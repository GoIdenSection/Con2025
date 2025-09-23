using System.Text.Json.Serialization;

namespace Considition2025_CsharpStarterKit.Dtos.Response;

public record CustomerDto
{
    public required Guid Id { get; init; }
    public required string Type { get; init; }
    public required Guid FromNode { get; init; }
    public required Guid ToNode { get; init; }
    public int DepartureTick { get; set; }
    public float ChargeRemaining { get; set; }
    public float MaxCharge { get; set; }

    [JsonConverter(typeof(JsonStringEnumConverter))]
    public CustomerState State { get; set; }
}