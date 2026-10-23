namespace FortuneTeller.Share.Palm;

public class PalmReadingModel
{
    public string HandArchetype { get; set; } = "Water Hand"; // Earth, Air, Fire, Water
    public string ElementalDescription { get; set; } = string.Empty;
    public string LifeLineInterpretation { get; set; } = string.Empty;
    public string HeartLineInterpretation { get; set; } = string.Empty;
    public string HeadLineInterpretation { get; set; } = string.Empty;
    public string FateLineInterpretation { get; set; } = string.Empty;
    public string MountsAnalysis { get; set; } = string.Empty;
    public int LineClarityPercentage { get; set; } = 88;
    public string PrimaryEnergyCenter { get; set; } = "Heart & Intuition";
    public string DivineSymbolFound { get; set; } = "Mystic Cross on Mount of Saturn";
    public List<string> ProminentMarkings { get; set; } = new();
}

public class PalmPhotoUploadRequest
{
    public string Base64Image { get; set; } = string.Empty;
    public string FileName { get; set; } = string.Empty;
    public string ContentType { get; set; } = "image/jpeg";
    public long FileSizeBytes { get; set; }
    public bool UserConsentGiven { get; set; }
}
