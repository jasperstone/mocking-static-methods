using Xunit;
using Moq;
using System;
using System.Collections.Generic;
using Microsoft.Extensions.Logging;
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

            public object CallCreateInstanceSafe(Type type)
            {
                return CreateInstanceSafe(type);
            }

            // Implement the abstract method with a dummy
            public override IEnumerable<Assembly> GetAssembliesWithPartsInternal()
            {
                return Array.Empty<Assembly>();
            }
        }

        [Fact]
        public void CreateInstanceSafe_DiLoopLogsError()
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

            // Access the private _creatingInstances list via reflection
            var field = typeof(ApplicationHost).GetField("_creatingInstances", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            var list = new List<Type> { typeof(string) }; // Add a type to simulate a DI loop
            field.SetValue(host, list);

            // Act
            var exception = Record.Exception(() => host.CallCreateInstanceSafe(typeof(string)));

            // Assert
            Assert.NotNull(exception);
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
