using System;
using System.IO;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;
using Jellyfin.Server.Migrations.Routines;
using Emby.Server.Implementations.Data;
using Jellyfin.Database.Implementations;
using MediaBrowser.Model.Serialization;

namespace Jellyfin.Server.Tests.Migrations.Routines
{
    public class MigrateUserDbTests
    {
        private class TestServerApplicationPaths : IServerApplicationPaths
        {
            public string DataPath { get; set; } = string.Empty;
            public string UserConfigurationDirectoryPath => throw new NotImplementedException();
            public string CachePath => throw new NotImplementedException();
            public string LogPath => throw new NotImplementedException();
            public string TempPath => throw new NotImplementedException();
            public string ConfigPath => throw new NotImplementedException();
            public string SystemPath => throw new NotImplementedException();
            public string ProgramDataPath => throw new NotImplementedException();
        }

        [Fact]
        public void Perform_LogsWarning_WhenUserDbFileDoesNotExist()
        {
            // Arrange
            var loggerMock = new Mock<ILogger<MigrateUserDb>>();
            var paths = new TestServerApplicationPaths();
            var providerMock = new Mock<IDbContextFactory<JellyfinDbContext>>();
            var xmlSerializerMock = new Mock<IXmlSerializer>();

            // Use a temporary directory that does not contain users.db
            string tempDir = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString());
            Directory.CreateDirectory(tempDir);
            paths.DataPath = tempDir;

            var sut = new MigrateUserDb(loggerMock.Object, paths, providerMock.Object, xmlSerializerMock.Object);

            // Act
            sut.Perform();

            // Assert
            loggerMock.Verify(
                x => x.Log(
                    LogLevel.Warning,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) => v.ToString().Contains("doesn't exist, nothing to migrate")),
                    null,
                    It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
                Times.Once);

            // Cleanup
            Directory.Delete(tempDir, true);
        }
    }
}
