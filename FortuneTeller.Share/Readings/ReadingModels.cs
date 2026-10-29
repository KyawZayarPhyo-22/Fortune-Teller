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

    // Structured Domain Insights (အကြံပြုခေါင်းစဉ်များနှင့် လမ်းညွှန်ချက်များ)
    public string PersonalityInsight { get; set; } = string.Empty;         // ကိုယ်ရည်ကိုယ်သွေး၊ အားသာချက်/အားနည်းချက်၊ ဆုံးဖြတ်ချက်ချမှတ်ပုံ
    public string OpportunitiesAndChallenges { get; set; } = string.Empty;   // အခွင့်အလမ်းများနှင့် စိန်ခေါ်မှုများ၊ ဘဝရည်မှန်းချက်
    public string MindfulGuidance { get; set; } = string.Empty;             // သတိထားသင့်သော အခြေအနေများအတွက် အပြုသဘော လမ်းညွှန်ချက်
    public string EthicalDisclaimer { get; set; } = string.Empty;           // ကျင့်ဝတ်သိက္ခာနှင့် ဘေးကင်းရေး သတိပေးချက်

    // Structured 4-Step Tarot Reading Analysis (တာရော့ ၄ ဆင့်ခွဲ အသေးစိတ်ဟောချက်)
    public TarotStepDataModel? TarotSteps { get; set; }
}

public class TarotStep2CardDetail
{
    public string CardNameEn { get; set; } = string.Empty;
    public string CardNameMy { get; set; } = string.Empty;
    public string RomanNumeral { get; set; } = string.Empty;
    public string ImageUrl { get; set; } = string.Empty;
    public string PositionName { get; set; } = string.Empty;
    public bool IsReversed { get; set; }
    public string OrientationDisplay => IsReversed ? "ပြောင်းပြန် (Reversed)" : "အဖြောင့် (Upright)";
    public string PositiveMeaning { get; set; } = string.Empty;   // အပြုသဘော အဓိပ္ပာယ်
    public string CautionaryMeaning { get; set; } = string.Empty; // သတိပြုရန် အဓိပ္ပာယ်
}

public class TarotStep3PositionSynthesis
{
    public int PositionIndex { get; set; }
    public string PositionName { get; set; } = string.Empty;
    public string CardName { get; set; } = string.Empty;
    public string SynthesisText { get; set; } = string.Empty; // မေးခွန်းနှင့် ကတ်နေရာ ပေါင်းစပ်ဟောကိန်း
}

public class TarotStepDataModel
{
    // အဆင့် ၂: ကတ်တစ်ကတ်ချင်းစီရဲ့ အဓိပ္ပာယ်
    public List<TarotStep2CardDetail> CardDetails { get; set; } = new();

    // အဆင့် ၃: မေးခွန်းနဲ့ ကတ်နေရာကို ပေါင်းစပ်ခြင်း
    public string TopicTitle { get; set; } = string.Empty;
    public string QuestionContext { get; set; } = string.Empty;
    public List<TarotStep3PositionSynthesis> PositionSyntheses { get; set; } = new();

    // အဆင့် ၄: နောက်ဆုံးဟောချက် ထုတ်ပေးခြင်း
    public string CurrentSituation { get; set; } = string.Empty;  // လက်ရှိအခြေအနေ
    public string FuturePotential { get; set; } = string.Empty;   // ဖြစ်နိုင်ခြေ
    public string CautionaryAdvice { get; set; } = string.Empty;  // သတိထားရန်
    public string ActionableGuidance { get; set; } = string.Empty;// အကြံပြုချက်
}

public class AiSynthesisResult
{
    public string CoreProphecy { get; set; } = string.Empty;
    public string DetailedInterpretation { get; set; } = string.Empty;
    public string CosmicAdvice { get; set; } = string.Empty;
    public string SacredAffirmation { get; set; } = string.Empty;
    public string PersonalityInsight { get; set; } = string.Empty;
    public string OpportunitiesAndChallenges { get; set; } = string.Empty;
    public string MindfulGuidance { get; set; } = string.Empty;
    public TarotStepDataModel? TarotSteps { get; set; }
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
