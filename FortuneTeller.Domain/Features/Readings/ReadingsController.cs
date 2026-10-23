using FortuneTeller.Domain.Features.ReadingOrchestrator;
using FortuneTeller.Share.Common;
using FortuneTeller.Share.Palm;
using FortuneTeller.Share.Readings;
using FortuneTeller.Share.Tarot;
using FortuneTeller.Share.WebSocket;
using Microsoft.AspNetCore.Mvc;

namespace FortuneTeller.Domain.Features.Readings;

[ApiController]
[Route("api/[controller]")]
public class ReadingsController : ControllerBase
{
    private readonly ReadingOrchestratorService _orchestrator;
    private readonly SpreadCardSelector _cardSelector;
    private readonly PalmImageAnalyzer _palmAnalyzer;

    public ReadingsController(
        ReadingOrchestratorService orchestrator,
        SpreadCardSelector cardSelector,
        PalmImageAnalyzer palmAnalyzer)
    {
        _orchestrator = orchestrator;
        _cardSelector = cardSelector;
        _palmAnalyzer = palmAnalyzer;
    }

    [HttpPost("generate")]
    public async Task<ActionResult<ResultModel<ReadingResponseModel>>> GenerateReading(
        [FromBody] ReadingRequestModel request,
        CancellationToken cancellationToken)
    {
        var clientIp = HttpContext.Connection.RemoteIpAddress?.ToString() ?? "default_client";
        var result = await _orchestrator.ProcessReadingAsync(request, clientIp, cancellationToken);
        if (!result.IsSuccess)
        {
            return BadRequest(result);
        }
        return Ok(result);
    }

    [HttpGet("tarot/deck")]
    public ActionResult<ResultModel<IReadOnlyList<TarotCardModel>>> GetTarotDeck()
    {
        var deck = _cardSelector.GetAllCards();
        return Ok(ResultModel<IReadOnlyList<TarotCardModel>>.Success(deck, "Tarot deck retrieved."));
    }

    [HttpGet("tarot/spread/{spreadType}")]
    public ActionResult<ResultModel<TarotSpreadInfoModel>> GetSpreadInfo(TarotSpreadType spreadType)
    {
        var info = _cardSelector.GetSpreadInfo(spreadType);
        return Ok(ResultModel<TarotSpreadInfoModel>.Success(info));
    }

    [HttpPost("palm/analyze")]
    public ActionResult<ResultModel<PalmReadingModel>> AnalyzePalm([FromBody] PalmPhotoUploadRequest request)
    {
        if (string.IsNullOrWhiteSpace(request.Base64Image))
        {
            return BadRequest(ResultModel<PalmReadingModel>.Failure("No image data provided."));
        }
        if (!request.UserConsentGiven)
        {
            return BadRequest(ResultModel<PalmReadingModel>.Failure("User consent is required."));
        }

        var analysis = _palmAnalyzer.AnalyzeImageInMemory(request.Base64Image, ReadingTopic.General, null);
        return Ok(ResultModel<PalmReadingModel>.Success(analysis, "Palm analyzed successfully in memory."));
    }

    [HttpGet("history")]
    public ActionResult<ResultModel<List<ReadingHistoryItemModel>>> GetHistory([FromQuery] int take = 20)
    {
        var history = _orchestrator.GetReadingHistory(take);
        return Ok(ResultModel<List<ReadingHistoryItemModel>>.Success(history));
    }

    [HttpGet("{id}")]
    public ActionResult<ResultModel<ReadingResponseModel>> GetReadingById(string id)
    {
        var reading = _orchestrator.GetReadingById(id);
        if (reading == null)
        {
            return NotFound(ResultModel<ReadingResponseModel>.Failure("Reading not found in celestial archive."));
        }
        return Ok(ResultModel<ReadingResponseModel>.Success(reading));
    }

    [HttpGet("stats")]
    public ActionResult<ResultModel<OrchestratorStatsModel>> GetStats()
    {
        var stats = _orchestrator.GetStats();
        return Ok(ResultModel<OrchestratorStatsModel>.Success(stats));
    }
}
