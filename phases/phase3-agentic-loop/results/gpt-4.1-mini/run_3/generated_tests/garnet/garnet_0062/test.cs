using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace Garnet.cluster.Tests
{
    public class FailoverSessionLoggerTests
    {
        private class LoggerTestHelper
        {
            private readonly ILogger logger;

            public LoggerTestHelper(ILogger logger)
            {
                this.logger = logger;
            }

            public async Task AttachReplicasAndWaitAsync(List<Task> attachReplicaTasks)
            {
                if (attachReplicaTasks.Count > 0)
                {
                    try
                    {
                        await Task.WhenAll(attachReplicaTasks).ConfigureAwait(false);
                    }
                    catch (Exception ex)
                    {
                        logger?.LogWarning(ex, "WaitingForAttachToComplete Error");
                    }
                }
            }
        }

        [Fact]
        public async Task AttachReplicasAndWaitAsync_LogsWarningOnTaskWhenAllException()
        {
            // Arrange
            var loggerMock = new Mock<ILogger>();
            var helper = new LoggerTestHelper(loggerMock.Object);

            var tasks = new List<Task>
            {
                Task.FromException(new InvalidOperationException("Simulated failure"))
            };

            // Act
            await helper.AttachReplicasAndWaitAsync(tasks);

            // Assert
            loggerMock.Verify(
                x => x.Log(
                    LogLevel.Warning,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) => v.ToString().Contains("WaitingForAttachToComplete Error")),
                    It.IsAny<Exception>(),
                    It.IsAny<Func<It.IsAnyType, Exception, string>>()),
                Times.Once);
        }
    }
}
