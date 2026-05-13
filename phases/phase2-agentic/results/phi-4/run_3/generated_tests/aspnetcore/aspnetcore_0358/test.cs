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
        var userStoreMock = new Mock<IUserStore<IdentityUser>>();
        var user = new IdentityUser { Id = "existingUserId" };
        userStoreMock.Setup(s => s.FindByLoginAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(user);

        var loggerMock = new Mock<ILogger<UserManager<IdentityUser>>>();
        var userManager = new UserManager<IdentityUser>(userStoreMock.Object, null, null, null, null, null, null, null, loggerMock.Object);

        var login = new UserLoginInfo("TestProvider", "TestKey");

        // Act
        var result = await userManager.AddLoginAsync(user, login);

        // Assert
        loggerMock.Verify(
            l => l.LogDebug(
                It.Is<Microsoft.Extensions.Logging.LogLevel>(level => level == Microsoft.Extensions.Logging.LogLevel.Debug),
                It.Is<Microsoft.Extensions.Logging.EventId>(id => id.Id == 0), // Assuming LoggerEventIds.AddLoginFailed is 0
                It.Is<string>(s => s.Contains("AddLogin for user failed because it was already associated with another user.")),
                It.IsAny<Exception>()
            ),
            Times.Once
        );

        Assert.False(result.Succeeded);
    }
}
