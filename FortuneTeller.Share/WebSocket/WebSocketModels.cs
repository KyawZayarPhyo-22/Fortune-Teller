using FortuneTeller.Share.Readings;

namespace FortuneTeller.Share.WebSocket;

public class WebSocketDivinationMessage
{
    public string MessageId { get; set; } = Guid.NewGuid().ToString("N");
    public string SessionId { get; set; } = string.Empty;
    public string Stage { get; set; } = "Initializing"; // Validating, SelectingCards, AnalyzingPalm, ConsultingOracle, Formatting, Completed
    public int ProgressPercent { get; set; }
    public string MysticalMessage { get; set; } = string.Empty;
    public bool IsComplete { get; set; }
    public bool IsError { get; set; }
    public string? ErrorMessage { get; set; }
    public ReadingResponseModel? Result { get; set; }
    public DateTime Timestamp { get; set; } = DateTime.UtcNow;
}

public class GatewayRateLimitState
{
    public string ClientId { get; set; } = string.Empty;
    public int CurrentRequestCount { get; set; }
    public int MaxAllowedPerMinute { get; set; } = 20;
    public DateTime WindowStartTime { get; set; }
    public bool IsLimitExceeded => CurrentRequestCount > MaxAllowedPerMinute;
}

public class OrchestratorStatsModel
{
    public int TotalReadingsProcessed { get; set; }
    public int TarotReadingsCount { get; set; }
    public int PalmReadingsCount { get; set; }
    public double AverageOrchestrationMs { get; set; }
    public int ActiveSessions { get; set; }
    public int BlockedByRateLimitCount { get; set; }
    public int BlockedBySizeLimitCount { get; set; }
    public DateTime StartedAt { get; set; } = DateTime.UtcNow;
}
