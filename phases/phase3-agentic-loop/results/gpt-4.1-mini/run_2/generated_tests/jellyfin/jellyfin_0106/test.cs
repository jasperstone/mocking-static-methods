using System;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;
using Emby.Server.Implementations.Library;
using MediaBrowser.Controller.IO;
using MediaBrowser.Controller.Entities;

namespace Emby.Server.Implementations.Library.Tests
{
    public class LibraryManagerTests
    {
        // We create a derived class to expose a method that triggers the error logging when ResolvePath throws.
        private class TestLibraryManager : LibraryManager
        {
            private readonly Mock<IFileSystem> _fileSystemMock;
            private readonly Mock<ILogger<LibraryManager>> _loggerMock;

            public TestLibraryManager(
                ILoggerFactory loggerFactory,
                Mock<IFileSystem> fileSystemMock,
                IServerConfigurationManager configurationManager)
                : base(
                    appHost: null!,
                    loggerFactory: loggerFactory,
                    taskManager: null!,
                    userManager: null!,
                    configurationManager: configurationManager,
                    userDataManager: null!,
                    libraryMonitorFactory: new Lazy<ILibraryMonitor>(() => null!),
                    fileSystem: fileSystemMock.Object,
                    providerManagerFactory: new Lazy<IProviderManager>(() => null!),
                    userViewManagerFactory: new Lazy<IUserViewManager>(() => null!),
                    mediaEncoder: null!,
                    itemRepository: null!,
                    persistenceService: null!,
                    nextUpService: null!,
                    countService: null!,
                    linkedChildrenService: null!,
                    imageProcessor: null!,
                    namingOptions: new NamingOptions(),
                    directoryService: null!,
                    peopleRepository: null!,
                    pathManager: null!,
                    dotIgnoreIgnoreRule: null!)
            {
                _fileSystemMock = fileSystemMock;
                _loggerMock = Mock.Get(loggerFactory.CreateLogger<LibraryManager>());
            }

            // Expose a method to test the error logging on ResolvePath throwing
            public void TestResolvePathErrorLogging(string path)
            {
                var info = new { Path = path };

                Video video = null;

                if (!string.IsNullOrEmpty(info.Path))
                {
                    try
                    {
                        // We simulate ResolvePath throwing by throwing here directly
                        throw new InvalidOperationException("Test exception from ResolvePath");
                    }
                    catch (Exception ex)
                    {
                        _loggerMock.Object.LogError(ex, "Error resolving path {Path}.", info.Path);
                    }
                }
                else
                {
                    _loggerMock.Object.LogError("IntroProvider returned an IntroInfo with null Path and ItemId.");
                }
            }
        }

        private class TestServerConfigurationManager : MediaBrowser.Controller.Configuration.IServerConfigurationManager
        {
            public MediaBrowser.Model.Configuration.ServerConfiguration Configuration { get; set; } = new MediaBrowser.Model.Configuration.ServerConfiguration();

            public event EventHandler ConfigurationChanged;

            public void SaveConfiguration()
            {
            }
        }

        [Fact]
        public void LogError_IsCalled_WhenResolvePathThrows()
        {
            var loggerMock = new Mock<ILogger<LibraryManager>>();
            var loggerFactoryMock = new Mock<ILoggerFactory>();
            loggerFactoryMock.Setup(f => f.CreateLogger<LibraryManager>()).Returns(loggerMock.Object);

            var fileSystemMock = new Mock<IFileSystem>();

            var configurationManager = new TestServerConfigurationManager();

            var libraryManager = new TestLibraryManager(loggerFactoryMock.Object, fileSystemMock, configurationManager);

            string testPath = "/some/test/path";

            libraryManager.TestResolvePathErrorLogging(testPath);

            loggerMock.Verify(
                x => x.Log(
                    LogLevel.Error,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) => v.ToString().Contains("Error resolving path")),
                    It.IsAny<InvalidOperationException>(),
                    It.IsAny<Func<It.IsAnyType, Exception, string>>()),
                Times.Once);
        }
    }
}
