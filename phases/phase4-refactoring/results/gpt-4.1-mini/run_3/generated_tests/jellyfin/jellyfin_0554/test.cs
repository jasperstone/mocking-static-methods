using System;
using System.IO;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;
using Jellyfin.Server.Migrations.Routines;
using Emby.Server.Implementations.Data;
using Jellyfin.Database.Implementations;
using Jellyfin.Database.Implementations.Entities;
using MediaBrowser.Model.Serialization;
using System.Collections.Generic;

namespace Jellyfin.Server.Tests.Migrations.Routines
{
    public class MigrateUserDbTests
    {
        private class FakeServerApplicationPaths : IServerApplicationPaths
        {
            public string DataPath { get; set; } = "";
            public string UserConfigurationDirectoryPath { get; set; } = "";
        }

        private class FakeDbContext : JellyfinDbContext
        {
            public List<User> Users { get; } = new List<User>();
            public override void RemoveRange(IEnumerable<User> users) => Users.Clear();
            public override int SaveChanges() => 0;
        }

        private class FakeDbContextFactory : IDbContextFactory<JellyfinDbContext>
        {
            public JellyfinDbContext CreateDbContext() => new FakeDbContext();
        }

        private class FakeXmlSerializer : IXmlSerializer
        {
            public object? DeserializeFromFile(Type type, string filePath) => null;
            public object? DeserializeFromStream(Type type, Stream stream) => null;
            public void SerializeToStream(object obj, Stream stream) { }
            public void SerializeToFile(object obj, string filePath) { }
            public object? DeserializeFromBytes(Type type, byte[] bytes) => null;
        }

        [Fact]
        public void Perform_LogsWarning_WhenUserDbFileDoesNotExist()
        {
            // Arrange
            var loggerMock = new Mock<ILogger<MigrateUserDb>>();
            var paths = new FakeServerApplicationPaths
            {
                DataPath = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString())
            };
            Directory.CreateDirectory(paths.DataPath);

            var provider = new FakeDbContextFactory();
            var xmlSerializer = new FakeXmlSerializer();

            var migrateUserDb = new MigrateUserDb(
                loggerMock.Object,
                paths,
                provider,
                xmlSerializer);

            // Act
            migrateUserDb.Perform();

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
            Directory.Delete(paths.DataPath, true);
        }
    }
}
