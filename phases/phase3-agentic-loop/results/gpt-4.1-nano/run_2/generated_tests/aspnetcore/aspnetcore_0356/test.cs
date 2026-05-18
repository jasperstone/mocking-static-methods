using System;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;
using Microsoft.AspNetCore.Identity;

namespace UserManagerTests
{
    public class ChangePasswordAsyncTests
    {
        [Fact]
        public async Task ChangePasswordAsync_LogsDebug_WhenChangePasswordFails()
        {
            // Arrange
            var user = new object() as dynamic; // replace with actual user type if known
            var storeMock = new Mock<IUserStore<object>>();
            var loggerMock = new Mock<ILogger<UserManager<object>>>();
            var userManager = new TestUserManager(storeMock.Object, loggerMock.Object);
            userManager.SetChangePasswordResult(IdentityResult.Failed);

            // Act
            await Assert.ThrowsAsync<IdentityException>(() => userManager.ChangePasswordAsync(user, "oldPassword", "newPassword"));

            // Assert
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

    // Subclass UserManager to override ChangePasswordCoreAsync for testing
    public class TestUserManager : UserManager<object>
    {
        private IdentityResult _changePasswordResult;

        public TestUserManager(IUserStore<object> store, ILogger<UserManager<object>> logger)
            : base(store, Options.Create(new IdentityOptions()), new PasswordHasher<object>(), Array.Empty<IUserValidator<object>>(), Array.Empty<IPasswordValidator<object>>(), new UpperInvariantLookupNormalizer(), new IdentityErrorDescriber(), new ServiceCollection().BuildServiceProvider(), logger)
        {
        }

        public void SetChangePasswordResult(IdentityResult result)
        {
            _changePasswordResult = result;
        }

        private new async Task<IdentityResult> ChangePasswordCoreAsync(object user, string currentPassword, string newPassword)
        {
            return await Task.FromResult(_changePasswordResult);
        }
    }
}
