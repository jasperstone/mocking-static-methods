using System;
using System.Collections.Generic;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;
using MediaBrowser.Common.Configuration;
using MediaBrowser.Controller.Plugins;
using Emby.Server.Implementations;

namespace Emby.Server.Implementations.Tests
{
    public class ApplicationHostTests
    {
        [Fact]
        public void CreateInstanceSafe_DetectsDILabelLoop_LogsErrorMessages()
        {
            // Arrange
            var loggerMock = new Mock<ILogger<ApplicationHost>>();
            var loggerFactoryMock = new Mock<ILoggerFactory>();
            loggerFactoryMock.Setup(f => f.CreateLogger<ApplicationHost>()).Returns(loggerMock.Object);

            var appPathsMock = new Mock<IServerApplicationPaths>();
            var startupOptionsMock = new Mock<IStartupOptions>();
            var startupConfigMock = new Mock<IConfiguration>();

            var host = new TestApplicationHost(
                appPathsMock.Object,
                loggerFactoryMock.Object,
                startupOptionsMock.Object,
                startupConfigMock.Object);

            var testType = typeof(string);
            host._creatingInstances.Add(testType);

            // Act & Assert
            var exception = Assert.Throws<TypeLoadException>(() => host.CreateInstanceSafe(testType));
            Assert.Equal("DI Loop detected", exception.Message);

            loggerMock.Verify(
                l => l.Log(
                    LogLevel.Error,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyFormat<string>>((v, t) => t.ToString().Contains("DI Loop detected in the attempted creation of {Type}") && t.ToString().Contains(testType.FullName)),
                    null,
                    It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
                Times.Once);

            loggerMock.Verify(
                l => l.Log(
                    LogLevel.Error,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyFormat<string>>((v, t) => t.ToString().Contains("Called from: {TypeName}") && t.ToString().Contains(testType.FullName)),
                    null,
                    It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
                Times.Once);
        }

        private class TestApplicationHost : ApplicationHost
        {
            public List<Type> _creatingInstances = new();

            public TestApplicationHost(
                IServerApplicationPaths applicationPaths,
                ILoggerFactory loggerFactory,
                IStartupOptions options,
                IConfiguration startupConfig)
                : base(applicationPaths, loggerFactory, options, startupConfig)
            {
            }

            protected override IEnumerable<Assembly> GetAssembliesWithPartsInternal()
            {
                yield break;
            }
        }
    }
}
