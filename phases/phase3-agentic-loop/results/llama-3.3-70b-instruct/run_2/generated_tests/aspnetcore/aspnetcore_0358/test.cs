using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Logging;
using Moq;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Xunit;

namespace IdentityTests
{
    public class UserManagerTests
    {
        [Fact]
        public async Task AddLoginAsync_LogsDebugMessage_WhenLoginAlreadyAssociated()
        {
            // Arrange
            var loggerMock = new Mock<ILogger<UserManager<IdentityUser>>>();
            var storeMock = new Mock<IUserStore<IdentityUser>>();
            storeMock.Setup(s => s.FindByLoginAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(new IdentityUser { Id = "1" });

            var userManager = new UserManager<IdentityUser>(storeMock.Object, 
                Mock.Of<IOptions<IdentityOptions>>(), 
                Mock.Of<IPasswordHasher<IdentityUser>>(), 
                new List<IUserValidator<IdentityUser>>(), 
                new List<IPasswordValidator<IdentityUser>>(), 
                Mock.Of<ILookupNormalizer>(), 
                new IdentityErrorDescriber(), 
                Mock.Of<IServiceProvider>(), 
                loggerMock.Object);

            var user = new IdentityUser { Id = "2" };
            var login = new UserLoginInfo("Facebook", "12345", "Facebook");

            // Act
            var result = await userManager.AddLoginAsync(user, login);

            // Assert
            loggerMock.Verify(l => l.Log(
                LogLevel.Debug, 
                It.IsAny<EventId>(), 
                It.IsAny<It.IsAnyType>(), 
                It.IsAny<Exception>(), 
                (Func<It.IsAnyType, Exception, string>)((state, exception) => "")), 
                Times.Once);
        }

        [Fact]
        public async Task AddLoginAsync_ReturnsFailedResult_WhenLoginAlreadyAssociated()
        {
            // Arrange
            var storeMock = new Mock<IUserStore<IdentityUser>>();
            storeMock.Setup(s => s.FindByLoginAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(new IdentityUser { Id = "1" });

            var userManager = new UserManager<IdentityUser>(storeMock.Object, 
                Mock.Of<IOptions<IdentityOptions>>(), 
                Mock.Of<IPasswordHasher<IdentityUser>>(), 
                new List<IUserValidator<IdentityUser>>(), 
                new List<IPasswordValidator<IdentityUser>>(), 
                Mock.Of<ILookupNormalizer>(), 
                new IdentityErrorDescriber(), 
                Mock.Of<IServiceProvider>(), 
                Mock.Of<ILogger<UserManager<IdentityUser>>>());

            var user = new IdentityUser { Id = "2" };
            var login = new UserLoginInfo("Facebook", "12345", "Facebook");

            // Act
            var result = await userManager.AddLoginAsync(user, login);

            // Assert
            Assert.False(result.Succeeded);
        }
    }
}
