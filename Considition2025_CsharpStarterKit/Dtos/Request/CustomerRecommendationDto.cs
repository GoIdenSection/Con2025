namespace Considition2025_CsharpStarterKit.Dtos.Request;

public record CustomerRecommendationDto
{
    public string? CustomerId { get; set; }
    public List<string> ChargingRecommendations { get; set; } = [];
}