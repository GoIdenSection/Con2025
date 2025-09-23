namespace Considition2025_CsharpStarterKit.Dtos.Request;

public record TickDto
{
    public int Tick { get; set; }
    public List<CustomerRecommendationDto> CustomerRecommendations { get; set; } = [];
}