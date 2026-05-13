using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Jellyfin.Database.Implementations;
using Jellyfin.Database.Implementations.Entities;
using Jellyfin.Server.Migrations.Routines;
using MediaBrowser.Controller;
using MediaBrowser.Controller.Library;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace Jellyfin.Server.Tests.Migrations.Routines
{
    public class MigrateLinkedChildrenTests : IDisposable
    {
        private readonly ServiceProvider _serviceProvider;
        private readonly ILoggerFactory _loggerFactory;

        public MigrateLinkedChildrenTests()
        {
            var services = new ServiceCollection();
            services.AddLogging();
            services.AddDbContext<JellyfinDbContext>(options => options.UseInMemoryDatabase(Guid.NewGuid().ToString()));
            _serviceProvider = services.BuildServiceProvider();
            _loggerFactory = _serviceProvider.GetRequiredService<ILoggerFactory>();
        }

        [Fact]
        public void CleanupItemsFromDeletedLibraries_WhenNoItemsFound_LogsExpectedMessage()
        {
            var dbContext = _serviceProvider.GetRequiredService<JellyfinDbContext>();
            dbContext.Database.EnsureCreated();

            var loggerMock = new Mock<ILogger<MigrateLinkedChildren>>();
            _loggerFactory.AddProvider(new MockLoggerProvider(loggerMock.Object));

            var dbContextFactoryMock = new Mock<IDbContextFactory<JellyfinDbContext>>();
            dbContextFactoryMock.Setup(x => x.CreateDbContext()).Returns(dbContext);

            var libraryManagerMock = new Mock<ILibraryManager>();
            var appHostMock = new Mock<IServerApplicationHost>();
            var appPathsMock = new Mock<IServerApplicationPaths>();

            var migration = new MigrateLinkedChildren(
                _loggerFactory,
                dbContextFactoryMock.Object,
                libraryManagerMock.Object,
                appHostMock.Object,
                appPathsMock.Object);

            InvokeCleanupItemsFromDeletedLibraries(migration, dbContext);

            loggerMock.Verify(
                l => l.Log(
                    LogLevel.Information,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((state, _) => state.ToString() == "No items from deleted libraries found."),
                    It.IsAny<Exception>(),
                    It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
                Times.Once);
        }

        private static void InvokeCleanupItemsFromDeletedLibraries(MigrateLinkedChildren migration, JellyfinDbContext context)
        {
            var method = typeof(MigrateLinkedChildren).GetMethod("CleanupItemsFromDeletedLibraries", BindingFlags.NonPublic | BindingFlags.Instance);
            Assert.NotNull(method);
            method.Invoke(migration, new object[] { context });
        }

        public void Dispose()
        {
            _serviceProvider.Dispose();
        }

        private sealed class MockLoggerProvider : ILoggerProvider
        {
            private readonly ILogger _logger;

            public MockLoggerProvider(ILogger logger)
            {
                _logger = logger;
            }

            public ILogger CreateLogger(string categoryName) => _logger;

            public void Dispose()
            {
            }
        }
    }
}
