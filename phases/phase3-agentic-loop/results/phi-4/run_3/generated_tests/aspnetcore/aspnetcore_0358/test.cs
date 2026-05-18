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
    public async Task AddLoginAsync_LogsDebugWhenUserAlreadyHasLogin()
    {
        // Arrange
        var mockLogger = new Mock<ILogger<UserManager<IdentityUser>>>();
        var mockUserStore = new Mock<IUserStore<IdentityUser>>();
        var mockLoginStore = new Mock<IUserLoginStore<IdentityUser>>();
        var userManager = new UserManager<IdentityUser>(
            mockUserStore.Object,
            Mock.Of<IOptions<IdentityOptions>>(),
            Mock.Of<IPasswordHasher<IdentityUser>>(),
            Array.Empty<IUserValidator<IdentityUser>>(),
            Array.Empty<IPasswordValidator<IdentityUser>>(),
            Mock.Of<ILookupNormalizer>(),
            Mock.Of<IdentityErrorDescriber>(),
            Mock.Of<IServiceProvider>(),
            mockLogger.Object);

        var user = new IdentityUser { Id = "userId" };
        var login = new UserLoginInfo("provider", "providerKey");

        mockUserStore.Setup(s => s.FindByIdAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(user);

        mockLoginStore.Setup(s => s.FindByLoginAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(user);

        // Act
        await userManager.AddLoginAsync(user, login);

        // Assert
        mockLogger.Verify(
            l => l.LogDebug(
                It.IsAny<LogLevel>(),
                It.Is<EventId>(e => e.Id == 1001), // Use the correct EventId
                It.IsAny<object>(),
                It.IsAny<Exception>(),
                (Func<object, Exception, string>)It.IsAny<object>()),
            Times.Once);
    }
}
