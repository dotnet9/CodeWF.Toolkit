using CodeWF.Toolkit.Logging;

namespace CodeWF.Toolkit.Network.Tests.Socket;

[CollectionDefinition(Name)]
public sealed class LoggingCollection : ICollectionFixture<LoggingFixture>
{
    public const string Name = "Logging";
}

public sealed class LoggingFixture : IAsyncLifetime
{
    public ValueTask InitializeAsync()
    {
        Logger.Initialize(new LoggerOptions
        {
            EnableConsole = false,
            EnableEventFeed = false
        });
        return ValueTask.CompletedTask;
    }

    public async ValueTask DisposeAsync() => await Logger.ShutdownAsync();
}
