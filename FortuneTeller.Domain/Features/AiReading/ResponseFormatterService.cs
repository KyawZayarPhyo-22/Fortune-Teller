using FortuneTeller.Share.Common;
using FortuneTeller.Share.Palm;
using FortuneTeller.Share.Readings;
using FortuneTeller.Share.Tarot;

namespace FortuneTeller.Domain.Features.AiReading;

public class ResponseFormatterService
{
    private static readonly (string Name, string Hex)[] AuraPalette = new[]
    {
        ("Amethyst Violet", "#a855f7"),
        ("Celestial Gold", "#f59e0b"),
        ("Astral Emerald", "#10b981"),
        ("Cosmic Sapphire", "#3b82f6"),
        ("Rose Quartz", "#f43f5e"),
        ("Starfire Indigo", "#6366f1"),
        ("Moonstone Cyan", "#06b6d4")
    };

    private static readonly string[] Gemstones = new[]
    {
        "Lapis Lazuli (Wisdom & Truth)",
        "Amethyst (Spiritual Protection)",
        "Citrine (Abundance & Sun Energy)",
        "Moldavite (Rapid Transformation)",
        "Rose Quartz (Unconditional Devotion)",
        "Black Tourmaline (Aura Shielding)",
        "Labradorite (Awakening Magic)"
    };

    private static readonly string[] Transits = new[]
    {
        "Jupiter trine the Ascendant in the House of Illumination",
        "The New Moon entering the Celestial Sanctuary",
        "Venus conjunct Neptune in the Realm of Divine Vision",
        "Mercury aligning with the North Node of Destiny",
        "Sun illuminated at the Zenith of Personal Power"
    };

    public ReadingResponseModel FormatAndCheckResponse(
        ReadingRequestModel request,
        List<SelectedCardModel> cards,
        PalmReadingModel? palmAnalysis,
        (string CoreProphecy, string DetailedInterpretation, string CosmicAdvice, string SacredAffirmation) aiSynthesis)
    {
        var rng = new Random();
        var aura = AuraPalette[rng.Next(AuraPalette.Length)];
        var luckyNum = (rng.Next(1, 9) * 3) % 21 + 1; // Sacred 1 to 21

        var response = new ReadingResponseModel
        {
            ReadingId = Guid.NewGuid().ToString("N"),
            CreatedAt = DateTime.UtcNow,
            ReadingType = request.ReadingType,
            Topic = request.Topic,
            OptionalQuestion = request.OptionalQuestion,
            OracleName = DevCode.DefaultAiOracleName,
            CelestialGreeting = "Greetings, beloved traveler between realms. The cosmic veil parts before your inquiry.",
            CoreProphecy = aiSynthesis.CoreProphecy,
            DetailedInterpretation = aiSynthesis.DetailedInterpretation,
            CosmicAdvice = aiSynthesis.CosmicAdvice,
            SacredAffirmation = aiSynthesis.SacredAffirmation,
            AuraColor = aura.Name,
            AuraColorHex = aura.Hex,
            AuraEnergyScore = rng.Next(88, 100),
            LuckyNumber = luckyNum,
            AuspiciousGemstone = Gemstones[rng.Next(Gemstones.Length)],
            AstrologicalTransit = Transits[rng.Next(Transits.Length)],
            SelectedCards = cards,
            PalmAnalysis = palmAnalysis
        };

        // Self-check validation
        if (string.IsNullOrWhiteSpace(response.CoreProphecy))
        {
            response.CoreProphecy = "A powerful divine shift is aligning with your highest path.";
        }

        return response;
    }
}
