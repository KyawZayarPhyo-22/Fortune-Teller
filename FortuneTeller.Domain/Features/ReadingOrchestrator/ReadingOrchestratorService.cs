using System.Diagnostics;
using System.Runtime.CompilerServices;
using FortuneTeller.Domain.Features.AiReading;
using FortuneTeller.Domain.Features.WebSocketGateway;
using FortuneTeller.Share.Common;
using FortuneTeller.Share.Palm;
using FortuneTeller.Share.Readings;
using FortuneTeller.Share.Tarot;
using FortuneTeller.Share.WebSocket;

namespace FortuneTeller.Domain.Features.ReadingOrchestrator;

public class ReadingOrchestratorService
{
    private readonly SpreadCardSelector _cardSelector;
    private readonly PalmImageAnalyzer _palmAnalyzer;
    private readonly AiReadingService _aiService;
    private readonly ResponseFormatterService _formatterService;
    private readonly WebSocketGatewayService _gatewayService;

    private static readonly List<ReadingResponseModel> HistoryStore = new();
    private static readonly object HistoryLock = new();
    private static int _totalReadings = 0;
    private static int _tarotCount = 0;
    private static int _palmCount = 0;
    private static double _totalMs = 0;
    private static readonly DateTime ServiceStartTime = DateTime.UtcNow;

    public ReadingOrchestratorService(
        SpreadCardSelector cardSelector,
        PalmImageAnalyzer palmAnalyzer,
        AiReadingService aiService,
        ResponseFormatterService formatterService,
        WebSocketGatewayService gatewayService)
    {
        _cardSelector = cardSelector;
        _palmAnalyzer = palmAnalyzer;
        _aiService = aiService;
        _formatterService = formatterService;
        _gatewayService = gatewayService;
    }

    /// <summary>
    /// Executes the full orchestrated divination pipeline synchronously or as a task.
    /// </summary>
    public async Task<ResultModel<ReadingResponseModel>> ProcessReadingAsync(
        ReadingRequestModel request,
        string clientId = "default_client",
        CancellationToken cancellationToken = default)
    {
        var sw = Stopwatch.StartNew();

        // 1. Gateway Validation
        var validation = _gatewayService.ValidateRequest(request, clientId);
        if (!validation.IsSuccess)
        {
            return ResultModel<ReadingResponseModel>.Failure(validation.Message ?? "Validation failed");
        }

        // Branch execution
        List<SelectedCardModel> selectedCards = new();
        PalmReadingModel? palmAnalysis = null;

        if (request.ReadingType == ReadingType.Tarot)
        {
            // Branch Tarot: Select spread and cards in memory
            selectedCards = _cardSelector.SelectSpreadAndCardsInMemory(
                request.SpreadType,
                request.SelectedCardIds);
            Interlocked.Increment(ref _tarotCount);
        }
        else if (request.ReadingType == ReadingType.Palm)
        {
            // Branch Palm: Analyze image in memory
            palmAnalysis = _palmAnalyzer.AnalyzeImageInMemory(
                request.PalmImageBase64,
                request.Topic,
                request.OptionalQuestion);
            Interlocked.Increment(ref _palmCount);
        }

        // 2. AI Reading Service
        var aiSynthesis = _aiService.GenerateReading(
            request.ReadingType,
            request.Topic,
            request.OptionalQuestion,
            selectedCards,
            palmAnalysis);

        // 3. Format and check response
        var response = _formatterService.FormatAndCheckResponse(
            request,
            selectedCards,
            palmAnalysis,
            aiSynthesis);

        sw.Stop();
        RecordStats(sw.ElapsedMilliseconds, response);

        await Task.CompletedTask;
        return ResultModel<ReadingResponseModel>.Success(response, "Celestial oracle reading completed.");
    }

    /// <summary>
    /// Real-time streaming generator matching the WebSocket diagram flow.
    /// </summary>
    public async IAsyncEnumerable<WebSocketDivinationMessage> StreamDivinationAsync(
        ReadingRequestModel request,
        string clientId = "default_client",
        [EnumeratorCancellation] CancellationToken cancellationToken = default)
    {
        var sessionId = Guid.NewGuid().ToString("N")[..8];

        // Step 1: Gateway validation
        yield return new WebSocketDivinationMessage
        {
            SessionId = sessionId,
            Stage = "Validating",
            ProgressPercent = 15,
            MysticalMessage = "Connecting to WebSocket Gateway... Validating payload and cosmic rate limits."
        };
        await Task.Delay(400, cancellationToken);

        var validation = _gatewayService.ValidateRequest(request, clientId);
        if (!validation.IsSuccess)
        {
            yield return new WebSocketDivinationMessage
            {
                SessionId = sessionId,
                Stage = "Error",
                ProgressPercent = 100,
                IsError = true,
                ErrorMessage = validation.Message,
                MysticalMessage = $"Gateway alert: {validation.Message}"
            };
            yield break;
        }

        yield return new WebSocketDivinationMessage
        {
            SessionId = sessionId,
            Stage = "Validated",
            ProgressPercent = 30,
            MysticalMessage = "Celestial clearance granted. Initiating in-memory orchestrator sequence."
        };
        await Task.Delay(400, cancellationToken);

        // Step 2: In-memory analysis branch
        List<SelectedCardModel> selectedCards = new();
        PalmReadingModel? palmAnalysis = null;

        if (request.ReadingType == ReadingType.Tarot)
        {
            yield return new WebSocketDivinationMessage
            {
                SessionId = sessionId,
                Stage = "SelectingCards",
                ProgressPercent = 50,
                MysticalMessage = "Shuffling sacred astral deck in memory... Drawing archetypal spread."
            };
            await Task.Delay(500, cancellationToken);

            selectedCards = _cardSelector.SelectSpreadAndCardsInMemory(
                request.SpreadType,
                request.SelectedCardIds);
            Interlocked.Increment(ref _tarotCount);
        }
        else
        {
            yield return new WebSocketDivinationMessage
            {
                SessionId = sessionId,
                Stage = "AnalyzingPalm",
                ProgressPercent = 50,
                MysticalMessage = "Processing palm photo in volatile memory... Tracing life, heart, and fate contours."
            };
            await Task.Delay(500, cancellationToken);

            palmAnalysis = _palmAnalyzer.AnalyzeImageInMemory(
                request.PalmImageBase64,
                request.Topic,
                request.OptionalQuestion);
            Interlocked.Increment(ref _palmCount);
        }

        // Step 3: AI Reading Service
        yield return new WebSocketDivinationMessage
        {
            SessionId = sessionId,
            Stage = "ConsultingOracle",
            ProgressPercent = 75,
            MysticalMessage = $"Consulting AI Reading Service for {request.Topic} prophecy and cosmic resonance..."
        };
        await Task.Delay(600, cancellationToken);

        var aiSynthesis = _aiService.GenerateReading(
            request.ReadingType,
            request.Topic,
            request.OptionalQuestion,
            selectedCards,
            palmAnalysis);

        // Step 4: Formatting and Response Check
        yield return new WebSocketDivinationMessage
        {
            SessionId = sessionId,
            Stage = "Formatting",
            ProgressPercent = 90,
            MysticalMessage = "Formatting response, checking celestial integrity, and sealing sacred aura..."
        };
        await Task.Delay(300, cancellationToken);

        var finalResult = _formatterService.FormatAndCheckResponse(
            request,
            selectedCards,
            palmAnalysis,
            aiSynthesis);

        RecordStats(1800, finalResult);

        // Step 5: Completed result
        yield return new WebSocketDivinationMessage
        {
            SessionId = sessionId,
            Stage = "Completed",
            ProgressPercent = 100,
            IsComplete = true,
            MysticalMessage = "The celestial oracle has spoken. Manifesting your prophecy now.",
            Result = finalResult
        };
    }

    public List<ReadingHistoryItemModel> GetReadingHistory(int take = 20)
    {
        lock (HistoryLock)
        {
            return HistoryStore
                .OrderByDescending(r => r.CreatedAt)
                .Take(take)
                .Select(r => new ReadingHistoryItemModel
                {
                    ReadingId = r.ReadingId,
                    CreatedAt = r.CreatedAt,
                    ReadingType = r.ReadingType,
                    Topic = r.Topic,
                    OptionalQuestion = r.OptionalQuestion,
                    CoreProphecySnippet = r.CoreProphecy.Length > 80 ? r.CoreProphecy[..80] + "..." : r.CoreProphecy,
                    AuraColor = r.AuraColor,
                    AuraEnergyScore = r.AuraEnergyScore,
                    LuckyNumber = r.LuckyNumber
                })
                .ToList();
        }
    }

    public ReadingResponseModel? GetReadingById(string id)
    {
        lock (HistoryLock)
        {
            return HistoryStore.FirstOrDefault(r => r.ReadingId == id);
        }
    }

    public OrchestratorStatsModel GetStats()
    {
        var (blockedRate, blockedSize) = _gatewayService.GetBlockedCounters();
        return new OrchestratorStatsModel
        {
            TotalReadingsProcessed = _totalReadings,
            TarotReadingsCount = _tarotCount,
            PalmReadingsCount = _palmCount,
            AverageOrchestrationMs = _totalReadings > 0 ? Math.Round(_totalMs / _totalReadings, 1) : 0,
            ActiveSessions = 1,
            BlockedByRateLimitCount = blockedRate,
            BlockedBySizeLimitCount = blockedSize,
            StartedAt = ServiceStartTime
        };
    }

    private static void RecordStats(double ms, ReadingResponseModel model)
    {
        Interlocked.Increment(ref _totalReadings);
        lock (HistoryLock)
        {
            _totalMs += ms;
            HistoryStore.Insert(0, model);
            if (HistoryStore.Count > 100)
            {
                HistoryStore.RemoveAt(HistoryStore.Count - 1);
            }
        }
    }
}
