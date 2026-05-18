using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace Microsoft.AspNetCore.Identity.Test
{
    public class UserManagerLoggerExtensionsTests
    {
        private class TestUser { }

        [Fact]
        public async Task ChangePasswordCoreAsync_LogsDebug_WhenPasswordVerificationFails()
        {
            // Arrange
            var user = new TestUser();
            var loggerMock = new Mock<ILogger<UserManager<TestUser>>>();
            var storeMock = new Mock<IUserPasswordStore<TestUser>>();
            var passwordHasherMock = new Mock<IPasswordHasher<TestUser>>();
            var userValidators = Array.Empty<IUserValidator<TestUser>>();
            var passwordValidators = Array.Empty<IPasswordValidator<TestUser>>();
            var keyNormalizerMock = new Mock<ILookupNormalizer>();
            var errorDescriber = new IdentityErrorDescriber();
            var servicesMock = new Mock<IServiceProvider>();

            // Setup store to return a password hash
            storeMock.Setup(s => s.GetPasswordHashAsync(user, It.IsAny<CancellationToken>())).ReturnsAsync("hashedPassword");
            // Setup password hasher to return Failed verification
            passwordHasherMock.Setup(p => p.VerifyHashedPassword(user, "hashedPassword", "currentPassword")).Returns(PasswordVerificationResult.Failed);

            var userManager = new UserManager<TestUser>(
                storeMock.Object,
                new Microsoft.Extensions.Options.OptionsWrapper<IdentityOptions>(new IdentityOptions()),
                passwordHasherMock.Object,
                userValidators,
                passwordValidators,
                keyNormalizerMock.Object,
                errorDescriber,
                servicesMock.Object,
                loggerMock.Object);

            // Act
            var changePasswordCoreAsyncMethod = typeof(UserManager<TestUser>).GetMethod("ChangePasswordCoreAsync", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            var resultTask = (Task<IdentityResult>)changePasswordCoreAsyncMethod.Invoke(userManager, new object[] { user, "currentPassword", "newPassword" });
            var result = await resultTask;

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
