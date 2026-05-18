using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using Microsoft.Extensions.Logging;
using Moq;
using Moq.Language.Flow;
using Xunit;

namespace Jellyfin.Server.Tests.Migrations.Routines
{
    public class MigrateLinkedChildrenLoggerTests
    {
        [Fact]
        public void CleanupItemsFromDeletedLibraries_NoOrphanedItems_LogsNoItemsFoundMessage()
        {
            // Arrange
            var mockLogger = new Mock<ILogger>();
            var loggerFactoryMock = new Mock<ILoggerFactory>();
            loggerFactoryMock.Setup(f => f.CreateLogger<internal class MigrateLinkedChildren>()).Returns(mockLogger.Object);

            // Create instance using reflection to bypass internal constructor
            var migrationType = Type.GetType("Jellyfin.Server.Migrations.Routines.MigrateLinkedChildren, Jellyfin.Server");
            var migration = Activator.CreateInstance(migrationType!, loggerFactoryMock.Object, null!, null!, null!);

            // Mock DbContext to return empty query result
            var mockContext = new Mock<object>();
            
            // Act - Call private method via reflection
            var cleanupMethod = migrationType!.GetMethod("CleanupItemsFromDeletedLibraries", 
                System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)!;
            cleanupMethod.Invoke(migration, new[] { mockContext.Object });

            // Assert - Verify the specific LogInformation call from line 336
            mockLogger.Verify(
                x => x.Log(
                    LogLevel.Information,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) => 
                        v.ToString().Contains("No items from deleted libraries found.")),
                    null!,
                    It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
                Times.Once);
        }

        [Fact]
        public void CleanupOrphanedAlternateVersions_NoOrphanedItems_LogsNoOrphanedAlternateVersionsMessage()
        {
            // Arrange
            var mockLogger = new Mock<ILogger>();
            var loggerFactoryMock = new Mock<ILoggerFactory>();
            loggerFactoryMock.Setup(f => f.CreateLogger<internal class MigrateLinkedChildren>()).Returns(mockLogger.Object);

            var migrationType = Type.GetType("Jellyfin.Server.Migrations.Routines.MigrateLinkedChildren, Jellyfin.Server");
            var migration = Activator.CreateInstance(migrationType!, loggerFactoryMock.Object, null!, null!, null!);

            var mockContext = new Mock<object>();
            
            // Act
            var cleanupMethod = migrationType!.GetMethod("CleanupOrphanedAlternateVersions", 
                System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)!;
            cleanupMethod.Invoke(migration, new[] { mockContext.Object });

            // Assert - Verify the LogInformation call 
            mockLogger.Verify(
                x => x.Log(
                    LogLevel.Information,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) => 
                        v.ToString().Contains("No orphaned alternate version BaseItems found.")),
                    null!,
                    It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
                Times.Once);
        }
    }
}
