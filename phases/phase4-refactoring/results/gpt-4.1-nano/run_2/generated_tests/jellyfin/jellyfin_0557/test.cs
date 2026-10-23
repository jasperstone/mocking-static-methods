using Xunit;
using Moq;
using Microsoft.Extensions.Logging;
using System.IO;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using System.Linq;
using System.Text.Json;
using System.Xml.Serialization;
using System.Reflection;
using System.IO.Abstractions;
using System.Threading;
using System.Collections;
using System.Collections.ObjectModel;

namespace Jellyfin.Tests
{
    public class MigrateUserDbTests
    {
        [Fact]
        public void LogError_IsCalled_WhenIOExceptionOccursDuringFileRename()
        {
            // Arrange
            var loggerMock = new Mock<ILogger<MigrateUserDb>>();
            var pathsMock = new Mock<IServerApplicationPaths>();
            var providerMock = new Mock<IDbContextFactory<JellyfinDbContext>>();
            var xmlSerializerMock = new Mock<IXmlSerializer>();
            var fileSystemMock = new Mock<IFileSystem>();

            var dataPath = "data";
            var userDbPath = Path.Combine(dataPath, "users.db");
            var oldFilePath = Path.Combine(dataPath, "users.db.old");
            var journalPath = Path.Combine(dataPath, "users.db-journal");
            var journalOldPath = Path.Combine(dataPath, "users.db-old-journal");

            pathsMock.Setup(p => p.DataPath).Returns(dataPath);
            var fileMock = new Mock<IFile>();
            var fileCollection = new List<IFile> { new Mock<IFile>().Object };
            var fileQuery = new List<IFile> { new Mock<IFile>().Object };
            // Simulate File.Exists returning true for the main db, false for journal
            var fileExistsDict = new Dictionary<string, bool>
            {
                { userDbPath, true },
                { journalPath, true }
            };
            // Setup File.Exists to return true for main db and journal
            fileSystemMock.Setup(fs => fs.File.Exists(It.IsAny<string>())).Returns<string>(path => fileExistsDict.ContainsKey(path) ? fileExistsDict[path] : false);
            // Setup File.Move to throw IOException when moving the main db
            fileSystemMock.Setup(fs => fs.File.Move(It.IsAny<string>(), It.IsAny<string>())).Callback<string, string>((src, dest) =>
            {
                if (src == userDbPath)
                {
                    throw new IOException("Simulated IO exception");
                }
            });

            // Create an instance of the class under test with dependencies
            var migration = new MigrateUserDb(loggerMock.Object, pathsMock.Object, providerMock.Object, xmlSerializerMock.Object);
            // Use reflection to set the private _logger field to our mock
            var field = typeof(MigrateUserDb).GetField("_logger", BindingFlags.NonPublic | BindingFlags.Instance);
            field.SetValue(migration, loggerMock.Object);

            // Act
            // Call Perform, which should trigger the IOException and log an error
            migration.Perform();

            // Assert
            // Verify that LogError was called
            loggerMock.Verify(
                x => x.Log(
                    LogLevel.Error,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) => v.ToString().Contains("Error renaming legacy user database to 'users.db.old'")),
                    It.IsAny<Exception>(),
                    It.IsAny<Func<It.IsAnyType, Exception, string>>()),
                Times.Once);
        }
    }
}
