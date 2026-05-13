using System;
using System.IO;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace Jellyfin.Server.Migrations.Routines.Tests
{
    public class MigrateUserDbTests
    {
        [Fact]
        public void LogError_IsCalled_WhenIOExceptionOccurs()
        {
            // Arrange
            var loggerMock = new Mock<ILogger<MigrateUserDb>>();
            var pathsMock = new Mock<IServerApplicationPaths>();
            var providerMock = new Mock<IDbContextFactory<JellyfinDbContext>>();
            var xmlSerializerMock = new Mock<IXmlSerializer>();

            pathsMock.Setup(p => p.DataPath).Returns("testDataPath");

            var migrateUserDb = new MigrateUserDb(loggerMock.Object, pathsMock.Object, providerMock.Object, xmlSerializerMock.Object);

            // Simulate IOException
            var exception = new IOException("Test exception");
            System.IO.File.SetAttributes("testDataPath/users.db", FileAttributes.Normal); // Ensure file exists for the test

            // Act
            try
            {
                migrateUserDb.Perform();
            }
            catch (IOException)
            {
                // Expected exception
            }

            // Assert
            loggerMock.Verify(
                l => l.LogError(
                    It.IsAny<ILoggerProvider>(),
                    It.IsAny<EventId>(),
                    It.Is<Exception>(e => e == exception),
                    It.Is<string>(s => s == "Error renaming legacy user database to 'users.db.old'"),
                    It.IsAny<object[]>()
                ),
                Times.Once
            );
        }
    }
}
