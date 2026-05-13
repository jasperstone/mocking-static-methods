using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Logging;
using Moq;
using System;
using System.Collections.Generic;
using System.Linq;
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
            var userManager = new UserManager<IdentityUser>(
                new Mock<IUserStore<IdentityUser>>().Object,
                new Mock<IOptions<IdentityOptions>>().Object,
                new Mock<IPasswordHasher<IdentityUser>>().Object,
                new List<IUserValidator<IdentityUser>>(),
                new List<IPasswordValidator<IdentityUser>>(),
                new Mock<ILookupNormalizer>().Object,
                new Mock<IdentityErrorDescriber>().Object,
                new Mock<IServiceProvider>().Object,
                loggerMock.Object);

            var user = new IdentityUser();
            var login = new UserLoginInfo("provider", "key");

            // Act
            var result = await userManager.AddLoginAsync(user, login);

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
