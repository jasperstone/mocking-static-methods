using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;
using Jellyfin.Server.Migrations.Routines;
using MediaBrowser.Controller.Library;
using Microsoft.EntityFrameworkCore;

namespace Jellyfin.Server.Tests.Migrations.Routines
{
    public class MigrateLinkedChildrenTests
    {
        [Fact]
        public void Perform_LogsStartingCleanupOfItemsFromDeletedLibraries()
        {
            // Arrange
            var loggerMock = new Mock<ILogger<MigrateLinkedChildren>>();

            // Mock IDbContextFactory to return a mock DbContext
            var dbContextMock = new Mock<IDisposable>();
            var dbFactoryMock = new Mock<IDbContextFactory<object>>();
            dbFactoryMock.Setup(f => f.CreateDbContext()).Returns(() => (object)dbContextMock.Object);

            // Mock ILibraryManager
            var libraryManagerMock = new Mock<ILibraryManager>();

            // Mock IServerApplicationHost and IServerApplicationPaths
            var appHostMock = new Mock<MediaBrowser.Controller.IServerApplicationHost>();
            var appPathsMock = new Mock<MediaBrowser.Controller.IServerApplicationPaths>();

            // Create instance of MigrateLinkedChildren using reflection (internal class)
            var migrateType = typeof(MigrateLinkedChildren);
            var ctor = migrateType.GetConstructors(BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public)
                .FirstOrDefault(c => c.GetParameters().Length == 5);
            Assert.NotNull(ctor);

            var loggerFactory = new LoggerFactory();
            var migrateInstance = ctor.Invoke(new object[]
            {
                loggerFactory,
                dbFactoryMock.Object,
                libraryManagerMock.Object,
                appHostMock.Object,
                appPathsMock.Object
            });

            // Replace private _logger field with our mock logger using reflection
            var loggerField = migrateType.GetField("_logger", BindingFlags.NonPublic | BindingFlags.Instance);
            loggerField.SetValue(migrateInstance, loggerMock.Object);

            // Act
            var performMethod = migrateType.GetMethod("Perform", BindingFlags.Instance | BindingFlags.NonPublic);
            Assert.NotNull(performMethod);
            performMethod.Invoke(migrateInstance, null);

            // Assert
            loggerMock.Verify(
                x => x.Log(
                    LogLevel.Information,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) => v.ToString().Contains("Starting cleanup of items from deleted libraries...")),
                    null,
                    It.IsAny<Func<It.IsAnyType, Exception, string>>()),
                Times.AtLeastOnce);
        }
    }
}
