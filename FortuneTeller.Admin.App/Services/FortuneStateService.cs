using FortuneTeller.Share.Common;
using FortuneTeller.Share.Readings;
using FortuneTeller.Share.Tarot;

namespace FortuneTeller.Admin.App.Services;

public class FortuneStateService
{
    public ReadingType ReadingType { get; set; } = ReadingType.Tarot;
    public ReadingTopic Topic { get; set; } = ReadingTopic.General;
    public string? OptionalQuestion { get; set; }

    // Tarot flow
    public TarotSpreadType SpreadType { get; set; } = TarotSpreadType.ThreeCards;
    public List<int> SelectedCardIds { get; set; } = new();
    public List<SelectedCardModel> SelectedCards { get; set; } = new();

    // Palm flow
    public string? PalmImageBase64 { get; set; }
    public string? PalmFileName { get; set; }
    public bool UserConsentGiven { get; set; } = false;

    // Output Result
    public ReadingResponseModel? CurrentResult { get; set; }

    public event Action? OnChange;

    public void NotifyStateChanged() => OnChange?.Invoke();

    public void Reset()
    {
        ReadingType = ReadingType.Tarot;
        Topic = ReadingTopic.General;
        OptionalQuestion = null;
        SpreadType = TarotSpreadType.ThreeCards;
        SelectedCardIds.Clear();
        SelectedCards.Clear();
        PalmImageBase64 = null;
        PalmFileName = null;
        UserConsentGiven = false;
        CurrentResult = null;
        NotifyStateChanged();
    }

    public ReadingRequestModel BuildRequest()
    {
        return new ReadingRequestModel
        {
            ReadingType = ReadingType,
            Topic = Topic,
            OptionalQuestion = OptionalQuestion,
            SpreadType = SpreadType,
            SelectedCardIds = SelectedCardIds.Count > 0 ? SelectedCardIds : null,
            PalmImageBase64 = PalmImageBase64,
            PalmImageFileName = PalmFileName,
            PalmImageSizeBytes = PalmImageBase64 != null ? (long)(PalmImageBase64.Length * 0.75) : 0,
            UserConsentGiven = UserConsentGiven
        };
    }
}
