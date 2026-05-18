using System;
using System.Collections.Generic;
using Microsoft.Extensions.Logging;
using Xunit;
using Moq;

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
        public void CreateInstanceSafe_DiLoopDetected_LogsError()
        {
            // Arrange
            var mockApplicationPaths = new Mock<IServerApplicationPaths>();
            var mockLoggerFactory = new Mock<ILoggerFactory>();
            var mockLogger = new Mock<ILogger<ApplicationHost>>();
            var mockConfig = new Mock<IConfiguration>();
            var mockOptions = new Mock<IStartupOptions>();

            mockLoggerFactory.Setup(f => f.CreateLogger<ApplicationHost>()).Returns(mockLogger.Object);

            var host = new TestApplicationHost(
                mockApplicationPaths.Object,
                mockLoggerFactory.Object,
                mockOptions.Object,
                mockConfig.Object);

            // Access protected method via public wrapper
            var type = typeof(string);
            host.InvokeCreateInstanceSafe(type);

            // Manually set _creatingInstances to simulate DI loop
            var field = typeof(ApplicationHost).GetField("_creatingInstances", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            var list = new List<Type> { type };
            field.SetValue(host, list);

            // Act
            var exceptionThrown = false;
            try
            {
                host.InvokeCreateInstanceSafe(type);
            }
            catch (TypeLoadException)
            {
                exceptionThrown = true;
            }

            // Assert
            mockLogger.Verify(
                x => x.Log(
                    LogLevel.Error,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) => v.ToString().Contains("DI Loop detected in the attempted creation of")),
                    null,
                    It.IsAny<Func<It.IsAnyType, Exception, string>>()),
                Times.AtLeastOnce);
            Assert.True(exceptionThrown);
        }
    }
}
