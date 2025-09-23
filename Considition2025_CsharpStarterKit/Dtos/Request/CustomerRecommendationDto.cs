namespace Considition2025_CsharpStarterKit.Dtos.Request;

public record CustomerRecommendationDto
{
    public Guid? CustomerId { get; set; }
    public List<Guid> ChargingRecommendations { get; set; } = [];
}