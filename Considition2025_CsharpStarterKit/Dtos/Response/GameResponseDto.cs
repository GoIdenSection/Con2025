namespace Considition2025_CsharpStarterKit.Dtos.Response;

public record GameResponseDto
{
    public Guid? GameId { get; set; }
    public required MapDto Map { get; set; }
    public required float Score { get; set; }
    public float KwhRevenue { get; set; }
    public float CustomerCompletionScore { get; set; }
}