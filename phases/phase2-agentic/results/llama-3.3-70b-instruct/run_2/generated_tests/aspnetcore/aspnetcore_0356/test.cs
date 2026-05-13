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
            var userManager = new UserManager<IdentityUser>(Mock.Of<IUserStore<IdentityUser>>(), 
                Mock.Of<IOptions<IdentityOptions>>(), 
                Mock.Of<IPasswordHasher<IdentityUser>>(), 
                new List<IUserValidator<IdentityUser>>(), 
                new List<IPasswordValidator<IdentityUser>>(), 
                Mock.Of<ILookupNormalizer>(), 
                new IdentityErrorDescriber(), 
                Mock.Of<IServiceProvider>(), 
                loggerMock.Object);

            var user = new IdentityUser { Id = "1" };
            var currentPassword = "currentPassword";
            var newPassword = "newPassword";

            // Act
            var result = await userManager.ChangePasswordCoreAsync(user, currentPassword, newPassword);

            // Assert
            loggerMock.Verify(l => l.Log(
                LogLevel.Debug, 
                It.IsAny<EventId>(), 
                It.Is<It.IsAnyType>((v, _) => true), 
                It.IsAny<Exception>(), 
                It.Is<Func<It.IsAnyType, Exception, string>>((v, _) => true)), 
                Times.Once);
        }
    }
}
