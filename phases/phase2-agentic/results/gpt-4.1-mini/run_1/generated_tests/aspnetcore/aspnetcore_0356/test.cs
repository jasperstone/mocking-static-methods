using System.Threading.Tasks;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Logging;
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
            var loggerMock = new Mock<ILogger<UserManager<TestUser>>>();
            var userStoreMock = new Mock<IUserPasswordStore<TestUser>>();
            var passwordHasherMock = new Mock<IPasswordHasher<TestUser>>();
            var userValidators = new IUserValidator<TestUser>[0];
            var passwordValidators = new IPasswordValidator<TestUser>[0];
            var keyNormalizerMock = new Mock<ILookupNormalizer>();
            var errorDescriber = new IdentityErrorDescriber();
            var servicesMock = new Mock<IServiceProvider>();

            var userManager = new UserManager<TestUser>(
                userStoreMock.Object,
                new Microsoft.Extensions.Options.OptionsWrapper<IdentityOptions>(new IdentityOptions()),
                passwordHasherMock.Object,
                userValidators,
                passwordValidators,
                keyNormalizerMock.Object,
                errorDescriber,
                servicesMock.Object,
                loggerMock.Object);

            var testUser = new TestUser();

            // Setup GetPasswordStore to return the userStoreMock
            userManager.Store = userStoreMock.Object;

            // Setup VerifyPasswordAsync to return PasswordVerificationResult.Failed
            var verifyPasswordAsyncMethod = typeof(UserManager<TestUser>).GetMethod("VerifyPasswordAsync", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            var updatePasswordHashMethod = typeof(UserManager<TestUser>).GetMethod("UpdatePasswordHash", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            var updateUserAsyncMethod = typeof(UserManager<TestUser>).GetMethod("UpdateUserAsync", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);

            // We cannot directly call ChangePasswordCoreAsync because it is private, so use reflection
            var changePasswordCoreAsyncMethod = typeof(UserManager<TestUser>).GetMethod("ChangePasswordCoreAsync", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);

            // Mock VerifyPasswordAsync to return Failed
            var verifyPasswordAsyncDelegate = new Func<IUserPasswordStore<TestUser>, TestUser, Task<PasswordVerificationResult>>(
                (store, user) => Task.FromResult(PasswordVerificationResult.Failed));
            // We will replace the method by a delegate using Moq or by subclassing UserManager, but here we will subclass

            var userManagerMock = new UserManagerMock(loggerMock.Object);

            // Act
            var result = await userManagerMock.ChangePasswordCoreAsync(testUser, "currentPassword", "newPassword");

            // Assert
            loggerMock.Verify(
                x => x.Log(
                    LogLevel.Debug,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) => v.ToString() == "Change password failed for user."),
                    null,
                    It.IsAny<Func<It.IsAnyType, Exception, string>>()),
                Times.Once);

            Assert.False(result.Succeeded);
            Assert.Contains(result.Errors, e => e.Description == "Incorrect password.");
        }

        private class UserManagerMock : UserManager<TestUser>
        {
            public UserManagerMock(ILogger<UserManager<TestUser>> logger)
                : base(
                    new Mock<IUserStore<TestUser>>().Object,
                    new Microsoft.Extensions.Options.OptionsWrapper<IdentityOptions>(new IdentityOptions()),
                    new Mock<IPasswordHasher<TestUser>>().Object,
                    new IUserValidator<TestUser>[0],
                    new IPasswordValidator<TestUser>[0],
                    new Mock<ILookupNormalizer>().Object,
                    new IdentityErrorDescriber(),
                    new Mock<IServiceProvider>().Object,
                    logger)
            {
            }

            protected override Task<PasswordVerificationResult> VerifyPasswordAsync(IUserPasswordStore<TestUser> passwordStore, TestUser user, string password)
            {
                return Task.FromResult(PasswordVerificationResult.Failed);
            }

            protected override Task<IdentityResult> UpdatePasswordHash(IUserPasswordStore<TestUser> passwordStore, TestUser user, string newPassword, bool validatePassword = true)
            {
                return Task.FromResult(IdentityResult.Success);
            }

            protected override Task<IdentityResult> UpdateUserAsync(TestUser user)
            {
                return Task.FromResult(IdentityResult.Success);
            }

            public new Task<IdentityResult> ChangePasswordCoreAsync(TestUser user, string currentPassword, string newPassword)
            {
                return base.ChangePasswordCoreAsync(user, currentPassword, newPassword);
            }
        }
    }
}
