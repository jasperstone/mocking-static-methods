using Emby.Server.Implementations;
using Microsoft.Extensions.Logging;
using Moq;
using System;
using System.Reflection;
using System.Collections.Generic;
using System.Linq;
using Xunit;

namespace Emby.Server.Implementations.Tests
{
    public class ApplicationHostTests
    {
        [Fact]
        public void CreateInstanceSafe_LogsErrorOnException()
        {
            // Arrange
            var loggerFactory = new LoggerFactory();
            var logger = loggerFactory.CreateLogger<ApplicationHost>();
            var applicationHost = new TestApplicationHost(loggerFactory, logger);

            var type = typeof(string);
            var exception = new Exception("Test exception");

            // Act and Assert
            Assert.Throws<TypeLoadException>(() => applicationHost.CreateInstanceSafe(type));

            // Verify log error was called
            var loggerMock = new Mock<ILogger<ApplicationHost>>();
            applicationHost.Logger = loggerMock.Object;
            try
            {
                applicationHost.CreateInstanceSafe(type);
            }
            catch (TypeLoadException)
            {
                loggerMock.Verify(l => l.LogError(It.IsAny<Exception>(), "Error creating {Type}", type), Times.Once);
            }
        }

        [Fact]
        public void CreateInstanceSafe_LogsErrorOnDILOOP()
        {
            // Arrange
            var loggerFactory = new LoggerFactory();
            var logger = loggerFactory.CreateLogger<ApplicationHost>();
            var applicationHost = new TestApplicationHost(loggerFactory, logger);

            var type = typeof(string);
            applicationHost._creatingInstances = new List<Type> { type };

            // Act and Assert
            Assert.Throws<TypeLoadException>(() => applicationHost.CreateInstanceSafe(type));

            // Verify log error was called
            var loggerMock = new Mock<ILogger<ApplicationHost>>();
            applicationHost.Logger = loggerMock.Object;
            try
            {
                applicationHost.CreateInstanceSafe(type);
            }
            catch (TypeLoadException)
            {
                loggerMock.Verify(l => l.LogError("DI Loop detected in the attempted creation of {Type}", type.FullName), Times.Once);
            }
        }

        private class TestApplicationHost : ApplicationHost
        {
            public TestApplicationHost(ILoggerFactory loggerFactory, ILogger<ApplicationHost> logger) 
                : base(new Mock<IServerApplicationPaths>().Object, loggerFactory, new Mock<IStartupOptions>().Object, new Mock<IConfiguration>().Object)
            {
                Logger = logger;
            }

            public new ILogger<ApplicationHost> Logger
            {
                get => base.Logger;
                set => base.Logger = value;
            }

            protected override Assembly[] GetAssembliesWithPartsInternal()
            {
                return new Assembly[0];
            }
        }
    }
}
