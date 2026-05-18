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

            public object InvokeCreateInstanceSafe(Type type)
            {
                return CreateInstanceSafe(type);
            }
        }

        [Fact]
        public void CreateInstanceSafe_DetectsLoop_LogsError()
        {
            // Arrange
            var mockApplicationPaths = new Mock<IServerApplicationPaths>();
            var mockLoggerFactory = new Mock<ILoggerFactory>();
            var mockLogger = new Mock<ILogger<ApplicationHost>>();
            mockLoggerFactory.Setup(f => f.CreateLogger<ApplicationHost>()).Returns(mockLogger.Object);
            var mockOptions = new Mock<IStartupOptions>();
            var mockConfig = new Mock<IConfiguration>();

            var host = new TestApplicationHost(
                mockApplicationPaths.Object,
                mockLoggerFactory.Object,
                mockOptions.Object,
                mockConfig.Object);

            // Access the protected method via the wrapper
            var type = typeof(object); // dummy type
            // Initialize _creatingInstances with the type to simulate a loop
            var field = typeof(ApplicationHost).GetField("_creatingInstances", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            var list = new List<Type> { type };
            field.SetValue(host, list);

            // Act
            var exception = Record.Exception(() => host.InvokeCreateInstanceSafe(type));

            // Assert
            Assert.IsType<TypeLoadException>(exception);
            mockLogger.Verify(
                x => x.Log(
                    LogLevel.Error,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) => v.ToString().Contains("DI Loop detected in the attempted creation of")),
                    null,
                    It.IsAny<Func<It.IsAnyType, Exception, string>>()),
                Times.AtLeastOnce);
        }
    }
}
