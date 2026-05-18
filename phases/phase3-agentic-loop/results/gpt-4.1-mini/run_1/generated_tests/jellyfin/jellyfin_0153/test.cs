using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Emby.Server.Implementations.Library.Validators;
using MediaBrowser.Controller.Entities;
using MediaBrowser.Controller.Library;
using MediaBrowser.Model.IO;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace Emby.Server.Implementations.Tests.Library.Validators
{
    public class PeopleValidatorTests
    {
        [Fact]
        public async Task ValidatePeople_LogsWarningWhenPersonIsNull()
        {
            // Arrange
            var people = new List<string> { "Person1", "Person2" };
            var mockLibraryManager = new Mock<ILibraryManager>();
            mockLibraryManager.Setup(m => m.GetPeopleNames(It.IsAny<InternalPeopleQuery>())).Returns(people);
            mockLibraryManager.Setup(m => m.GetPerson("Person1")).Returns(new Mock<Person>().Object);
            mockLibraryManager.Setup(m => m.GetPerson("Person2")).Returns((Person)null);

            // Setup GetItemList to return empty list to avoid null reference in Chunk call
            mockLibraryManager.Setup(m => m.GetItemList(It.IsAny<InternalItemsQuery>())).Returns(new List<BaseItem>());

            var mockLogger = new Mock<ILogger>();
            var mockFileSystem = new Mock<IFileSystem>();

            var validator = new PeopleValidator(mockLibraryManager.Object, mockLogger.Object, mockFileSystem.Object);

            var progress = new Progress<double>();

            // Act
            await validator.ValidatePeople(CancellationToken.None, progress);

            // Assert
            mockLogger.Verify(
                x => x.Log(
                    LogLevel.Warning,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) => v.ToString().Contains("Failed to get person: Person2")),
                    null,
                    It.IsAny<Func<It.IsAnyType, Exception, string>>()),
                Times.Once);
        }
    }
}
