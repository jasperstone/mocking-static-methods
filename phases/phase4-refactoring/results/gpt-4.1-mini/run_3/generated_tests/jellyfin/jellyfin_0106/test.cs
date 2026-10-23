using System;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;
using Emby.Server.Implementations.Library;

namespace Emby.Server.Implementations.Library.Tests
{
    public class LibraryManagerLoggerTests
    {
        [Fact]
        public void LogsErrorWhenIntroInfoPathAndItemIdNull()
        {
            // Arrange
            var loggerMock = new Mock<ILogger<LibraryManager>>();
            var loggerFactoryMock = new Mock<ILoggerFactory>();
            loggerFactoryMock.Setup(f => f.CreateLogger<LibraryManager>()).Returns(loggerMock.Object);

            var libraryManager = CreateLibraryManager(loggerFactoryMock.Object);

            // Act
            var result = libraryManager.GetVideoFromIntroInfo(new IntroInfoStub { Path = null, ItemId = null });

            // Assert
            Assert.Null(result);
            loggerMock.Verify(
                x => x.Log(
                    LogLevel.Error,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) => v.ToString().Contains("IntroProvider returned an IntroInfo with null Path and ItemId.")),
                    null,
                    It.IsAny<Func<It.IsAnyType, Exception, string>>()),
                Times.Once);
        }

        private static LibraryManager CreateLibraryManager(ILoggerFactory loggerFactory)
        {
            // Pass null for other dependencies as they are not used in this test
            return new LibraryManager(
                appHost: null!,
                loggerFactory: loggerFactory,
                taskManager: null!,
                userManager: null!,
                configurationManager: null!,
                userDataManager: null!,
                libraryMonitorFactory: null!,
                fileSystem: null!,
                providerManagerFactory: null!,
                userViewManagerFactory: null!,
                mediaEncoder: null!,
                itemRepository: null!,
                persistenceService: null!,
                nextUpService: null!,
                countService: null!,
                linkedChildrenService: null!,
                imageProcessor: null!,
                namingOptions: null!,
                directoryService: null!,
                peopleRepository: null!,
                pathManager: null!,
                dotIgnoreIgnoreRule: null!);
        }

        private class IntroInfoStub
        {
            public string? Path { get; set; }
            public Guid? ItemId { get; set; }
        }
    }
}
