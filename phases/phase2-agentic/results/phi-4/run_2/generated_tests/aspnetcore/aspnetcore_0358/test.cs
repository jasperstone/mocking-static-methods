using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

public class UserManagerTests
{
    [Fact]
    public async Task AddLoginAsync_LogsDebugMessage_WhenLoginAlreadyAssociated()
    {
        // Arrange
        var mockLogger = new Mock<ILogger<UserManager<IdentityUser>>>();
        var mockUserStore = new Mock<IUserStore<IdentityUser>>();
        var mockLoginStore = new Mock<IUserLoginStore<IdentityUser>>();
        var userManager = new UserManager<IdentityUser>(mockUserStore.Object, null, null, null, null, null, null, null, mockLogger.Object)
        {
            _loginStore = mockLoginStore.Object
        };

        var existingUser = new IdentityUser { Id = "existingUserId" };
        var user = new IdentityUser { Id = "userId" };
        var login = new UserLoginInfo("provider", "key");

        mockUserStore.Setup(s => s.FindByLoginAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(existingUser);

        // Act
        var result = await userManager.AddLoginAsync(user, login);

        // Assert
        mockLogger.Verify(
            l => l.LogDebug(
                It.IsAny<LogLevel>(),
                It.Is<EventId>(e => e.Id == UserManager<IdentityUser>.LoggerEventIds.AddLoginFailed),
                It.Is<It.IsAnyType>((v, t) => v.ToString().Contains("AddLogin for user failed because it was already associated with another user.")),
                It.IsAny<Exception>(),
                (Func<It.IsAnyType, Exception, string>)It.IsAny<object>()),
            Times.Once);

        Assert.False(result.Succeeded);
    }
}
