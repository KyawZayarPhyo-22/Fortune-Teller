using System.Net.Http.Json;
using System.Text.Json;
using FortuneTeller.Share.Common;
using FortuneTeller.Share.Palm;
using FortuneTeller.Share.Readings;
using FortuneTeller.Share.Tarot;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace FortuneTeller.Domain.Features.AiReading;

public class AiReadingService
{
    private readonly IConfiguration _config;
    private readonly IHttpClientFactory _httpClientFactory;
    private readonly ILogger<AiReadingService> _logger;

    public AiReadingService(
        IConfiguration config,
        IHttpClientFactory httpClientFactory,
        ILogger<AiReadingService> logger)
    {
        _config = config;
        _httpClientFactory = httpClientFactory;
        _logger = logger;
    }

    public AiSynthesisResult GenerateReading(
        ReadingType type,
        ReadingTopic topic,
        string? question,
        List<SelectedCardModel>? cards,
        PalmReadingModel? palmAnalysis)
    {
        return GenerateReadingAsync(type, topic, question, cards, palmAnalysis).GetAwaiter().GetResult();
    }

    public async Task<AiSynthesisResult> GenerateReadingAsync(
        ReadingType type,
        ReadingTopic topic,
        string? question,
        List<SelectedCardModel>? cards,
        PalmReadingModel? palmAnalysis,
        CancellationToken cancellationToken = default)
    {
        var cleanQuestion = question?.Trim();
        var questionContext = !string.IsNullOrWhiteSpace(cleanQuestion)
            ? $"သင်မေးမြန်းထားသော \"{cleanQuestion}\" နှင့်စပ်လျဉ်း၍"
            : "သင်၏ ရှေ့လျှောက်လှမ်းရမည့် ဘဝကံကြမ္မာခရီးနှင့် စပ်လျဉ်း၍";

        // Prohibited and Sensitive Question Guardrail & Guidance Interceptor
        var questionSafetyNote = EvaluateQuestionEthics(cleanQuestion);

        // 1. Generate high-quality deterministic base oracle result (guaranteed fallback)
        AiSynthesisResult result;
        if (type == ReadingType.Tarot && cards != null && cards.Count > 0)
        {
            result = SynthesizeTarotReading(topic, questionContext, cards);
        }
        else if (type == ReadingType.Palm && palmAnalysis != null)
        {
            result = SynthesizePalmReading(topic, questionContext, palmAnalysis);
        }
        else
        {
            result = new AiSynthesisResult
            {
                CoreProphecy = "စကြဝဠာ၏ ကံကြမ္မာစွမ်းအင်များ စုဆုံလျက်ရှိပါပြီ။",
                DetailedInterpretation = "ကောင်းကင်ဘုံ ကြယ်တာရာများက သင်၏ ဘဝခရီးအတွက် မင်္ဂလာရှိသော လမ်းညွှန်ချက်များကို ဖော်ပြနေပါသည်။",
                CosmicAdvice = "စိတ်ကို တည်ငြိမ်စွာထားပြီး အပြုသဘောဆောင်သော ရည်မှန်းချက်များကို စိုက်ထူပါ။",
                SacredAffirmation = "ကျွန်ုပ်သည် စကြဝဠာ၏ အဆုံးမဲ့ ငြိမ်းချမ်းသာယာမှုနှင့် တစ်သားတည်း ဖြစ်တည်နေပါသည်။",
                PersonalityInsight = "သင်သည် မွေးရာပါ ပင်ကိုယ်ထိုးထွင်းဉာဏ် ကောင်းမွန်ပြီး စိတ်ရှည်သည်းခံနိုင်စွမ်း မြင့်မားသူ ဖြစ်သည်။",
                OpportunitiesAndChallenges = "အခွင့်အလမ်းသစ်များ ရောက်ရှိလာချိန်တွင် တွေဝေမနေဘဲ သင့်တော်သော ပြင်ဆင်မှုဖြင့် လက်ခံရယူပါ။",
                MindfulGuidance = "စိတ်ဖိစီးမှုနှင့် စိုးရိမ်ပူပန်မှုများကို စိတ်အေးချမ်းစွာ ဆင်ခြင်သုံးသပ်ခြင်းဖြင့် ပြေလျော့စေပါ။"
            };
        }

        // 2. Attempt Live Google Gemini AI Call if ApiKey is configured
        var geminiKey = _config["Gemini:ApiKey"];
        var geminiModel = _config["Gemini:Model"] ?? "gemini-2.5-flash";

        if (!string.IsNullOrWhiteSpace(geminiKey) && !geminiKey.StartsWith("YOUR_"))
        {
            try
            {
                var liveAiResult = await CallGeminiLiveAsync(geminiKey, geminiModel, type, topic, cleanQuestion, cards, palmAnalysis, cancellationToken);
                if (liveAiResult != null)
                {
                    if (!string.IsNullOrWhiteSpace(liveAiResult.CoreProphecy)) result.CoreProphecy = liveAiResult.CoreProphecy;
                    if (!string.IsNullOrWhiteSpace(liveAiResult.DetailedInterpretation)) result.DetailedInterpretation = liveAiResult.DetailedInterpretation;
                    if (!string.IsNullOrWhiteSpace(liveAiResult.CosmicAdvice)) result.CosmicAdvice = liveAiResult.CosmicAdvice;
                    if (!string.IsNullOrWhiteSpace(liveAiResult.SacredAffirmation)) result.SacredAffirmation = liveAiResult.SacredAffirmation;
                    if (!string.IsNullOrWhiteSpace(liveAiResult.PersonalityInsight)) result.PersonalityInsight = liveAiResult.PersonalityInsight;
                    if (!string.IsNullOrWhiteSpace(liveAiResult.OpportunitiesAndChallenges)) result.OpportunitiesAndChallenges = liveAiResult.OpportunitiesAndChallenges;
                    if (!string.IsNullOrWhiteSpace(liveAiResult.MindfulGuidance)) result.MindfulGuidance = liveAiResult.MindfulGuidance;
                }
            }
            catch (Exception ex)
            {
                _logger.LogWarning("Gemini API call timed out or network error (requires VPN on some networks): {Message}. Seamlessly falling back to built-in oracle engine.", ex.Message);
            }
        }

        // Prepend safety / ethical guidance if question touched sensitive or prohibited boundaries
        if (!string.IsNullOrEmpty(questionSafetyNote))
        {
            result.MindfulGuidance = $"{questionSafetyNote}\n\n{result.MindfulGuidance}";
        }

        return result;
    }

    private async Task<AiSynthesisResult?> CallGeminiLiveAsync(
        string apiKey,
        string modelName,
        ReadingType type,
        ReadingTopic topic,
        string? question,
        List<SelectedCardModel>? cards,
        PalmReadingModel? palmAnalysis,
        CancellationToken cancellationToken)
    {
        var client = _httpClientFactory.CreateClient();
        client.Timeout = TimeSpan.FromMilliseconds(1500);

        var promptBuilder = new System.Text.StringBuilder();
        promptBuilder.AppendLine("You are Astraea (အက်စ်ထရီယာ), a wise, mystical Burmese celestial fortune teller and palmistry/tarot oracle.");
        promptBuilder.AppendLine("Please deliver the fortune prediction strictly in natural, fluent, elegant Burmese (မြန်မာဘာသာ).");
        promptBuilder.AppendLine($"Reading Modality: {type}");
        promptBuilder.AppendLine($"Topic: {topic}");
        if (!string.IsNullOrEmpty(question))
        {
            promptBuilder.AppendLine($"User Question: \"{question}\"");
        }

        if (type == ReadingType.Tarot && cards != null)
        {
            promptBuilder.AppendLine("Selected Tarot Cards: " + string.Join(", ", cards.Select(c => $"{c.PositionName}: {c.Card.Name}{(c.IsReversed ? " (Reversed)" : "")}")));
        }
        else if (type == ReadingType.Palm && palmAnalysis != null)
        {
            promptBuilder.AppendLine($"Hand Archetype: {palmAnalysis.HandArchetype}");
            promptBuilder.AppendLine($"Elemental Description: {palmAnalysis.ElementalDescription}");
            promptBuilder.AppendLine($"Heart Line: {palmAnalysis.HeartLineInterpretation}");
            promptBuilder.AppendLine($"Head Line: {palmAnalysis.HeadLineInterpretation}");
            promptBuilder.AppendLine($"Life Line: {palmAnalysis.LifeLineInterpretation}");
            promptBuilder.AppendLine($"Fate Line: {palmAnalysis.FateLineInterpretation}");
        }

        promptBuilder.AppendLine(@"
Respond strictly in JSON format matching this schema:
{
  ""coreProphecy"": ""Short powerful prophecy sentence in Burmese"",
  ""detailedInterpretation"": ""In-depth rich interpretation story in Burmese (do not list lines as bullet items)"",
  ""cosmicAdvice"": ""Constructive spiritual advice in Burmese"",
  ""sacredAffirmation"": ""Sacred affirmation mantra in Burmese"",
  ""personalityInsight"": ""Personality and strengths insight in Burmese"",
  ""opportunitiesAndChallenges"": ""Upcoming opportunities and challenges in Burmese"",
  ""mindfulGuidance"": ""Compassionate mindful guidance in Burmese""
}
Remember ethical rules: do not diagnose illnesses, do not promise lottery numbers, do not predict fatal accident dates, and do not scaremonger with dark curses.");

        var requestBody = new
        {
            contents = new[]
            {
                new
                {
                    parts = new[]
                    {
                        new { text = promptBuilder.ToString() }
                    }
                }
            },
            generationConfig = new
            {
                temperature = 0.7,
                response_mime_type = "application/json"
            }
        };

        var url = $"https://generativelanguage.googleapis.com/v1beta/models/{modelName}:generateContent?key={apiKey}";
        using var response = await client.PostAsJsonAsync(url, requestBody, cancellationToken);
        if (!response.IsSuccessStatusCode)
        {
            return null;
        }

        var jsonStr = await response.Content.ReadAsStringAsync(cancellationToken);
        using var doc = JsonDocument.Parse(jsonStr);
        var root = doc.RootElement;
        if (root.TryGetProperty("candidates", out var candidates) && candidates.GetArrayLength() > 0)
        {
            var text = candidates[0].GetProperty("content").GetProperty("parts")[0].GetProperty("text").GetString();
            if (!string.IsNullOrWhiteSpace(text))
            {
                return JsonSerializer.Deserialize<AiSynthesisResult>(text, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
            }
        }

        return null;
    }

    private static string? EvaluateQuestionEthics(string? question)
    {
        if (string.IsNullOrWhiteSpace(question)) return null;
        var q = question.ToLowerInvariant();

        if (q.Contains("ရောဂါ") || q.Contains("ကင်ဆာ") || q.Contains("ဆေးရုံ") || q.Contains("ကျန်းမာရေးမကောင်း") || q.Contains("doctor") || q.Contains("disease") || q.Contains("cancer") || q.Contains("illness"))
        {
            return "✦ [ကျန်းမာရေးဆိုင်ရာ ကျင့်ဝတ်သတိပေးချက်]: လက်ဝါးလက္ခဏာနှင့် တာရော့ဗေဒင်သည် ဆေးပညာဆိုင်ရာ ရောဂါရှာဖွေအတည်ပြုပေးခြင်း မပြုလုပ်ပါ။ ကျန်းမာရေးနှင့် ပတ်သက်၍ ကျွမ်းကျင်သော ဆရာဝန်၊ ဆေးပညာရှင်များနှင့်သာ တိုင်ပင်ကုသသင့်ပြီး ဤဟောကိန်းသည် စိတ်ဓာတ်ခွန်အားနှင့် နေထိုင်မှု ဟန်ချက်ညီစေရေးအတွက်သာ အကြံပြုပါသည်။";
        }

        if (q.Contains("ထီ") || q.Contains("ထီပေါက်") || q.Contains("လောင်းကစား") || q.Contains("ဂဏန်း") || q.Contains("2d") || q.Contains("3d") || q.Contains("lottery") || q.Contains("gamble"))
        {
            return "✦ [ငွေကြေးနှင့် ကံစမ်းမဲ ကျင့်ဝတ်သတိပေးချက်]: ဗေဒင်ပညာသည် ထီပေါက်ဂဏန်း သို့မဟုတ် လောင်းကစားအနိုင်ရလဒ်များကို အာမခံဟောကြားခြင်း မပြုပါ။ စစ်မှန်သော ဓနဥစ္စာသည် သင်၏ ကြိုးစားအားထုတ်မှု၊ စနစ်တကျ ငွေကြေးစီမံခန့်ခွဲမှုနှင့် အသုံးစရိတ် စိစစ်ခြင်းတို့ဖြင့်သာ ရေရှည်တည်တံ့ခိုင်မြဲမည် ဖြစ်ပါသည်။";
        }

        if (q.Contains("သစ္စာဖောက်") || q.Contains("လင်ငယ်") || q.Contains("မယားငယ်") || q.Contains("စွပ်စွဲ") || q.Contains("cheat") || q.Contains("betray") || q.Contains("affair"))
        {
            return "✦ [ဆက်ဆံရေး သတိပြုဖွယ် လမ်းညွှန်ချက်]: လူတစ်ဦးတစ်ယောက်၏ သစ္စာရှိမှု သို့မဟုတ် အပြစ်ကို အထောက်အထားမရှိဘဲ စွပ်စွဲအတည်ပြုခြင်း မပြုသင့်ပါ။ မိမိဘက်မှ စိတ်ကြည်လင်မှုရှိစေရန်၊ မိမိ၏ စိတ်ခံစားမှုနယ်နိမိတ်ကို ခိုင်မာစွာ ထိန်းသိမ်းရန်နှင့် ပွင့်လင်းရိုးသားစွာ ဆွေးနွေးညှိနှိုင်းရန် အကြံပြုပါသည်။";
        }

        if (q.Contains("ဘယ်တော့သေ") || q.Contains("ကားတိုက်") || q.Contains("ကွာရှင်းမည့်ရက်") || q.Contains("မင်္ဂလာရက်အတိအကျ") || q.Contains("accident") || q.Contains("death"))
        {
            return "✦ [ဘဝဘေးကင်းရေးနှင့် ကံကြမ္မာသဘောထား]: မတော်တဆမှု၊ သေဆုံးမည့်ရက် သို့မဟုတ် ကွာရှင်းမည့်ရက်ကို အတိအကျ သေချာပေါက်ဖြစ်မည်ဟု သတ်မှတ်ဟောကြားခြင်း မရှိပါ။ လူသားတို့၏ ကံကြမ္မာသည် မိမိ၏ သတိတရား၊ ဆင်ခြင်တုံတရားနှင့် နေ့စဉ် မှန်ကန်သော ဆုံးဖြတ်ချက်များဖြင့် ကောင်းမွန်စွာ ပြောင်းလဲဖန်တီးနိုင်ပါသည်။";
        }

        if (q.Contains("ကျိန်စာ") || q.Contains("အောက်လမ်း") || q.Contains("နတ်ဆိုး") || q.Contains("မကောင်းဆိုးဝါး") || q.Contains("ယတြာ") || q.Contains("curse") || q.Contains("hex") || q.Contains("black magic"))
        {
            return "✦ [စိတ်ခွန်အားဆိုင်ရာ လမ်းညွှန်ချက်]: အက်စ်ထရီယာ ဗေဒင်စခန်းသည် ကျိန်စာ၊ အောက်လမ်းနှင့် မကောင်းသော စွမ်းအားများဖြင့် ခြောက်လှန့်ခြင်းကို လုံးဝ လက်မခံပါ။ သင့်အတွင်းစိတ်ရှိ သီလ၊ မေတ္တာနှင့် အကောင်းမြင်စိတ်တို့သည် အဆိုးလွှမ်းမိုးမှုမှန်သမျှကို ကျော်လွှားနိုင်သော အကြီးမားဆုံး အကာအကွယ် စွမ်းအားများ ဖြစ်ပါသည်။";
        }

        return null;
    }

    private static AiSynthesisResult SynthesizeTarotReading(
        ReadingTopic topic,
        string questionContext,
        List<SelectedCardModel> cards)
    {
        var primaryCard = cards[0];
        var cardSummary = string.Join(" ➔ ", cards.Select(c =>
            $"[{c.PositionName}: {c.Card.Name}{(c.IsReversed ? " (ပြောင်းပြန်)" : "")}]"));

        var result = new AiSynthesisResult();

        switch (topic)
        {
            case ReadingTopic.Love:
                result.CoreProphecy = $"ကောင်းကင်ဘုံ ကံကြမ္မာသည် သင်၏ နှလုံးသားဆန္ဒကို {primaryCard.Card.Name} ၏ စွမ်းအင်ဖြင့် အသစ်တစ်ဖန် ပြန်လည်နိုးထစေပါပြီ။";
                result.DetailedInterpretation = $"{questionContext}၊ တာရော့ကတ်များက မေတ္တာရေးရာ အလှည့်အပြောင်းကို ဖော်ပြနေပါသည် - {cardSummary}။ " +
                                                $"{primaryCard.Card.Name} ကျရောက်နေခြင်း ({primaryCard.RevealedMeaning}) က အတိတ်မှ စိတ်ဒဏ်ရာဟောင်းများ ပျောက်ကွယ်ပြီး ပိုမိုနက်ရှိုင်းသော မေတ္တာသစ္စာနှင့် နားလည်မှုဆီသို့ ကူးပြောင်းနေပြီဖြစ်ကြောင်း ညွှန်ပြနေပါသည်။ " +
                                                (cards.Count > 1
                                                    ? $"ဤအခြေအနေသည် {cards[1].Card.Name} ၏ စစ်မှန်သော ရိုးသားမှုကို လိုအပ်ပြီး {cards.Last().Card.Name} ၏ ရေရှည်တည်တံ့သော မေတ္တာရလဒ်ဖြင့် အဆုံးသတ်ပါမည်။"
                                                    : "ပွင့်လင်းရိုးသားသော နှလုံးသားဖြင့် ယုံကြည်စွာ ရှေ့သို့ လှမ်းချီပါ။");
                result.CosmicAdvice = "ချစ်ခြင်းမေတ္တာ၏ အချိန်အခါကို အတင်းအကျပ် မထိန်းချုပ်ပါနှင့်။ သင်လိုလားသော မေတ္တာစွမ်းအင်ကို မိမိကိုယ်တိုင်မှ စတင်ဖြာထွက်စေပါ။";
                result.SacredAffirmation = "ကျွန်ုပ်သည် စစ်မှန်သော မေတ္တာတရား၏ စံအိမ်ဖြစ်ပြီး ချစ်ခြင်းမေတ္တာကို အပြည့်အဝ ပေးဆပ်ရယူနိုင်သောသူ ဖြစ်သည်။";
                result.PersonalityInsight = "သင်သည် မေတ္တာရေးတွင် သစ္စာတရားကို အလေးထားပြီး စိတ်ခံစားချက်ကို နူးညံ့သိမ်မွေ့စွာ ဖော်ပြတတ်သူ ဖြစ်သည်။ အားသာချက်မှာ စာနာနားလည်မှု ကြီးမားခြင်းဖြစ်ပြီး အားနည်းချက်မှာ တစ်ဖက်သားအပေါ် မျှော်လင့်ချက် မြင့်မားတတ်ခြင်း ဖြစ်သည်။";
                result.OpportunitiesAndChallenges = "လက်ရှိအချစ်ရေး သို့မဟုတ် အနာဂတ်လက်တွဲဖော်နှင့် ဆက်ဆံရေးတွင် ပွင့်လင်းစွာ ရင်ဖွင့်နိုင်မည့် အခွင့်အလမ်း ပေါ်ပေါက်လာပါမည်။ နားလည်မှုလွဲမှားခြင်း စိန်ခေါ်မှုကို စိတ်ရှည်စွာ ဖြေရှင်းပါ။";
                result.MindfulGuidance = "အချစ်ရေး အဆင်မပြေမှုများနှင့် သဘောထားကွဲလွဲမှုများ ကြုံလာပါက ဒေါသဖြင့် မတုံ့ပြန်ဘဲ အပြန်အလှန် လေးစားမှုဖြင့် ညှိနှိုင်းပါ။ အတိတ်မှ နာကျင်မှုများကို သင်ခန်းစာယူ၍ လက်ရှိဆက်ဆံရေးကို ခိုင်မာအောင် တည်ဆောက်ပါ။";
                break;

            case ReadingTopic.Education:
                result.CoreProphecy = $"ပညာဗဟုသုတ၏ ရွှေရောင်တံခါးပေါက်သည် {primaryCard.Card.Name} ၏ လမ်းညွှန်မှုဖြင့် ပွင့်လှစ်သွားပါပြီ။";
                result.DetailedInterpretation = $"{questionContext}၊ ဉာဏ်ပညာ လမ်းကြောင်းကို ကြယ်တာရာများက လင်းလက်စွာ ပြသနေပါသည် - {cardSummary}။ " +
                                                $"{primaryCard.Card.Name} ကျရောက်နေခြင်း ({primaryCard.RevealedMeaning}) က ခက်ခဲနက်နဲသော စာပေပညာရပ်များကို ထိုးထွင်းသိမြင်နိုင်စွမ်း ရရှိမည်ဖြစ်ကြောင်း ညွှန်ပြနေပါသည်။ " +
                                                (cards.Count > 1
                                                    ? $"{cards[1].Card.Name} ပြသနေသော ယာယီသံသယများကို ဇွဲဖြင့် ကျော်လွှားပြီးနောက် {cards.Last().Card.Name} ၏ ထူးချွန်ထက်မြက်သော အောင်မြင်မှုကို ရရှိပါမည်။"
                                                    : "သင်၏ ဉာဏ်ရည်သည် စိန်ရတနာကဲ့သို့ ထက်မြက်နေပါသည်။");
                result.CosmicAdvice = "ဇွဲလုံ့လ စည်းကမ်းရှိမှုနှင့် အနားယူမှုကို ဟန်ချက်ညီညီ ပေါင်းစပ်ပါ။ စစ်မှန်သော ထူးချွန်မှုသည် ကြိုးစားအားထုတ်မှုနှင့် စိတ်တည်ငြိမ်မှုတို့ ပေါင်းစပ်ခြင်းမှ ရရှိပါသည်။";
                result.SacredAffirmation = "ဉာဏ်ပညာ စွမ်းအားများသည် ကျွန်ုပ်၏ အတွေးအခေါ်ထဲသို့ အစဉ်အမြဲ စီးဆင်းနေပြီး သင်ယူမှုတိုင်းသည် ထူးချွန်ပြောင်မြောက်မှုသို့ ဦးတည်နေသည်။";
                result.PersonalityInsight = "သင်၏ သင်ယူမှုပုံစံသည် အခြေခံသဘောတရားကို အသေးစိတ် ခွဲခြမ်းစိတ်ဖြာပြီး လက်တွေ့အသုံးချနိုင်မှုကို ဦးစားပေးသည်။ အားသာချက်မှာ အာရုံစူးစိုက်နိုင်မှု ကောင်းမွန်ခြင်းဖြစ်ပြီး ပညာရပ်အသစ်များကို လျင်မြန်စွာ ဆုပ်ကိုင်နိုင်စွမ်း ရှိသည်။";
                result.OpportunitiesAndChallenges = "စာမေးပွဲ၊ ပညာသင်ဆု သို့မဟုတ် သင်တန်းအသစ်များတွင် ထူးချွန်စွာ အောင်မြင်နိုင်မည့် အခွင့်အလမ်းများ ရှိသည်။ စာမေးပွဲနီးကပ်ချိန် စိတ်ဖိစီးမှုနှင့် ပင်ပန်းနွမ်းနယ်မှု စိန်ခေါ်မှုကို စနစ်တကျ အချိန်ဇယားဆွဲ၍ ကျော်လွှားပါ။";
                result.MindfulGuidance = "စာပေလေ့လာရာတွင် မကြာခဏ ပင်ပန်းအားလျော့ခြင်း ဖြစ်ပေါ်တတ်သဖြင့် အိပ်ရေးဝဝအိပ်ခြင်းနှင့် အာဟာရပြည့်ဝစွာ စားသုံးခြင်းဖြင့် စိတ်ရောကိုယ်ပါ ကျန်းမာသန်စွမ်းအောင် ထိန်းသိမ်းပါ။";
                break;

            case ReadingTopic.CareerBusiness:
                result.CoreProphecy = $"လုပ်ငန်းခွင်နှင့် စီးပွားရေး တိုးတက်မှု စွမ်းအင်များသည် {primaryCard.Card.Name} ၏ အောင်မြင်သော အရှိန်အဟုန်အောက်တွင် ပိုမိုအားကောင်းလာပါပြီ။";
                result.DetailedInterpretation = $"{questionContext}၊ Career Path နှင့် စီးပွားရေးလမ်းကြောင်းကို ကတ်ပြားများက ပြသနေပါသည် - {cardSummary}။ " +
                                                $"{primaryCard.Card.Name} ({primaryCard.RevealedMeaning}) ၏ လမ်းညွှန်ချက်အရ မိမိ၏ လုပ်ငန်းခွင် စွမ်းဆောင်ရည်ကို အထက်လူကြီးနှင့် လုပ်ဖော်ကိုင်ဖက်များက အသိအမှတ်ပြုလာမည့် အချိန်အခါ ရောက်ရှိနေပြီ ဖြစ်ပါသည်။ " +
                                                (cards.Count > 1
                                                    ? $"{cards[1].Card.Name} ၏ စိန်ခေါ်မှုများကို ရဲဝံ့စွာ ဖြတ်ကျော်ပြီး {cards.Last().Card.Name} ၏ ခိုင်မာသော အောင်မြင်မှုပန်းတိုင်ကို အရောက်လှမ်းနိုင်ပါမည်။"
                                                    : "ကိုယ်ပိုင်လုပ်ငန်း သို့မဟုတ် အလုပ်အကိုင်တွင် မဟာဗျူဟာမြောက် ပြင်ဆင်မှုဖြင့် ရှေ့သို့ ချီတက်ပါ။");
                result.CosmicAdvice = "အခွင့်အလမ်းသည် ပြင်ဆင်ထားသူထံသို့သာ ရောက်ရှိတတ်ပါသည်။ မိမိ၏ ကျွမ်းကျင်မှုများကို စဉ်ဆက်မပြတ် မြှင့်တင်ပြီး မိတ်ဖက်ကောင်းများနှင့် ပူးပေါင်းဆောင်ရွက်ပါ။";
                result.SacredAffirmation = "ကျွန်ုပ်သည် အလုပ်အကိုင်နှင့် စီးပွားရေးတွင် တောက်ပသော အောင်မြင်မှုကို ဖန်တီးနိုင်ပြီး အတားအဆီးတိုင်းကို အခွင့်အလမ်းအဖြစ် ပြောင်းလဲနိုင်သည်။";
                result.PersonalityInsight = "သင်သည် တာဝန်ယူလိုစိတ် အပြည့်ရှိပြီး ပြဿနာများကို လက်တွေ့ကျကျ ဖြေရှင်းနိုင်သော ခေါင်းဆောင်မှု စိတ်ဓာတ်ရှိသည်။ ဆုံးဖြတ်ချက်ချမှတ်ရာတွင် အချက်အလက်နှင့် အတွေ့အကြုံကို ပေါင်းစပ်စဉ်းစားတတ်သူ ဖြစ်သည်။";
                result.OpportunitiesAndChallenges = "ရာထူးတိုးခြင်း၊ လုပ်ငန်းသစ် စတင်ခြင်း သို့မဟုတ် အကျိုးတူပူးပေါင်းဆောင်ရွက်ခွင့် အခွင့်အလမ်းကောင်းများ ပေါ်ပေါက်လာမည်။ အပြိုင်အဆိုင်များပြားမှုနှင့် အလုပ်တာဝန် ဖိစီးမှု စိန်ခေါ်မှုကို ဦးစားပေးအလိုက် ခွဲခြမ်းလုပ်ဆောင်ပါ။";
                result.MindfulGuidance = "အလုပ်အကိုင် ပြောင်းလဲမှု သို့မဟုတ် လုပ်ငန်းခွင် အပြောင်းအလဲများ ကြုံတွေ့ရပါက ထိတ်လန့်တုန်လှုပ်ခြင်းမရှိဘဲ မိမိ၏ ရေရှည်ဘဝရည်မှန်းချက်နှင့် ကိုက်ညီမှုရှိမရှိ တည်ငြိမ်စွာ ချင့်ချိန်သုံးသပ်ပါ။";
                break;

            case ReadingTopic.Finance:
                result.CoreProphecy = $"ဓနဥစ္စာ စီမံခန့်ခွဲမှုနှင့် ငွေကြေးစီးဆင်းမှုသည် {primaryCard.Card.Name} ၏ အလင်းရောင်ဖြင့် ပိုမိုတည်ငြိမ်တိုးတက်လာပါမည်။";
                result.DetailedInterpretation = $"{questionContext}၊ ငွေကြေးဆိုင်ရာ အခြေအနေကို ကတ်များက ထင်ဟပ်ပြနေပါသည် - {cardSummary}။ " +
                                                $"{primaryCard.Card.Name} ({primaryCard.RevealedMeaning}) ကျရောက်နေခြင်းက စနစ်တကျ ငွေကြေးစီမံမှု၊ အသုံးစရိတ် ချွေတာစုဆောင်းမှုတို့သည် ရေရှည်ချမ်းသာကြွယ်ဝမှု၏ အဓိကသော့ချက်ဖြစ်ကြောင်း ဖော်ပြနေပါသည်။ " +
                                                (cards.Count > 1
                                                    ? $"{cards[1].Card.Name} ၏ ငွေကြေးစွန့်စားရမှုများကို သတိဖြင့် ထိန်းချုပ်ကာ {cards.Last().Card.Name} ၏ ခိုင်မာသော ဓနဥစ္စာ အခြေခံကို တည်ဆောက်ပါ။"
                                                    : "ငွေကြေးစီမံခန့်ခွဲမှုကို ယခုအချိန်မှစ၍ စနစ်တကျ ပြန်လည်စိစစ်ပါ။");
                result.CosmicAdvice = "မဆင်မခြင် သုံးစွဲခြင်းနှင့် လွယ်လွယ်ကူကူ ချမ်းသာလိုသော လောင်းကစား/ကံစမ်းမှုများကို ရှောင်ကြဉ်ပါ။ စဉ်ဆက်မပြတ် စုဆောင်းငွေသည် သင်၏ အနာဂတ်လုံခြုံရေး ဖြစ်သည်။";
                result.SacredAffirmation = "ကျွန်ုပ်၏ ငွေကြေးစီမံခန့်ခွဲမှုသည် စည်းကမ်းရှိပြီး တရားမျှတသော ကြိုးစားမှုဖြင့် ဓနဥစ္စာများ အစဉ်တစိုက် တိုးပွားနေပါသည်။";
                result.PersonalityInsight = "သင်သည် ငွေကြေးတန်ဖိုးကို နားလည်ပြီး မိမိ၏ လိုအပ်ချက်နှင့် လိုချင်တပ်မက်မှုကို ခွဲခြားသိမြင်တတ်သူ ဖြစ်သည်။ အားသာချက်မှာ စိတ်ရှည်စွာ ချွေတာနိုင်ခြင်းဖြစ်ပြီး အားနည်းချက်မှာ ရံဖန်ရံခါ အရေးမကြီးသော နေရာများတွင် မမျှော်လင့်ဘဲ အသုံးစရိတ် ပိုမိုသွားတတ်ခြင်း ဖြစ်သည်။";
                result.OpportunitiesAndChallenges = "အပိုဝင်ငွေရရှိမည့် နည်းလမ်းများ၊ ရင်းနှီးမြှုပ်နှံမှု အခွင့်အလမ်းသစ်များ ရောက်ရှိလာပါမည်။ မလိုလားအပ်သော အကြွေးပြဿနာများနှင့် မမျှော်လင့်သော ကုန်ကျစရိတ်များကို ကြိုတင်ကာကွယ်ရန် အရေးပေါ်ရန်ပုံငွေ ဖယ်ထားပါ။";
                result.MindfulGuidance = "ငွေကြေးဆုံးရှုံးမှု သို့မဟုတ် အကြွေးဆိုင်ရာ ဖိစီးမှုများ ရှိနေပါက စိတ်ဓာတ်မကျဘဲ ဝင်ငွေထွက်ငွေ ဇယားကို နေ့စဉ်မှတ်တမ်းတင်၍ အဆင့်လိုက် ပြန်လည်ဖြေရှင်းပါ။ အန္တရာယ်များသော ငွေကြေးလိမ်လည်မှုများကို သတိပြုပါ။";
                break;

            case ReadingTopic.Relationship:
                result.CoreProphecy = $"မိတ်ဆွေအပေါင်းအသင်းနှင့် မိသားစု လူမှုဆက်ဆံရေးသည် {primaryCard.Card.Name} ၏ စွမ်းအင်အောက်တွင် သဟဇာတဖြစ်စွာ ခိုင်မြဲလာပါမည်။";
                result.DetailedInterpretation = $"{questionContext}၊ လူမှုဆက်ဆံရေး မှန်ဘီလူးသည် အောက်ပါအတိုင်း ရှင်းလင်းစွာ ထင်ဟပ်ပြနေပါသည် - {cardSummary}။ " +
                                                $"{primaryCard.Card.Name} ({primaryCard.RevealedMeaning}) ၏ လမ်းညွှန်မှုက မိတ်ကောင်းဆွေကောင်းများနှင့် မိသားစုနွေးထွေးမှုသည် ဘဝတိုးတက်ရေးအတွက် မရှိမဖြစ် အဖိုးတန်ရတနာများဖြစ်ကြောင်း သတိပေးနေပါသည်။ " +
                                                (cards.Count > 1
                                                    ? $"{cards[1].Card.Name} ပြသနေသော သဘောထားကွဲလွဲမှုများကို ပွင့်လင်းစွာ ဖြေရှင်းခြင်းဖြင့် {cards.Last().Card.Name} ၏ အပြန်အလှန် ယုံကြည်ခိုင်မြဲမှုကို ရရှိပါမည်။"
                                                    : "မိမိ၏ ရပ်တည်ချက်နှင့် အခြားသူအပေါ် စာနာနားလည်မှုကို ဟန်ချက်ညီစေပါ။");
                result.CosmicAdvice = "ဒေါသမာန်မပါဘဲ ပွင့်လင်းရိုးသားစွာ ပြောဆိုဆက်ဆံပါ။ နားထောင်ပေးနိုင်သော နှလုံးသားသည် မိတ်ကောင်းဆွေကောင်းကို ဖိတ်ခေါ်နိုင်ပါသည်။";
                result.SacredAffirmation = "ကျွန်ုပ်သည် သစ္စာတရား၊ အပြန်အလှန် လေးစားမှုနှင့် မေတ္တာတရားတို့ အခြေခံသော ဆက်ဆံရေးကောင်းများကို တည်ဆောက်နိုင်သူ ဖြစ်သည်။";
                result.PersonalityInsight = "သင်သည် မိတ်ဆွေများနှင့် မိသားစုအပေါ် စေတနာထက်သန်ပြီး အကူအညီလိုအပ်ချိန်တွင် ဖေးမကူညီတတ်သူ ဖြစ်သည်။ ဆုံးဖြတ်ချက်ချရာတွင် ပတ်ဝန်းကျင်၏ ခံစားချက်ကို ထည့်သွင်းစဉ်းစားတတ်သည်။";
                result.OpportunitiesAndChallenges = "မိတ်ဆွေအသစ်များနှင့် တွေ့ဆုံခြင်း၊ လုပ်ငန်းခွင် ကွန်ရက်ကျယ်ပြန့်လာခြင်း စသည့် အခွင့်အလမ်းများ ရှိသည်။ မိသားစုအတွင်း သဘောထားမတိုက်ဆိုင်မှု စိန်ခေါ်မှုကို စိတ်ရှည်စွာ ဖြေရှင်းပါ။";
                result.MindfulGuidance = "ဆက်ဆံရေးတွင် တတိယလူ၏ ကြားဝင်စကားများ သို့မဟုတ် သံသယများကြောင့် စိတ်ဝမ်းကွဲခြင်း မဖြစ်စေရန် ကာယကံရှင်နှင့် တိုက်ရိုက် မေးမြန်းညှိနှိုင်းပါ။ မိမိ၏ စိတ်ပိုင်းဆိုင်ရာ နယ်နိမိတ်ကို ခိုင်မာစွာ ထိန်းသိမ်းပါ။";
                break;

            case ReadingTopic.PersonalGrowth:
                result.CoreProphecy = $"မိမိကိုယ်ကို ပြန်လည်ရှာဖွေတွေ့ရှိမှုနှင့် ဘဝရည်မှန်းချက် ပန်းတိုင်ဆီသို့ {primaryCard.Card.Name} ၏ စွမ်းအားဖြင့် ယုံကြည်ချက်အပြည့် လျှောက်လှမ်းနိုင်ပါပြီ။";
                result.DetailedInterpretation = $"{questionContext}၊ အတွင်းစိတ်ခွန်အားနှင့် ဘဝရည်မှန်းချက်များကို ကတ်ပြားများက ဖွင့်လှစ်ပြသနေပါသည် - {cardSummary}။ " +
                                                $"{primaryCard.Card.Name} ({primaryCard.RevealedMeaning}) က အတိတ်က အမှားများ သို့မဟုတ် မသေချာမှုများသည် သင့်ကို ပိုမိုရင့်ကျက်သန်မာစေသော ဘဝသင်ခန်းစာများဖြစ်ကြောင်း အသိပေးနေပါသည်။ " +
                                                (cards.Count > 1
                                                    ? $"{cards[1].Card.Name} ပြသနေသော အတွင်းစိတ်မသေချာမှုများကို ကျော်လွှားပြီး {cards.Last().Card.Name} ၏ အမြင့်မားဆုံး အောင်မြင်မှုကို ရယူပါ။"
                                                    : "မိမိကိုယ်ကို ယုံကြည်ပြီး တစ်နေ့ထက်တစ်နေ့ ပိုမိုတိုးတက်အောင် ကြိုးပမ်းပါ။");
                result.CosmicAdvice = "ကိုယ့်ကိုယ်ကို အခြားသူများနှင့် မနှိုင်းယှဉ်ပါနှင့်။ သင်၏ ယနေ့တိုးတက်မှုသည် သင်၏ မနေ့ကထက် သာလွန်နေပါက အောင်မြင်သော ခရီးကို လျှောက်လှမ်းနေခြင်း ဖြစ်သည်။";
                result.SacredAffirmation = "ကျွန်ုပ်သည် နေ့စဉ်နှင့်အမျှ ပိုမိုဉာဏ်ပညာပြည့်စုံ၊ စိတ်ဓာတ်ခိုင်မာပြီး အောင်မြင်ပျော်ရွှင်သော ပုဂ္ဂိုလ်တစ်ဦး ဖြစ်လာနေသည်။";
                result.PersonalityInsight = "သင်သည် အတွေးအခေါ်နက်နဲပြီး အမှန်တရားကို မြတ်နိုးသူ ဖြစ်သည်။ အားသာချက်မှာ ကိုယ့်ကိုယ်ကို သုံးသပ်ဆင်ခြင်နိုင်စွမ်း မြင့်မားခြင်းဖြစ်ပြီး အားနည်းချက်မှာ တစ်ခါတစ်ရံ စိတ်ဖိစီးမှုနှင့် စိုးရိမ်ပူပန်မှုများကို အလွန်အကျွံ စဉ်းစားမိတတ်ခြင်း ဖြစ်သည်။";
                result.OpportunitiesAndChallenges = "ကိုယ်ရည်ကိုယ်သွေး အဆင့်မြှင့်တင်နိုင်မည့် အခွင့်အလမ်းများ၊ ဝါသနာသစ်များ ရှာဖွေတွေ့ရှိနိုင်ခြေ မြင့်မားသည်။ ဘဝအပြောင်းအလဲကြီးများ ကြုံလာချိန်တွင် မတုန်လှုပ်ဘဲ ရင်ဆိုင်နိုင်စွမ်းကို မွေးမြူပါ။";
                result.MindfulGuidance = "စိတ်ဖိစီးမှုနှင့် စိုးရိမ်ပူပန်မှုများ ဖြစ်ပေါ်လာပါက အသက်ရှူလေ့ကျင့်ခန်း ပြုလုပ်ခြင်း၊ သဘာဝတရားနှင့် ထိတွေ့ခြင်း၊ မိမိစိတ်ကို အနားပေးခြင်းတို့ဖြင့် အတွင်းစိတ် အေးချမ်းမှုကို ပြန်လည်တည်ဆောက်ပါ။";
                break;

            case ReadingTopic.General:
            default:
                result.CoreProphecy = $"မဟာကံကြမ္မာ စက်ဝန်းကြီးသည် {primaryCard.Card.Name} ၏ မင်္ဂလာနိမိတ်ဖြင့် သင့်ဘက်သို့ အောင်မြင်စွာ လှည့်လည်လာပါပြီ။";
                result.DetailedInterpretation = $"{questionContext}၊ ကံကြမ္မာဇာတာ ကတ်ပြားများက သင်၏ ရှေ့ခရီးကို အောက်ပါအတိုင်း ဖွင့်လှစ်ပြသနေပါသည် - {cardSummary}။ " +
                                                $"{primaryCard.Card.Name} ၏ မူလစွမ်းအင် ({primaryCard.RevealedMeaning}) အရ သင်၏ ဘဝအလှည့်အပြောင်းသစ်တစ်ခု စတင်နေပြီဖြစ်ကြောင်း အသိပေးနေပါသည်။ " +
                                                (cards.Count > 1
                                                    ? $"{cards[1].Card.Name} ၏ အခွင့်အလမ်းတံခါးကို ရဲဝံ့စွာ ဖြတ်သန်းပြီး {cards.Last().Card.Name} ၏ အောင်မြင်သော အနာဂတ်ပန်းတိုင်ကို အရောက်လှမ်းပါ။"
                                                    : "ကောင်းကင်ဘုံ စွမ်းအားများသည် သင်၏ ရဲဝံ့သော ခြေလှမ်းတိုင်းကို ကူညီပံ့ပိုးပေးနေပါသည်။");
                result.CosmicAdvice = "တိတ်ဆိတ်ငြိမ်သက်နေခြင်းကို တုံ့ဆိုင်းနေသည်ဟု မထင်ပါနှင့်။ စကြဝဠာသည် သင့်ကို အမြင့်ဆုံး အောင်မြင်မှုသို့ ပို့ဆောင်ရန် အခြေအနေများကို ပြင်ဆင်ပေးနေခြင်း ဖြစ်သည်။";
                result.SacredAffirmation = "ကျွန်ုပ်သည် အခက်အခဲများကို ကျော်လွှားနိုင်သော ခွန်အားအပြည့်ဖြင့် အောင်မြင်သော ကံကြမ္မာခရီးကို မဆုတ်မနစ် လျှောက်လှမ်းမည်။";
                result.PersonalityInsight = "သင်သည် ဟန်ချက်ညီသော ကိုယ်ရည်ကိုယ်သွေးရှိပြီး အခြေအနေအချိန်အခါအလိုက် လိုက်လျောညီထွေ နေထိုင်တတ်သူ ဖြစ်သည်။ အားသာချက်မှာ စိတ်ဓာတ်ကြံ့ခိုင်ခြင်းဖြစ်ပြီး လောကီအပြောင်းအလဲများကို အပြုသဘောဖြင့် ရှုမြင်နိုင်သည်။";
                result.OpportunitiesAndChallenges = "ဘဝကဏ္ဍစုံတွင် အခွင့်အလမ်းသစ်များ ဆိုက်ရောက်လာမည်။ အနာဂတ်မသေချာမှုများနှင့် မျှော်လင့်မထားသော အပြောင်းအလဲများကို တည်ငြိမ်သော ဉာဏ်ပညာဖြင့် ဖြတ်သန်းပါ။";
                result.MindfulGuidance = "ဘဝတွင် အတက်အကျ သဘောတရားများသည် သဘာဝဖြစ်သဖြင့် အောင်မြင်ချိန်တွင် မာန်မာနမတက်ဘဲ၊ အခက်အခဲကြုံချိန်တွင် စိတ်ဓာတ်မကျဘဲ အစဉ်သတိတရားဖြင့် ဆင်ခြင်နေထိုင်ပါ။";
                break;
        }

        return result;
    }

    private static AiSynthesisResult SynthesizePalmReading(
        ReadingTopic topic,
        string questionContext,
        PalmReadingModel palm)
    {
        string core;
        string detailed;
        var advice = $"သင်၏ ပင်မစွမ်းအင်ဗဟိုချက် ({palm.PrimaryEnergyCenter}) ကို ပိုမိုအားကောင်းစေရန် နေ့စဉ် စိတ်တည်ငြိမ်အေးချမ်းမှုကို မွေးမြူပါ။";
        var affirmation = "ကျွန်ုပ်၏ လက်ဝါးပြင်သည် ဘဝဖန်တီးမှု စွမ်းအားကို ဆောင်ကြဉ်းပြီး ကံကြမ္မာလမ်းကြောင်းများသည် ထာဝစဉ် လင်းလက်တောက်ပနေပါသည်။";

        string personalityInsight;
        string opportunitiesAndChallenges;
        string mindfulGuidance;

        switch (topic)
        {
            case ReadingTopic.Love:
                core = "သင်၏ လက်ဝါးပြင် လက္ခဏာများသည် စစ်မှန်သော မေတ္တာသစ္စာနှင့် သဟဇာတဖြစ်သော အချစ်ရေး ကံကြမ္မာကို ထင်ဟပ်ပြနေပါသည်။";
                detailed = $"{questionContext}၊ သင်၏ {palm.HandArchetype} အရ {palm.ElementalDescription} မေတ္တာရေးရာတွင် စိတ်ရင်းစေတနာ မှန်ကန်ပြီး ချစ်ခင်သူအပေါ် နွေးထွေးစွာ ဂရုစိုက်တတ်သူ ဖြစ်ပါသည်။ အတိတ်မှ မပြေလည်မှုများကို ကျော်လွန်၍ နားလည်မှုခိုင်မာသော လက်တွဲဖော်နှင့်အတူ မင်္ဂလာရှိသော ဘဝခရီးကို အတူတကွ တည်ဆောက်နိုင်မည့် ကောင်းသော အလှည့်အပြောင်း ရောက်ရှိနေပြီ ဖြစ်ပါသည်။";
                personalityInsight = "သင်၏ နှလုံးလမ်းကြောင်းနှင့် သောကြာကုန်းမို့တို့အရ မေတ္တာရေးတွင် စိတ်ရင်းစေတနာ မှန်ကန်ပြီး ချစ်ခင်သူအပေါ် နွေးထွေးစွာ ဂရုစိုက်တတ်သည်။ အားသာချက်မှာ သစ္စာရှိမှုဖြစ်ပြီး အားနည်းချက်မှာ ခံစားချက်ကို ရင်ထဲတွင် မြိုသိပ်ထားတတ်ခြင်း ဖြစ်သည်။";
                opportunitiesAndChallenges = "ဖူးစာရှင်နှင့် ဆုံတွေ့ရမည့် သို့မဟုတ် အိမ်ထောင်ဖက်နှင့် ပိုမိုနားလည်မှုရရှိမည့် မင်္ဂလာအခွင့်အရေးများ ရှိနေသည်။ သဘောထားကွဲလွဲမှု စိန်ခေါ်မှုကို ညှိနှိုင်းဖြေရှင်းပါ။";
                mindfulGuidance = "အချစ်ရေးနှင့် အိမ်ထောင်ရေးတွင် တစ်ဖက်သတ် စွပ်စွဲခြင်း သို့မဟုတ် တတိယလူ၏ ကောလာဟလများကို မယုံကြည်ဘဲ မျက်နှာချင်းဆိုင် ပွင့်လင်းစွာ စကားပြောဆိုခြင်းဖြင့် သံသယများကို ဖြေဖျောက်ပါ။";
                break;

            case ReadingTopic.Education:
                core = "ဉာဏ်ပညာ လမ်းကြောင်းနှင့် ပညာရေး အောင်မြင်မှု တံခါးပေါက်သည် သင်၏ လက်ဝါးပြင် လက္ခဏာများအရ ပွင့်လှစ်သွားပါပြီ။";
                detailed = $"{questionContext}၊ သင်၏ {palm.HandArchetype} အရ {palm.ElementalDescription} ခက်ခဲနက်နဲသော စာပေပညာရပ်များကို အလွယ်တကူ ထိုးထွင်းသိမြင်နိုင်စွမ်းနှင့် လျင်မြန်သော သင်ယူနိုင်စွမ်းရှိသူ ဖြစ်ပါသည်။ စာမေးပွဲနှင့် လေ့လာမှုများတွင် စိတ်တည်ငြိမ်စွာ အာရုံစိုက်ပြီး ဇွဲဖြင့် ကြိုးစားပါက ထူးချွန်ပြောင်မြောက်သော အောင်မြင်မှုကို ဆွတ်ခူးနိုင်ပါမည်။";
                personalityInsight = "ဦးခေါင်းလမ်းကြောင်း၏ ရှည်လျားကြည်လင်မှုအရ ဉာဏ်ပညာ ထက်မြက်ပြီး ဆင်ခြင်တုံတရားနှင့် ခွဲခြမ်းစိတ်ဖြာနိုင်စွမ်း ကောင်းမွန်သည်။ အားသာချက်မှာ လေ့လာသင်ယူလိုစိတ် ပြင်းပြခြင်း ဖြစ်သည်။";
                opportunitiesAndChallenges = "ပညာရေးတွင် ဘွဲ့ထူးဂုဏ်ထူး ရရှိခြင်းနှင့် စာမေးပွဲ အောင်မြင်နိုင်မည့် အခွင့်အလမ်းများ ကြီးမားသည်။ စာကျက်ချိန် အာရုံပျံ့လွင့်မှု စိန်ခေါ်မှုကို စနစ်တကျ အချိန်ခွဲဝေ၍ ဖြေရှင်းပါ။";
                mindfulGuidance = "စာမေးပွဲဖိအားနှင့် ပညာရေး စိတ်ဖိစီးမှုများကို မလျှော့သော ဇွဲလုံ့လဖြင့် ရင်ဆိုင်ပါ။ ဦးနှောက်နှင့် ခန္ဓာကိုယ် ဟန်ချက်ညီစေရန် အနားယူချိန် မှန်ကန်စွာ ထားရှိပါ။";
                break;

            case ReadingTopic.CareerBusiness:
                core = "လုပ်ငန်းခွင်နှင့် စီးပွားရေး တိုးတက်မှု စွမ်းအင်များသည် သင်၏ လက်ဝါးပြင် လက္ခဏာများအရ အောင်မြင်စွာ စတင်တက်လှမ်းနေပါပြီ။";
                detailed = $"{questionContext}၊ သင်၏ {palm.HandArchetype} အရ {palm.ElementalDescription} မိမိ၏ လုပ်ငန်းခွင် စွမ်းဆောင်ရည်နှင့် မဟာဗျူဟာမြောက် တွေးခေါ်နိုင်စွမ်းတို့ကြောင့် လုပ်ဖော်ကိုင်ဖက်များနှင့် အထက်လူကြီးများ၏ ယုံကြည်အားကိုးမှုကို ရရှိမည် ဖြစ်ပါသည်။ ကိုယ်ပိုင်လုပ်ငန်း သို့မဟုတ် အလုပ်အကိုင်တွင် အခွင့်အလမ်းသစ်များ ပေါ်ထွန်းလာမည်ဖြစ်ပြီး မဆုတ်မနစ်သော ဇွဲသတ္တိဖြင့် ရဲဝံ့စွာ ရှေ့သို့ ချီတက်ပါ။";
                personalityInsight = "ကံကြမ္မာလမ်းကြောင်းနှင့် စနေကုန်းမို့တို့က စီးပွားရေးအမြင် ထက်မြက်မှု၊ တာဝန်ယူနိုင်စွမ်းနှင့် မဟာဗျူဟာမြောက် စဉ်းစားတွေးခေါ်နိုင်မှုကို ညွှန်ပြနေသည်။ အားသာချက်မှာ ဇွဲသတ္တိ ကြီးမားခြင်း ဖြစ်သည်။";
                opportunitiesAndChallenges = "Career Path တွင် ရာထူးတိုးတက်ခြင်း၊ လုပ်ငန်းခွင် အသိအမှတ်ပြုခံရခြင်းနှင့် ကိုယ်ပိုင်လုပ်ငန်း စတင်နိုင်မည့် အခွင့်အလမ်းများ ရှိသည်။ လုပ်ငန်းပြိုင်ဆိုင်မှု စိန်ခေါ်မှုကို အရည်အသွေးဖြင့် အနိုင်ယူပါ။";
                mindfulGuidance = "အလုပ်အကိုင် အပြောင်းအလဲများ ကြုံလာပါက စိတ်မလောဘဲ ရေရှည်အကျိုးကျေးဇူးကို စိစစ်ပါ။ လုပ်ဖော်ကိုင်ဖက်များနှင့် အပြန်အလှန် လေးစားမှုရှိသော ဆက်ဆံရေးကို တည်ဆောက်ပါ။";
                break;

            case ReadingTopic.Finance:
                core = "ဓနဥစ္စာ စီမံခန့်ခွဲမှုနှင့် ငွေကြေးစီးဆင်းမှုသည် စနစ်တကျ ကြိုးပမ်းအားထုတ်မှုဖြင့် ပိုမိုတည်ငြိမ်တိုးတက်လာပါမည်။";
                detailed = $"{questionContext}၊ သင်၏ {palm.HandArchetype} အရ {palm.ElementalDescription} စည်းကမ်းရှိသော ငွေကြေးစီမံခန့်ခွဲမှု၊ စနစ်တကျ အသုံးစရိတ် စိစစ်ခြင်းနှင့် ရေရှည်စုဆောင်းခြင်းတို့သည် ဥစ္စာဓန တည်တံ့ခိုင်မြဲမှု၏ အဓိကသော့ချက် ဖြစ်ပါသည်။ မလိုလားအပ်သော စွန့်စားမှုများကို ရှောင်ကြဉ်ပြီး တရားမျှတသော ကြိုးစားမှုဖြင့် ဓနဥစ္စာ အခြေခံကို ခိုင်မာစွာ တည်ဆောက်နိုင်ပါမည်။";
                personalityInsight = "လက်ဖဝါးပြင်ရှိ ဓနတြိဂံသင်္ကေတအရ သင်သည် ငွေကြေးကို စနစ်တကျ သုံးစွဲနိုင်သော အရည်အချင်းရှိပြီး စုဆောင်းလိုစိတ် ကောင်းမွန်သည်။ အားသာချက်မှာ ရေရှည်ဘဏ္ဍာရေး စီမံနိုင်ခြင်း ဖြစ်သည်။";
                opportunitiesAndChallenges = "စီးပွားရေးနှင့် ရင်းနှီးမြှုပ်နှံမှုမှ အကျိုးအမြတ် ရရှိမည့် အခွင့်အလမ်းများ ရှိသည်။ မဆင်မခြင် သုံးစွဲမှုနှင့် မလိုအပ်သော ငွေချေးငှားမှု စိန်ခေါ်မှုများကို တင်းကြပ်စွာ ရှောင်ကြဉ်ပါ။";
                mindfulGuidance = "ငွေကြေးဆုံးရှုံးမှုနှင့် အကြွေးပြဿနာများကို ကာကွယ်ရန် လောင်းကစားနှင့် ကံစမ်းမဲများကို အားမကိုးဘဲ ကိုယ်ပိုင်လုပ်အားနှင့် စနစ်တကျ စုဆောင်းမှုအပေါ်တွင်သာ ရပ်တည်ပါ။";
                break;

            case ReadingTopic.Relationship:
                core = "မိတ်ဆွေအပေါင်းအသင်းနှင့် မိသားစု လူမှုဆက်ဆံရေးသည် နွေးထွေးသော သဟဇာတဖြစ်မှုဖြင့် ပိုမိုခိုင်မြဲလာပါမည်။";
                detailed = $"{questionContext}၊ သင်၏ {palm.HandArchetype} အရ {palm.ElementalDescription} စာနာနားလည်နိုင်စွမ်း မြင့်မားပြီး ပတ်ဝန်းကျင်နှင့် ပြေပြစ်စွာ ဆက်ဆံနိုင်သူ ဖြစ်ပါသည်။ မိတ်ကောင်းဆွေကောင်းများနှင့် မိသားစု၏ ကူညီပံ့ပိုးမှုများကို ရရှိမည်ဖြစ်ပြီး အပြန်အလှန် လေးစားမှုဖြင့် နွေးထွေးသော ဆက်ဆံရေးကောင်းများကို တည်ဆောက်နိုင်ပါမည်။";
                personalityInsight = "နှလုံးလမ်းကြောင်းနှင့် လကုန်းမို့တို့အရ လူမှုဆက်ဆံရေး ပြေပြစ်ပြီး သူတစ်ပါးကို စာနာနားလည်နိုင်စွမ်း အလွန်ကောင်းမွန်သည်။ အားသာချက်မှာ မိတ်ဆွေများအပေါ် ဖော်ရွေနွေးထွေးခြင်း ဖြစ်သည်။";
                opportunitiesAndChallenges = "မိတ်ဆွေကောင်းများ၏ ဖေးမကူညီမှုနှင့် မိသားစု နွေးထွေးမှု ရရှိမည့် အခွင့်အလမ်းများ ရှိသည်။ လူမှုဆက်ဆံရေး ပဋိပက္ခ စိန်ခေါ်မှုကို သည်းခံနားလည်မှုဖြင့် ဖြေရှင်းပါ။";
                mindfulGuidance = "မိသားစု သို့မဟုတ် မိတ်ဆွေများကြားတွင် အထင်အမြင်လွဲမှားမှုများ ပေါ်ပေါက်ပါက အချိန်မဆွဲဘဲ ချက်ချင်း ရိုးသားစွာ ရှင်းလင်းဆွေးနွေးပါ။ မိမိ၏ စိတ်ပိုင်းဆိုင်ရာ နယ်နိမိတ်ကို ခိုင်မာစွာ ထားရှိပါ။";
                break;

            case ReadingTopic.PersonalGrowth:
                core = "မိမိကိုယ်ကို ပြန်လည်ရှာဖွေတွေ့ရှိမှုနှင့် ဘဝရည်မှန်းချက် ပန်းတိုင်ဆီသို့ ယုံကြည်ချက်အပြည့် လျှောက်လှမ်းနိုင်ပါပြီ။";
                detailed = $"{questionContext}၊ သင်၏ {palm.HandArchetype} အရ {palm.ElementalDescription} အတွင်းစိတ် ခွန်အားနှင့် ပင်ကိုယ်စွမ်းရည်များကို အပြည့်အဝ အသုံးချနိုင်မည့် အချိန်အခါ ရောက်ရှိနေပြီ ဖြစ်ပါသည်။ အားနည်းချက်များကို သင်ခန်းစာယူ၍ အားသာချက်အဖြစ် ပြောင်းလဲကာ ဘဝရည်မှန်းချက် ပန်းတိုင်များကို မဆုတ်မနစ် အရောက်လှမ်းနိုင်ပါမည်။";
                personalityInsight = "ဦးခေါင်းလမ်းကြောင်းနှင့် ကြာသပတေးကုန်းမို့တို့အရ မိမိကိုယ်ကို တိုးတက်လိုစိတ် အလွန်မြင့်မားပြီး စိတ်ဓာတ်ခိုင်မာသူ ဖြစ်သည်။ အားသာချက်မှာ မိမိ၏ အားနည်းချက်ကို သိမြင်၍ ပြုပြင်နိုင်စွမ်း ရှိခြင်း ဖြစ်သည်။";
                opportunitiesAndChallenges = "ဘဝရည်မှန်းချက် အသစ်များ ချမှတ်နိုင်ခြင်းနှင့် စိတ်စွမ်းအင် နိုးထလာမည့် အခွင့်အလမ်းများ ရှိသည်။ ဘဝအပြောင်းအလဲကြီးများ ကြုံတွေ့ရမှု စိန်ခေါ်မှုကို တည်ငြိမ်စွာ ရင်ဆိုင်ပါ။";
                mindfulGuidance = "စိတ်ဖိစီးမှုနှင့် အနာဂတ်မသေချာမှုများကို စိုးရိမ်မနေဘဲ လက်ရှိပစ္စုပ္ပန်တွင် အကောင်းဆုံး ကြိုးစားပါ။ မိမိကိုယ်ကို ချစ်ခင်တန်ဖိုးထားပြီး အတွင်းစိတ် အေးချမ်းမှုကို ရှာဖွေပါ။";
                break;

            case ReadingTopic.General:
            default:
                core = "မဟာကံကြမ္မာ စက်ဝန်းကြီးသည် သင်၏ လက်ဝါးပြင် လက္ခဏာများအရ မင်္ဂလာနိမိတ်ဖြင့် သင့်ဘက်သို့ အောင်မြင်စွာ လှည့်လည်လာပါပြီ။";
                detailed = $"{questionContext}၊ သင်၏ {palm.HandArchetype} အရ {palm.ElementalDescription} ဘဝ၏ အလှည့်အပြောင်းသစ်တစ်ခု စတင်နေပြီဖြစ်ပြီး လောကီအခက်အခဲများကို ဆန်းသစ်သော ဉာဏ်ပညာဖြင့် အောင်မြင်စွာ ကျော်လွှားနိုင်ပါမည်။ စကြဝဠာ၏ ကောင်းချီးမင်္ဂလာများက သင်၏ ရဲဝံ့သော ခြေလှမ်းတိုင်းကို ကူညီပံ့ပိုးပေးနေပါသည်။";
                personalityInsight = "သင်၏ လက်ဝါးလက္ခဏာများသည် ဟန်ချက်ညီသော ကိုယ်ရည်ကိုယ်သွေး၊ ရိုးသားဖြောင့်မတ်မှုနှင့် ပြဿနာများကို ဉာဏ်ဖြင့် ဖြေရှင်းနိုင်စွမ်းကို ဖော်ပြနေသည်။";
                opportunitiesAndChallenges = "ဘဝခရီးလမ်းတွင် အောင်မြင်မှု အခွင့်အလမ်းများ ပွင့်လင်းလာမည် ဖြစ်သော်လည်း ဇွဲမလျှော့သော ကြိုးပမ်းအားထုတ်မှု လိုအပ်မည်။";
                mindfulGuidance = "လောကဓံ အတက်အကျများကို အေးချမ်းစွာ ရင်ဆိုင်ပြီး ကောင်းမှုကုသိုလ်နှင့် မင်္ဂလာတရားများကို လက်ကိုင်ထားကာ ဘဝခရီးကို ဆက်လက် လျှောက်လှမ်းပါ။";
                break;
        }

        return new AiSynthesisResult
        {
            CoreProphecy = core,
            DetailedInterpretation = detailed,
            CosmicAdvice = advice,
            SacredAffirmation = affirmation,
            PersonalityInsight = personalityInsight,
            OpportunitiesAndChallenges = opportunitiesAndChallenges,
            MindfulGuidance = mindfulGuidance
        };
    }
}
