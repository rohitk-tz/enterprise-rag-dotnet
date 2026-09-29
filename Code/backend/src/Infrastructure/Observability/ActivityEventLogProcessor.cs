using System.Diagnostics;
using OpenTelemetry;
using OpenTelemetry.Logs;

namespace Infrastructure.Observability;

/// <summary>
/// Jaeger stores traces only and rejects the OTLP logs signal, so instead of exporting logs
/// separately this processor attaches each log record to the span that is active when it is
/// written. Jaeger renders span events as that span's "Logs", keeping them next to the request
/// that produced them. Logs written outside any span (e.g. startup) are left to the console.
/// </summary>
public sealed class ActivityEventLogProcessor : BaseProcessor<LogRecord>
{
    public override void OnEnd(LogRecord data)
    {
        var activity = Activity.Current;
        if (activity is null || !activity.IsAllDataRequested)
        {
            return;
        }

        var tags = new ActivityTagsCollection
        {
            ["log.severity"] = data.LogLevel.ToString(),
            ["log.category"] = data.CategoryName,
        };

        if (data.EventId.Id != 0)
        {
            tags["log.event_id"] = data.EventId.Id;
        }

        if (data.Attributes is not null)
        {
            foreach (var (key, value) in data.Attributes)
            {
                // "{OriginalFormat}" is the raw message template; the rendered message is the event name.
                if (key != "{OriginalFormat}")
                {
                    tags[key] = value?.ToString();
                }
            }
        }

        if (data.Exception is { } exception)
        {
            tags["exception.type"] = exception.GetType().FullName;
            tags["exception.message"] = exception.Message;
            tags["exception.stacktrace"] = exception.ToString();
        }

        var message = data.FormattedMessage ?? data.Body ?? data.CategoryName ?? "log";
        activity.AddEvent(new ActivityEvent(message, data.Timestamp, tags));
    }
}
