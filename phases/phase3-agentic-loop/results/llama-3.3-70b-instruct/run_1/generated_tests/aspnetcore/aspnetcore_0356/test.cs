using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Logging;
using Moq;
using System;
using System.Threading.Tasks;
using Xunit;

namespace IdentityTests
{
    public class UserManagerTests
    {
        [Fact]
        public async Task ChangePasswordCoreAsync_LogsDebugWhenPasswordVerificationFails()
        {
            // Arrange
            var userStoreMock = new Mock<IUserStore<IdentityUser>>();
            var passwordHasherMock = new Mock<IPasswordHasher<IdentityUser>>();
            var userValidatorsMock = new Mock<IEnumerable<IUserValidator<IdentityUser>>>();
            var passwordValidatorsMock = new Mock<IEnumerable<IPasswordValidator<IdentityUser>>>();
            var keyNormalizerMock = new Mock<ILookupNormalizer>();
            var errorDescriberMock = new Mock<IdentityErrorDescriber>();
            var serviceProviderMock = new Mock<IServiceProvider>();
            var loggerMock = new Mock<ILogger<UserManager<IdentityUser>>>();

            var userManager = new UserManager<IdentityUser>(
                userStoreMock.Object,
                new Mock<IOptions<IdentityOptions>>().Object,
                passwordHasherMock.Object,
                userValidatorsMock.Object,
                passwordValidatorsMock.Object,
                keyNormalizerMock.Object,
                errorDescriberMock.Object,
                serviceProviderMock.Object,
                loggerMock.Object);

            var user = new IdentityUser { Id = "1" };
            var currentPassword = "currentPassword";
            var newPassword = "newPassword";

            // Act
            await userManager.ChangePasswordAsync(user, currentPassword, newPassword);

            // Assert
            loggerMock.Verify(l => l.Log(
                LogLevel.Debug,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => true),
                It.IsAny<Exception>(),
                It.Is<Func<It.IsAnyType, Exception, string>>((v, t) => true)), Times.Once);
        }
    }
}
