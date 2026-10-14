using CodeWF.Toolkit.Logging;
using Microsoft.Extensions.Logging;
using System.Diagnostics;

namespace CodeWF.Toolkit.Logging.Microsoft;

internal sealed class CodeWFLogger(string categoryName, CodeWFLoggerProvider provider) : ILogger
{
    public IDisposable? BeginScope<TState>(TState state) where TState : notnull =>
        provider.ScopeProvider.Push(state);

    public bool IsEnabled(LogLevel logLevel) => provider.IsEnabled(logLevel);

    public void Log<TState>(
        LogLevel logLevel,
        EventId eventId,
        TState state,
        Exception? exception,
        Func<TState, Exception?, string> formatter)
    {
        if (!IsEnabled(logLevel)) return;
        ArgumentNullException.ThrowIfNull(formatter);

        var stateSnapshot = CaptureState(state);
        var message = FormatMessage(state, exception, formatter);
        if (string.IsNullOrWhiteSpace(message) && exception is null && eventId.Id == 0 &&
            stateSnapshot.Properties.Count == 0 && string.IsNullOrWhiteSpace(stateSnapshot.UserMessage)) return;

        var activity = provider.CaptureActivity ? Activity.Current : null;
        provider.Write(new CodeWFLogEvent
        {
            Sequence = 0,
            Timestamp = DateTimeOffset.Now,
            Level = logLevel,
            CategoryName = categoryName,
            EventId = eventId,
            MessageTemplate = stateSnapshot.MessageTemplate,
            Message = message,
            UserMessage = stateSnapshot.UserMessage,
            RequestNotification = stateSnapshot.RequestNotification,
            Exception = LogExceptionInfo.Capture(exception),
            Properties = stateSnapshot.Properties,
            Scopes = provider.CaptureScopes ? CaptureScopes() : [],
            TraceId = activity?.TraceId.ToString(),
            SpanId = activity?.SpanId.ToString(),
            ParentId = activity?.ParentId,
            TraceState = activity?.TraceStateString,
            TraceFlags = activity?.ActivityTraceFlags ?? default,
            ActivityTags = activity is not null && provider.CaptureActivityTags
                ? LogValueFormatter.CaptureProperties(activity.TagObjects)
                : [],
            ActivityBaggage = activity is not null && provider.CaptureActivityBaggage
                ? LogValueFormatter.CaptureProperties(activity.Baggage.Select(x =>
                    new KeyValuePair<string, object?>(x.Key, x.Value)))
                : []
        });
    }

    private IReadOnlyList<LogScope> CaptureScopes()
    {
        var scopes = new List<LogScope>();
        try
        {
            provider.ScopeProvider.ForEachScope(static (scope, state) =>
                state.Add(LogStateSnapshot.CaptureScope(scope)), scopes);
        }
        catch (Exception ex) { LoggerSelfDiagnostics.Report(global::CodeWF.Toolkit.Logging.Microsoft.LibraryText.Get("Toolkit.Logging.Microsoft.Diagnostics.ScopeCaptureFailed"), ex); }
        return scopes;
    }

    private static LogStateSnapshot CaptureState<TState>(TState state)
    {
        try { return LogStateSnapshot.Capture(state); }
        catch (Exception ex)
        {
            LoggerSelfDiagnostics.Report(global::CodeWF.Toolkit.Logging.Microsoft.LibraryText.Get("Toolkit.Logging.Microsoft.Diagnostics.StateCaptureFailed"), ex);
            var userState = state as ICodeWFUserLogState;
            return new LogStateSnapshot(null, [], userState?.UserMessage, userState?.RequestNotification == true);
        }
    }

    private static string FormatMessage<TState>(TState state, Exception? exception, Func<TState, Exception?, string> formatter)
    {
        try { return formatter(state, exception) ?? string.Empty; }
        catch (Exception ex)
        {
            LoggerSelfDiagnostics.Report(global::CodeWF.Toolkit.Logging.Microsoft.LibraryText.Get("Toolkit.Logging.Microsoft.Diagnostics.MessageFormatFailed"), ex);
            try { return state?.ToString() ?? string.Empty; }
            catch { return string.Empty; }
        }
    }
}
