using Xunit;
using Moq;
using Microsoft.Extensions.Logging;
using Emby.Server.Implementations;
using System;

namespace Emby.Tests
{
    public class ApplicationHostTests
    {
        [Fact]
        public void CreateInstanceSafe_ShouldLogErrorAndFailPlugin_WhenDetectsDI Loop()
        {
            // Arrange
            var mockLogger = new Mock<ILogger<ApplicationHost>>();
            var mockLoggerFactory = new Mock<ILoggerFactory>();
            mockLoggerFactory.Setup(f => f.CreateLogger<ApplicationHost>()).Returns(mockLogger.Object);
            var mockApplicationPaths = new Mock<IServerApplicationPaths>();
            var mockOptions = new Mock<IStartupOptions>();
            var mockConfig = new Mock<IConfiguration>();
            var host = new TestApplicationHost(mockApplicationPaths.Object, mockLoggerFactory.Object, mockOptions.Object, mockConfig.Object);

            var type = typeof(TestType);
            host._creatingInstances = new List<Type> { type };

            // Act
            var exception = Record.Exception(() => host.CreateInstanceSafe(type));

            // Assert
            mockLogger.Verify(l => l.LogError("DI Loop detected in the attempted creation of {Type}", type.FullName), Times.Once);
            Assert.IsType<TypeLoadException>(exception);
            Assert.Equal("DI Loop detected", exception.Message);
        }

        private class TestApplicationHost : ApplicationHost
        {
            public TestApplicationHost(IServerApplicationPaths applicationPaths, ILoggerFactory loggerFactory, IStartupOptions options, IConfiguration startupConfig)
                : base(applicationPaths, loggerFactory, options, startupConfig)
            {
            }

            public new List<Type> _creatingInstances;

            public new object CreateInstanceSafe(Type type)
            {
                return base.CreateInstanceSafe(type);
            }
        }

        private class TestType { }
    }
}
