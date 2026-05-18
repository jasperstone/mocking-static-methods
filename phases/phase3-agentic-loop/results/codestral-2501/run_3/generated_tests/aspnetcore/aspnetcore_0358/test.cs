using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace Microsoft.AspNetCore.Identity.Tests
{
    public class UserManagerTests
    {
        private readonly Mock<IUserStore<IdentityUser>> _userStoreMock;
        private readonly Mock<ILogger<UserManager<IdentityUser>>> _loggerMock;
        private readonly UserManager<IdentityUser> _userManager;

        public UserManagerTests()
        {
            _userStoreMock = new Mock<IUserStore<IdentityUser>>();
            _loggerMock = new Mock<ILogger<UserManager<IdentityUser>>>();
            _userManager = new UserManager<IdentityUser>(
                _userStoreMock.Object,
                null,
                null,
                null,
                null,
                null,
                new IdentityErrorDescriber(),
                null,
                _loggerMock.Object);
        }

        [Fact]
        public async Task AddLoginAsync_ExistingUser_LogsDebugMessage()
        {
            // Arrange
            var user = new IdentityUser { UserName = "testuser" };
            var login = new UserLoginInfo("testprovider", "testkey", "testdisplay");

            _userStoreMock.Setup(store => store.FindByLoginAsync(It.IsAny<string>(), It.IsAny<string>(), default))
                .ReturnsAsync(user);

            // Act
            var result = await _userManager.AddLoginAsync(user, login);

            // Assert
            _loggerMock.Verify(
                logger => logger.Log(
                    LogLevel.Debug,
                    LoggerEventIds.AddLoginFailed,
                    It.IsAny<It.IsAnyType>(),
                    null,
                    (Func<It.IsAnyType, Exception, string>)It.IsAny<object>()),
                Times.Once);

            Assert.False(result.Succeeded);
            Assert.Contains(result.Errors, error => error.Code == "LoginAlreadyAssociated");
        }
    }
}
