using Microsoft.Extensions.Logging;

namespace Add2Number.Tests
{
    /// <summary>
    /// Minimal ILogger that records every log entry above its minimum level,
    /// so tests can assert on exactly what MyBigNumber logs without depending
    /// on console output.
    /// </summary>
    internal sealed class RecordingLogger : ILogger
    {
        private readonly LogLevel _minLevel;

        public RecordingLogger(LogLevel minLevel)
        {
            _minLevel = minLevel;
        }

        public List<(LogLevel Level, string Message)> Entries { get; } = new();

        public bool IsEnabled(LogLevel logLevel) => logLevel >= _minLevel;

        public IDisposable? BeginScope<TState>(TState state) where TState : notnull => null;

        public void Log<TState>(
            LogLevel logLevel,
            EventId eventId,
            TState state,
            Exception? exception,
            Func<TState, Exception?, string> formatter)
        {
            if (!IsEnabled(logLevel))
            {
                return;
            }

            Entries.Add((logLevel, formatter(state, exception)));
        }
    }
}
