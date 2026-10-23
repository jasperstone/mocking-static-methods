using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;
using Emby.Server.Implementations.Library.Validators;
using MediaBrowser.Controller.Library;
using MediaBrowser.Model.IO;
using MediaBrowser.Controller.Entities;
using MediaBrowser.Controller.Providers;

namespace Emby.Server.Implementations.Tests.Library.Validators
{
    public class PeopleValidatorTests
    {
        private class TestPerson : Person
        {
            public override Task RefreshMetadata(MetadataRefreshOptions options, CancellationToken cancellationToken)
            {
                return Task.CompletedTask;
            }
        }

        private class TestInternalPeopleQuery : InternalPeopleQuery { }

        [Fact]
        public async Task ValidatePeople_LogsWarning_WhenPersonIsNull()
        {
            // Arrange
            var people = new List<string> { "Person1", "Person2" };
            var mockLibraryManager = new Mock<ILibraryManager>();
            var mockLogger = new Mock<ILogger>();
            var mockFileSystem = new Mock<IFileSystem>();

            mockLibraryManager.Setup(m => m.GetPeopleNames(It.IsAny<InternalPeopleQuery>())).Returns(people);
            mockLibraryManager.Setup(m => m.GetPerson("Person1")).Returns((Person)null);
            var mockPersonItem = new TestPerson();
            mockLibraryManager.Setup(m => m.GetPerson("Person2")).Returns(mockPersonItem);

            var validator = new PeopleValidator(mockLibraryManager.Object, mockLogger.Object, mockFileSystem.Object);

            var progress = new Progress<double>();

            // Act
            await validator.ValidatePeople(CancellationToken.None, progress);

            // Assert
            mockLogger.Verify(
                x => x.Log(
                    LogLevel.Warning,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) => v.ToString().Contains("Failed to get person: Person1")),
                    null,
                    It.IsAny<Func<It.IsAnyType, Exception, string>>()),
                Times.Once);
        }
    }
}
