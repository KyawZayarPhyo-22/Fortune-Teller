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
            return ResultModel.Failure($"မေးမြန်းမှု အကြိမ်ရေများလွန်းနေပါသည်။ တစ်မိနစ်လျှင် {DevCode.RateLimitPerMinute} ကြိမ်သာ မေးမြန်းနိုင်ပါသည်။ ခေတ္တခဏစောင့်ဆိုင်းပေးပါ။");
        }

        // 2. Validate Reading Type
        if (!Enum.IsDefined(typeof(ReadingType), request.ReadingType))
        {
            return ResultModel.Failure("မှန်ကန်သော ဗေဒင်ပုံစံ မဟုတ်ပါ။");
        }

        // 3. Enforce Size Limit for Palm Photo
        if (request.ReadingType == ReadingType.Palm)
        {
            if (string.IsNullOrWhiteSpace(request.PalmImageBase64))
            {
                return ResultModel.Failure("လက်ဝါးလက္ခဏာ ဖတ်ရှုရန်အတွက် လက်ဝါးဓာတ်ပုံ ပေးပို့ပေးပါ။");
            }

            // Approximate byte size from Base64 string
            var approxBytes = (long)(request.PalmImageBase64.Length * 0.75);
            if (approxBytes > DevCode.MaxPhotoSizeBytes)
            {
                Interlocked.Increment(ref _blockedSizeCount);
                return ResultModel.Failure($"ဓာတ်ပုံဖိုင်ဆိုဒ်သည် သတ်မှတ်ထားသော {DevCode.MaxPhotoSizeBytes / (1024 * 1024)} MB ထက် ကျော်လွန်နေပါသည်။");
            }

            if (!request.UserConsentGiven)
            {
                return ResultModel.Failure("လက်ဝါးလက္ခဏာ ခွဲခြမ်းစိတ်ဖြာရန်အတွက် သဘောတူညီချက် လိုအပ်ပါသည်။");
            }
        }

        return ResultModel.Success("စိစစ်ချက် အောင်မြင်ပါသည်။");
    }

    public (int BlockedRate, int BlockedSize) GetBlockedCounters()
        => (_blockedRateCount, _blockedSizeCount);
}
