using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Microsoft.AspNetCore.Identity;
using Moq;
using Xunit;

namespace Microsoft.AspNetCore.Identity.Test
{
    public class UserManagerTests
    {
        private class TestUser { }

        [Fact]
        public async Task ChangePasswordCoreAsync_LogsDebug_WhenPasswordVerificationFails()
        {
            // Arrange
            var user = new TestUser();
            var loggerMock = new Mock<ILogger<UserManager<TestUser>>>();
            var storeMock = new Mock<IUserPasswordStore<TestUser>>();
            var optionsMock = new Mock<Microsoft.Extensions.Options.IOptions<IdentityOptions>>();
            optionsMock.Setup(o => o.Value).Returns(new IdentityOptions());
            var passwordHasherMock = new Mock<IPasswordHasher<TestUser>>();
            var userValidators = new IUserValidator<TestUser>[0];
            var passwordValidators = new IPasswordValidator<TestUser>[0];
            var keyNormalizerMock = new Mock<ILookupNormalizer>();
            var errorDescriber = new IdentityErrorDescriber();
            var serviceProviderMock = new Mock<System.IServiceProvider>();

            var userManager = new UserManager<TestUser>(
                storeMock.Object,
                optionsMock.Object,
                passwordHasherMock.Object,
                userValidators,
                passwordValidators,
                keyNormalizerMock.Object,
                errorDescriber,
                serviceProviderMock.Object,
                loggerMock.Object);

            // Setup GetPasswordStore to return the password store mock
            var getPasswordStoreMethod = typeof(UserManager<TestUser>).GetMethod("GetPasswordStore", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            getPasswordStoreMethod.Invoke(userManager, null);

            // Setup VerifyPasswordAsync to return PasswordVerificationResult.Failed
            var verifyPasswordAsyncMethod = typeof(UserManager<TestUser>).GetMethod("VerifyPasswordAsync", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            var verifyPasswordAsyncDelegate = (Task<PasswordVerificationResult>)verifyPasswordAsyncMethod.Invoke(userManager, new object[] { storeMock.Object, user, "currentPassword" });

            // Instead of invoking private VerifyPasswordAsync, we mock it by overriding UserManager
            var userManagerMock = new UserManagerMock(loggerMock.Object);

            // Act
            var result = await userManagerMock.ChangePasswordCoreAsync(user, "currentPassword", "newPassword");

            // Assert
            loggerMock.Verify(
                x => x.Log(
                    LogLevel.Debug,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) => v.ToString() == "Change password failed for user."),
                    null,
                    It.IsAny<Func<It.IsAnyType, System.Exception, string>>()),
                Times.Once);

            Assert.False(result.Succeeded);
            Assert.Contains(errorDescriber.PasswordMismatch().Code, result.Errors, e => e.Code == errorDescriber.PasswordMismatch().Code);
        }

        private class UserManagerMock : UserManager<TestUser>
        {
            public UserManagerMock(ILogger<UserManager<TestUser>> logger)
                : base(
                    new Mock<IUserStore<TestUser>>().Object,
                    new Mock<Microsoft.Extensions.Options.IOptions<IdentityOptions>>().Object,
                    new Mock<IPasswordHasher<TestUser>>().Object,
                    new IUserValidator<TestUser>[0],
                    new IPasswordValidator<TestUser>[0],
                    new Mock<ILookupNormalizer>().Object,
                    new IdentityErrorDescriber(),
                    new Mock<System.IServiceProvider>().Object,
                    logger)
            {
            }

            protected override Task<PasswordVerificationResult> VerifyPasswordAsync(IUserPasswordStore<TestUser> passwordStore, TestUser user, string password)
            {
                return Task.FromResult(PasswordVerificationResult.Failed);
            }
        }
    }
}
