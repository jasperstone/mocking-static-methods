using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

public class UserManagerTests
{
    [Fact]
    public async Task AddLoginAsync_LogsDebugMessage_WhenLoginAlreadyExists()
    {
        // Arrange
        var mockLogger = new Mock<ILogger<UserManager<IdentityUser>>>();
        var mockUserStore = new Mock<IUserStore<IdentityUser>>();
        var mockLoginStore = new Mock<IUserLoginStore<IdentityUser>>();
        var mockMetrics = new Mock<UserManagerMetrics>();

        var user = new IdentityUser { Id = "1" };
        var login = new UserLoginInfo("provider", "key");

        mockUserStore.Setup(s => s.FindByIdAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(user);

        mockLoginStore.Setup(s => s.GetLoginsAsync(It.IsAny<IdentityUser>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<UserLoginInfo> { login });

        var options = new IdentityOptions();
        var passwordHasher = new PasswordHasher<IdentityUser>();
        var userValidators = new List<IUserValidator<IdentityUser>>();
        var passwordValidators = new List<IPasswordValidator<IdentityUser>>();
        var keyNormalizer = new UpperInvariantLookupNormalizer();
        var errorDescriber = new IdentityErrorDescriber();
        var services = null; // Not used in this test

        var userManager = new UserManager<IdentityUser>(
            mockUserStore.Object,
            new Microsoft.Extensions.Options.OptionsWrapper<IdentityOptions>(options),
            passwordHasher,
            userValidators,
            passwordValidators,
            keyNormalizer,
            errorDescriber,
            services,
            mockLogger.Object)
        {
            _metrics = mockMetrics.Object
        };

        // Act
        var result = await userManager.AddLoginAsync(user, login);

        // Assert
        mockLogger.Verify(
            l => l.LogDebug(
                It.IsAny<Microsoft.Extensions.Logging.EventId>(),
                It.Is<string>(s => s.Contains("AddLogin for user failed because it was already associated with another user.")),
                It.IsAny<object[]>()),
            Times.Once);

        Assert.False(result.Succeeded);
    }
}
