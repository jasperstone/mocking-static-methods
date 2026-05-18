using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Xunit;
using Moq;
using Microsoft.AspNetCore.Identity;

namespace Identity.Tests
{
    public class UserManagerLoggingTests
    {
        private class DummyUser { }

        [Fact]
        public async Task AddLoginAsync_Should_Call_LogDebug_When_ExistingUserFound()
        {
            // Arrange
            var user = new DummyUser();
            var loginInfo = new UserLoginInfo("provider", "key", "display");
            var storeMock = new Mock<IUserStore<DummyUser>>();
            var loggerMock = new Mock<ILogger>();
            var userManager = new TestUserManager<DummyUser>(storeMock.Object, loggerMock.Object);

            // Setup FindByLoginAsync to return a user, simulating existing login
            userManager.SetupFindByLoginAsync(loginInfo.LoginProvider, loginInfo.ProviderKey, existingUser: true);

            // Act
            var result = await userManager.AddLoginAsync(user, loginInfo);

            // Assert
            // Verify that LogDebug was called
            loggerMock.Verify(
                x => x.Log(
                    LogLevel.Debug,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) => v.ToString().Contains("AddLogin for user failed because it was already associated with another user.")),
                    null,
                    It.IsAny<Func<It.IsAnyType, Exception, string>>()),
                Times.Once);
        }

        // Helper class to override AddLoginCoreAsync for testing
        private class TestUserManager<TUser> : UserManager<TUser> where TUser : class
        {
            private readonly ILogger _logger;
            private bool _existingUser;

            public TestUserManager(IUserStore<TUser> store, ILogger logger)
                : base(store, Options.Create(new IdentityOptions()), new PasswordHasher<TUser>(), new List<IUserValidator<TUser>>(), new List<IPasswordValidator<TUser>>(), new Mock<ILookupNormalizer>().Object, new IdentityErrorDescriber(), new Mock<IServiceProvider>().Object, logger)
            {
                _logger = logger;
            }

            public void SetupFindByLoginAsync(string loginProvider, string providerKey, bool existingUser)
            {
                _existingUser = existingUser;
            }

            protected override async Task<TUser> FindByLoginAsync(string loginProvider, string providerKey)
            {
                if (_existingUser)
                {
                    return new TUser(); // simulate found user
                }
                return null;
            }
        }
    }
}
