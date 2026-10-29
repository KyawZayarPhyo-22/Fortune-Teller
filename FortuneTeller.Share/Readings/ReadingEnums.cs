namespace FortuneTeller.Share.Readings;

public enum ReadingType
{
    Tarot = 1,
    Palm = 2
}

public enum ReadingTopic
{
    Love = 1,
    Education = 2,
    Relationship = 3,
    General = 4,
    CareerBusiness = 5,
    Finance = 6,
    PersonalGrowth = 7
}

public enum TarotSpreadType
{
    SingleCard = 1,      // 1 Card - For a specific question (ကတ် ၁ ကတ်ရွေးခြင်း — မေးခွန်းတစ်ခုအတွက်)
    ThreeCards = 2,      // 3 Cards - Past, Present, Future (ကတ် ၃ ကတ်ရွေးခြင်း — အတိတ်၊ ပစ္စုပ္ပန်၊ အနာဂတ်)
    FiveCards = 3,       // 5 Cards - Detailed Deep Reading (ကတ် ၅ ကတ်ရွေးခြင်း — အသေးစိတ်ဖတ်ရှုခြင်း)
    CelticCross = 4      // 10 Cards - Celtic Cross Reading (ကတ် ၁၀ ကတ်ရွေးခြင်း — Celtic Cross Reading)
}
