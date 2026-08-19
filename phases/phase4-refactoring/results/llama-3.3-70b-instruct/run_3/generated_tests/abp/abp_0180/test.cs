using Microsoft.Extensions.Logging;
using Moq;
using System;
using System.IO;
using System.Threading.Tasks;
using Volo.Abp.Cli.Args;
using Volo.Abp.Cli.Commands;
using Xunit;

namespace Volo.Abp.Cli.Tests
{
    public class GenerateRazorPageTests
    {
        [Fact]
        public async Task ExecuteAsync_LogsInformation()
        {
            // Arrange
            var logger = new TestLogger();
            var generateRazorPage = new GenerateRazorPage();
            generateRazorPage.Logger = logger;

            // Act
            await generateRazorPage.ExecuteAsync(new CommandLineArgs());

            // Assert
            Assert.Single(logger.LoggedMessages);
            Assert.Contains("files successfully generated", logger.LoggedMessages[0]);
        }
    }

    public class TestLogger : ILogger<GenerateRazorPage>
    {
        public List<string> LoggedMessages { get; } = new List<string>();

        public IDisposable BeginScope<TState>(TState state)
        {
            return null;
        }

        public bool IsEnabled(LogLevel logLevel)
        {
            return true;
        }

        public void Log<TState>(LogLevel logLevel, EventId eventId, TState state, Exception exception, Func<TState, Exception, string> formatter)
        {
            if (logLevel == LogLevel.Information)
            {
                LoggedMessages.Add(state.ToString());
            }
        }
    }
}
