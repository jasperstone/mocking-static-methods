using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Moq;
using Moq.Language.Flow;
using Xunit;

namespace Microsoft.AspNetCore.Identity;

public class UserManagerAddLoginTests
{
    [Fact]
    public async Task AddLoginAsync_LogsDebug_WhenLoginAlreadyAssociatedWithAnotherUser()
    {
        // Arrange
        var user1 = new IdentityUser { Id = "user1" };
        var user2 = new IdentityUser { Id = "user2" };
        var login = new UserLoginInfo("provider", "key", "display");

        var store = new Mock<IUserStore<IdentityUser>>();
        var loginStore = new Mock<IUserLoginStore<IdentityUser>>();
        store.Setup(s => s.GetUserLoginStore()).Returns(loginStore.Object);
        loginStore.Setup(s => s.FindByLoginAsync("provider", "key", It.IsAny<CancellationToken>()))
                  .ReturnsAsync(user1);

        var logger = new Mock<ILogger<UserManager<IdentityUser>>>();
        logger.Setup(l => l.IsEnabled(It.Is<LogLevel>(level => level == LogLevel.Debug))).Returns(true);
        logger.Setup(l => l.LogDebug(
            It.Is<EventId>(id => id.Id == 4 && id.Name == "AddLoginFailed"),
            It.Is<string>(msg => msg == "AddLogin for user failed because it was already associated with another user.")));

        var services = new ServiceCollection();
        services.AddLogging();
        var serviceProvider = services.BuildServiceProvider();

        var options = new IdentityOptions();
        var optionsMonitor = new Mock<IOptionsMonitor<IdentityOptions>>();
        optionsMonitor.Setup(o => o.CurrentValue).Returns(options);

        var passwordHasher = new Mock<IPasswordHasher<IdentityUser>>().Object;
        var userValidators = new List<IUserValidator<IdentityUser>>();
        var passwordValidators = new List<IPasswordValidator<IdentityUser>>();
        var keyNormalizer = new Mock<ILookupNormalizer>().Object;
        var errorDescriber = new IdentityErrorDescriber();

        var userManager = new UserManager<IdentityUser>(
            store.Object,
            optionsMonitor.Object,
            passwordHasher,
            userValidators,
            passwordValidators,
            keyNormalizer.Object,
            errorDescriber,
            serviceProvider,
            logger.Object);

        // Act
        var result = await userManager.AddLoginAsync(user2, login);

        // Assert
        Assert.False(result.Succeeded);
        logger.Verify(
            l => l.LogDebug(
                It.Is<EventId>(id => id.Id == 4 && id.Name == "AddLoginFailed"),
                It.Is<string>(msg => msg == "AddLogin for user failed because it was already associated with another user.")),
            Times.Once);
    }
}
