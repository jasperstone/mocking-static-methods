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
        public async Task ChangePasswordCoreAsync_LogsChangePasswordFailed_WhenPasswordChangeFails()
        {
            // Arrange
            var loggerMock = new Mock<ILogger<UserManager<IdentityUser>>>();
            var storeMock = new Mock<IUserStore<IdentityUser>>();
            var optionsMock = new Mock<IOptions<IdentityOptions>>();
            var passwordHasherMock = new Mock<IPasswordHasher<IdentityUser>>();
            var keyNormalizerMock = new Mock<ILookupNormalizer>();
            var errorDescriberMock = new Mock<IdentityErrorDescriber>();
            var serviceProviderMock = new Mock<IServiceProvider>();

            var userManager = new UserManager<IdentityUser>(storeMock.Object, 
                optionsMock.Object, 
                passwordHasherMock.Object, 
                new List<IUserValidator<IdentityUser>>(), 
                new List<IPasswordValidator<IdentityUser>>(), 
                keyNormalizerMock.Object, 
                errorDescriberMock.Object, 
                serviceProviderMock.Object, 
                loggerMock.Object);

            // Act
            var result = await userManager.ChangePasswordCoreAsync(new IdentityUser(), "currentPassword", "newPassword");

            // Assert
            loggerMock.Verify(l => l.Log(
                LogLevel.Debug, 
                LoggerEventIds.ChangePasswordFailed, 
                It.IsAny<object>(), 
                It.IsAny<Exception>(), 
                (Func<It.IsAnyType, Exception, string>)(v, t) => true), 
                Times.Once);
        }
    }
}
