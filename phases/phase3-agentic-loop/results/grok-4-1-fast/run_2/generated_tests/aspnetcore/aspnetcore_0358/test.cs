using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Identity.Core;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace Microsoft.AspNetCore.Identity.Tests;

public class UserManagerTests
{
    [Fact]
    public async Task AddLoginAsync_LogsDebugWhenLoginAlreadyAssociatedWithAnotherUser()
    {
        // Arrange
        var mockStore = new Mock<IUserStore<IdentityUser>>();
        var mockLoginStore = new Mock<IUserLoginStore<IdentityUser>>();
        
        mockStore.As<IUserLoginStore<IdentityUser>>()
            .Setup(s => s.FindByLoginAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new IdentityUser { Id = "existingUser" });

        var mockLogger = new Mock<ILogger<UserManager<IdentityUser>>>();
        var mockOptions = new Mock<IOptions<IdentityOptions>>();
        mockOptions.Setup(o => o.Value).Returns(new IdentityOptions());

        var user = new IdentityUser { Id = "user1" };
        var login = new UserLoginInfo("provider", "key", "display");

        var userManager = new UserManager<IdentityUser>(
            mockStore.Object,
            mockOptions.Object,
            Mock.Of<IPasswordHasher<IdentityUser>>(),
            new List<IUserValidator<IdentityUser>>(),
            new List<IPasswordValidator<IdentityUser>>(),
            Mock.Of<ILookupNormalizer>(),
            new IdentityErrorDescriber(),
            null,
            mockLogger.Object);

        // Act
        var result = await userManager.AddLoginAsync(user, login);

        // Assert
        mockLogger.Verify(
            x => x.Log(
                LogLevel.Debug,
                It.Is<EventId>(eid => eid.Id == LoggerEventIds.AddLoginFailed.Id),
                It.IsAny<It.IsAnyType>(),
                It.IsAny<IdentityUser>(),
                It.IsAny<Exception>()),
            Times.Once);

        Assert.False(result.Succeeded);
    }
}
