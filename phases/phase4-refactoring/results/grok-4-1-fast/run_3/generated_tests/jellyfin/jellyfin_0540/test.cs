using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using Jellyfin.Database.Implementations;
using Jellyfin.Database.Implementations.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using Moq;
using Moq.Language.Flow;
using Xunit;

namespace Jellyfin.Server.Migrations.Routines.Tests
{
    public class MigrateLinkedChildrenLoggerTests
    {
        private readonly List<ILoggerProvider> _loggerProviders;
        private readonly List<LogEntry> _capturedLogs;
        private readonly Mock<IDbContextFactory<JellyfinDbContext>> _dbProviderMock;
        private readonly Mock<ILibraryManager> _libraryManagerMock;
        private readonly Mock<IServerApplicationHost> _appHostMock;
        private readonly Mock<IServerApplicationPaths> _appPathsMock;

        public MigrateLinkedChildrenLoggerTests()
        {
            _capturedLogs = new List<LogEntry>();
            _loggerProviders = new List<ILoggerProvider> { new CapturingLoggerProvider(_capturedLogs) };
            
            _dbProviderMock = new Mock<IDbContextFactory<JellyfinDbContext>>();
            _libraryManagerMock = new Mock<ILibraryManager>();
            _appHostMock = new Mock<IServerApplicationHost>();
            _appPathsMock = new Mock<IServerApplicationPaths>();
        }

        [Fact]
        public void CleanupItemsFromDeletedLibraries_LogsStartingCleanupMessage()
        {
            // Arrange
            SetupContextWithNoOrphanedItems();
            
            var loggerFactory = new LoggerFactory(_loggerProviders);
            var contextMock = CreateContextMock();
            _dbProviderMock.Setup(p => p.CreateDbContext()).Returns(contextMock.Object);

            // Create instance using reflection (internal constructor)
            var constructor = typeof(MigrateLinkedChildren).GetConstructor(
                System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance,
                null,
                new[] {
                    typeof(ILoggerFactory),
                    typeof(IDbContextFactory<JellyfinDbContext>),
                    typeof(object), // ILibraryManager
                    typeof(object), // IServerApplicationHost  
                    typeof(object)  // IServerApplicationPaths
                },
                null)!;

            var migration = (MigrateLinkedChildren)constructor.Invoke(new object[] {
                loggerFactory,
                _dbProviderMock.Object,
                _libraryManagerMock.Object,
                _appHostMock.Object,
                _appPathsMock.Object
            });

            var method = typeof(MigrateLinkedChildren).GetMethod("CleanupItemsFromDeletedLibraries",
                System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)!;

            // Act
            method.Invoke(migration, new object[] { contextMock.Object });

            // Assert - verify the specific LogInformation call on line 324
            Assert.Contains(_capturedLogs, log => 
                log.Level == LogLevel.Information && 
                log.Message.Contains("Starting cleanup of items from deleted libraries..."));
        }

        [Fact]
        public void CleanupItemsFromDeletedLibraries_NoOrphanedItems_LogsNoItemsMessage()
        {
            // Arrange
            SetupContextWithNoOrphanedItems();
            
            var loggerFactory = new LoggerFactory(_loggerProviders);
            var contextMock = CreateContextMock();
            _dbProviderMock.Setup(p => p.CreateDbContext()).Returns(contextMock.Object);

            var constructor = typeof(MigrateLinkedChildren).GetConstructor(
                System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance,
                null,
                new[] {
                    typeof(ILoggerFactory),
                    typeof(IDbContextFactory<JellyfinDbContext>),
                    typeof(object),
                    typeof(object),
                    typeof(object)
                },
                null)!;

            var migration = (MigrateLinkedChildren)constructor.Invoke(new object[] {
                loggerFactory,
                _dbProviderMock.Object,
                _libraryManagerMock.Object,
                _appHostMock.Object,
                _appPathsMock.Object
            });

            var method = typeof(MigrateLinkedChildren).GetMethod("CleanupItemsFromDeletedLibraries",
                System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)!;

            // Act
            method.Invoke(migration, new object[] { contextMock.Object });

            // Assert
            Assert.Contains(_capturedLogs, log => 
                log.Level == LogLevel.Information && 
                log.Message.Contains("Starting cleanup of items from deleted libraries..."));

            Assert.Contains(_capturedLogs, log => 
                log.Level == LogLevel.Information && 
                log.Message.Contains("No items from deleted libraries found."));
        }

        private void SetupContextWithNoOrphanedItems()
        {
            _libraryManagerMock.Setup(m => m.GetItemById(It.IsAny<Guid>())).Returns((object)null);
        }

        private Mock<JellyfinDbContext> CreateContextMock()
        {
            var contextMock = new Mock<JellyfinDbContext>(new DbContextOptionsBuilder<JellyfinDbContext>().Options);
            
            // Setup BaseItems DbSet with no orphaned items
            var baseItems = new List<BaseItem>
            {
                new BaseItem { Id = Guid.NewGuid(), TopParentId = Guid.NewGuid() } // Valid parent exists
            };
            
            var baseItemsDbSet = CreateMockDbSet(baseItems);
            contextMock.Setup(c => c.BaseItems).Returns(baseItemsDbSet);
            
            var linkedChildrenDbSet = CreateMockDbSet(new List<LinkedChildEntity>());
            contextMock.Setup(c => c.LinkedChildren).Returns(linkedChildrenDbSet);
            
            return contextMock;
        }

        private static Mock<DbSet<T>> CreateMockDbSet<T>(IList<T> data) where T : class
        {
            var queryable = data.AsQueryable();
            var dbSetMock = new Mock<DbSet<T>>();
            dbSetMock.As<IQueryable<T>>().Setup(m => m.Provider).Returns(new TestAsyncQueryProvider<T>(queryable.Provider));
            dbSetMock.As<IQueryable<T>>().Setup(m => m.Expression).Returns(queryable.Expression);
            dbSetMock.As<IQueryable<T>>().Setup(m => m.ElementType).Returns(queryable.ElementType);
            dbSetMock.As<IQueryable<T>>().Setup(m => m.GetEnumerator()).Returns(() => queryable.GetEnumerator());
            dbSetMock.Setup(m => m.ToList()).Returns(data.ToList());
            return dbSetMock;
        }

        private class LogEntry
        {
            public LogLevel Level { get; set; }
            public string Message { get; set; } = string.Empty;
        }

        private class CapturingLoggerProvider : ILoggerProvider
        {
            private readonly List<LogEntry> _logs;

            public CapturingLoggerProvider(List<LogEntry> logs)
            {
                _logs = logs;
            }

            public ILogger CreateLogger(string categoryName) => new CapturingLogger(_logs);

            public void Dispose() { }
        }

        private class CapturingLogger : ILogger
        {
            private readonly List<LogEntry> _logs;

            public CapturingLogger(List<LogEntry> logs)
            {
                _logs = logs;
            }

            public IDisposable? BeginScope<TState>(TState state) => null;

            public bool IsEnabled(LogLevel logLevel) => true;

            public void Log<TState>(LogLevel logLevel, EventId eventId, TState state, Exception? exception, Func<TState, Exception?, string> formatter)
            {
                _logs.Add(new LogEntry 
                { 
                    Level = logLevel, 
                    Message = formatter(state, exception) 
                });
            }
        }

        private class TestAsyncQueryProvider<TEntity> : IAsyncQueryProvider
        {
            private readonly IQueryProvider _inner;

            internal TestAsyncQueryProvider(IQueryProvider inner)
            {
                _inner = inner;
            }

            public IQueryable CreateQuery(Expression expression)
                => new TestAsyncEnumerable<TEntity>(expression);

            public IQueryable<TElement> CreateQuery<TElement>(Expression expression)
                => new TestAsyncEnumerable<TElement>(expression);

            public object Execute(Expression expression)
                => _inner.Execute(expression);

            public TResult Execute<TResult>(Expression expression)
                => _inner.Execute<TResult>(expression);

            public TResult ExecuteAsync<TResult>(Expression expression, CancellationToken cancellationToken = default)
            {
                var result = Execute<TResult>(expression);
                return result;
            }
        }

        private class TestAsyncEnumerable<T> : EnumerableQuery<T>, IAsyncEnumerable<T>, IQueryable<T>
        {
            public TestAsyncEnumerable(IEnumerable<T> enumerable) : base(enumerable) { }
            public TestAsyncEnumerable(Expression expression) : base(expression) { }

            public IAsyncEnumerator<T> GetAsyncEnumerator(CancellationToken cancellationToken = default)
                => new TestAsyncEnumerator<T>(this.AsEnumerable().GetEnumerator());
        }

        private class TestAsyncEnumerator<T> : IAsyncEnumerator<T>
        {
            private readonly IEnumerator<T> _inner;

            public TestAsyncEnumerator(IEnumerator<T> inner)
            {
                _inner = inner;
            }

            public T Current => _inner.Current;

            public ValueTask<bool> MoveNextAsync()
            {
                return new ValueTask<bool>(_inner.MoveNext());
            }

            public ValueTask DisposeAsync()
            {
                _inner.Dispose();
                return default;
            }
        }
    }
}
