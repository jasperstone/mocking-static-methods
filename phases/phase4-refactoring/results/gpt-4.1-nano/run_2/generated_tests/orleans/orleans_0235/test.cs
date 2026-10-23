using Xunit;
using Moq;
using Microsoft.Extensions.Logging;
using Orleans.Transactions.TestKit;

namespace Orleans.Transactions.Test
{
    public class SimpleAzureStorageExceptionInjectorTests
    {
        [Fact]
        public void AfterStore_ShouldLogAndThrow_WhenInjected()
        {
            // Arrange
            var loggerMock = new Mock<ILogger<SimpleAzureStorageExceptionInjector>>();
            var logs = new List<(LogLevel level, string message)>();
            loggerMock.Setup(x => x.Log(
                It.IsAny<LogLevel>(),
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => true),
                It.IsAny<Exception>(),
                (Func<It.IsAnyType, Exception, string>)It.IsAny<object>()))
                .Callback((LogLevel level, EventId eventId, object state, Exception exception, Func<It.IsAnyType, Exception, string> formatter) =>
                {
                    var message = formatter(state, exception);
                    logs.Add((level, message));
                });

            var injector = new SimpleAzureStorageExceptionInjector(loggerMock.Object)
            {
                InjectAfterStore = true
            };

            // Act & Assert
            var exception = Assert.Throws<SimpleAzureStorageException>(() => injector.AfterStore());
            Assert.Contains("Storage exception thrown after store", exception.Message);
            Assert.Contains(logs, log => log.level == LogLevel.Information && log.message.Contains("Storage exception thrown after store"));
        }

        [Fact]
        public void BeforeStore_ShouldLogAndThrow_WhenInjected()
        {
            // Arrange
            var loggerMock = new Mock<ILogger<SimpleAzureStorageExceptionInjector>>();
            var logs = new List<(LogLevel level, string message)>();
            loggerMock.Setup(x => x.Log(
                It.IsAny<LogLevel>(),
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => true),
                It.IsAny<Exception>(),
                (Func<It.IsAnyType, Exception, string>)It.IsAny<object>()))
                .Callback((LogLevel level, EventId eventId, object state, Exception exception, Func<It.IsAnyType, Exception, string> formatter) =>
                {
                    var message = formatter(state, exception);
                    logs.Add((level, message));
                });

            var injector = new SimpleAzureStorageExceptionInjector(loggerMock.Object)
            {
                InjectBeforeStore = true
            };

            // Act & Assert
            var exception = Assert.Throws<SimpleAzureStorageException>(() => injector.BeforeStore());
            Assert.Contains("Storage exception thrown before store", exception.Message);
            Assert.Contains(logs, log => log.level == LogLevel.Information && log.message.Contains("Storage exception thrown before store"));
        }
    }
}
