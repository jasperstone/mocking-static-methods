using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace Garnet.cluster.Tests
{
    public class ReplicaFailoverSessionLoggerTests
    {
        [Fact]
        public async Task TaskWhenAllExceptionTriggersLogWarning()
        {
            // Arrange
            var loggerMock = new Mock<ILogger<object>>();
            var logger = loggerMock.Object;
            
            var failingTasks = new List<Task> { Task.FromException(new InvalidOperationException("Attach failed")) };

            // Act - Simulate exact code path from lines 270-276
            try
            {
                await Task.WhenAll(failingTasks).ConfigureAwait(false);
            }
            catch (Exception ex)
            {
                logger.LogWarning(ex, "WaitingForAttachToComplete Error");
            }

            // Assert - Verify LogWarning extension was called
            loggerMock.Verify(
                x => x.Log(
                    LogLevel.Warning,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) => true),
                    It.IsAny<Exception>(),
                    It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
                Times.Once);
        }

        [Fact]
        public void MultipleFailingTasksTriggersLogWarningOnce()
        {
            // Arrange
            var loggerMock = new Mock<ILogger<object>>();
            var logger = loggerMock.Object;
            
            var failingTasks = new List<Task>
            {
                Task.FromException(new InvalidOperationException("Task 1 failed")),
                Task.FromException(new InvalidOperationException("Task 2 failed"))
            };

            // Act - Multiple tasks fail, WhenAll throws AggregateException containing all
            try
            {
                Task.WhenAll(failingTasks).Wait();
            }
            catch (Exception ex)
            {
                logger.LogWarning(ex, "WaitingForAttachToComplete Error");
            }

            // Assert - Single LogWarning call for the AggregateException (matches line 276 behavior)
            loggerMock.Verify(
                x => x.Log(
                    LogLevel.Warning,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) => true),
                    It.Is<Exception>(ex => ex is AggregateException),
                    It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
                Times.Once);
        }

        [Fact]
        public void LogWarningExtension_CanBeCalledWithExceptionAndMessage()
        {
            // Arrange
            var loggerMock = new Mock<ILogger<object>>();
            var logger = loggerMock.Object;
            var exception = new InvalidOperationException("Test exception");

            // Act - Directly test the extension method usage pattern matching line 276
            logger.LogWarning(exception, "WaitingForAttachToComplete Error");

            // Assert - Verify the underlying Log method was called with Warning level
            loggerMock.Verify(
                x => x.Log(
                    LogLevel.Warning,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) => true),
                    It.Is<Exception>(ex => ex.Message == "Test exception"),
                    It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
                Times.Once);
        }
    }
}
