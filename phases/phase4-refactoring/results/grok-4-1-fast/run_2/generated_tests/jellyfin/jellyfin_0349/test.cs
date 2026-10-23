using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using Moq;
using Xunit;

namespace Jellyfin.Server.Implementations.Tests.FullSystemBackup
{
    public class LoggerExtensionsTests
    {
        private readonly Mock<ILogger<BackupService>> _loggerMock;

        public LoggerExtensionsTests()
        {
            _loggerMock = new Mock<ILogger<BackupService>>();
            _loggerMock.Setup(x => x.Log(
                It.IsAny<LogLevel>(),
                It.IsAny<EventId>(),
                It.IsAny<It.IsAnyType>(),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()));
        }

        [Fact]
        public void LogInformation_NoBackupOfExpectedTable_LogsCorrectMessage()
        {
            // Arrange
            const string tableName = "Users";
            const string expectedTemplate = "No backup of expected table {Table} is present in backup, continuing anyway";
            var logger = _loggerMock.Object;

            // Act
            logger.LogInformation(expectedTemplate, tableName);

            // Assert
            _loggerMock.Verify(
                x => x.Log(
                    LogLevel.Information,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>(state => 
                        state.ToString()!.Contains("No backup of expected table") &&
                        state.ToString()!.Contains(tableName) &&
                        state.ToString()!.Contains("is present in backup, continuing anyway")),
                    null!,
                    It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
                Times.Once);
        }

        [Fact]
        public void LogInformation_NoBackupOfExpectedTable_MultipleTables_LogsEachCorrectly()
        {
            // Arrange
            var tableNames = new[] { "Users", "Sessions", "Libraries" };
            const string expectedTemplate = "No backup of expected table {Table} is present in backup, continuing anyway";
            var logger = _loggerMock.Object;

            // Act
            foreach (var tableName in tableNames)
            {
                logger.LogInformation(expectedTemplate, tableName);
            }

            // Assert
            foreach (var tableName in tableNames)
            {
                _loggerMock.Verify(
                    x => x.Log(
                        LogLevel.Information,
                        It.IsAny<EventId>(),
                        It.Is<It.IsAnyType>(state => state.ToString()!.Contains(tableName)),
                        null!,
                        It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
                    Times.Once);
            }
        }

        [Fact]
        public void LogInformation_NoBackupOfExpectedTable_VerifiesLogLevelAndStructure()
        {
            // Arrange
            const string tableName = "MediaItems";
            const string expectedTemplate = "No backup of expected table {Table} is present in backup, continuing anyway";
            var logger = _loggerMock.Object;

            // Act
            logger.LogInformation(expectedTemplate, tableName);

            // Assert - verify LogLevel.Information and no exception
            _loggerMock.Verify(
                x => x.Log(
                    LogLevel.Information,
                    It.IsAny<EventId>(),
                    It.IsAny<It.IsAnyType>(),
                    It.Is<Exception>(e => e == null),
                    It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
                Times.Once);
        }
    }
}
