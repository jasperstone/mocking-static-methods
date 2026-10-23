using System;
using System.Collections.Generic;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace Emby.Server.Implementations.Tests
{
    public class ApplicationHostTests
    {
        private class TestApplicationHost : ApplicationHost
        {
            public TestApplicationHost(
                IServerApplicationPaths applicationPaths,
                ILoggerFactory loggerFactory,
                IStartupOptions options,
                IConfiguration startupConfig)
                : base(applicationPaths, loggerFactory, options, startupConfig)
            {
            }

            public object CreateInstanceSafePublic(Type type)
            {
                return base.CreateInstanceSafe(type);
            }
        }

        [Fact]
        public void CreateInstanceSafe_DetectsDILoop_LogsError()
        {
            // Arrange
            var mockLogger = new Mock<ILogger<ApplicationHost>>();
            var mockLoggerFactory = new Mock<ILoggerFactory>();
            mockLoggerFactory.Setup(f => f.CreateLogger<ApplicationHost>()).Returns(mockLogger.Object);

            var mockApplicationPaths = new Mock<IServerApplicationPaths>();
            var mockOptions = new Mock<IStartupOptions>();
            var mockConfig = new Mock<IConfiguration>();

            var host = new TestApplicationHost(
                mockApplicationPaths.Object,
                mockLoggerFactory.Object,
                mockOptions.Object,
                mockConfig.Object);

            // Setup internal state
            var type = typeof(string);
            var allTypes = new List<Type> { type };
            var hostType = typeof(ApplicationHost);
            var fieldInfo = hostType.GetField("_creatingInstances", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            var creatingInstances = new List<Type> { type };
            fieldInfo.SetValue(host, creatingInstances);

            // Act
            // Call CreateInstanceSafe, which should detect the DI loop and log error
            var exception = Record.Exception(() => host.CreateInstanceSafePublic(type));

            // Assert
            Assert.IsType<TypeLoadException>(exception);
            mockLogger.Verify(
                x => x.Log(
                    LogLevel.Error,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) => v.ToString().Contains("DI Loop detected in the attempted creation of")),
                    null,
                    It.IsAny<Func<It.IsAnyType, Exception, string>>()),
                Times.Once);

            // Verify that the inner logs for each entry in _creatingInstances are called
            mockLogger.Verify(
                x => x.Log(
                    LogLevel.Error,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) => v.ToString().Contains("Called from:")),
                    null,
                    It.IsAny<Func<It.IsAnyType, Exception, string>>()),
                Times.AtLeastOnce);
        }
    }
}
