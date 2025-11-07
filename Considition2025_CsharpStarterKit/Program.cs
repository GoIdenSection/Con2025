using System.Diagnostics;
using System.Linq;
using Considition2025_CsharpStarterKit;
using Considition2025_CsharpStarterKit.Dtos.Request;
using Considition2025_CsharpStarterKit.Dtos.Response;

var apiKey = "c3b0d524-fb70-4914-a87f-4ab727004914";
var client = new ConsiditionClient("http://localhost:8080", apiKey);

const string mapName = "Turbohill";
var map = await client.GetMap(mapName);

if (map is null)
{
    Console.WriteLine("Failed to fetch map!");
    return;
}

var finalScore = 0.0f;
var goodTicks = new List<TickDto>();

// Initial input for the first tick
var currentTick = GenerateTick(map, 0);

var input = new GameInputDto
{
    MapName = mapName,
    Ticks = [currentTick]
};

for (var i = 0; i < map.Ticks; i++)
{
    while (true)
    {
        Console.WriteLine($"Playing tick: {i} with input: {input}");
        var gameResponse = await client.PostGame(input);

        if (gameResponse is null)
        {
            Console.WriteLine("Got no game response");
            return;
        }

        finalScore = gameResponse.CustomerCompletionScore + gameResponse.KwhRevenue + gameResponse.Score;

        // Check if we are happy with the response
        if (ShouldMoveOnToNextTick(gameResponse))
        {
            // If we are, we save the current ticks in the list of good ticks
            goodTicks.Add(currentTick);

            // Generate new tick for next iteration
            currentTick = GenerateTick(gameResponse.Map, i + 1);

            // Set new input
            input = new GameInputDto
            {
                MapName = mapName,
                PlayToTick = i + 1,
                Ticks = [..goodTicks, currentTick]
            };
            break;
        }

        // Not happy with the result
        // Try with different input
        currentTick = GenerateTick(gameResponse.Map, i);

        input = new GameInputDto
        {
            MapName = mapName,
            PlayToTick = i,
            Ticks = [..goodTicks, currentTick]
        };
    }
}

Console.WriteLine($"Final score: {finalScore}");

return;

bool ShouldMoveOnToNextTick(GameResponseDto _response)
{
    // Implement logic to decide if the current tick should continue to be iterated on or move on to the next tick
    return true;
}

TickDto GenerateTick(MapDto _map, int _currentTick)
{
    // Implement logic to generate ticks for the optimal score
    return new TickDto
    {
        Tick = _currentTick,
        CustomerRecommendations = GenerateCustomerRecommendations(_map, _currentTick)
    };
}

List<CustomerRecommendationDto> GenerateCustomerRecommendations(MapDto map, int currentTick)
{
    var recs = new List<CustomerRecommendationDto>();

    if (map == null || map.Nodes == null || map.Nodes.Count == 0)
        return recs;

    // Slå upp noder via Id
    var nodesById = map.Nodes.ToDictionary(n => n.Id, n => n);

    // Hitta alla laddstationsnoder
    var stationNodes = map.Nodes
        .Where(n => n.Target is ChargingStationDto)
        .ToList();

    if (stationNodes.Count == 0)
        return recs;

    // Hämta alla kunder vi känner till just nu
    var customers = GetAllCustomers(map);

    foreach (var c in customers)
    {
        // Hoppa över kunder som är klara eller körda
        if (c.State is CustomerState.DestinationReached
                     or CustomerState.RanOutOfJuice
                     or CustomerState.FailedToCharge)
            continue;

        // Har inte lämnat än → vänta
        if (c.DepartureTick > currentTick)
            continue;

        // Skippa redan "okej" laddade kunder (för att inte spamma allt)
        if (c.MaxCharge > 0 && c.ChargeRemaining >= c.MaxCharge * 0.8f)
            continue;

        // Försök hitta nod där kunden står just nu
        NodeDto? currentNode = FindCurrentNodeForCustomer(map, c.Id);

        // Om vi inte hittar, fallback till deras FromNode
        if (currentNode == null)
            nodesById.TryGetValue(c.FromNode, out currentNode);

        if (currentNode == null)
            continue;

        // Välj närmaste laddstation i rak linje från currentNode
        var bestStation = stationNodes
            .OrderBy(s => Dist2(currentNode.PosX, currentNode.PosY, s.PosX, s.PosY))
            .First();

        // Sätt ett enkelt mål: ladda upp till minst 80% eller högre än nu
        var targetCharge = c.MaxCharge > 0
            ? Math.Max(c.ChargeRemaining, c.MaxCharge * 0.8f)
            : c.ChargeRemaining + 10f; // fallback om MaxCharge är 0

        // Bygg rekommendation
        recs.Add(new CustomerRecommendationDto
        {
            CustomerId = c.Id,
            ChargingRecommendations = new List<ChargingRecommendationDto>
            {
                new ChargingRecommendationDto
                {
                    NodeId = bestStation.Id,
                    ChargeTo = targetCharge
                }
            }
        });
    }

    return recs;
}

// Hjälp: hitta en nod som innehåller kunden (om de står på en nod)
NodeDto? FindCurrentNodeForCustomer(MapDto map, string customerId)
{
    foreach (var node in map.Nodes)
    {
        if (node.Customers.Any(c => c.Id == customerId))
            return node;
    }

    // Om vi vill kan vi kolla edges också, men för en enkel baseline räcker detta
    return null;
}

// Hjälp: samla kunder från alla noder och edges (utan dubletter)
List<CustomerDto> GetAllCustomers(MapDto map)
{
    var dict = new Dictionary<string, CustomerDto>();

    foreach (var node in map.Nodes)
    {
        foreach (var c in node.Customers)
            dict[c.Id] = c;
    }

    foreach (var edge in map.Edges)
    {
        foreach (var c in edge.Customers)
            dict[c.Id] = c;
    }

    return dict.Values.ToList();
}

// Hjälp: enkel avstånds-funktion
float Dist2(float ax, float ay, float bx, float by)
    => (ax - bx) * (ax - bx) + (ay - by) * (ay - by);


