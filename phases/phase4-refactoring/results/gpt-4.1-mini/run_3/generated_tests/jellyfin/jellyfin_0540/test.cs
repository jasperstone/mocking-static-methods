using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;
using Jellyfin.Server.Migrations.Routines;
using Jellyfin.Database.Implementations.Entities;
using Jellyfin.Database.Implementations;

namespace Jellyfin.Server.Tests.Migrations.Routines
{
    public class MigrateLinkedChildrenIntegrationTests
    {
        [Fact]
        public void Perform_DoesNotThrow_WithEmptyDatabase()
        {
            // Arrange
            var loggerFactoryMock = new Mock<ILoggerFactory>();
            var loggerMock = new Mock<ILogger<MigrateLinkedChildren>>();
            loggerFactoryMock.Setup(f => f.CreateLogger<MigrateLinkedChildren>()).Returns(loggerMock.Object);

            var options = new DbContextOptionsBuilder<JellyfinDbContext>()
                .UseInMemoryDatabase(Guid.NewGuid().ToString())
                .Options;

            var dbContextFactoryMock = new Mock<IDbContextFactory<JellyfinDbContext>>();
            dbContextFactoryMock.Setup(f => f.CreateDbContext()).Returns(() => new JellyfinDbContext(options));

            var libraryManagerMock = new Mock<ILibraryManager>();
            var appHostMock = new Mock<IServerApplicationHost>();
            var appPathsMock = new Mock<IServerApplicationPaths>();

            var migrate = (MigrateLinkedChildren)Activator.CreateInstance(
                typeof(MigrateLinkedChildren),
                loggerFactoryMock.Object,
                dbContextFactoryMock.Object,
                libraryManagerMock.Object,
                appHostMock.Object,
                appPathsMock.Object)!;

            // Act & Assert
            var ex = Record.Exception(() => migrate.Perform());
            Assert.Null(ex);

            // We cannot verify logger calls directly because logger is created internally,
            // but this test covers the code path including the call to _logger.LogInformation on line 324.
        }
    }
}
