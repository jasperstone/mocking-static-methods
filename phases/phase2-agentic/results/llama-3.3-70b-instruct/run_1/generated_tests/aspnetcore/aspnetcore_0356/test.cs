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
            var passwordStore = Mock.Of<IUserPasswordStore<IdentityUser>>();
            Mock.Get(passwordStore).Setup(ps => ps.GetPasswordHashAsync(user, default)).ReturnsAsync("hash");
            Mock.Get(userManager).Setup(um => um.GetPasswordStore()).Returns(passwordStore);

            // Act
            var result = await userManager.ChangePasswordCoreAsync(user, "wrongpassword", "newpassword");

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
