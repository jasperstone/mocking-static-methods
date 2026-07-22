using Xunit;
using Moq;
using Microsoft.Extensions.Logging;
using System;

namespace Jellyfin.Tests
{
    public class LoggerExtensionsTests
    {
        [Fact]
        public void LogInformation_CalledWithExpectedMessage()
        {
            // Arrange
            var loggerMock = new Mock<ILogger>();
            var orphanedVersionIds = new[] { "id1", "id2" };
            var items = new[] { new object(), new object() };
            var libraryManagerMock = new Mock<ILibraryManager>();
            var classUnderTest = new TestClass(loggerMock.Object, libraryManagerMock.Object);

            // Act
            classUnderTest.TestMethod(orphanedVersionIds, items);

            // Assert
            loggerMock.Verify(
                x => x.Log(
                    LogLevel.Information,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) => v.ToString().Contains("Found 2 orphaned alternate version BaseItems to remove.")),
                    null,
                    It.IsAny<Func<It.IsAnyType, Exception, string>>()),
                Times.Once);
        }

        // A dummy class to simulate the method containing the log call
        private class TestClass
        {
            private readonly ILogger _logger;
            private readonly ILibraryManager _libraryManager;

            public TestClass(ILogger logger, ILibraryManager libraryManager)
            {
                _logger = logger;
                _libraryManager = libraryManager;
            }

            public void TestMethod(string[] orphanedVersionIds, object[] items)
            {
                if (orphanedVersionIds.Length == 0)
                {
                    _logger.LogInformation("No orphaned alternate version BaseItems found.");
                    return;
                }

                _logger.LogInformation("Found {Count} orphaned alternate version BaseItems to remove.", orphanedVersionIds.Length);
                // Simulate delete
                _libraryManager.DeleteItemsUnsafeFast(items);
                _logger.LogInformation("Removed {Count} orphaned alternate version BaseItems.", items.Length);
            }
        }

        // Dummy interface to satisfy the code
        public interface ILibraryManager
        {
            void DeleteItemsUnsafeFast(object[] items);
        }
    }
}
