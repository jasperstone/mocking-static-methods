using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.Data.Sqlite;
using Jellyfin.Database.Providers.Sqlite;

namespace Jellyfin.Tests
{
    public class SqliteDatabaseProviderTests
    {
        [Fact]
        public void Initialise_Should_Log_EnableSensitiveDataLogging_Message_When_Enabled()
        {
            // Arrange
            var mockLogger = new Mock<ILogger<SqliteDatabaseProvider>>();
            var mockPaths = new Mock<IApplicationPaths>();
            mockPaths.Setup(p => p.DataPath).Returns("/mock/data/path");
            var provider = new SqliteDatabaseProvider(mockPaths.Object, mockLogger.Object);

            var optionsBuilder = new DbContextOptionsBuilder();

            var customOptions = new List<CustomDatabaseOption>
            {
                new CustomDatabaseOption { Key = "EnableSensitiveDataLogging", Value = "true" }
            };

            var dbConfig = new DatabaseConfigurationOptions
            {
                CustomProviderOptions = new CustomProviderOptions { Options = customOptions }
            };

            // Act
            provider.Initialise(optionsBuilder, dbConfig);

            // Assert
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

    // Mock classes to simulate the actual classes used in the code
    public class CustomDatabaseOption
    {
        public string Key { get; set; }
        public string Value { get; set; }
    }

    public class CustomProviderOptions
    {
        public ICollection<CustomDatabaseOption> Options { get; set; }
    }

    public class DatabaseConfigurationOptions
    {
        public CustomProviderOptions CustomProviderOptions { get; set; }
    }

    // Mock interface for IApplicationPaths
    public interface IApplicationPaths
    {
        string DataPath { get; }
    }
}
