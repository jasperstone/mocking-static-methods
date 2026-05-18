using Emby.Server.Implementations;
using Microsoft.Extensions.Logging;
using Moq;
using System;
using System.Reflection;
using System.Collections.Generic;
using MediaBrowser.Common.Plugins;
using MediaBrowser.Controller;
using MediaBrowser.Model.IO;
using Microsoft.Extensions.Configuration;
using Xunit;

namespace Emby.Server.Implementations.Tests
{
    public class ApplicationHostTests
    {
        [Fact]
        public void CreateInstanceSafe_DetectsDILOOP()
        {
            // Arrange
            var loggerFactory = new LoggerFactory();
            var logger = loggerFactory.CreateLogger<ApplicationHost>();
            var applicationPaths = new Mock<IServerApplicationPaths>();
            var startupOptions = new Mock<IStartupOptions>();
            var configuration = new Mock<IConfiguration>();
            var applicationHost = new TestApplicationHost(
                applicationPaths.Object,
                loggerFactory,
                startupOptions.Object,
                configuration.Object);

            applicationHost._creatingInstances = new List<Type>();
            applicationHost._creatingInstances.Add(typeof(string));

            // Act and Assert
            Assert.Throws<TypeLoadException>(() => applicationHost.CreateInstanceSafe(typeof(string)));
        }

        [Fact]
        public void CreateInstanceSafe_CreatesInstance()
        {
            // Arrange
            var loggerFactory = new LoggerFactory();
            var logger = loggerFactory.CreateLogger<ApplicationHost>();
            var applicationPaths = new Mock<IServerApplicationPaths>();
            var startupOptions = new Mock<IStartupOptions>();
            var configuration = new Mock<IConfiguration>();
            var applicationHost = new TestApplicationHost(
                applicationPaths.Object,
                loggerFactory,
                startupOptions.Object,
                configuration.Object);

            applicationHost._creatingInstances = new List<Type>();

            // Act
            var instance = applicationHost.CreateInstanceSafe(typeof(string));

            // Assert
            Assert.NotNull(instance);
        }

        [Fact]
        public void CreateInstanceSafe_LogsErrorOnException()
        {
            // Arrange
            var loggerFactory = new LoggerFactory();
            var logger = loggerFactory.CreateLogger<ApplicationHost>();
            var applicationPaths = new Mock<IServerApplicationPaths>();
            var startupOptions = new Mock<IStartupOptions>();
            var configuration = new Mock<IConfiguration>();
            var applicationHost = new TestApplicationHost(
                applicationPaths.Object,
                loggerFactory,
                startupOptions.Object,
                configuration.Object);

            applicationHost._creatingInstances = new List<Type>();

            // Act
            try
            {
                applicationHost.CreateInstanceSafe(typeof(InvalidType));
            }
            catch (Exception ex)
            {
                // Assert
                Assert.IsType<InvalidTypeException>(ex);
            }
        }
    }

    public class InvalidTypeException : Exception
    {
    }

    public class InvalidType
    {
        public InvalidType()
        {
            throw new InvalidTypeException("Invalid type");
        }
    }

    public class TestApplicationHost : ApplicationHost
    {
        public TestApplicationHost(
            IServerApplicationPaths applicationPaths,
            ILoggerFactory loggerFactory,
            IStartupOptions options,
            IConfiguration startupConfig)
            : base(applicationPaths, loggerFactory, options, startupConfig)
        {
        }

        public new object CreateInstanceSafe(Type type)
        {
            return base.CreateInstanceSafe(type);
        }

        public List<Type> _creatingInstances { get; set; }

        protected override Assembly[] GetAssembliesWithPartsInternal()
        {
            return new Assembly[0];
        }
    }
}
