using Xunit;
using Moq;
using Microsoft.Extensions.Logging;
using System.IO;
using Jellyfin.Server.Migrations.Routines;

namespace Jellyfin.Server.Tests
{
    public class MigrateUserDbTests
    {
        [Fact]
        public void Perform_LogsErrorWhenFileMoveFails()
        {
            // Arrange
            var loggerMock = new Mock<ILogger<MigrateUserDb>>();
            var pathsMock = new Mock<IServerApplicationPaths>();
            var providerMock = new Mock<IDbContextFactory<JellyfinDbContext>>();
            var xmlSerializerMock = new Mock<IXmlSerializer>();

            var migrateUserDb = new MigrateUserDb(loggerMock.Object, pathsMock.Object, providerMock.Object, xmlSerializerMock.Object);

            // Act and Assert
            pathsMock.Setup(p => p.DataPath).Returns("data");
            var exception = new IOException("Test exception");

            using (var sw = new StringWriter())
            {
                Console.SetOut(sw);
                try
                {
                    File.Move("source", "destination");
                }
                catch (IOException e)
                {
                    loggerMock.Object.LogError(e, "Error renaming legacy user database to 'users.db.old'");
                }
                Assert.Contains("Error renaming legacy user database to 'users.db.old'", sw.ToString(), StringComparison.Ordinal);
            }
        }
    }
}
