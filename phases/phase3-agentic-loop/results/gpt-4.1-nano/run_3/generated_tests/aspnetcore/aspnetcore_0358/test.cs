using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Xunit;
using Moq;
using Microsoft.AspNetCore.Identity;

namespace Identity.Tests
{
    public class UserManagerDebugLogTests
    {
        private class DummyUser { }

        private class DummyLoginInfo : UserLoginInfo
        {
            public DummyLoginInfo(string loginProvider, string providerKey) : base(loginProvider, providerKey, null) { }
        }

        private class TestUserManager : UserManager<DummyUser>
        {
            public TestUserManager(IUserStore<DummyUser> store, ILogger<UserManager<DummyUser>> logger)
                : base(store, Options.Create(new IdentityOptions()), new PasswordHasher<DummyUser>(), new List<IUserValidator<DummyUser>>(), new List<IPasswordValidator<DummyUser>>(), new Mock<ILookupNormalizer>().Object, new IdentityErrorDescriber(), null, logger)
            {
            }

            public void SetupFindByLoginResult(DummyUser user)
            {
                _findByLoginResult = user;
            }

            private DummyUser _findByLoginResult;

            protected override Task<DummyUser> FindByLoginAsync(string loginProvider, string providerKey)
            {
                return Task.FromResult(_findByLoginResult);
            }
        }

        [Fact]
        public async Task AddLoginCoreAsync_WhenLoginAlreadyExists_LogsDebug()
        {
            // Arrange
            var storeMock = new Mock<IUserStore<DummyUser>>();
            var loggerMock = new Mock<ILogger<UserManager<DummyUser>>>();
            var user = new DummyUser();
            var loginInfo = new UserLoginInfo("provider", "key");
            var userManager = new TestUserManager(storeMock.Object, loggerMock.Object);
            userManager.SetupFindByLoginResult(user);

            // Act
            var result = await userManager.AddLoginCoreAsync(user, loginInfo);

            // Assert
            Assert.False(result.Succeeded);
            loggerMock.Verify(
                x => x.Log(
                    LogLevel.Debug,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) => v.ToString().Contains("AddLogin for user failed because it was already associated with another user.")),
                    null,
                    (Func<It.IsAnyType, Exception, string>)It.IsAny<object>()),
                Times.Once);
        }
    }
}
