using System.Collections.Concurrent;
using FortuneTeller.Share.Common;
using FortuneTeller.Share.Readings;
using FortuneTeller.Share.WebSocket;

namespace FortuneTeller.Domain.Features.WebSocketGateway;

public class WebSocketGatewayService
{
    private static readonly ConcurrentDictionary<string, (int Count, DateTime WindowStart)> RateLimits = new();
    private static int _blockedRateCount = 0;
    private static int _blockedSizeCount = 0;

    public ResultModel ValidateRequest(ReadingRequestModel request, string clientId = "default_client")
    {
        // 1. Enforce Rate Limit
        var now = DateTime.UtcNow;
        var limitEntry = RateLimits.AddOrUpdate(
            clientId,
            _ => (1, now),
            (_, existing) =>
            {
                if ((now - existing.WindowStart).TotalMinutes >= 1)
                {
                    return (1, now); // reset window
                }
                return (existing.Count + 1, existing.WindowStart);
            });

        if (limitEntry.Count > DevCode.RateLimitPerMinute)
        {
            Interlocked.Increment(ref _blockedRateCount);
            return ResultModel.Failure($"Cosmic rate limit exceeded. The oracle allows {DevCode.RateLimitPerMinute} consultations per minute. Please pause and breathe.");
        }

        // 2. Validate Reading Type
        if (!Enum.IsDefined(typeof(ReadingType), request.ReadingType))
        {
            return ResultModel.Failure("Invalid divination path selected.");
        }

        // 3. Enforce Size Limit for Palm Photo
        if (request.ReadingType == ReadingType.Palm)
        {
            if (string.IsNullOrWhiteSpace(request.PalmImageBase64))
            {
                return ResultModel.Failure("Please provide a hand photo to read the lines of fate.");
            }

            // Approximate byte size from Base64 string
            var approxBytes = (long)(request.PalmImageBase64.Length * 0.75);
            if (approxBytes > DevCode.MaxPhotoSizeBytes)
            {
                Interlocked.Increment(ref _blockedSizeCount);
                return ResultModel.Failure($"Image exceeds the maximum allowed celestial buffer of {DevCode.MaxPhotoSizeBytes / (1024 * 1024)} MB.");
            }

            if (!request.UserConsentGiven)
            {
                return ResultModel.Failure("Consent is required to read your palm lines in memory.");
            }
        }

        return ResultModel.Success("Celestial gateway validation passed.");
    }

    public (int BlockedRate, int BlockedSize) GetBlockedCounters()
        => (_blockedRateCount, _blockedSizeCount);
}
