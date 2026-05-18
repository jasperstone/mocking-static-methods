using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Moq;
using Xunit;

namespace Microsoft.AspNetCore.Identity;

public class UserManagerTests
{
    [Fact]
    public async Task AddLoginAsync_LogsDebug_WhenLoginAlreadyAssociatedWithAnotherUser()
    {
        // Arrange
        var store = new Mock<IUserStore<IdentityUser>>();
        var loginStore = new Mock<IUserLoginStore<IdentityUser>>();
        store.As<IUserLoginStore<IdentityUser>>().Setup(s => s).Returns(loginStore.Object);
        var logger = new Mock<ILogger<UserManager<IdentityUser>>>();
        var options = Options.Create(new IdentityOptions());
        var passwordHasher = new Mock<IPasswordHasher<IdentityUser>>();
        var userValidators = new List<IUserValidator<IdentityUser>>();
        var passwordValidators = new List<IPasswordValidator<IdentityUser>>();
        var keyNormalizer = new Mock<ILookupNormalizer>();
        var errors = new IdentityErrorDescriber();
        var services = new ServiceCollection().BuildServiceProvider();

        var manager = new UserManager<IdentityUser>(
            store.Object,
            options,
            passwordHasher.Object,
            userValidators,
            passwordValidators,
            keyNormalizer.Object,
            errors,
            services,
            logger.Object);

        var user = new IdentityUser { Id = "user1" };
        var existingUser = new IdentityUser { Id = "user2" };
        var login = new UserLoginInfo("provider", "key", "display");

        loginStore.Setup(ls => ls.FindByLoginAsync("provider", "key", It.IsAny<CancellationToken>()))
                  .ReturnsAsync(existingUser);

        // Act
        var result = await manager.AddLoginAsync(user, login);

        // Assert
        Assert.False(result.Succeeded);
        logger.Verify(
            l => l.Log(
                LogLevel.Debug,
                It.Is<EventId>(eid => eid.Id == 4),
                It.IsAny<It.IsAnyType>(),
                "AddLogin for user failed because it was already associated with another user.",
                It.IsAny<object[]>()),
            Times.Once);
    }
}
