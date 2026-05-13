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

            // Setup VerifyPasswordAsync to return Failed
            var passwordStore = userStoreMock.As<IUserPasswordStore<TestUser>>();
            userManager.GetType().GetMethod("GetPasswordStore", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)
                .Invoke(userManager, null);

            // We need to mock VerifyPasswordAsync to return Failed
            // Since VerifyPasswordAsync is private, we simulate by mocking passwordStore and passwordHasher behavior
            // But since VerifyPasswordAsync is private and calls passwordHasher.VerifyHashedPassword, we can mock passwordHasher

            passwordHasherMock.Setup(ph => ph.VerifyHashedPassword(user, It.IsAny<string>(), It.IsAny<string>()))
                .Returns(PasswordVerificationResult.Failed);

            // We also need to mock GetPasswordStore to return our passwordStore mock
            // But GetPasswordStore is private, so we use reflection to set Store to passwordStoreMock.Object
            userManager.Store = userStoreMock.Object;

            // Act
            var method = typeof(UserManager<TestUser>).GetMethod("ChangePasswordCoreAsync", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            var task = (Task<IdentityResult>)method.Invoke(userManager, new object[] { user, "currentPassword", "newPassword" });
            var result = await task;

            // Assert
            Assert.False(result.Succeeded);
            loggerMock.Verify(
                x => x.Log(
                    LogLevel.Debug,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) => v.ToString().Contains("Change password failed for user.")),
                    null,
                    It.IsAny<Func<It.IsAnyType, Exception, string>>()),
                Times.Once);
        }
    }
}
