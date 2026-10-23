using FortuneTeller.Share.Palm;
using FortuneTeller.Share.Tarot;

namespace FortuneTeller.Share.Readings;

public class ReadingRequestModel
{
    public ReadingType ReadingType { get; set; } = ReadingType.Tarot;
    public ReadingTopic Topic { get; set; } = ReadingTopic.General;
    public string? OptionalQuestion { get; set; }

    // Tarot specific
    public TarotSpreadType SpreadType { get; set; } = TarotSpreadType.ThreeCards;
    public List<int>? SelectedCardIds { get; set; }

    // Palm specific
    public string? PalmImageBase64 { get; set; }
    public string? PalmImageFileName { get; set; }
    public long PalmImageSizeBytes { get; set; }
    public bool UserConsentGiven { get; set; }
}

public class ReadingResponseModel
{
    public string ReadingId { get; set; } = Guid.NewGuid().ToString("N");
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public ReadingType ReadingType { get; set; }
    public ReadingTopic Topic { get; set; }
    public string? OptionalQuestion { get; set; }

    // Branch results
    public List<SelectedCardModel> SelectedCards { get; set; } = new();
    public PalmReadingModel? PalmAnalysis { get; set; }

    // AI Reading Synthesis
    public string OracleName { get; set; } = "Astraea the Celestial Seer";
    public string CelestialGreeting { get; set; } = string.Empty;
    public string CoreProphecy { get; set; } = string.Empty;
    public string DetailedInterpretation { get; set; } = string.Empty;
    public string CosmicAdvice { get; set; } = string.Empty;
    public string AuraColor { get; set; } = "Amethyst Violet";
    public string AuraColorHex { get; set; } = "#a855f7";
    public int AuraEnergyScore { get; set; } = 92; // 1-100
    public int LuckyNumber { get; set; } = 7;
    public string AuspiciousGemstone { get; set; } = "Lapis Lazuli";
    public string AstrologicalTransit { get; set; } = "Jupiter entering the House of Intuition";
    public string SacredAffirmation { get; set; } = string.Empty;
}

public class ReadingHistoryItemModel
{
    public string ReadingId { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
    public ReadingType ReadingType { get; set; }
    public ReadingTopic Topic { get; set; }
    public string? OptionalQuestion { get; set; }
    public string CoreProphecySnippet { get; set; } = string.Empty;
    public string AuraColor { get; set; } = string.Empty;
    public int AuraEnergyScore { get; set; }
    public int LuckyNumber { get; set; }
}
