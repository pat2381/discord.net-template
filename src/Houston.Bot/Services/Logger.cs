using System;
using Microsoft.Extensions.Logging;

namespace TicketBot.Services;

public class Logger<T> : ILogger<T>
{
	private readonly ILogger _logger;

	public Logger(ILoggerFactory factory)
	{
		if (factory == null)
		{
			throw new ArgumentNullException(nameof(factory));
		}

		_logger = factory.CreateLogger(typeof(T).Name);
	}

	IDisposable ILogger.BeginScope<TState>(TState state)
	{
		return Task.FromResult(_logger.BeginScope(state));
	}

	bool ILogger.IsEnabled(LogLevel logLevel)
	{
		return _logger.IsEnabled(logLevel);
	}

    void ILogger.Log<TState>(LogLevel logLevel, EventId eventId, TState state, Exception exception, Func<TState, Exception, string> formatter)
    {
        // Wrap the formatter to match the expected nullability of Exception?
        _logger.Log(logLevel, eventId, state, exception, (s, e) => formatter(s, e!));
    }
}