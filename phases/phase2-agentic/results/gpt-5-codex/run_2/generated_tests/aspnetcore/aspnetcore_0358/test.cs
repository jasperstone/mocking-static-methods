using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Xunit;

namespace Microsoft.AspNetCore.Identity.Tests
{
    public class UserManagerLoggerTests
    {
        [Fact]
        public async Task AddLoginAsync_LogsDebugWhenLoginAlreadyAssociated()
        {
            // Arrange
            var store = new TestUserStore();
            var logger = new TestLogger<UserManager<TestUser>>();
            var manager = new TestUserManager(store, logger);
            var user = new TestUser();
            var loginInfo = new UserLoginInfo("provider", "key", "display");

            manager.FindByLoginResult = new TestUser();

            // Act
            var result = await manager.AddLoginAsync(user, loginInfo);

            // Assert
            Assert.False(result.Succeeded);
            var error = Assert.Single(result.Errors);
            var expectedError = manager.ErrorDescriber.LoginAlreadyAssociated();
            Assert.Equal(expectedError.Code, error.Code);
            Assert.Equal(expectedError.Description, error.Description);

            var logEntry = Assert.Single(logger.LogEntries);
            Assert.Equal(LogLevel.Debug, logEntry.LogLevel);
            Assert.Equal("AddLogin for user failed because it was already associated with another user.", logEntry.Message);
            Assert.False(store.AddLoginCalled);
        }

        private sealed class TestUser
        {
        }

        private sealed class TestUserStore : IUserLoginStore<TestUser>
        {
            public bool AddLoginCalled { get; private set; }

            public Task AddLoginAsync(TestUser user, UserLoginInfo login, CancellationToken cancellationToken)
            {
                AddLoginCalled = true;
                return Task.CompletedTask;
            }

            public Task RemoveLoginAsync(TestUser user, string loginProvider, string providerKey, CancellationToken cancellationToken)
                => Task.CompletedTask;

            public Task<IList<UserLoginInfo>> GetLoginsAsync(TestUser user, CancellationToken cancellationToken)
                => Task.FromResult<IList<UserLoginInfo>>(new List<UserLoginInfo>());

            public Task<TestUser?> FindByLoginAsync(string loginProvider, string providerKey, CancellationToken cancellationToken)
                => Task.FromResult<TestUser?>(null);

            public Task<IdentityResult> CreateAsync(TestUser user, CancellationToken cancellationToken)
                => Task.FromResult(IdentityResult.Success);

            public Task<IdentityResult> UpdateAsync(TestUser user, CancellationToken cancellationToken)
                => Task.FromResult(IdentityResult.Success);

            public Task<IdentityResult> DeleteAsync(TestUser user, CancellationToken cancellationToken)
                => Task.FromResult(IdentityResult.Success);

            public Task<TestUser?> FindByIdAsync(string userId, CancellationToken cancellationToken)
                => Task.FromResult<TestUser?>(null);

            public Task<TestUser?> FindByNameAsync(string normalizedUserName, CancellationToken cancellationToken)
                => Task.FromResult<TestUser?>(null);

            public Task<string> GetUserIdAsync(TestUser user, CancellationToken cancellationToken)
                => Task.FromResult("test-id");

            public Task<string?> GetUserNameAsync(TestUser user, CancellationToken cancellationToken)
                => Task.FromResult<string?>("test-user");

            public Task SetUserNameAsync(TestUser user, string? userName, CancellationToken cancellationToken)
                => Task.CompletedTask;

            public Task<string?> GetNormalizedUserNameAsync(TestUser user, CancellationToken cancellationToken)
                => Task.FromResult<string?>("TEST-USER");

            public Task SetNormalizedUserNameAsync(TestUser user, string? normalizedName, CancellationToken cancellationToken)
                => Task.CompletedTask;

            public void Dispose()
            {
            }
        }

        private sealed class TestUserManager : UserManager<TestUser>
        {
            public TestUserManager(IUserStore<TestUser> store, ILogger<UserManager<TestUser>> logger)
                : base(
                    store,
                    Options.Create(new IdentityOptions()),
                    new PasswordHasher<TestUser>(),
                    Array.Empty<IUserValidator<TestUser>>(),
                    Array.Empty<IPasswordValidator<TestUser>>(),
                    new UpperInvariantLookupNormalizer(),
                    new IdentityErrorDescriber(),
                    services: null,
                    logger)
            {
            }

            public TestUser? FindByLoginResult { get; set; }

            public override Task<TestUser?> FindByLoginAsync(string loginProvider, string providerKey)
                => Task.FromResult<TestUser?>(FindByLoginResult);
        }

        private sealed class TestLogger<T> : ILogger<T>
        {
            private readonly List<LogEntry> _entries = new();
            public IReadOnlyList<LogEntry> LogEntries => _entries;

            public IDisposable BeginScope<TState>(TState state) => NullScope.Instance;

            public bool IsEnabled(LogLevel logLevel) => true;

            public void Log<TState>(LogLevel logLevel, EventId eventId, TState state, Exception? exception, Func<TState, Exception?, string> formatter)
            {
                _entries.Add(new LogEntry(logLevel, eventId, formatter(state, exception), exception));
            }

            public readonly record struct LogEntry(LogLevel LogLevel, EventId EventId, string Message, Exception? Exception);

            private sealed class NullScope : IDisposable
            {
                public static NullScope Instance { get; } = new NullScope();

                public void Dispose()
                {
                }
            }
        }
    }
}
