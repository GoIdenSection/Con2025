namespace Considition2025_CsharpStarterKit.Dtos.Response;

public record EdgeDto
{
    public required Guid FromNode { get; init; }
    public required Guid ToNode { get; init; }
    public required float Lenght { get; init; }
    public required List<CustomerDto> Customers { get; init; }
}