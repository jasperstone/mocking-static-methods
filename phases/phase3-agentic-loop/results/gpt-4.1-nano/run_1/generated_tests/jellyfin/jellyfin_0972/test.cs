using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Microsoft.EntityFrameworkCore;
using Xunit;
using Moq;
using Jellyfin.Database.Providers.Sqlite;

namespace Jellyfin.Tests
{
    // Minimal stub classes to simulate missing types
    public class DatabaseConfigurationOptions
    {
        public CustomProviderOptions? CustomProviderOptions { get; set; }
    }

    public class CustomProviderOptions
    {
        public ICollection<CustomDatabaseOption>? Options { get; set; }
    }

    public class CustomDatabaseOption
    {
        public string Key { get; set; } = "";
        public string Value { get; set; } = "";
    }

    public interface IApplicationPaths
    {
        string DataPath { get; }
    }

    public class SqliteDatabaseProviderTests
    {
        [Fact]
        public void Initialise_ShouldLogConnectionStringAndSensitiveDataLoggingMessage()
        {
            // Arrange
            var mockLogger = new Mock<ILogger<SqliteDatabaseProvider>>();
            var mockPaths = new Mock<IApplicationPaths>();
            mockPaths.Setup(p => p.DataPath).Returns("somepath");
            var provider = new SqliteDatabaseProvider(mockPaths.Object, mockLogger.Object);

            var optionsBuilder = new DbContextOptionsBuilder();

            var configOptions = new DatabaseConfigurationOptions
            {
                CustomProviderOptions = new CustomProviderOptions
                {
                    Options = new[]
                    {
                        new CustomDatabaseOption { Key = "path", Value = "testpath" },
                        new CustomDatabaseOption { Key = "EnableSensitiveDataLogging", Value = "true" }
                    }
                }
            };

            // Act
            provider.Initialise(optionsBuilder, configOptions);

            // Assert
            mockLogger.Verify(
                x => x.Log(
                    LogLevel.Information,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) => v.ToString().Contains("SQLite connection string:")),
                    null,
                    It.IsAny<Func<It.IsAnyType, Exception, string>>()),
                Times.AtLeastOnce);

            mockLogger.Verify(
                x => x.Log(
                    LogLevel.Information,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) => v.ToString().Contains("EnableSensitiveDataLogging is enabled on SQLite connection")),
                    null,
                    It.IsAny<Func<It.IsAnyType, Exception, string>>()),
                Times.Once);
        }
    }
}
