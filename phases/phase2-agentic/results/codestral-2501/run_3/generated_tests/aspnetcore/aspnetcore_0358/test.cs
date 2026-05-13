using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Moq;
using Xunit;

public class UserManagerTests
{
    [Fact]
    public async Task AddLoginAsync_WhenLoginAlreadyAssociated_LogsDebugMessage()
    {
        // Arrange
        var userStoreMock = new Mock<IUserStore<IdentityUser>>();
        var optionsAccessorMock = new Mock<IOptions<IdentityOptions>>();
        var passwordHasherMock = new Mock<IPasswordHasher<IdentityUser>>();
        var userValidatorsMock = new Mock<IEnumerable<IUserValidator<IdentityUser>>>();
        var passwordValidatorsMock = new Mock<IEnumerable<IPasswordValidator<IdentityUser>>>();
        var keyNormalizerMock = new Mock<ILookupNormalizer>();
        var errorDescriberMock = new Mock<IdentityErrorDescriber>();
        var serviceProviderMock = new Mock<IServiceProvider>();
        var loggerMock = new Mock<ILogger<UserManager<IdentityUser>>>();

        var userManager = new UserManager<IdentityUser>(
            userStoreMock.Object,
            optionsAccessorMock.Object,
            passwordHasherMock.Object,
            userValidatorsMock.Object,
            passwordValidatorsMock.Object,
            keyNormalizerMock.Object,
            errorDescriberMock.Object,
            serviceProviderMock.Object,
            loggerMock.Object);

        var user = new IdentityUser();
        var login = new UserLoginInfo("provider", "key", "displayName");

        userStoreMock.Setup(store => store.FindByLoginAsync("provider", "key", CancellationToken.None))
            .ReturnsAsync(user);

        // Act
        await userManager.AddLoginAsync(user, login);

        // Assert
        loggerMock.Verify(
            logger => logger.LogDebug(
                LoggerEventIds.AddLoginFailed,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString().Contains("AddLogin for user failed because it was already associated with another user.")),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception, string>>()),
            Times.Once);
    }
}
