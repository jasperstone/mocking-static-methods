using System;
using System.Collections.Generic;
using System.Reflection;
using System.Runtime.Serialization;
using Microsoft.Extensions.Logging;
using Volo.Abp.Cli.Commands;
using Xunit;

namespace Volo.Abp.Cli.Core.Tests.Commands
{
    public class SuiteCommandTests
    {
        [Fact]
        public void ShowSuiteManualUpdateCommand_Should_Log_Error_For_Manual_Update_Instructions()
        {
            var suiteCommand = (SuiteCommand)FormatterServices.GetUninitializedObject(typeof(SuiteCommand));
            var logger = new TestLogger<SuiteCommand>();
            suiteCommand.Logger = logger;

            var method = typeof(SuiteCommand).GetMethod(
                "ShowSuiteManualUpdateCommand",
                BindingFlags.Instance | BindingFlags.NonPublic);

            Assert.NotNull(method);

            method!.Invoke(suiteCommand, null);

            Assert.Contains(logger.Logs, entry =>
                entry.Level == LogLevel.Error &&
                entry.Message == "You can also run the following command to update ABP Suite.");

            Assert.Contains(logger.Logs, entry =>
                entry.Level == LogLevel.Error &&
                entry.Message.Contains("dotnet tool update -g Volo.Abp.Suite"));
        }

        private class TestLogger<T> : ILogger<T>
        {
            public List<(LogLevel Level, string Message)> Logs { get; } = new();

            public IDisposable BeginScope<TState>(TState state) => NullScope.Instance;

            public bool IsEnabled(LogLevel logLevel) => true;

            public void Log<TState>(LogLevel logLevel, EventId eventId, TState state, Exception exception,
                Func<TState, Exception, string> formatter)
            {
                var message = formatter != null ? formatter(state, exception) : state?.ToString();
                Logs.Add((logLevel, message ?? string.Empty));
            }

            private sealed class NullScope : IDisposable
            {
                public static readonly NullScope Instance = new();

                public void Dispose()
                {
                }
            }
        }
    }
}
