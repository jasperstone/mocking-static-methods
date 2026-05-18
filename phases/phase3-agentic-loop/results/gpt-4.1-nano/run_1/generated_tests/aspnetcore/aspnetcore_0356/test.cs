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
        public async Task ChangePasswordAsync_CallsLogDebug_WhenChangePasswordFails()
        {
            // Arrange
            var user = new object();
            var userStoreMock = new Mock<IUserStore<object>>();
            var loggerMock = new Mock<ILogger<UserManager<object>>>();
            var passwordHasherMock = new Mock<IPasswordHasher<object>>();
            var options = new IdentityOptions();
            var errors = new IdentityErrorDescriber();
            var services = new ServiceCollection().BuildServiceProvider();

            var userManager = new UserManager<object>(
                userStoreMock.Object,
                Options.Create(options),
                passwordHasherMock.Object,
                Array.Empty<IUserValidator<object>>(),
                Array.Empty<IPasswordValidator<object>>(),
                null,
                errors,
                services,
                loggerMock.Object);

            // Mock ChangePasswordCoreAsync to return IdentityResult.Failed
            var mockUserManager = new Mock<UserManager<object>>(userStoreMock.Object, Options.Create(options), passwordHasherMock.Object, Array.Empty<IUserValidator<object>>(), Array.Empty<IPasswordValidator<object>>(), null, errors, services, loggerMock.Object);
            mockUserManager.CallBase = true;
            mockUserManager.Setup(m => m.ChangePasswordCoreAsync(It.IsAny<object>(), It.IsAny<string>(), It.IsAny<string>()))
                .ReturnsAsync(IdentityResult.Failed());

            // Act
            await mockUserManager.Object.ChangePasswordAsync(user, "current", "new");

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
}
