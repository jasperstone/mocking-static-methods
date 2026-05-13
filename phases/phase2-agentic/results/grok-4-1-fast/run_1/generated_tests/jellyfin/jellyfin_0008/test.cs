using System;
using System.Collections.Generic;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace Emby.Server.Implementations.Tests
{
    public class ApplicationHostTests
    {
        [Fact]
        public void CreateInstanceSafe_ThrowsException_LogsErrorWithExceptionAndType()
        {
            // Arrange
            var loggerMock = new Mock<ILogger<ApplicationHost>>();
            var applicationHost = new TestApplicationHost(loggerMock.Object);
            var testType = typeof(string);

            // Act
            var result = applicationHost.CreateInstanceSafe(testType);

            // Assert
            Assert.Null(result);
            loggerMock.Verify(
                x => x.Log(
                    LogLevel.Error,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyFormat<string>>((v, t) => true),
                    It.Is<Exception>(ex => ex is Exception),
                    It.IsAny<Func<It.IsAnyFormat<string>, Exception?, string>>()),
                Times.Once);
        }

        [Fact]
        public void CreateInstanceSafe_DetectsCircularDependency_LogsErrorMessages()
        {
            // Arrange
            var loggerMock = new Mock<ILogger<ApplicationHost>>();
            var applicationHost = new TestApplicationHost(loggerMock.Object);
            var testType = typeof(TestCircularType);

            // Act
            Assert.Throws<TypeLoadException>(() => applicationHost.CreateInstanceSafe(testType));

            // Assert - First error log for DI loop detection
            loggerMock.Verify(
                x => x.Log(
                    LogLevel.Error,
                    It.IsAny<EventId>(),
                    It.Is<string>(msg => msg.Contains("DI Loop detected in the attempted creation of TestCircularType")),
                    It.IsAny<Exception>(),
                    It.IsAny<Func<It.IsAnyFormat<string>, Exception?, string>>()),
                Times.Once);

            // Assert - Second error log for "Called from"
            loggerMock.Verify(
                x => x.Log(
                    LogLevel.Error,
                    It.IsAny<EventId>(),
                    It.Is<string>(msg => msg.Contains("Called from: TestCircularType")),
                    It.IsAny<Exception>(),
                    It.IsAny<Func<It.IsAnyFormat<string>, Exception?, string>>()),
                Times.Once);
        }

        private class TestApplicationHost : ApplicationHost
        {
            public TestApplicationHost(Mock<ILogger<ApplicationHost>> loggerMock) : base(
                new Mock<IServerApplicationPaths>().Object,
                new Mock<ILoggerFactory>().Object,
                new Mock<IStartupOptions>().Object,
                new Mock<IConfiguration>().Object)
            {
                // Use reflection or protected access to set Logger for testing
                typeof(ApplicationHost).GetField("_logger", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)?.SetValue(this, loggerMock.Object);
                
                // Mock plugin manager that does nothing
                _pluginManager = new Mock<IPluginManager>().Object;
            }

            private IPluginManager _pluginManager;
        }

        private class TestCircularType { }
    }
}
