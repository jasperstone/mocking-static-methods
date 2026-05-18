using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Microsoft.Extensions.Logging;
using Moq;
using Moq.Language.Flow;
using Xunit;
using MediaBrowser.Common.Configuration;
using MediaBrowser.Controller;
using MediaBrowser.Model.Configuration;
using MediaBrowser.Model.IO;
using Emby.Server.Implementations.Plugins;

namespace Emby.Server.Implementations.Tests
{
    public class ApplicationHostTests
    {
        [Fact]
        public void CreateInstanceSafe_DetectsDILoop_LogsErrorAndCalledFromTypes()
        {
            // Arrange
            var loggerMock = new Mock<ILogger<ApplicationHost>>();
            var capturedMessages = new List<string>();
            
            loggerMock.Setup(x => x.Log(
                    It.Is<LogLevel>(l => l == LogLevel.Error),
                    It.IsAny<EventId>(),
                    It.IsAny<It.IsAnyType>(),
                    It.IsAny<Exception>(),
                    It.IsAny<Func<It.IsAnyType, Exception?, string>>()))
                .Callback<LogLevel, EventId, object, Exception, Func<It.IsAnyType, Exception?, string>>(
                    (level, eventId, state, ex, formatter) => capturedMessages.Add(formatter(state, ex)));

            var appPathsMock = new Mock<IServerApplicationPaths>();
            var startupOptionsMock = new Mock<IStartupOptions>();
            var configMock = new Mock<Microsoft.Extensions.Configuration.IConfiguration>();
            
            var appHost = new TestApplicationHost(
                appPathsMock.Object,
                new NullLoggerFactory(),
                startupOptionsMock.Object,
                configMock.Object,
                loggerMock.Object);

            // Set up the loop condition
            appHost.SetCreatingInstances(new List<Type> { typeof(string) });

            // Act
            var ex = Assert.Throws<TypeLoadException>(() => appHost.CreateInstanceSafe(typeof(int)));

            // Assert
            Assert.Equal("DI Loop detected", ex.Message);
            Assert.Contains("DI Loop detected in the attempted creation of int", capturedMessages);
            Assert.Contains("Called from: String", capturedMessages);
        }
    }

    public class TestApplicationHost : ApplicationHost
    {
        private List<Type> _creatingInstances = new List<Type>();

        public TestApplicationHost(
            IServerApplicationPaths applicationPaths,
            ILoggerFactory loggerFactory,
            IStartupOptions options,
            Microsoft.Extensions.Configuration.IConfiguration startupConfig,
            ILogger<ApplicationHost> logger) : base(applicationPaths, loggerFactory, options, startupConfig)
        {
            Logger = logger;
        }

        public void SetCreatingInstances(List<Type> instances)
        {
            _creatingInstances = instances;
        }

        protected new List<Type> _creatingInstances => base._creatingInstances ??= new List<Type>();

        protected override PluginManager CreatePluginManager()
        {
            return new TestPluginManager(LoggerFactory.CreateLogger<PluginManager>());
        }

        // Minimal implementations to satisfy abstract members
        public override IEnumerable<Assembly> GetAssembliesWithPartsInternal() => Enumerable.Empty<Assembly>();
        
        // Return minimal implementations for required properties
        public override string Name => "TestHost";
        
        public override bool HasPendingRestart => false;
        public override bool ShouldRestart { get; set; }
        public override bool CoreStartupHasCompleted { get; protected set; }
        
        public override IServerApplicationPaths ApplicationPaths => throw new NotImplementedException();
        public override IServiceProvider ResolveServiceProvider() => null;
    }

    public class TestPluginManager : PluginManager
    {
        public TestPluginManager(ILogger<PluginManager> logger) 
            : base(logger, new Mock<IServerApplicationHost>().Object, new ServerConfiguration(), "testpath", new Version(1, 0))
        {
        }

        public new void FailPlugin(Assembly assembly)
        {
            // No-op for test
        }
    }
}
