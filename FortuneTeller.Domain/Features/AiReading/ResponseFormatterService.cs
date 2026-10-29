using FortuneTeller.Share.Common;
using FortuneTeller.Share.Palm;
using FortuneTeller.Share.Readings;
using FortuneTeller.Share.Tarot;

namespace FortuneTeller.Domain.Features.AiReading;

public class ResponseFormatterService
{
    private static readonly (string Name, string Hex)[] AuraPalette = new[]
    {
        ("ခရမ်းရောင် အိုရာ", "#a855f7"),
        ("ရွှေဝါရောင် အိုရာ", "#f59e0b"),
        ("မြစိမ်းရောင် အိုရာ", "#10b981"),
        ("နီလာပြာရောင် အိုရာ", "#3b82f6"),
        ("ပန်းရောင် အိုရာ", "#f43f5e"),
        ("မိုးပြာရောင် အိုရာ", "#6366f1"),
        ("လရောင်စိမ်း အိုရာ", "#06b6d4")
    };

    private static readonly string[] Gemstones = new[]
    {
        "မြကျောက် (ဉာဏ်ပညာနှင့် သစ္စာတရား)",
        "သီဟိုဠ် နီလာခရမ်း (စိတ်ဝိညာဉ် အကာအကွယ်)",
        "ရွှေဝါကျောက် (ကြွယ်ဝချမ်းသာမှုနှင့် နေမင်းစွမ်းအင်)",
        "ပတ္တမြား (လျင်မြန်သော ကံကြမ္မာအပြောင်းအလဲ)",
        "နှင်းဆီကျောက်သလင်း (မေတ္တာနှင့် သစ္စာတရား)",
        "နက်မှောင်သော ကျောက်မျက် (မကောင်းဆိုးဝါး အကာအကွယ်)",
        "လရောင်ကျောက် (လျှို့ဝှက်ဆန်းကြယ်သော စွမ်းအင်)"
    };

    private static readonly string[] Transits = new[]
    {
        "ကြာသပတေးဂြိုဟ်၏ မင်္ဂလာအလင်းတန်း လွှမ်းခြုံမှု",
        "လဆန်းစ ကြယ်တာရာများ စုဆုံညီညွတ်မှု",
        "သောကြာဂြိုဟ်နှင့် နက်ပကျွန်းဂြိုဟ်တို့၏ ကံမြင့်မားမှု",
        "ဗုဒ္ဓဟူးဂြိုဟ်၏ အောင်မြင်သော ကံကြမ္မာလမ်းကြောင်း",
        "နေမင်း၏ အမြင့်မားဆုံး စွမ်းအင်စက်ဝန်း"
    };

    public ReadingResponseModel FormatAndCheckResponse(
        ReadingRequestModel request,
        List<SelectedCardModel> cards,
        PalmReadingModel? palmAnalysis,
        AiSynthesisResult aiSynthesis)
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
            OracleName = "အက်စ်ထရီယာ ဗေဒင်ပညာရှင်",
            CelestialGreeting = "မင်္ဂလာပါ၊ ကံကြမ္မာခရီးသည်။ သင်၏မေးခွန်းအတွက် စကြဝဠာ၏ လျှို့ဝှက်ချက် ကုလားကာများ ပွင့်လှစ်သွားပါပြီ။",
            CoreProphecy = aiSynthesis.CoreProphecy,
            DetailedInterpretation = aiSynthesis.DetailedInterpretation,
            CosmicAdvice = aiSynthesis.CosmicAdvice,
            SacredAffirmation = aiSynthesis.SacredAffirmation,
            PersonalityInsight = aiSynthesis.PersonalityInsight,
            OpportunitiesAndChallenges = aiSynthesis.OpportunitiesAndChallenges,
            MindfulGuidance = aiSynthesis.MindfulGuidance,
            EthicalDisclaimer = "✦ ကျင့်ဝတ်သိက္ခာနှင့် ဘေးကင်းရေး သတိပေးချက်: အက်စ်ထရီယာ၏ ကံကြမ္မာဟောကြားမှုများသည် ဘဝခရီးလမ်းအတွက် အပြုသဘောဆောင် လမ်းညွှန်ချက်နှင့် စိတ်ခွန်အားပေးရန်သာ ဖြစ်ပါသည်။ ဆေးပညာဆိုင်ရာ ရောဂါအမည်သတ်မှတ်ခြင်း၊ ကံစမ်းမဲ/လောင်းကစားဂဏန်းများ၊ သေချာပေါက်ဖြစ်မည်ဟု အာမခံသော ဘေးအန္တရာယ်နှင့် ခြိမ်းခြောက်စွပ်စွဲချက်များ၊ ကျိန်စာအောက်လမ်း ခြောက်လှန့်မှုများကို လုံးဝရှောင်ကြဉ်ပြီး မိမိ၏ ကံကြမ္မာအစစ်အမှန်သည် မိမိ၏ လက်ရှိကြိုးစားအားထုတ်မှုနှင့် ကောင်းမွန်သော ဆုံးဖြတ်ချက်များပေါ်တွင်သာ တည်မှီပါသည်။",
            AuraColor = aura.Name,
            AuraColorHex = aura.Hex,
            AuraEnergyScore = rng.Next(88, 100),
            LuckyNumber = luckyNum,
            AuspiciousGemstone = Gemstones[rng.Next(Gemstones.Length)],
            AstrologicalTransit = Transits[rng.Next(Transits.Length)],
            SelectedCards = cards,
            PalmAnalysis = palmAnalysis,
            TarotSteps = aiSynthesis.TarotSteps
        };

        // Self-check validation
        if (string.IsNullOrWhiteSpace(response.CoreProphecy))
        {
            response.CoreProphecy = "A powerful divine shift is aligning with your highest path.";
        }

        return response;
    }
}
