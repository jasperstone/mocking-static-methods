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
        public async Task ChangePasswordCoreAsync_LogsDebug_WhenPasswordVerificationFails()
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

            var user = new IdentityUser { Id = "1", UserName = "user" };
            storeMock.Setup(s => s.FindByNameAsync(It.IsAny<string>(), It.IsAny<CancellationToken>())).ReturnsAsync(user);

            // Act
            var result = await userManager.ChangePasswordCoreAsync(user, "incorrectPassword", "newPassword");

            // Assert
            loggerMock.Verify(l => l.LogDebug(It.IsAny<EventId>(), It.IsAny<string>()), Times.Once);
        }
    }
}
