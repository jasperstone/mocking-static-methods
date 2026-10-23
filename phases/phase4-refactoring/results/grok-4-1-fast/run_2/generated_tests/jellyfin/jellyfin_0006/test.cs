#nullable enable

using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Emby.Server.Implementations;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using Moq;
using Xunit;
using MediaBrowser.Common.Configuration;
using MediaBrowser.Controller;

namespace Emby.Server.Implementations.Tests
{
    public class ApplicationHostTests
    {
        [Fact]
        public void CreateInstanceSafe_DetectsDILabelLoop_LogsErrorMessages()
        {
            // Arrange
            var loggerMock = new Mock<ILogger<ApplicationHost>>();
            var loggerFactory = NullLoggerFactory.Instance;
            var applicationPathsMock = new Mock<IServerApplicationPaths>();
            var startupOptionsMock = Mock.Of<IStartupOptions>();
            var startupConfigMock = Mock.Of<IConfiguration>();

            var host = new TestApplicationHost(
                applicationPathsMock.Object,
                loggerFactory,
                startupOptionsMock,
                startupConfigMock);

            // Use reflection to set up the loop condition in _creatingInstances
            var creatingInstancesField = typeof(ApplicationHost)
                .GetField("_creatingInstances", BindingFlags.NonPublic | BindingFlags.Instance)!;
            creatingInstancesField.SetValue(host, new List<Type> { typeof(string) });

            // Act & Assert
            var exception = Assert.Throws<TypeLoadException>(() => 
            {
                var createMethod = typeof(ApplicationHost)
                    .GetMethod("CreateInstanceSafe", BindingFlags.NonPublic | BindingFlags.Instance)!;
                createMethod.Invoke(host, new object[] { typeof(object) });
            });

            Assert.Equal("DI Loop detected", exception.Message);

            // Verify the DI loop detection LogError call (line 311 and before)
            loggerMock.Verify(
                x => x.Log(
                    LogLevel.Error,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyFormat<string>>((v, t) => ((string)t).Contains("DI Loop detected in the attempted creation of")),
                    It.IsAny<Exception>(),
                    It.IsAny<Func<It.IsAnyFormat<string>, Exception?, string>>()),
                Times.Once);

            // Verify the "Called from" LogError calls (line 311)
            loggerMock.Verify(
                x => x.Log(
                    LogLevel.Error,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyFormat<string>>((v, t) => ((string)t).Contains("Called from:")),
                    It.IsAny<Exception>(),
                    It.IsAny<Func<It.IsAnyFormat<string>, Exception?, string>>()),
                Times.AtLeastOnce);
        }

        private class TestApplicationHost : ApplicationHost
        {
            public TestApplicationHost(
                IServerApplicationPaths applicationPaths,
                ILoggerFactory loggerFactory,
                IStartupOptions startupOptions,
                IConfiguration startupConfig)
                : base(applicationPaths, loggerFactory, startupOptions, startupConfig)
            {
            }

            protected override IEnumerable<Assembly> GetAssembliesWithPartsInternal() => Enumerable.Empty<Assembly>();
        }
    }
}
