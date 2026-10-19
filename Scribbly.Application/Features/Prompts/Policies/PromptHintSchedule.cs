using Scribbly.Domain.Entities;
using Scribbly.Application.Features.Settings;
using Scribbly.Application.Features.Settings.Configuration;

namespace Scribbly.Application.Features.Prompts;

public static class PromptHintSchedule
{
    public static int RevealCount(
        GameSettings settings,
        int remainingSeconds,
        int letterCount,
        GameSettingsOptions options)
    {
        if (letterCount <= 2 || settings.DrawingTimeSeconds <= 0)
        {
            return 0;
        }
        (int firstPercent, int intervalPercent, int minimumInterval) = settings.HintSpeed switch
        {
            "slow" => (60, 20, 8),
            "normal" => (45, 15, 6),
            "fast" => (30, 10, 4),
            _ => (0, 0, 0)
        };
        if (minimumInterval == 0)
        {
            return 0;
        }
        int total = settings.DrawingTimeSeconds;
        int first = (total * firstPercent + 99) / 100;
        int interval = Math.Max(minimumInterval, (total * intervalPercent + 99) / 100);
        int elapsed = Math.Min(Math.Max(0, total - remainingSeconds), total - 5);
        if (elapsed < first)
        {
            return 0;
        }
        int budget = letterCount * GameSettingsPolicy.NormalizeHintPercentage(settings.HintPercentage, options) / 100;
        return Math.Min(budget, 1 + (elapsed - first) / interval);
    }
}
