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
using Jellyfin.Database.Providers.Sqlite;

namespace Jellyfin.Tests
{
    public class SqliteDatabaseProviderTests
    {
        [Fact]
        public void Initialise_ShouldLogConnectionStringAndEnableSensitiveDataLogging()
        {
            // Arrange
            var mockLogger = new Mock<ILogger<SqliteDatabaseProvider>>();
            var mockPaths = new Mock<IApplicationPaths>();
            mockPaths.Setup(p => p.DataPath).Returns("somepath");
            var provider = new SqliteDatabaseProvider(mockPaths.Object, mockLogger.Object);

            var optionsBuilder = new DbContextOptionsBuilder();

            var customOptions = new[]
            {
                new CustomDatabaseOption { Key = "path", Value = "testpath" },
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

    // Dummy classes to satisfy the code dependencies
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
}
