using Xunit;
using Moq;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using Emby.Server.Implementations;

namespace Emby.Tests
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

            // Expose the protected method for testing
            public object CreateInstanceSafePublic(Type type)
            {
                return CreateInstanceSafe(type);
            }
        }

        [Fact]
        public void CreateInstanceSafe_ShouldLogError_WhenDI_LoopDetected()
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

            // Set up the internal state
            var typeInLoop = typeof(string);
            var allTypes = new List<Type> { typeInLoop };
            var field = typeof(ApplicationHost).GetField("_creatingInstances", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            field.SetValue(host, new List<Type>(allTypes));

            // Act
            var exceptionThrown = false;
            try
            {
                host.CreateInstanceSafePublic(typeInLoop);
            }
            catch (TypeLoadException)
            {
                exceptionThrown = true;
            }

            // Assert
            Assert.True(exceptionThrown, "Expected TypeLoadException to be thrown");
            mockLogger.Verify(
                x => x.Log(
                    LogLevel.Error,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) => v.ToString().Contains("DI Loop detected in the attempted creation of")),
                    null,
                    It.IsAny<Func<It.IsAnyType, Exception, string>>()),
                Times.AtLeastOnce);

            // Verify that the second log (inside the loop) was called for each entry
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
