namespace CodeWF.Toolkit.Logging;

internal interface ILogSink : IAsyncDisposable
{
    ValueTask WriteAsync(CodeWFLogEvent logEvent, CancellationToken cancellationToken);

    Task FlushAsync(CancellationToken cancellationToken);
}
