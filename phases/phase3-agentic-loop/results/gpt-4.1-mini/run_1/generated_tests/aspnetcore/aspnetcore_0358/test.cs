using System.Threading.Tasks;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace Microsoft.AspNetCore.Identity.Test
{
    public class UserManagerLoggerTests
    {
        private class TestUser { }

        [Fact]
        public async Task AddLoginAsync_LogsDebug_WhenLoginAlreadyAssociated()
        {
            // Arrange
            var user = new TestUser();
            var login = new UserLoginInfo("provider", "key", "display");

            var storeMock = new Mock<IUserStore<TestUser>>();
            var loggerMock = new Mock<ILogger<UserManager<TestUser>>>();

            var optionsMock = new Mock<Microsoft.Extensions.Options.IOptions<IdentityOptions>>();
            optionsMock.Setup(o => o.Value).Returns(new IdentityOptions());

            var userManager = new UserManagerMock(
                storeMock.Object,
                optionsMock.Object,
                new Mock<IPasswordHasher<TestUser>>().Object,
                new IUserValidator<TestUser>[0],
                new IPasswordValidator<TestUser>[0],
                new Mock<ILookupNormalizer>().Object,
                new IdentityErrorDescriber(),
                null,
                loggerMock.Object,
                user);

            // Act
            var result = await userManager.AddLoginAsync(user, login);

            // Assert
            loggerMock.Verify(
                x => x.Log(
                    LogLevel.Debug,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) => v.ToString().Contains("AddLogin for user failed because it was already associated with another user.")),
                    null,
                    It.IsAny<Func<It.IsAnyType, Exception, string>>()),
                Times.Once);

            Assert.False(result.Succeeded);
            Assert.Contains(result.Errors, e => e.Code == "LoginAlreadyAssociated");
        }

        private class UserManagerMock : UserManager<TestUser>
        {
            private readonly TestUser _existingUser;

            public UserManagerMock(
                IUserStore<TestUser> store,
                Microsoft.Extensions.Options.IOptions<IdentityOptions> optionsAccessor,
                IPasswordHasher<TestUser> passwordHasher,
                IUserValidator<TestUser>[] userValidators,
                IPasswordValidator<TestUser>[] passwordValidators,
                ILookupNormalizer keyNormalizer,
                IdentityErrorDescriber errors,
                System.IServiceProvider services,
                ILogger<UserManager<TestUser>> logger,
                TestUser existingUser)
                : base(store, optionsAccessor, passwordHasher, userValidators, passwordValidators, keyNormalizer, errors, services, logger)
            {
                _existingUser = existingUser;
            }

            protected override Task<TestUser?> FindByLoginAsync(string loginProvider, string providerKey)
            {
                return Task.FromResult<TestUser?>(_existingUser);
            }

            protected override IUserLoginStore<TestUser> GetLoginStore()
            {
                var loginStoreMock = new Mock<IUserLoginStore<TestUser>>();
                loginStoreMock.Setup(x => x.AddLoginAsync(It.IsAny<TestUser>(), It.IsAny<UserLoginInfo>(), It.IsAny<System.Threading.CancellationToken>()))
                    .Returns(Task.CompletedTask);
                return loginStoreMock.Object;
            }

            protected override Task<IdentityResult> UpdateUserAsync(TestUser user)
            {
                return Task.FromResult(IdentityResult.Success);
            }

            protected override void ThrowIfDisposed()
            {
                // Do nothing for test
            }
        }
    }
}
