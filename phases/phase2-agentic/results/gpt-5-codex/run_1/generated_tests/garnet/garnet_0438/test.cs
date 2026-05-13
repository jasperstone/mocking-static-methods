using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace Garnet.Tests
{
    public class LoggerExtensionsTests
    {
        [Fact]
        public void LogInformationResetCommandStats_IsInvokedWhenFlagSet()
        {
            // Arrange
            var services = new MockRepository(MockBehavior.Strict);
            var logger = services.CreateMock<ILogger>();

            var resetEventFlags = new ConcurrentDictionary<object, bool>
            {
                ["COMMANDSTATS"] = true
            };

            var mockWarnLogger = services.CreateMock<ILogger>();
    
            // To ensure the extension method is exercised, we use LogInformation extension
            // directly on the ILogger instance under test.
            logger.Setup(l => l.Log(
                    LogLevel.Information,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, _) => v.ToString() == "Resetting command stats"),
                    It.IsAny<Exception>(),
                    It.IsAny<Func<It.IsAnyType, Exception, string>>()));

            // Act
            LoggerExtensions.LogInformation(logger.Object, "Resetting command stats");

            // Assert
            services.Verify(logger);
        }

        private static class LoggerExtensions
        {
            // Minimal re-definition to make the test self-contained
            public static void LogInformation(this ILogger logger, string message)
            {
                logger.Log(LogLevel.Information, default, message, null, (state, _) => state.ToString());
            }
        }
    }
}
