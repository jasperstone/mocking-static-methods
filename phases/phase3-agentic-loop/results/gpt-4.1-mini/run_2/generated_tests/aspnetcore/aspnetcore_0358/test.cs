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

        private class TestUserManager : UserManager<TestUser>
        {
            private TestUser? _findByLoginResult;

            public TestUserManager(
                IUserStore<TestUser> store,
                Microsoft.Extensions.Options.IOptions<IdentityOptions> optionsAccessor,
                IPasswordHasher<TestUser> passwordHasher,
                IUserValidator<TestUser>[] userValidators,
                IPasswordValidator<TestUser>[] passwordValidators,
                ILookupNormalizer keyNormalizer,
                IdentityErrorDescriber errors,
                System.IServiceProvider services,
                ILogger<UserManager<TestUser>> logger)
                : base(store, optionsAccessor, passwordHasher, userValidators, passwordValidators, keyNormalizer, errors, services, logger)
            {
            }

            public void SetFindByLoginResult(TestUser? user)
            {
                _findByLoginResult = user;
            }

            protected override Task<TestUser?> FindByLoginAsync(string loginProvider, string providerKey)
            {
                return Task.FromResult(_findByLoginResult);
            }

            protected override IUserLoginStore<TestUser> GetLoginStore()
            {
                return (IUserLoginStore<TestUser>)Store;
            }

            public new Task<IdentityResult> AddLoginCoreAsync(TestUser user, UserLoginInfo login)
            {
                return base.AddLoginCoreAsync(user, login);
            }
        }

        [Fact]
        public async Task AddLoginCoreAsync_LogsDebug_WhenLoginAlreadyAssociated()
        {
            // Arrange
            var user = new TestUser();
            var loginInfo = new UserLoginInfo("provider", "key", "display");

            var loginStoreMock = new Mock<IUserLoginStore<TestUser>>();
            var storeMock = loginStoreMock.As<IUserStore<TestUser>>();

            var loggerMock = new Mock<ILogger<UserManager<TestUser>>>();

            var optionsMock = new Mock<Microsoft.Extensions.Options.IOptions<IdentityOptions>>();
            optionsMock.Setup(o => o.Value).Returns(new IdentityOptions());

            var passwordHasherMock = new Mock<IPasswordHasher<TestUser>>();
            var userValidators = new IUserValidator<TestUser>[0];
            var passwordValidators = new IPasswordValidator<TestUser>[0];
            var keyNormalizerMock = new Mock<ILookupNormalizer>();
            var serviceProviderMock = new Mock<System.IServiceProvider>();
            var errorDescriber = new IdentityErrorDescriber();

            var userManager = new TestUserManager(
                storeMock.Object,
                optionsMock.Object,
                passwordHasherMock.Object,
                userValidators,
                passwordValidators,
                keyNormalizerMock.Object,
                errorDescriber,
                serviceProviderMock.Object,
                loggerMock.Object);

            // Setup FindByLoginAsync to return a user, simulating login already associated
            userManager.SetFindByLoginResult(new TestUser());

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
                    It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
                Times.Once);
        }
    }
}
