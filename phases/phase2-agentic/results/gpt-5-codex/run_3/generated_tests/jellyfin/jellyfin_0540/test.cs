using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Jellyfin.Server.Migrations.Routines;
using MediaBrowser.Controller.Library;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace Jellyfin.Tests.Migrations.Routines
{
    public class MigrateLinkedChildrenTests
    {
        private sealed class FakeDbContextFactory : IDbContextFactory<JellyfinDbContext>
        {
            private readonly JellyfinDbContext _context;

            public FakeDbContextFactory(JellyfinDbContext context)
            {
                _context = context;
            }

            public JellyfinDbContext CreateDbContext()
            {
                return _context;
            }
        }

        [Fact]
        public void CleanupItemsFromDeletedLibraries_LogsStartMessage()
        {
            var loggerMock = new Mock<ILogger<MigrateLinkedChildren>>();
            loggerMock.Setup(
                    l => l.Log(
                        LogLevel.Information,
                        It.IsAny<EventId>(),
                        It.Is<It.IsAnyType>((state, _) =>
                            state.ToString() ==
                            "Starting cleanup of items from deleted libraries..."),
                        It.IsAny<Exception>(),
                        It.IsAny<Func<It.IsAnyType, Exception?, string>>()))
                .Verifiable();

            var loggerFactory = new Mock<ILoggerFactory>();
            loggerFactory.Setup(lf => lf.CreateLogger(It.IsAny<string>()))
                .Returns(loggerMock.Object);

            var dbContextOptions = new DbContextOptionsBuilder<JellyfinDbContext>()
                .UseInMemoryDatabase(Guid.NewGuid().ToString())
                .Options;

            using var dbContext = new JellyfinDbContext(dbContextOptions);

            var dbContextFactory = new FakeDbContextFactory(dbContext);
            var libraryManagerMock = new Mock<ILibraryManager>(MockBehavior.Strict);
            var appHostMock = new Mock<IServerApplicationHost>(MockBehavior.Strict);
            var appPathsMock = new Mock<IServerApplicationPaths>(MockBehavior.Strict);

            var migrateLinkedChildren = new MigrateLinkedChildren(
                loggerFactory.Object,
                dbContextFactory,
                libraryManagerMock.Object,
                appHostMock.Object,
                appPathsMock.Object);

            var methodInfo = typeof(MigrateLinkedChildren).GetMethod(
                "CleanupItemsFromDeletedLibraries",
                BindingFlags.NonPublic | BindingFlags.Instance);

            Assert.NotNull(methodInfo);

            methodInfo!.Invoke(migrateLinkedChildren, new object[] { dbContext });

            loggerMock.Verify(
                l => l.Log(
                    LogLevel.Information,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((state, _) =>
                        state.ToString() ==
                        "Starting cleanup of items from deleted libraries..."),
                    It.IsAny<Exception>(),
                    It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
                Times.Once);
        }
    }
}
