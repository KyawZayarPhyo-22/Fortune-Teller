namespace FortuneTeller.Share.Common;

public static class DevCode
{
    public const int DefaultPageSize = 10;
    public const int MaxPhotoSizeBytes = 5 * 1024 * 1024; // 5 MB Max Photo Size
    public const int RateLimitPerMinute = 20; // 20 requests/minute per client
    public const string DefaultAiOracleName = "Astraea the Celestial Seer";

    public static class Topics
    {
        public const string Love = "Love";
        public const string Education = "Education";
        public const string Relationship = "Relationship";
        public const string General = "General";
    }

    public static class ReadingTypes
    {
        public const string Tarot = "Tarot";
        public const string Palm = "Palm";
    }

    public static class WebSocketEvents
    {
        public const string Connecting = "Connecting";
        public const string Validated = "Validated";
        public const string IngestingInput = "IngestingInput";
        public const string Orchestrating = "Orchestrating";
        public const string AiGenerating = "AiGenerating";
        public const string Formatting = "Formatting";
        public const string Completed = "Completed";
        public const string Error = "Error";
    }
}
