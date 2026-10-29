using FortuneTeller.Share.Readings;
using FortuneTeller.Share.Tarot;

namespace FortuneTeller.Domain.Features.ReadingOrchestrator;

public class SpreadCardSelector
{
    private static readonly List<TarotCardModel> FullDeck = InitializeDeck();

    public IReadOnlyList<TarotCardModel> GetAllCards() => FullDeck.AsReadOnly();

    public TarotSpreadInfoModel GetSpreadInfo(TarotSpreadType spreadType)
    {
        return spreadType switch
        {
            TarotSpreadType.SingleCard => new TarotSpreadInfoModel
            {
                Type = TarotSpreadType.SingleCard,
                Title = "ကတ် ၁ ကတ်ရွေးခြင်း — မေးခွန်းတစ်ခုအတွက်",
                Description = "တိကျသော မေးခွန်းတစ်ခု၊ နေ့စဉ် လမ်းညွှန်ချက် သို့မဟုတ် ချက်ချင်း လိုအပ်သော အဖြေနှင့် ရှင်းလင်းချက်အတွက် ဗေဒင်မျက်လုံး။",
                CardCount = 1,
                Positions = new() { "ပင်မ အဖြေနှင့် ရှင်းလင်းချက် (Core Answer & Oracle Revelation)" }
            },
            TarotSpreadType.ThreeCards => new TarotSpreadInfoModel
            {
                Type = TarotSpreadType.ThreeCards,
                Title = "ကတ် ၃ ကတ်ရွေးခြင်း — အတိတ်၊ ပစ္စုပ္ပန်၊ အနာဂတ်",
                Description = "အတိတ်၏ အကြောင်းရင်း၊ ပစ္စုပ္ပန် အခြေအနေနှင့် အနာဂတ် ကံကြမ္မာ ရလဒ်များကို ချိတ်ဆက်ဖော်ပြပေးပါသည်။",
                CardCount = 3,
                Positions = new()
                {
                    "၁။ အတိတ် (Past) — အရင်းခံအကြောင်းရင်း",
                    "၂။ ပစ္စုပ္ပန် (Present) — လက်ရှိအခြေအနေနှင့် စွမ်းအင်",
                    "၃။ အနာဂတ် (Future) — ရှေ့ဆက်ဖြစ်ပေါ်လာမည့် ရလဒ်"
                }
            },
            TarotSpreadType.FiveCards => new TarotSpreadInfoModel
            {
                Type = TarotSpreadType.FiveCards,
                Title = "ကတ် ၅ ကတ်ရွေးခြင်း — အသေးစိတ်ဖတ်ရှုခြင်း",
                Description = "ပြဿနာ၏ အရင်းအမြစ်မှသည် နောက်ဆုံး ရလဒ်အထိ အသေးစိတ် နက်ရှိုင်းစွာ ခွဲခြမ်းစိတ်ဖြာခြင်း။",
                CardCount = 5,
                Positions = new()
                {
                    "၁။ ပစ္စုပ္ပန် အခြေအနေ (Current Situation)",
                    "၂။ စိန်ခေါ်မှုနှင့် အတားအဆီး (Challenge / Obstacle)",
                    "၃။ အတိတ် အရင်းအမြစ် (Past Foundation)",
                    "၄။ မဝေးတော့သော အနာဂတ် (Near Future)",
                    "၅။ နောက်ဆုံး ရလဒ် (Ultimate Outcome)"
                }
            },
            TarotSpreadType.CelticCross => new TarotSpreadInfoModel
            {
                Type = TarotSpreadType.CelticCross,
                Title = "ကတ် ၁၀ ကတ်ရွေးခြင်း — Celtic Cross Reading",
                Description = "ကမ္ဘာကျော် ဆဲလ်တစ်ခရော့စ် — အလုံးစုံသော ဘဝကံကြမ္မာ၊ အတွင်းစိတ်၊ ပတ်ဝန်းကျင်နှင့် နောက်ဆုံးရလဒ်ကို အသေးစိတ် အနက်ဖွင့်ခြင်း။",
                CardCount = 10,
                Positions = new()
                {
                    "၁။ ပင်မ အခြေအနေ (The Present / Heart of the Matter)",
                    "၂။ စိန်ခေါ်မှု အတားအဆီး (The Crossing / Challenge)",
                    "၃။ မသိစိတ်နှင့် အခြေခံအကြောင်း (The Root / Subconscious)",
                    "၄။ လွန်ခဲ့သော အတိတ် (The Past)",
                    "၅။ အသိစိတ်နှင့် မျှော်မှန်းချက် (The Crown / Conscious Mind)",
                    "၆။ မကြာမီ ဖြစ်လာမည့် အနာဂတ် (The Near Future)",
                    "၇။ မိမိ၏ အတွင်းစိတ်နှင့် သဘောထား (The Self / Attitude)",
                    "၈။ ပြင်ပ ပတ်ဝန်းကျင်နှင့် လွှမ်းမိုးမှု (External Environment)",
                    "၉။ မျှော်လင့်ချက်များနှင့် စိုးရိမ်မှုများ (Hopes and Fears)",
                    "၁၀။ နောက်ဆုံး ကံကြမ္မာ အဖြေ (The Ultimate Outcome)"
                }
            },
            _ => GetSpreadInfo(TarotSpreadType.ThreeCards)
        };
    }

    public List<SelectedCardModel> SelectSpreadAndCardsInMemory(
        TarotSpreadType spreadType,
        List<int>? userSelectedCardIds = null)
    {
        var spreadInfo = GetSpreadInfo(spreadType);
        var rng = new Random();
        var selected = new List<SelectedCardModel>();
        var chosenCards = new List<TarotCardModel>();

        if (userSelectedCardIds != null && userSelectedCardIds.Count >= spreadInfo.CardCount)
        {
            foreach (var id in userSelectedCardIds.Take(spreadInfo.CardCount))
            {
                var card = FullDeck.FirstOrDefault(c => c.Id == id) ?? FullDeck[rng.Next(FullDeck.Count)];
                chosenCards.Add(card);
            }
        }
        else
        {
            var shuffled = FullDeck.OrderBy(_ => rng.Next()).ToList();
            chosenCards = shuffled.Take(spreadInfo.CardCount).ToList();
        }

        for (int i = 0; i < chosenCards.Count; i++)
        {
            var isReversed = rng.Next(0, 100) < 22;
            var posName = i < spreadInfo.Positions.Count ? spreadInfo.Positions[i] : $"Position {i + 1}";
            selected.Add(new SelectedCardModel
            {
                Card = chosenCards[i],
                PositionIndex = i + 1,
                PositionName = posName,
                IsReversed = isReversed
            });
        }
        return selected;
    }

    private static List<TarotCardModel> InitializeDeck()
    {
        return new List<TarotCardModel>
        {
            new() {
                Id = 0, Name = "The Fool", BurmeseName = "လူမိုက် / အစပြုခြင်း", RomanNumeral = "0", Arcana = "Major", Suit = "None", Number = 0, ImageUrl = "/images/tarot/m00.jpg", Element = "Air", AstrologicalSign = "Uranus", Symbol = "🌌", Keywords = "Beginnings, Innocence, Spontaneity, Leap of Faith", UprightMeaning = "လွတ်လပ်သော အစပြုခြင်းနှင့် မကြောက်မရွံ့ ရှေ့သို့ လှမ်းတက်ရမည့် အချိန်ဖြစ်ပါသည်။", ReversedMeaning = "တွေဝေတုံ့ဆိုင်းခြင်း သို့မဟုတ် မဆင်မခြင် စွန့်စားမှုများကို သတိပြုပါ။", Description = "A traveler on the precipice of infinity, ready to explore the cosmos."
            },
            new() {
                Id = 1, Name = "The Magician", BurmeseName = "မှော်ဆရာ / ဖန်တီးနိုင်စွမ်း", RomanNumeral = "I", Arcana = "Major", Suit = "None", Number = 1, ImageUrl = "/images/tarot/m01.jpg", Element = "Air", AstrologicalSign = "Mercury", Symbol = "✨", Keywords = "Manifestation, Power, Resourcefulness, Willpower", UprightMeaning = "သင်၏ အရည်အချင်းနှင့် ဖန်တီးနိုင်စွမ်းများ အထွတ်အထိပ်သို့ ရောက်ရှိနေပါသည်။ ရည်မှန်းချက်များကို လက်တွေ့ အကောင်အထည်ဖော်နိုင်မည်။", ReversedMeaning = "စွမ်းရည်များကို အသုံးမချနိုင်ခြင်း သို့မဟုတ် လှည့်စားမှုများကို သတိထားပါ။", Description = "Standing beneath an infinity symbol with tools of all four elements."
            },
            new() {
                Id = 2, Name = "The High Priestess", BurmeseName = "ဘုန်းတော်ကြီးမ / ဆဌမအာရုံ", RomanNumeral = "II", Arcana = "Major", Suit = "None", Number = 2, ImageUrl = "/images/tarot/m02.jpg", Element = "Water", AstrologicalSign = "Moon", Symbol = "🔮", Keywords = "Intuition, Sacred Secrets, Subconscious, Serenity", UprightMeaning = "အတွင်းစိတ် ထိုးထွင်းသိမြင်မှုနှင့် ဆဌမအာရုံ စွမ်းအား အလွန်အားကောင်းနေပါသည်။ စိတ်၏ အသံကို နားထောင်ပါ။", ReversedMeaning = "အတွင်းစိတ်ခံစားချက်ကို လျစ်လျူရှုမိခြင်း သို့မဟုတ် လျှို့ဝှက်ချက်များ ပေါ်ပေါက်လာခြင်း။", Description = "Seated between twin pillars of darkness and light."
            },
            new() {
                Id = 3, Name = "The Empress", BurmeseName = "ဧကရီမိဖုရား / ပြည့်စုံကြွယ်ဝခြင်း", RomanNumeral = "III", Arcana = "Major", Suit = "None", Number = 3, ImageUrl = "/images/tarot/m03.jpg", Element = "Earth", AstrologicalSign = "Venus", Symbol = "🌿", Keywords = "Abundance, Fertility, Nurturing, Creative Bloom", UprightMeaning = "မင်္ဂလာရှိသော ပြည့်စုံကြွယ်ဝမှု၊ မေတ္တာနှင့် တီထွင်ဖန်တီးမှုများ အောင်မြင်စွာ ပွင့်လန်းလာပါမည်။", ReversedMeaning = "တီထွင်ဖန်တီးနိုင်စွမ်း ပိတ်ဆို့နေခြင်း သို့မဟုတ် စိတ်ပင်ပန်းနွမ်းနယ်ခြင်း။", Description = "Crowned with twelve celestial stars amidst golden wheat fields."
            },
            new() {
                Id = 4, Name = "The Emperor", BurmeseName = "ဧကရာဇ်မင်း / တန်ခိုးအာဏာ", RomanNumeral = "IV", Arcana = "Major", Suit = "None", Number = 4, ImageUrl = "/images/tarot/m04.jpg", Element = "Fire", AstrologicalSign = "Aries", Symbol = "👑", Keywords = "Authority, Structure, Sovereign Will, Stability", UprightMeaning = "ခိုင်မာသော စည်းစနစ်၊ တည်ငြိမ်မှုနှင့် ဦးဆောင်နိုင်စွမ်းဖြင့် အောင်မြင်မှု အခိုင်အမာ ရရှိပါမည်။", ReversedMeaning = "တင်းကျပ်လွန်းခြင်း သို့မဟုတ် ထိန်းချုပ်မှု လျော့ရဲခြင်းတို့ကို သတိပြုပါ။", Description = "A commanding sovereign seated upon a stone throne."
            },
            new() {
                Id = 5, Name = "The Hierophant", BurmeseName = "ဆရာတော် / ဓမ္မနှင့် အဆုံးအမ", RomanNumeral = "V", Arcana = "Major", Suit = "None", Number = 5, ImageUrl = "/images/tarot/m05.jpg", Element = "Earth", AstrologicalSign = "Taurus", Symbol = "📜", Keywords = "Spiritual Wisdom, Tradition, Mentorship, Sacred Truth", UprightMeaning = "အတွေ့အကြုံရင့် ဆရာသမားများ၏ လမ်းညွှန်မှုနှင့် ရိုးရာဓလေ့ ကောင်းမွန်သော တန်ဖိုးများကို လိုက်နာပါက ကောင်းကျိုးရမည်။", ReversedMeaning = "ခေတ်မမီသော စည်းမျဉ်းများ သို့မဟုတ် ကိုယ်ပိုင်လမ်းကြောင်း ရှာဖွေရမည့် အချိန်ဖြစ်ခြင်း။", Description = "A sacred pontiff blessing seekers between twin pillars of divine knowledge."
            },
            new() {
                Id = 6, Name = "The Lovers", BurmeseName = "ချစ်သူများ / မေတ္တာနှင့် ရွေးချယ်မှု", RomanNumeral = "VI", Arcana = "Major", Suit = "None", Number = 6, ImageUrl = "/images/tarot/m06.jpg", Element = "Air", AstrologicalSign = "Gemini", Symbol = "💖", Keywords = "Sacred Union, Harmony, Values Alignment, Choice", UprightMeaning = "နက်ရှိုင်းသော မေတ္တာတရား ချိတ်ဆက်မှုနှင့် ဘဝအတွက် အရေးကြီးသော မင်္ဂလာရှိသည့် ဆုံးဖြတ်ချက် ရောက်ရှိလာပါပြီ။", ReversedMeaning = "သဘောထား ကွဲလွဲခြင်း သို့မဟုတ် ဆုံးဖြတ်ချက်ချရန် ခက်ခဲနေခြင်း။", Description = "Two souls blessed by the angel Raphael under the celestial sun."
            },
            new() {
                Id = 7, Name = "The Chariot", BurmeseName = "ရထားလုံး / အောင်ပွဲနှင့် ဇွဲလုံ့လ", RomanNumeral = "VII", Arcana = "Major", Suit = "None", Number = 7, ImageUrl = "/images/tarot/m07.jpg", Element = "Water", AstrologicalSign = "Cancer", Symbol = "⚡", Keywords = "Triumph, Momentum, Directed Will, Victory", UprightMeaning = "အတားအဆီးများကို ကျော်လွှား၍ မဆုတ်မနစ် ကြိုးစားမှုဖြင့် ကြီးကျယ်သော အောင်ပွဲကို ရရှိပါမည်။", ReversedMeaning = "ဦးတည်ချက် ပျောက်ဆုံးခြင်း သို့မဟုတ် စိတ်မြန်လက်မြန် လုပ်ဆောင်မိခြင်း။", Description = "A triumphant warrior driving twin sphinxes of black and white."
            },
            new() {
                Id = 8, Name = "Strength", BurmeseName = "ခွန်အား / စိတ်ဓာတ်ကြံ့ခိုင်မှု", RomanNumeral = "VIII", Arcana = "Major", Suit = "None", Number = 8, ImageUrl = "/images/tarot/m08.jpg", Element = "Fire", AstrologicalSign = "Leo", Symbol = "🦁", Keywords = "Inner Strength, Bravery, Compassion, Resilience", UprightMeaning = "နူးညံ့သိမ်မွေ့သော မေတ္တာနှင့် အတွင်းစိတ် ခွန်အားဖြင့် အခက်အခဲမှန်သမျှကို အောင်နိုင်ပါမည်။", ReversedMeaning = "မိမိကိုယ်ကို ယုံကြည်မှု အားနည်းခြင်း သို့မဟုတ် စိတ်မရှည် ဒေါသထွက်လွယ်ခြင်း။", Description = "A gentle maiden taming a lion with quiet cosmic grace."
            },
            new() {
                Id = 9, Name = "The Hermit", BurmeseName = "ရသေ့ / အတွင်းစိတ်ရှာဖွေခြင်း", RomanNumeral = "IX", Arcana = "Major", Suit = "None", Number = 9, ImageUrl = "/images/tarot/m09.jpg", Element = "Earth", AstrologicalSign = "Virgo", Symbol = "🏮", Keywords = "Soul Searching, Introspection, Wisdom, Inner Light", UprightMeaning = "ဆိတ်ငြိမ်စွာ စဉ်းစားဆင်ခြင်၍ အတွင်းစိတ် အမှန်တရားကို ရှာဖွေရမည့် အချိန်ဖြစ်ပါသည်။", ReversedMeaning = "အထီးကျန်ဆန်လွန်းခြင်း သို့မဟုတ် ပတ်ဝန်းကျင်နှင့် အဆက်အသွယ် ပြတ်တောက်ခြင်း။", Description = "An ascetic holding a glowing lantern atop a celestial peak."
            },
            new() {
                Id = 10, Name = "Wheel of Fortune", BurmeseName = "ကံကြမ္မာစက်ဝန်း / ဘဝအလှည့်အပြောင်း", RomanNumeral = "X", Arcana = "Major", Suit = "None", Number = 10, ImageUrl = "/images/tarot/m10.jpg", Element = "Fire", AstrologicalSign = "Jupiter", Symbol = "🎡", Keywords = "Good Fortune, Karma, Life Cycles, Destiny", UprightMeaning = "ကံကြမ္မာစက်ဝန်းသည် သင့်ဘက်သို့ အကောင်းဘက်မှ စတင်လည်ပတ်လာပါပြီ။ ကံကောင်းခြင်းများ ရောက်လာမည်။", ReversedMeaning = "မမျှော်လင့်သော အပြောင်းအလဲငယ်များ သို့မဟုတ် ကံအညွန့်ခေတ္တ ထိုင်းနေခြင်း။", Description = "The cosmic wheel turning under the gaze of mystical sphinxes."
            },
            new() {
                Id = 11, Name = "Justice", BurmeseName = "တရားမျှတခြင်း / သမ္မာဒိဋ္ဌိ", RomanNumeral = "XI", Arcana = "Major", Suit = "None", Number = 11, ImageUrl = "/images/tarot/m11.jpg", Element = "Air", AstrologicalSign = "Libra", Symbol = "⚖️", Keywords = "Fairness, Truth, Cause and Effect, Clarity", UprightMeaning = "တရားမျှတမှု၊ ရိုးသားဖြောင့်မတ်မှုနှင့် မှန်ကန်သော အဆုံးအဖြတ်တို့၏ ကောင်းကျိုးကို ခံစားရပါမည်။", ReversedMeaning = "မမျှတမှု ကြုံတွေ့ရခြင်း သို့မဟုတ် တာဝန်ယူလိုစိတ် အားနည်းခြင်း။", Description = "Justice seated holding the balance scale and the double-edged sword."
            },
            new() {
                Id = 12, Name = "The Hanged Man", BurmeseName = "ဇောက်ထိုးလူ / စွန့်လွှတ်အနစ်နာခံခြင်း", RomanNumeral = "XII", Arcana = "Major", Suit = "None", Number = 12, ImageUrl = "/images/tarot/m12.jpg", Element = "Water", AstrologicalSign = "Neptune", Symbol = "🌀", Keywords = "Surrender, New Perspective, Enlightenment, Pause", UprightMeaning = "အမြင်သစ်ဖြင့် ကြည့်မြင်ခြင်းနှင့် ခေတ္တရပ်နား စဉ်းစားခြင်းက ကြီးမြတ်သော အလင်းရောင်ကို ယူဆောင်လာပါမည်။", ReversedMeaning = "အကျိုးမရှိသော စွန့်လွှတ်မှု သို့မဟုတ် မပြတ်သားဘဲ ကြန့်ကြာနေခြင်း။", Description = "Suspended upside down from the living world tree, halo glowing."
            },
            new() {
                Id = 13, Name = "Death", BurmeseName = "သေခြင်းတရား / ဘဝသစ် အစပြုခြင်း", RomanNumeral = "XIII", Arcana = "Major", Suit = "None", Number = 13, ImageUrl = "/images/tarot/m13.jpg", Element = "Water", AstrologicalSign = "Scorpio", Symbol = "💀", Keywords = "Transformation, Endings, Rebirth, Fresh Chapter", UprightMeaning = "အခန်းဟောင်းတစ်ခု ပြီးဆုံး၍ သာလွန်ကောင်းမွန်သော အခန်းသစ်တစ်ခု အစပြုပါပြီ။", ReversedMeaning = "အပြောင်းအလဲကို လက်မခံဘဲ အဟောင်းကို တွယ်ကပ်နေခြင်း။", Description = "The dark rider carrying the mystic white rose flag of rebirth."
            },
            new() {
                Id = 14, Name = "Temperance", BurmeseName = "ဟန်ချက်ညီခြင်း / မျှတမှု", RomanNumeral = "XIV", Arcana = "Major", Suit = "None", Number = 14, ImageUrl = "/images/tarot/m14.jpg", Element = "Fire", AstrologicalSign = "Sagittarius", Symbol = "🏺", Keywords = "Balance, Moderation, Patience, Alchemy", UprightMeaning = "စိတ်ရှည်သည်းခံမှုနှင့် ဟန်ချက်ညီညီ ပေါင်းစပ်ထိန်းကျောင်းခြင်းက ငြိမ်းချမ်းသာယာမှုကို ဆောင်ကြဉ်းပေးပါမည်။", ReversedMeaning = "အတိုင်းအဆ လွန်ကဲခြင်း သို့မဟုတ် စိတ်မရှည်ဘဲ ဟန်ချက်ပျက်ခြင်း။", Description = "An angel blending two waters between silver vessels."
            },
            new() {
                Id = 15, Name = "The Devil", BurmeseName = "မာရ်နတ် / မာယာကျော့ကွင်း", RomanNumeral = "XV", Arcana = "Major", Suit = "None", Number = 15, ImageUrl = "/images/tarot/m15.jpg", Element = "Earth", AstrologicalSign = "Capricorn", Symbol = "⛓️", Keywords = "Shadow Self, Attachment, Addiction, Liberation", UprightMeaning = "မိမိကို ချည်နှောင်ထားသော စွဲလမ်းမှုများနှင့် အဆိုးမြင်စိတ်များကို ဖြတ်တောက်ရန် အချိန်တန်ပါပြီ။", ReversedMeaning = "ကျော့ကွင်းမှ လွတ်မြောက်ခြင်း၊ လွတ်လပ်မှုနှင့် ဉာဏ်အလင်း ပြန်လည်ရရှိခြင်း။", Description = "Chains loose enough to cast away under shadow illumination."
            },
            new() {
                Id = 16, Name = "The Tower", BurmeseName = "မျှော်စင်ပြိုကျခြင်း / ရုတ်တရက် အပြောင်းအလဲ", RomanNumeral = "XVI", Arcana = "Major", Suit = "None", Number = 16, ImageUrl = "/images/tarot/m16.jpg", Element = "Fire", AstrologicalSign = "Mars", Symbol = "⚡", Keywords = "Sudden Breakthrough, Shattered Illusions, Awakening", UprightMeaning = "အမှန်တရား မဟုတ်သော အရာများ ပြိုပျက်သွားပြီး အခြေခံကောင်း အသစ်ကို ပြန်လည်တည်ဆောက်နိုင်မည်။", ReversedMeaning = "ဘေးအန္တရာယ်မှ သီသီလေး လွတ်မြောက်ခြင်း သို့မဟုတ် အပြောင်းအလဲကို ကြောက်ရွံ့နေခြင်း။", Description = "A tower struck by lightning, shattering illusions into starry dust."
            },
            new() {
                Id = 17, Name = "The Star", BurmeseName = "ကြယ်ပွင့် / မျှော်လင့်ချက်အလင်း", RomanNumeral = "XVII", Arcana = "Major", Suit = "None", Number = 17, ImageUrl = "/images/tarot/m17.jpg", Element = "Air", AstrologicalSign = "Aquarius", Symbol = "⭐", Keywords = "Hope, Cosmic Inspiration, Renewal, Serenity", UprightMeaning = "မျှော်လင့်ချက်၊ စိတ်အေးချမ်းမှုနှင့် စကြဝဠာ၏ ကောင်းချီးမင်္ဂလာများ ပြည့်လျှံနေပါပြီ။", ReversedMeaning = "ယုံကြည်ချက် အားနည်းခြင်း သို့မဟုတ် အဆိုးမြင် စိတ်ကူးများ လွှမ်းမိုးနေခြင်း။", Description = "A maiden pouring celestial starlight onto sacred waters."
            },
            new() {
                Id = 18, Name = "The Moon", BurmeseName = "လမင်း / စိတ်ကူးယဉ်နှင့် မသေချာမှု", RomanNumeral = "XVIII", Arcana = "Major", Suit = "None", Number = 18, ImageUrl = "/images/tarot/m18.jpg", Element = "Water", AstrologicalSign = "Pisces", Symbol = "🌙", Keywords = "Mystery, Illusion, Dreams, Psychic Flow", UprightMeaning = "မျက်မြင်ထက် အတွင်းစိတ် အသိဉာဏ်ကို အားကိုးပါ။ မရှင်းလင်းသော အရာများ မကြာမီ ပေါ်ပေါက်လာပါမည်။", ReversedMeaning = "စိုးရိမ်ပူပန်မှုများ လျော့ပါးသွားခြင်းနှင့် အမှန်တရား ရှင်းလင်းလာခြင်း။", Description = "The radiant moon glowing over the mystical winding path."
            },
            new() {
                Id = 19, Name = "The Sun", BurmeseName = "နေမင်း / အောင်မြင်မှုနှင့် ပျော်ရွှင်ခြင်း", RomanNumeral = "XIX", Arcana = "Major", Suit = "None", Number = 19, ImageUrl = "/images/tarot/m19.jpg", Element = "Fire", AstrologicalSign = "Sun", Symbol = "☀️", Keywords = "Radiance, Joy, Vitality, Supreme Success", UprightMeaning = "ထူးကဲသော အောင်မြင်မှု၊ ပျော်ရွှင်ချမ်းမြေ့ခြင်းနှင့် အလင်းရောင်တောက်ပသော ဘဝခရီး ရောက်ရှိလာပါပြီ။", ReversedMeaning = "ခေတ္တမိုးအုံ့သကဲ့သို့ စိတ်မကြည်မလင်ဖြစ်ခြင်း သို့မဟုတ် အောင်မြင်မှု အနည်းငယ် နောက်ကျခြင်း။", Description = "A joyful child riding a white horse beneath sunflowers."
            },
            new() {
                Id = 20, Name = "Judgement", BurmeseName = "တရားစီရင်ခြင်း / နိုးထခြင်း", RomanNumeral = "XX", Arcana = "Major", Suit = "None", Number = 20, ImageUrl = "/images/tarot/m20.jpg", Element = "Fire", AstrologicalSign = "Pluto", Symbol = "🎺", Keywords = "Awakening, Higher Calling, Absolution, Renewal", UprightMeaning = "ဘဝရည်မှန်းချက် အဆင့်မြင့်သစ်တစ်ခုဆီသို့ နိုးထကာ ကြီးကျယ်သော တာဝန်ကို ထမ်းဆောင်နိုင်ပါပြီ။", ReversedMeaning = "မိမိကိုယ်ကို အပြစ်တင်နေခြင်း သို့မဟုတ် ဆုံးဖြတ်ချက်ချရန် တွန့်ဆုတ်နေခြင်း။", Description = "Gabriel sounding the golden trumpet of rebirth and awakening."
            },
            new() {
                Id = 21, Name = "The World", BurmeseName = "ကမ္ဘာလောက / ပြည့်စုံပြီးမြောက်ခြင်း", RomanNumeral = "XXI", Arcana = "Major", Suit = "None", Number = 21, ImageUrl = "/images/tarot/m21.jpg", Element = "Earth", AstrologicalSign = "Saturn", Symbol = "🌍", Keywords = "Completion, Cosmic Harmony, Wholeness, Fulfillment", UprightMeaning = "ကြိုးပမ်းခဲ့သမျှ အရာအားလုံး အောင်မြင်ပြည့်စုံစွာ ပြီးမြောက်၍ အထွတ်အထိပ် ရလဒ်ကို ပိုင်ဆိုင်ပါမည်။", ReversedMeaning = "နောက်ဆုံးအဆင့်တွင် အနည်းငယ် နှောင့်နှေးခြင်း သို့မဟုတ် နောက်ဆုံးခြေလှမ်း လိုအပ်နေခြင်း။", Description = "Dancing within the sacred laurel wreath of cosmic triumph."
            },
            new() {
                Id = 22, Name = "Ace of Wands", BurmeseName = "တုတ် ၁ ချောင်း", RomanNumeral = "I", Arcana = "Minor", Suit = "Wands", Number = 1, ImageUrl = "/images/tarot/w01.jpg", Element = "Fire", AstrologicalSign = "Wands", Symbol = "🔥", Keywords = "Wands, Fire, Rank I", UprightMeaning = "တီထွင်ဖန်တီးနိုင်စွမ်း စတင်ခြင်း၊ စိတ်အားထက်သန်မှုနှင့် အခွင့်အလမ်းသစ်။", ReversedMeaning = "ရည်ရွယ်ချက် မသေချာခြင်း သို့မဟုတ် နှောင့်နှေးကြန့်ကြာမှုများ။", Description = "Authentic Rider-Waite symbolic artwork of Ace of Wands (တုတ် ၁ ချောင်း)."
            },
            new() {
                Id = 23, Name = "Two of Wands", BurmeseName = "တုတ် ၂ ချောင်း", RomanNumeral = "II", Arcana = "Minor", Suit = "Wands", Number = 2, ImageUrl = "/images/tarot/w02.jpg", Element = "Fire", AstrologicalSign = "Wands", Symbol = "🔥", Keywords = "Wands, Fire, Rank II", UprightMeaning = "အနာဂတ်အစီအစဉ် ချမှတ်ခြင်း၊ ရှေ့သို့လှမ်းတက်ရန် ပြင်ဆင်ခြင်း။", ReversedMeaning = "ဆုံးဖြတ်ချက်မပြတ်သားခြင်း သို့မဟုတ် အစီအစဉ် ပျက်ပြားခြင်း။", Description = "Authentic Rider-Waite symbolic artwork of Two of Wands (တုတ် ၂ ချောင်း)."
            },
            new() {
                Id = 24, Name = "Three of Wands", BurmeseName = "တုတ် ၃ ချောင်း", RomanNumeral = "III", Arcana = "Minor", Suit = "Wands", Number = 3, ImageUrl = "/images/tarot/w03.jpg", Element = "Fire", AstrologicalSign = "Wands", Symbol = "🔥", Keywords = "Wands, Fire, Rank III", UprightMeaning = "ကြိုးပမ်းမှုများ အသီးအပွင့် စတင်ဝေဆာခြင်း၊ မိုးကုတ်စက်ဝိုင်းသစ်များ မျှော်ကြည့်ခြင်း။", ReversedMeaning = "မျှော်မှန်းထားသလောက် မရောက်ရှိသေးခြင်း သို့မဟုတ် စိတ်မရှည်ခြင်း။", Description = "Authentic Rider-Waite symbolic artwork of Three of Wands (တုတ် ၃ ချောင်း)."
            },
            new() {
                Id = 25, Name = "Four of Wands", BurmeseName = "တုတ် ၄ ချောင်း", RomanNumeral = "IV", Arcana = "Minor", Suit = "Wands", Number = 4, ImageUrl = "/images/tarot/w04.jpg", Element = "Fire", AstrologicalSign = "Wands", Symbol = "🔥", Keywords = "Wands, Fire, Rank IV", UprightMeaning = "အောင်မြင်မှု အထိမ်းအမှတ်၊ ပျော်ရွှင်ချမ်းမြေ့သော မိသားစု သို့မဟုတ် လူမှုဘဝ။", ReversedMeaning = "အိမ်တွင်း မငြိမ်မသက်ဖြစ်ခြင်း သို့မဟုတ် ပွဲတော်ရွှေ့ဆိုင်းရခြင်း။", Description = "Authentic Rider-Waite symbolic artwork of Four of Wands (တုတ် ၄ ချောင်း)."
            },
            new() {
                Id = 26, Name = "Five of Wands", BurmeseName = "တုတ် ၅ ချောင်း", RomanNumeral = "V", Arcana = "Minor", Suit = "Wands", Number = 5, ImageUrl = "/images/tarot/w05.jpg", Element = "Fire", AstrologicalSign = "Wands", Symbol = "🔥", Keywords = "Wands, Fire, Rank V", UprightMeaning = "ပြိုင်ဆိုင်မှု၊ အမြင်မတူ ကွဲလွဲမှုများနှင့် စိန်ခေါ်မှုများ။", ReversedMeaning = "ပဋိပက္ခများ ငြိမ်းအေးသွားခြင်း သို့မဟုတ် ပြေလည်မှု ရှာဖွေနိုင်ခြင်း။", Description = "Authentic Rider-Waite symbolic artwork of Five of Wands (တုတ် ၅ ချောင်း)."
            },
            new() {
                Id = 27, Name = "Six of Wands", BurmeseName = "တုတ် ၆ ချောင်း", RomanNumeral = "VI", Arcana = "Minor", Suit = "Wands", Number = 6, ImageUrl = "/images/tarot/w06.jpg", Element = "Fire", AstrologicalSign = "Wands", Symbol = "🔥", Keywords = "Wands, Fire, Rank VI", UprightMeaning = "အောင်ပွဲခံခြင်း၊ အများအသိအမှတ်ပြု ခံရခြင်းနှင့် ဂုဏ်သတင်းကျော်ကြားခြင်း။", ReversedMeaning = "မာနထောင်လွှားမိခြင်း သို့မဟုတ် အသိအမှတ်ပြု မခံရခြင်း။", Description = "Authentic Rider-Waite symbolic artwork of Six of Wands (တုတ် ၆ ချောင်း)."
            },
            new() {
                Id = 28, Name = "Seven of Wands", BurmeseName = "တုတ် ၇ ချောင်း", RomanNumeral = "VII", Arcana = "Minor", Suit = "Wands", Number = 7, ImageUrl = "/images/tarot/w07.jpg", Element = "Fire", AstrologicalSign = "Wands", Symbol = "🔥", Keywords = "Wands, Fire, Rank VII", UprightMeaning = "မိမိရပ်တည်ချက်ကို ခိုင်မာစွာ ကာကွယ်ခြင်း၊ ဇွဲလုံ့လဖြင့် ယှဉ်ပြိုင်အောင်မြင်ခြင်း။", ReversedMeaning = "ဖိအားများပြားခြင်း သို့မဟုတ် အရှုံးပေးလိုစိတ် ပေါ်ပေါက်ခြင်း။", Description = "Authentic Rider-Waite symbolic artwork of Seven of Wands (တုတ် ၇ ချောင်း)."
            },
            new() {
                Id = 29, Name = "Eight of Wands", BurmeseName = "တုတ် ၈ ချောင်း", RomanNumeral = "VIII", Arcana = "Minor", Suit = "Wands", Number = 8, ImageUrl = "/images/tarot/w08.jpg", Element = "Fire", AstrologicalSign = "Wands", Symbol = "🔥", Keywords = "Wands, Fire, Rank VIII", UprightMeaning = "လျင်မြန်သော တိုးတက်မှု၊ သတင်းကောင်းများ ဆက်တိုက် ရောက်ရှိလာခြင်း။", ReversedMeaning = "အလျင်စလိုဖြစ်ခြင်း သို့မဟုတ် သတင်းအချက်အလက် လွဲမှားခြင်း။", Description = "Authentic Rider-Waite symbolic artwork of Eight of Wands (တုတ် ၈ ချောင်း)."
            },
            new() {
                Id = 30, Name = "Nine of Wands", BurmeseName = "တုတ် ၉ ချောင်း", RomanNumeral = "IX", Arcana = "Minor", Suit = "Wands", Number = 9, ImageUrl = "/images/tarot/w09.jpg", Element = "Fire", AstrologicalSign = "Wands", Symbol = "🔥", Keywords = "Wands, Fire, Rank IX", UprightMeaning = "နောက်ဆုံးခံတပ်၊ ခံနိုင်ရည်ရှိစွာဖြင့် အောင်မြင်မှုပန်းတိုင်ကို စောင့်ကြပ်ခြင်း။", ReversedMeaning = "ပင်ပန်းနွမ်းနယ်လွန်းခြင်း သို့မဟုတ် သံသယလွန်ကဲခြင်း။", Description = "Authentic Rider-Waite symbolic artwork of Nine of Wands (တုတ် ၉ ချောင်း)."
            },
            new() {
                Id = 31, Name = "Ten of Wands", BurmeseName = "တုတ် ၁၀ ချောင်း", RomanNumeral = "X", Arcana = "Minor", Suit = "Wands", Number = 10, ImageUrl = "/images/tarot/w10.jpg", Element = "Fire", AstrologicalSign = "Wands", Symbol = "🔥", Keywords = "Wands, Fire, Rank X", UprightMeaning = "တာဝန်ဝတ္တရားများ လေးလံလွန်းခြင်း၊ ပန်းတိုင်နီးချိန် ပင်ပန်းမှု။", ReversedMeaning = "ဝန်ထုပ်ဝန်ပိုးများကို ခွဲဝေချထားနိုင်ခြင်းနှင့် သက်သာရာရခြင်း။", Description = "Authentic Rider-Waite symbolic artwork of Ten of Wands (တုတ် ၁၀ ချောင်း)."
            },
            new() {
                Id = 32, Name = "Page of Wands", BurmeseName = "တုတ်လုလင်", RomanNumeral = "Page", Arcana = "Minor", Suit = "Wands", Number = 11, ImageUrl = "/images/tarot/w11.jpg", Element = "Fire", AstrologicalSign = "Wands", Symbol = "🔥", Keywords = "Wands, Fire, Rank Page", UprightMeaning = "စိတ်လှုပ်ရှားဖွယ် သတင်းစကား၊ တက်ကြွသော စိတ်ကူးသစ်များနှင့် စူးစမ်းလိုစိတ်။", ReversedMeaning = "စိတ်မတည်ငြိမ်ခြင်း သို့မဟုတ် စိတ်မြန်လက်မြန် ဖြစ်လွယ်ခြင်း။", Description = "Authentic Rider-Waite symbolic artwork of Page of Wands (တုတ်လုလင်)."
            },
            new() {
                Id = 33, Name = "Knight of Wands", BurmeseName = "တုတ်မြင်းစီးသူရဲ", RomanNumeral = "Knight", Arcana = "Minor", Suit = "Wands", Number = 12, ImageUrl = "/images/tarot/w12.jpg", Element = "Fire", AstrologicalSign = "Wands", Symbol = "🔥", Keywords = "Wands, Fire, Rank Knight", UprightMeaning = "ရဲရင့်တက်ကြွသော စွန့်စားမှု၊ စိတ်အားထက်သန်စွာ ရှေ့သို့ချီတက်ခြင်း။", ReversedMeaning = "စိတ်မြန်ဒေါသကြီးခြင်း သို့မဟုတ် သတိလက်လွတ် ပြုမူခြင်း။", Description = "Authentic Rider-Waite symbolic artwork of Knight of Wands (တုတ်မြင်းစီးသူရဲ)."
            },
            new() {
                Id = 34, Name = "Queen of Wands", BurmeseName = "တုတ်ဘုရင်မ", RomanNumeral = "Queen", Arcana = "Minor", Suit = "Wands", Number = 13, ImageUrl = "/images/tarot/w13.jpg", Element = "Fire", AstrologicalSign = "Wands", Symbol = "🔥", Keywords = "Wands, Fire, Rank Queen", UprightMeaning = "ယုံကြည်မှုအပြည့်ရှိသော ဆွဲဆောင်မှု၊ ရဲဝံ့မှုနှင့် နွေးထွေးဖော်ရွေသော သြဇာ။", ReversedMeaning = "မနာလိုဝန်တိုမှု သို့မဟုတ် လွှမ်းမိုးလိုစိတ် လွန်ကဲခြင်း။", Description = "Authentic Rider-Waite symbolic artwork of Queen of Wands (တုတ်ဘုရင်မ)."
            },
            new() {
                Id = 35, Name = "King of Wands", BurmeseName = "တုတ်ဘုရင်", RomanNumeral = "King", Arcana = "Minor", Suit = "Wands", Number = 14, ImageUrl = "/images/tarot/w14.jpg", Element = "Fire", AstrologicalSign = "Wands", Symbol = "🔥", Keywords = "Wands, Fire, Rank King", UprightMeaning = "ရည်မှန်းချက်ကြီးမားသော ခေါင်းဆောင်မှု၊ အမြော်အမြင်နှင့် အောင်မြင်ကျော်ကြားမှု။", ReversedMeaning = "အာဏာရှင်ဆန်ခြင်း သို့မဟုတ် အတင်းအကျပ် စေခိုင်းခြင်း။", Description = "Authentic Rider-Waite symbolic artwork of King of Wands (တုတ်ဘုရင်)."
            },
            new() {
                Id = 36, Name = "Ace of Cups", BurmeseName = "ဖလား ၁ လုံး", RomanNumeral = "I", Arcana = "Minor", Suit = "Cups", Number = 1, ImageUrl = "/images/tarot/c01.jpg", Element = "Water", AstrologicalSign = "Cups", Symbol = "🏆", Keywords = "Cups, Water, Rank I", UprightMeaning = "မေတ္တာအသစ်၊ နက်ရှိုင်းသော စိတ်ခံစားမှုနှင့် ဝိညာဉ်ရေးရာ ပြည့်စုံမှု။", ReversedMeaning = "စိတ်ခံစားချက် ပိတ်ဆို့နေခြင်း သို့မဟုတ် မေတ္တာရေးရာ အားငယ်ခြင်း။", Description = "Authentic Rider-Waite symbolic artwork of Ace of Cups (ဖလား ၁ လုံး)."
            },
            new() {
                Id = 37, Name = "Two of Cups", BurmeseName = "ဖလား ၂ လုံး", RomanNumeral = "II", Arcana = "Minor", Suit = "Cups", Number = 2, ImageUrl = "/images/tarot/c02.jpg", Element = "Water", AstrologicalSign = "Cups", Symbol = "🏆", Keywords = "Cups, Water, Rank II", UprightMeaning = "အပြန်အလှန် မေတ္တာမျှတခြင်း၊ တွဲဖက်ညီသော မိတ်ဖက် သို့မဟုတ် အချစ်ရေး။", ReversedMeaning = "သဘောထားကွဲလွဲခြင်း သို့မဟုတ် နားလည်မှုလွဲမှားခြင်း။", Description = "Authentic Rider-Waite symbolic artwork of Two of Cups (ဖလား ၂ လုံး)."
            },
            new() {
                Id = 38, Name = "Three of Cups", BurmeseName = "ဖလား ၃ လုံး", RomanNumeral = "III", Arcana = "Minor", Suit = "Cups", Number = 3, ImageUrl = "/images/tarot/c03.jpg", Element = "Water", AstrologicalSign = "Cups", Symbol = "🏆", Keywords = "Cups, Water, Rank III", UprightMeaning = "မိတ်ဆွေများနှင့် အတူတကွ ပျော်ရွှင်ပွဲခံခြင်း၊ နွေးထွေးသော လူမှုပေါင်းသင်းဆက်ဆံရေး။", ReversedMeaning = "အတင်းအဖျင်းများ သို့မဟုတ် မိတ်ဆွေတုများ၏ သွေးခွဲမှု။", Description = "Authentic Rider-Waite symbolic artwork of Three of Cups (ဖလား ၃ လုံး)."
            },
            new() {
                Id = 39, Name = "Four of Cups", BurmeseName = "ဖလား ၄ လုံး", RomanNumeral = "IV", Arcana = "Minor", Suit = "Cups", Number = 4, ImageUrl = "/images/tarot/c04.jpg", Element = "Water", AstrologicalSign = "Cups", Symbol = "🏆", Keywords = "Cups, Water, Rank IV", UprightMeaning = "စိတ်မပါခြင်း၊ ပျင်းရိငြီးငွေ့ခြင်း သို့မဟုတ် ရောက်ရှိလာသော အခွင့်အရေးကို မမြင်ခြင်း။", ReversedMeaning = "အခွင့်အလမ်းသစ်ကို ပြန်လည်သတိပြုမိခြင်း၊ စိတ်အားသစ် ပြန်ရခြင်း။", Description = "Authentic Rider-Waite symbolic artwork of Four of Cups (ဖလား ၄ လုံး)."
            },
            new() {
                Id = 40, Name = "Five of Cups", BurmeseName = "ဖလား ၅ လုံး", RomanNumeral = "V", Arcana = "Minor", Suit = "Cups", Number = 5, ImageUrl = "/images/tarot/c05.jpg", Element = "Water", AstrologicalSign = "Cups", Symbol = "🏆", Keywords = "Cups, Water, Rank V", UprightMeaning = "ဆုံးရှုံးမှုအပေါ် နောင်တရခြင်း၊ ဝမ်းနည်းကြေကွဲမှု။ နောက်ကျောမှ ဖလားများကို သတိပြုပါ။", ReversedMeaning = "ဝမ်းနည်းမှုမှ ပြန်လည်ရုန်းထနိုင်ခြင်း၊ သင်ခန်းစာရယူခြင်း။", Description = "Authentic Rider-Waite symbolic artwork of Five of Cups (ဖလား ၅ လုံး)."
            },
            new() {
                Id = 41, Name = "Six of Cups", BurmeseName = "ဖလား ၆ လုံး", RomanNumeral = "VI", Arcana = "Minor", Suit = "Cups", Number = 6, ImageUrl = "/images/tarot/c06.jpg", Element = "Water", AstrologicalSign = "Cups", Symbol = "🏆", Keywords = "Cups, Water, Rank VI", UprightMeaning = "အတိတ်မှ အမှတ်တရကောင်းများ၊ ငယ်သူငယ်ချင်းများနှင့် ပြန်လည်ဆုံဆည်းခြင်း၊ အပြစ်ကင်းစင်မှု။", ReversedMeaning = "အတိတ်တွင် ပိတ်မိနေခြင်း သို့မဟုတ် အနာဂတ်ကို လျစ်လျူရှုမိခြင်း။", Description = "Authentic Rider-Waite symbolic artwork of Six of Cups (ဖလား ၆ လုံး)."
            },
            new() {
                Id = 42, Name = "Seven of Cups", BurmeseName = "ဖလား ၇ လုံး", RomanNumeral = "VII", Arcana = "Minor", Suit = "Cups", Number = 7, ImageUrl = "/images/tarot/c07.jpg", Element = "Water", AstrologicalSign = "Cups", Symbol = "🏆", Keywords = "Cups, Water, Rank VII", UprightMeaning = "ရွေးချယ်စရာများပြားခြင်း၊ စိတ်ကူးယဉ်အိပ်မက်များနှင့် မာယာကျော့ကွင်းများ။", ReversedMeaning = "အမှန်တရားကို ရှင်းလင်းစွာ မြင်တွေ့ခြင်း၊ တိကျသော ရွေးချယ်မှုပြုနိုင်ခြင်း။", Description = "Authentic Rider-Waite symbolic artwork of Seven of Cups (ဖလား ၇ လုံး)."
            },
            new() {
                Id = 43, Name = "Eight of Cups", BurmeseName = "ဖလား ၈ လုံး", RomanNumeral = "VIII", Arcana = "Minor", Suit = "Cups", Number = 8, ImageUrl = "/images/tarot/c08.jpg", Element = "Water", AstrologicalSign = "Cups", Symbol = "🏆", Keywords = "Cups, Water, Rank VIII", UprightMeaning = "အကျိုးမရှိတော့သော အရာများကို စွန့်ခွာ၍ ပိုမိုမြင့်မြတ်သော ပန်းတိုင်ဆီသို့ ခရီးနှင်ခြင်း။", ReversedMeaning = "စွန့်ခွာရန် ကြောက်ရွံ့နေခြင်း သို့မဟုတ် တွန့်ဆုတ်နေခြင်း။", Description = "Authentic Rider-Waite symbolic artwork of Eight of Cups (ဖလား ၈ လုံး)."
            },
            new() {
                Id = 44, Name = "Nine of Cups", BurmeseName = "ဖလား ၉ လုံး", RomanNumeral = "IX", Arcana = "Minor", Suit = "Cups", Number = 9, ImageUrl = "/images/tarot/c09.jpg", Element = "Water", AstrologicalSign = "Cups", Symbol = "🏆", Keywords = "Cups, Water, Rank IX", UprightMeaning = "ဆန္ဒများ ပြည့်ဝခြင်း၊ စိတ်ကျေနပ်နှစ်သိမ့်မှုနှင့် အောင်မြင်ပြည့်စုံခြင်း။", ReversedMeaning = "လောဘလွန်ကဲခြင်း သို့မဟုတ် စိတ်တိုင်းမကျသေးခြင်း။", Description = "Authentic Rider-Waite symbolic artwork of Nine of Cups (ဖလား ၉ လုံး)."
            },
            new() {
                Id = 45, Name = "Ten of Cups", BurmeseName = "ဖလား ၁၀ လုံး", RomanNumeral = "X", Arcana = "Minor", Suit = "Cups", Number = 10, ImageUrl = "/images/tarot/c10.jpg", Element = "Water", AstrologicalSign = "Cups", Symbol = "🏆", Keywords = "Cups, Water, Rank X", UprightMeaning = "စစ်မှန်သော ပျော်ရွှင်မှု၊ ပြည့်စုံငြိမ်းချမ်းသော မိသားစုနှင့် ရေရှည်တည်တံ့သော မေတ္တာ။", ReversedMeaning = "မိသားစုတွင်း မပြေလည်မှု သို့မဟုတ် အထင်အမြင်လွဲမှားခြင်း။", Description = "Authentic Rider-Waite symbolic artwork of Ten of Cups (ဖလား ၁၀ လုံး)."
            },
            new() {
                Id = 46, Name = "Page of Cups", BurmeseName = "ဖလားလုလင်", RomanNumeral = "Page", Arcana = "Minor", Suit = "Cups", Number = 11, ImageUrl = "/images/tarot/c11.jpg", Element = "Water", AstrologicalSign = "Cups", Symbol = "🏆", Keywords = "Cups, Water, Rank Page", UprightMeaning = "ချစ်ခင်မြတ်နိုးဖွယ် သတင်းစကား၊ တီထွင်ဖန်တီးဉာဏ်နှင့် စိတ်လှုပ်ရှားဖွယ်ရာ အစပြုမှု။", ReversedMeaning = "စိတ်ခံစားချက် မတည်ငြိမ်ခြင်း သို့မဟုတ် စိတ်ကူးယဉ်လွန်ခြင်း။", Description = "Authentic Rider-Waite symbolic artwork of Page of Cups (ဖလားလုလင်)."
            },
            new() {
                Id = 47, Name = "Knight of Cups", BurmeseName = "ဖလားမြင်းစီးသူရဲ", RomanNumeral = "Knight", Arcana = "Minor", Suit = "Cups", Number = 12, ImageUrl = "/images/tarot/c12.jpg", Element = "Water", AstrologicalSign = "Cups", Symbol = "🏆", Keywords = "Cups, Water, Rank Knight", UprightMeaning = "ရိုမန်းတစ်ဆန်သော ကမ်းလှမ်းမှု၊ နှလုံးသား၏ ခေါ်ဆောင်ရာသို့ လိုက်ပါခြင်း။", ReversedMeaning = "စိတ်ကူးယဉ်ဆန်လွန်းခြင်း သို့မဟုတ် ကတိမတည်ခြင်း။", Description = "Authentic Rider-Waite symbolic artwork of Knight of Cups (ဖလားမြင်းစီးသူရဲ)."
            },
            new() {
                Id = 48, Name = "Queen of Cups", BurmeseName = "ဖလားဘုရင်မ", RomanNumeral = "Queen", Arcana = "Minor", Suit = "Cups", Number = 13, ImageUrl = "/images/tarot/c13.jpg", Element = "Water", AstrologicalSign = "Cups", Symbol = "🏆", Keywords = "Cups, Water, Rank Queen", UprightMeaning = "နက်ရှိုင်းသော စာနာနားလည်မှု၊ မေတ္တာဓာတ်နှင့် ဆဌမအာရုံ ထက်မြက်ခြင်း။", ReversedMeaning = "စိတ်ခံစားချက် လွန်ကဲခြင်း သို့မဟုတ် အားနာလွန်းခြင်း။", Description = "Authentic Rider-Waite symbolic artwork of Queen of Cups (ဖလားဘုရင်မ)."
            },
            new() {
                Id = 49, Name = "King of Cups", BurmeseName = "ဖလားဘုရင်", RomanNumeral = "King", Arcana = "Minor", Suit = "Cups", Number = 14, ImageUrl = "/images/tarot/c14.jpg", Element = "Water", AstrologicalSign = "Cups", Symbol = "🏆", Keywords = "Cups, Water, Rank King", UprightMeaning = "စိတ်ခံစားမှုကို ဉာဏ်ဖြင့် ထိန်းကျောင်းနိုင်ခြင်း၊ တည်ငြိမ်ရင့်ကျက်သော မေတ္တာရှင်။", ReversedMeaning = "စိတ်ခံစားချက်ကို ချုပ်တည်းလွန်းခြင်း သို့မဟုတ် အေးစက်စက်ဆက်ဆံခြင်း။", Description = "Authentic Rider-Waite symbolic artwork of King of Cups (ဖလားဘုရင်)."
            },
            new() {
                Id = 50, Name = "Ace of Swords", BurmeseName = "ဓား ၁ လက်", RomanNumeral = "I", Arcana = "Minor", Suit = "Swords", Number = 1, ImageUrl = "/images/tarot/s01.jpg", Element = "Air", AstrologicalSign = "Swords", Symbol = "⚔️", Keywords = "Swords, Air, Rank I", UprightMeaning = "ရှင်းလင်းပြတ်သားသော ဉာဏ်ပညာ၊ အမှန်တရား ဖော်ထုတ်နိုင်ခြင်းနှင့် အောင်မြင်မှု။", ReversedMeaning = "ရှုပ်ထွေးမှုများ သို့မဟုတ် စကားအပြောအဆို ကြမ်းတမ်းမိခြင်း။", Description = "Authentic Rider-Waite symbolic artwork of Ace of Swords (ဓား ၁ လက်)."
            },
            new() {
                Id = 51, Name = "Two of Swords", BurmeseName = "ဓား ၂ လက်", RomanNumeral = "II", Arcana = "Minor", Suit = "Swords", Number = 2, ImageUrl = "/images/tarot/s02.jpg", Element = "Air", AstrologicalSign = "Swords", Symbol = "⚔️", Keywords = "Swords, Air, Rank II", UprightMeaning = "ခက်ခဲသော ရွေးချယ်မှု၊ မျက်စိမှိတ် အကျပ်ရိုက်နေသော အခြေအနေ။", ReversedMeaning = "အမှန်တရားကို ရင်ဆိုင်ရဲလာခြင်း၊ ဆုံးဖြတ်ချက်ချမှတ်နိုင်ခြင်း။", Description = "Authentic Rider-Waite symbolic artwork of Two of Swords (ဓား ၂ လက်)."
            },
            new() {
                Id = 52, Name = "Three of Swords", BurmeseName = "ဓား ၃ လက်", RomanNumeral = "III", Arcana = "Minor", Suit = "Swords", Number = 3, ImageUrl = "/images/tarot/s03.jpg", Element = "Air", AstrologicalSign = "Swords", Symbol = "⚔️", Keywords = "Swords, Air, Rank III", UprightMeaning = "နှလုံးသားထိခိုက်မှု၊ နာကျင်ဖွယ် အမှန်တရား သို့မဟုတ် ဝမ်းနည်းကြေကွဲခြင်း။", ReversedMeaning = "အနာကျက်စပြုလာခြင်း၊ နာကျင်မှုမှ သင်ခန်းစာယူနိုင်ခြင်း။", Description = "Authentic Rider-Waite symbolic artwork of Three of Swords (ဓား ၃ လက်)."
            },
            new() {
                Id = 53, Name = "Four of Swords", BurmeseName = "ဓား ၄ လက်", RomanNumeral = "IV", Arcana = "Minor", Suit = "Swords", Number = 4, ImageUrl = "/images/tarot/s04.jpg", Element = "Air", AstrologicalSign = "Swords", Symbol = "⚔️", Keywords = "Swords, Air, Rank IV", UprightMeaning = "စိတ်ရောကိုယ်ပါ အနားယူခြင်း၊ တရားထိုင်ဆင်ခြင်ခြင်းနှင့် အားပြန်ဖြည့်ခြင်း။", ReversedMeaning = "နိုးထလာချိန်၊ အနားယူပြီးနောက် ရှေ့သို့ပြန်လည်လှမ်းတက်ခြင်း။", Description = "Authentic Rider-Waite symbolic artwork of Four of Swords (ဓား ၄ လက်)."
            },
            new() {
                Id = 54, Name = "Five of Swords", BurmeseName = "ဓား ၅ လက်", RomanNumeral = "V", Arcana = "Minor", Suit = "Swords", Number = 5, ImageUrl = "/images/tarot/s05.jpg", Element = "Air", AstrologicalSign = "Swords", Symbol = "⚔️", Keywords = "Swords, Air, Rank V", UprightMeaning = "အနိုင်ရသော်လည်း အရှုံးနှင့်တူသော အောင်ပွဲ၊ ပဋိပက္ခနှင့် သဘောထားကွဲလွဲမှု။", ReversedMeaning = "အငြင်းပွားမှုများကို အဆုံးသတ်ခြင်း၊ ပြေလည်အောင် ဖြေရှင်းခြင်း။", Description = "Authentic Rider-Waite symbolic artwork of Five of Swords (ဓား ၅ လက်)."
            },
            new() {
                Id = 55, Name = "Six of Swords", BurmeseName = "ဓား ၆ လက်", RomanNumeral = "VI", Arcana = "Minor", Suit = "Swords", Number = 6, ImageUrl = "/images/tarot/s06.jpg", Element = "Air", AstrologicalSign = "Swords", Symbol = "⚔️", Keywords = "Swords, Air, Rank VI", UprightMeaning = "မုန်တိုင်းထန်သော အခြေအနေမှ အေးချမ်းရာ ကမ်းခြေသို့ ကူးပြောင်းခြင်း။", ReversedMeaning = "အတိတ်မှ ဝန်ထုပ်ဝန်ပိုးများ သယ်ဆောင်လာမိခြင်း သို့မဟုတ် ခရီးကြန့်ကြာခြင်း။", Description = "Authentic Rider-Waite symbolic artwork of Six of Swords (ဓား ၆ လက်)."
            },
            new() {
                Id = 56, Name = "Seven of Swords", BurmeseName = "ဓား ၇ လက်", RomanNumeral = "VII", Arcana = "Minor", Suit = "Swords", Number = 7, ImageUrl = "/images/tarot/s07.jpg", Element = "Air", AstrologicalSign = "Swords", Symbol = "⚔️", Keywords = "Swords, Air, Rank VII", UprightMeaning = "ပါးနပ်စွာ အကြံထုတ်ခြင်း၊ လျှို့ဝှက်စွာ လုပ်ဆောင်ခြင်း သို့မဟုတ် သတိထားလှုပ်ရှားရခြင်း။", ReversedMeaning = "အမှန်တရား ပေါ်ပေါက်လာခြင်း သို့မဟုတ် လှည့်စားခံရခြင်းမှ သတိပြုမိခြင်း။", Description = "Authentic Rider-Waite symbolic artwork of Seven of Swords (ဓား ၇ လက်)."
            },
            new() {
                Id = 57, Name = "Eight of Swords", BurmeseName = "ဓား ၈ လက်", RomanNumeral = "VIII", Arcana = "Minor", Suit = "Swords", Number = 8, ImageUrl = "/images/tarot/s08.jpg", Element = "Air", AstrologicalSign = "Swords", Symbol = "⚔️", Keywords = "Swords, Air, Rank VIII", UprightMeaning = "ကိုယ့်စိတ်ကူးဖြင့် ကိုယ်ပိတ်မိနေခြင်း၊ ကူကယ်ရာမဲ့သလို ခံစားရခြင်း။", ReversedMeaning = "ကြောက်ရွံ့မှုများကို ကျော်လွှား၍ လွတ်မြောက်လမ်း ရှာဖွေနိုင်ခြင်း။", Description = "Authentic Rider-Waite symbolic artwork of Eight of Swords (ဓား ၈ လက်)."
            },
            new() {
                Id = 58, Name = "Nine of Swords", BurmeseName = "ဓား ၉ လက်", RomanNumeral = "IX", Arcana = "Minor", Suit = "Swords", Number = 9, ImageUrl = "/images/tarot/s09.jpg", Element = "Air", AstrologicalSign = "Swords", Symbol = "⚔️", Keywords = "Swords, Air, Rank IX", UprightMeaning = "စိုးရိမ်ပူပန်မှု၊ ညဘက်အိပ်မပျော်ခြင်းနှင့် စိတ်ဖိစီးမှုလွန်ကဲခြင်း။", ReversedMeaning = "စိုးရိမ်မှုများ ပြေလျော့သွားခြင်း၊ အမှန်တရားမှာ ထင်သလောက် မဆိုးရွားကြောင်း သိမြင်ခြင်း။", Description = "Authentic Rider-Waite symbolic artwork of Nine of Swords (ဓား ၉ လက်)."
            },
            new() {
                Id = 59, Name = "Ten of Swords", BurmeseName = "ဓား ၁၀ လက်", RomanNumeral = "X", Arcana = "Minor", Suit = "Swords", Number = 10, ImageUrl = "/images/tarot/s10.jpg", Element = "Air", AstrologicalSign = "Swords", Symbol = "⚔️", Keywords = "Swords, Air, Rank X", UprightMeaning = "အခက်ခဲဆုံး အဆုံးသတ်ရောက်ရှိခြင်း။ ဒုက္ခများ ပြီးဆုံး၍ နေမင်းအသစ် ထွက်ပေါ်လာတော့မည်။", ReversedMeaning = "အဆိုးရွားဆုံး အခြေအနေကို ကျော်လွန်သွားပြီဖြစ်၍ ဘဝသစ် အစပြုခြင်း။", Description = "Authentic Rider-Waite symbolic artwork of Ten of Swords (ဓား ၁၀ လက်)."
            },
            new() {
                Id = 60, Name = "Page of Swords", BurmeseName = "ဓားလုလင်", RomanNumeral = "Page", Arcana = "Minor", Suit = "Swords", Number = 11, ImageUrl = "/images/tarot/s11.jpg", Element = "Air", AstrologicalSign = "Swords", Symbol = "⚔️", Keywords = "Swords, Air, Rank Page", UprightMeaning = "စူးစမ်းလေ့လာလိုစိတ်၊ သတင်းအချက်အလက် စုဆောင်းခြင်းနှင့် ထက်မြက်သော ဉာဏ်ရည်။", ReversedMeaning = "စကားများလွန်းခြင်း သို့မဟုတ် သတင်းမှားများ ဖြန့်ဝေမိခြင်း။", Description = "Authentic Rider-Waite symbolic artwork of Page of Swords (ဓားလုလင်)."
            },
            new() {
                Id = 61, Name = "Knight of Swords", BurmeseName = "ဓားမြင်းစီးသူရဲ", RomanNumeral = "Knight", Arcana = "Minor", Suit = "Swords", Number = 12, ImageUrl = "/images/tarot/s12.jpg", Element = "Air", AstrologicalSign = "Swords", Symbol = "⚔️", Keywords = "Swords, Air, Rank Knight", UprightMeaning = "ရည်မှန်းချက်ဆီသို့ လျင်မြန်ပြတ်သားစွာ ထိုးဖောက်ခြင်း၊ ဇွဲသန်ခြင်း။", ReversedMeaning = "မဆင်မခြင် အလျင်စလိုလုပ်မိခြင်း သို့မဟုတ် ရိုင်းစိုင်းမိခြင်း။", Description = "Authentic Rider-Waite symbolic artwork of Knight of Swords (ဓားမြင်းစီးသူရဲ)."
            },
            new() {
                Id = 62, Name = "Queen of Swords", BurmeseName = "ဓားဘုရင်မ", RomanNumeral = "Queen", Arcana = "Minor", Suit = "Swords", Number = 13, ImageUrl = "/images/tarot/s13.jpg", Element = "Air", AstrologicalSign = "Swords", Symbol = "⚔️", Keywords = "Swords, Air, Rank Queen", UprightMeaning = "ပြတ်သားသော ဉာဏ်ပညာ၊ အမှန်တရားကို တည့်တည့်မြင်နိုင်စွမ်းနှင့် သီးခြားရပ်တည်နိုင်ခြင်း။", ReversedMeaning = "အေးစက်လွန်းခြင်း သို့မဟုတ် ဝေဖန်ပြစ်တင်မှု ပြင်းထန်ခြင်း။", Description = "Authentic Rider-Waite symbolic artwork of Queen of Swords (ဓားဘုရင်မ)."
            },
            new() {
                Id = 63, Name = "King of Swords", BurmeseName = "ဓားဘုရင်", RomanNumeral = "King", Arcana = "Minor", Suit = "Swords", Number = 14, ImageUrl = "/images/tarot/s14.jpg", Element = "Air", AstrologicalSign = "Swords", Symbol = "⚔️", Keywords = "Swords, Air, Rank King", UprightMeaning = "ဥာဏ်ပညာပြည့်ဝသော တရားစီရင်ရေး၊ ဖြောင့်မတ်တည်ကြည်သော ခေါင်းဆောင်မှု။", ReversedMeaning = "အာဏာအလွဲသုံးစားလုပ်ခြင်း သို့မဟုတ် စည်းမျဉ်းတင်းကျပ်လွန်းခြင်း။", Description = "Authentic Rider-Waite symbolic artwork of King of Swords (ဓားဘုရင်)."
            },
            new() {
                Id = 64, Name = "Ace of Pentacles", BurmeseName = "ဒင်္ဂါး ၁ ပြား", RomanNumeral = "I", Arcana = "Minor", Suit = "Pentacles", Number = 1, ImageUrl = "/images/tarot/p01.jpg", Element = "Earth", AstrologicalSign = "Pentacles", Symbol = "🪙", Keywords = "Pentacles, Earth, Rank I", UprightMeaning = "ငွေကြေးဥစ္စာ အခွင့်အလမ်းသစ်၊ လက်တွေ့ကျသော အောင်မြင်မှုနှင့် တည်ငြိမ်မှု။", ReversedMeaning = "ငွေကြေးအခွင့်အလမ်း လွတ်သွားခြင်း သို့မဟုတ် အခြေခံမခိုင်မာခြင်း။", Description = "Authentic Rider-Waite symbolic artwork of Ace of Pentacles (ဒင်္ဂါး ၁ ပြား)."
            },
            new() {
                Id = 65, Name = "Two of Pentacles", BurmeseName = "ဒင်္ဂါး ၂ ပြား", RomanNumeral = "II", Arcana = "Minor", Suit = "Pentacles", Number = 2, ImageUrl = "/images/tarot/p02.jpg", Element = "Earth", AstrologicalSign = "Pentacles", Symbol = "🪙", Keywords = "Pentacles, Earth, Rank II", UprightMeaning = "တာဝန်များ ဟန်ချက်ညီညီ ထိန်းကျောင်းခြင်း၊ လိုက်လျောညီထွေ ပြောင်းလဲနိုင်စွမ်း။", ReversedMeaning = "ဟန်ချက်ပျက်ပြားခြင်း သို့မဟုတ် ငွေကြေးစီမံခန့်ခွဲမှု လိုအပ်ခြင်း။", Description = "Authentic Rider-Waite symbolic artwork of Two of Pentacles (ဒင်္ဂါး ၂ ပြား)."
            },
            new() {
                Id = 66, Name = "Three of Pentacles", BurmeseName = "ဒင်္ဂါး ၃ ပြား", RomanNumeral = "III", Arcana = "Minor", Suit = "Pentacles", Number = 3, ImageUrl = "/images/tarot/p03.jpg", Element = "Earth", AstrologicalSign = "Pentacles", Symbol = "🪙", Keywords = "Pentacles, Earth, Rank III", UprightMeaning = "ပူးပေါင်းဆောင်ရွက်မှု၊ ကျွမ်းကျင်မှုလက်ရာနှင့် အများ၏ လေးစားအသိအမှတ်ပြုမှု။", ReversedMeaning = "အဖွဲ့အစည်းတွင်း ပူးပေါင်းမှုအားနည်းခြင်း သို့မဟုတ် အရည်အသွေးကျဆင်းခြင်း။", Description = "Authentic Rider-Waite symbolic artwork of Three of Pentacles (ဒင်္ဂါး ၃ ပြား)."
            },
            new() {
                Id = 67, Name = "Four of Pentacles", BurmeseName = "ဒင်္ဂါး ၄ ပြား", RomanNumeral = "IV", Arcana = "Minor", Suit = "Pentacles", Number = 4, ImageUrl = "/images/tarot/p04.jpg", Element = "Earth", AstrologicalSign = "Pentacles", Symbol = "🪙", Keywords = "Pentacles, Earth, Rank IV", UprightMeaning = "ငွေကြေးစုဆောင်းထိန်းသိမ်းခြင်း၊ လုံခြုံစိတ်ချရသော ဘဝအခြေအနေ။", ReversedMeaning = "ငွေကြေးအပေါ် စေးနှဲလွန်းခြင်း သို့မဟုတ် ပြောင်းလဲမှုကို ကြောက်ရွံ့ခြင်း။", Description = "Authentic Rider-Waite symbolic artwork of Four of Pentacles (ဒင်္ဂါး ၄ ပြား)."
            },
            new() {
                Id = 68, Name = "Five of Pentacles", BurmeseName = "ဒင်္ဂါး ၅ ပြား", RomanNumeral = "V", Arcana = "Minor", Suit = "Pentacles", Number = 5, ImageUrl = "/images/tarot/p05.jpg", Element = "Earth", AstrologicalSign = "Pentacles", Symbol = "🪙", Keywords = "Pentacles, Earth, Rank V", UprightMeaning = "ခေတ္တငွေကြေးကျပ်တည်းမှု သို့မဟုတ် အထီးကျန်ခြင်း။ ကူညီမည့်နေရာ ရှိသည်ကို သတိပြုပါ။", ReversedMeaning = "အခက်အခဲများမှ ပြန်လည်လွတ်မြောက်လာခြင်း၊ အကူအညီရရှိခြင်း။", Description = "Authentic Rider-Waite symbolic artwork of Five of Pentacles (ဒင်္ဂါး ၅ ပြား)."
            },
            new() {
                Id = 69, Name = "Six of Pentacles", BurmeseName = "ဒင်္ဂါး ၆ ပြား", RomanNumeral = "VI", Arcana = "Minor", Suit = "Pentacles", Number = 6, ImageUrl = "/images/tarot/p06.jpg", Element = "Earth", AstrologicalSign = "Pentacles", Symbol = "🪙", Keywords = "Pentacles, Earth, Rank VI", UprightMeaning = "ပေးကမ်းလှူဒါန်းခြင်း၊ အပြန်အလှန် ဖေးမကူညီမှုနှင့် ကံကောင်းမှုများ ရရှိခြင်း။", ReversedMeaning = "မမျှတသော ခွဲဝေမှု သို့မဟုတ် လှည့်စားခံရခြင်း။", Description = "Authentic Rider-Waite symbolic artwork of Six of Pentacles (ဒင်္ဂါး ၆ ပြား)."
            },
            new() {
                Id = 70, Name = "Seven of Pentacles", BurmeseName = "ဒင်္ဂါး ၇ ပြား", RomanNumeral = "VII", Arcana = "Minor", Suit = "Pentacles", Number = 7, ImageUrl = "/images/tarot/p07.jpg", Element = "Earth", AstrologicalSign = "Pentacles", Symbol = "🪙", Keywords = "Pentacles, Earth, Rank VII", UprightMeaning = "စိုက်ပျိုးထားသော အပင်၏ အသီးအပွင့်ကို စိတ်ရှည်စွာ စောင့်ဆိုင်းသုံးသပ်ခြင်း။", ReversedMeaning = "စိတ်မရှည်ဘဲ စောစီးစွာ ရိတ်သိမ်းလိုခြင်း သို့မဟုတ် အားထုတ်မှုအလဟဿဖြစ်ခြင်း။", Description = "Authentic Rider-Waite symbolic artwork of Seven of Pentacles (ဒင်္ဂါး ၇ ပြား)."
            },
            new() {
                Id = 71, Name = "Eight of Pentacles", BurmeseName = "ဒင်္ဂါး ၈ ပြား", RomanNumeral = "VIII", Arcana = "Minor", Suit = "Pentacles", Number = 8, ImageUrl = "/images/tarot/p08.jpg", Element = "Earth", AstrologicalSign = "Pentacles", Symbol = "🪙", Keywords = "Pentacles, Earth, Rank VIII", UprightMeaning = "ကျွမ်းကျင်မှု ပညာရပ်ကို ဇွဲလုံ့လဖြင့် လေ့ကျင့်ဆည်းပူးခြင်း၊ လက်ရာမြောက်ခြင်း။", ReversedMeaning = "ပျင်းရိခြင်း သို့မဟုတ် အသေးစိတ်ကို လျစ်လျူရှုမိခြင်း။", Description = "Authentic Rider-Waite symbolic artwork of Eight of Pentacles (ဒင်္ဂါး ၈ ပြား)."
            },
            new() {
                Id = 72, Name = "Nine of Pentacles", BurmeseName = "ဒင်္ဂါး ၉ ပြား", RomanNumeral = "IX", Arcana = "Minor", Suit = "Pentacles", Number = 9, ImageUrl = "/images/tarot/p09.jpg", Element = "Earth", AstrologicalSign = "Pentacles", Symbol = "🪙", Keywords = "Pentacles, Earth, Rank IX", UprightMeaning = "ကိုယ်ပိုင်ကြိုးစားမှုဖြင့် ရရှိသော ကြွယ်ဝချမ်းသာမှု၊ လွတ်လပ်ငြိမ်းချမ်းသော ဘဝ။", ReversedMeaning = "ငွေကြေးအလဟဿ သုံးစွဲမိခြင်း သို့မဟုတ် အပေါ်ယံ အလှအပတွင် သာယာခြင်း။", Description = "Authentic Rider-Waite symbolic artwork of Nine of Pentacles (ဒင်္ဂါး ၉ ပြား)."
            },
            new() {
                Id = 73, Name = "Ten of Pentacles", BurmeseName = "ဒင်္ဂါး ၁၀ ပြား", RomanNumeral = "X", Arcana = "Minor", Suit = "Pentacles", Number = 10, ImageUrl = "/images/tarot/p10.jpg", Element = "Earth", AstrologicalSign = "Pentacles", Symbol = "🪙", Keywords = "Pentacles, Earth, Rank X", UprightMeaning = "မျိုးဆက်အလိုက် ရေရှည်တည်တံ့သော စည်းစိမ်ဥစ္စာ၊ ပြည့်စုံသော မိသားစုအမွေအနှစ်။", ReversedMeaning = "မိသားစုတွင်း အမွေပြဿနာ သို့မဟုတ် ငွေကြေးဆုံးရှုံးမှု။", Description = "Authentic Rider-Waite symbolic artwork of Ten of Pentacles (ဒင်္ဂါး ၁၀ ပြား)."
            },
            new() {
                Id = 74, Name = "Page of Pentacles", BurmeseName = "ဒင်္ဂါးလုလင်", RomanNumeral = "Page", Arcana = "Minor", Suit = "Pentacles", Number = 11, ImageUrl = "/images/tarot/p11.jpg", Element = "Earth", AstrologicalSign = "Pentacles", Symbol = "🪙", Keywords = "Pentacles, Earth, Rank Page", UprightMeaning = "လက်တွေ့ကျသော သင်ယူလိုစိတ်၊ ငွေကြေးအခွင့်အလမ်းသစ်နှင့် အလုပ်အကိုင်အစပြုမှု။", ReversedMeaning = "လက်တွေ့မကျသော စိတ်ကူးများ သို့မဟုတ် တာဝန်မဲ့ခြင်း။", Description = "Authentic Rider-Waite symbolic artwork of Page of Pentacles (ဒင်္ဂါးလုလင်)."
            },
            new() {
                Id = 75, Name = "Knight of Pentacles", BurmeseName = "ဒင်္ဂါးမြင်းစီးသူရဲ", RomanNumeral = "Knight", Arcana = "Minor", Suit = "Pentacles", Number = 12, ImageUrl = "/images/tarot/p12.jpg", Element = "Earth", AstrologicalSign = "Pentacles", Symbol = "🪙", Keywords = "Pentacles, Earth, Rank Knight", UprightMeaning = "ဇွဲလုံ့လရှိစွာ တာဝန်ကျေပွန်ခြင်း၊ စိတ်ချယုံကြည်ရသော လက်တွေ့သမား။", ReversedMeaning = "ခေါင်းမာလွန်းခြင်း သို့မဟုတ် တိုးတက်မှု နှေးကွေးလွန်းခြင်း။", Description = "Authentic Rider-Waite symbolic artwork of Knight of Pentacles (ဒင်္ဂါးမြင်းစီးသူရဲ)."
            },
            new() {
                Id = 76, Name = "Queen of Pentacles", BurmeseName = "ဒင်္ဂါးဘုရင်မ", RomanNumeral = "Queen", Arcana = "Minor", Suit = "Pentacles", Number = 13, ImageUrl = "/images/tarot/p13.jpg", Element = "Earth", AstrologicalSign = "Pentacles", Symbol = "🪙", Keywords = "Pentacles, Earth, Rank Queen", UprightMeaning = "ကြွယ်ဝချမ်းသာပြီး မေတ္တာရှင်၊ လက်တွေ့ကျကျ စီမံခန့်ခွဲနိုင်သော မိခင်သဖွယ်။", ReversedMeaning = "ငွေကြေးစိုးရိမ်ပူပန်လွန်ခြင်း သို့မဟုတ် မိမိကိုယ်ကို ဂရုမစိုက်မိခြင်း။", Description = "Authentic Rider-Waite symbolic artwork of Queen of Pentacles (ဒင်္ဂါးဘုရင်မ)."
            },
            new() {
                Id = 77, Name = "King of Pentacles", BurmeseName = "ဒင်္ဂါးဘုရင်", RomanNumeral = "King", Arcana = "Minor", Suit = "Pentacles", Number = 14, ImageUrl = "/images/tarot/p14.jpg", Element = "Earth", AstrologicalSign = "Pentacles", Symbol = "🪙", Keywords = "Pentacles, Earth, Rank King", UprightMeaning = "စီးပွားရေးနှင့် ဓနဥစ္စာ အထွတ်အထိပ်သို့ ရောက်ရှိခြင်း၊ တည်ငြိမ်အေးချမ်းသော ခေါင်းဆောင်။", ReversedMeaning = "စီးပွားရေး အာရုံလွန်ကဲခြင်း သို့မဟုတ် လောဘကြီးခြင်း။", Description = "Authentic Rider-Waite symbolic artwork of King of Pentacles (ဒင်္ဂါးဘုရင်)."
            },
        };
    }
}