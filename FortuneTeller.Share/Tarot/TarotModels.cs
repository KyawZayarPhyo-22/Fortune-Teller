using FortuneTeller.Share.Readings;

namespace FortuneTeller.Share.Tarot;

public class TarotCardModel
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string BurmeseName { get; set; } = string.Empty;
    public string RomanNumeral { get; set; } = string.Empty;
    public string Arcana { get; set; } = "Major"; // Major or Minor
    public string Suit { get; set; } = "None"; // Wands, Cups, Swords, Pentacles, None
    public int Number { get; set; }
    public string ImageUrl { get; set; } = string.Empty;
    public string UprightMeaning { get; set; } = string.Empty;
    public string ReversedMeaning { get; set; } = string.Empty;
    public string Element { get; set; } = "Ether"; // Fire, Water, Air, Earth, Ether
    public string AstrologicalSign { get; set; } = string.Empty;
    public string Symbol { get; set; } = "✨";
    public string Keywords { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
}

public class SelectedCardModel
{
    public TarotCardModel Card { get; set; } = new();
    public int PositionIndex { get; set; }
    public string PositionName { get; set; } = string.Empty;
    public bool IsReversed { get; set; }
    public string RevealedMeaning => IsReversed ? Card.ReversedMeaning : Card.UprightMeaning;
}

public class TarotSpreadInfoModel
{
    public TarotSpreadType Type { get; set; }
    public string Title { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public int CardCount { get; set; }
    public List<string> Positions { get; set; } = new();
}
