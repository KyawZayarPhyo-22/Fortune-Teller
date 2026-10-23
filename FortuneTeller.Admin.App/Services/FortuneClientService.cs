using FortuneTeller.Domain.Features.ReadingOrchestrator;
using FortuneTeller.Share.Common;
using FortuneTeller.Share.Readings;
using FortuneTeller.Share.Tarot;
using FortuneTeller.Share.WebSocket;

namespace FortuneTeller.Admin.App.Services;

public class FortuneClientService
{
    private readonly ReadingOrchestratorService _orchestrator;
    private readonly SpreadCardSelector _cardSelector;

    public FortuneClientService(
        ReadingOrchestratorService orchestrator,
        SpreadCardSelector cardSelector)
    {
        _orchestrator = orchestrator;
        _cardSelector = cardSelector;
    }

    public async Task<ResultModel<ReadingResponseModel>> ProcessReadingAsync(
        ReadingRequestModel request,
        string clientId = "admin_app_client")
    {
        return await _orchestrator.ProcessReadingAsync(request, clientId);
    }

    public IAsyncEnumerable<WebSocketDivinationMessage> StreamDivinationAsync(
        ReadingRequestModel request,
        string clientId = "admin_app_client",
        CancellationToken cancellationToken = default)
    {
        return _orchestrator.StreamDivinationAsync(request, clientId, cancellationToken);
    }

    public IReadOnlyList<TarotCardModel> GetAllTarotCards() => _cardSelector.GetAllCards();

    public TarotSpreadInfoModel GetSpreadInfo(TarotSpreadType spreadType) => _cardSelector.GetSpreadInfo(spreadType);

    public List<ReadingHistoryItemModel> GetHistory(int take = 20) => _orchestrator.GetReadingHistory(take);

    public ReadingResponseModel? GetReadingById(string id) => _orchestrator.GetReadingById(id);

    public OrchestratorStatsModel GetStats() => _orchestrator.GetStats();
}
