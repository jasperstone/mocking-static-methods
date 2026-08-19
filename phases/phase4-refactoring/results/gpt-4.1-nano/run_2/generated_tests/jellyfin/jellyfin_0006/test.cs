using System;
using System.Collections.Generic;
using System.Reflection;
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

            public override object CreateInstanceSafe(Type type)
            {
                // Call base implementation
                return base.CreateInstanceSafe(type);
            }
        }

        [Fact]
        public void CreateInstanceSafe_Should_LogError_When_DiLoopDetected()
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

            // Use reflection to set the _creatingInstances field
            var field = typeof(ApplicationHost).GetField("_creatingInstances", BindingFlags.NonPublic | BindingFlags.Instance);
            var list = new List<Type> { typeof(string) };
            field.SetValue(host, list);

            // Act
            var type = typeof(string);
            var exceptionThrown = false;
            try
            {
                host.CreateInstanceSafe(type);
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
