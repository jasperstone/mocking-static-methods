using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Jellyfin.Data.Enums;
using MediaBrowser.Controller.Entities;
using MediaBrowser.Controller.Library;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace Emby.Server.Implementations.Library.Validators.Tests
{
    public class PeopleValidatorTests
    {
        [Fact]
        public async Task ValidatePeople_LogsWarning_WhenPersonIsNull()
        {
            // Arrange
            var mockLibraryManager = new Mock<ILibraryManager>();
            var mockLogger = new Mock<ILogger>();
            var mockFileSystem = new Mock<IFileSystem>();

            var people = new List<string> { "Person1", "Person2" };
            mockLibraryManager.Setup(m => m.GetPeopleNames(It.IsAny<InternalPeopleQuery>()))
                .Returns(people);

            mockLibraryManager.Setup(m => m.GetPerson("Person1"))
                .Returns((IPerson)null);

            mockLibraryManager.Setup(m => m.GetPerson("Person2"))
                .Returns(new Mock<IPerson>().Object);

            var validator = new PeopleValidator(mockLibraryManager.Object, mockLogger.Object, mockFileSystem.Object);

            var progress = new Mock<IProgress<double>>();
            var cancellationToken = new CancellationToken();

            // Act
            await validator.ValidatePeople(cancellationToken, progress.Object);

            // Assert
            mockLogger.Verify(
                logger => logger.LogWarning(
                    "Failed to get person: {Name}", "Person1"),
                Times.Once);
        }
    }
}
