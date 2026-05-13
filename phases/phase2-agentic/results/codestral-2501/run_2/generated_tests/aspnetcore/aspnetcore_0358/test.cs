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
    public async Task AddLoginAsync_ExistingUser_LogsDebugMessage()
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
        var loginInfo = new UserLoginInfo("provider", "key", "displayName");

        userStoreMock.Setup(store => store.FindByLoginAsync("provider", "key", CancellationToken.None))
                     .ReturnsAsync(user);

        // Act
        await userManager.AddLoginAsync(user, loginInfo);

        // Assert
        loggerMock.Verify(
            logger => logger.Log(
                LogLevel.Debug,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => true),
                It.IsAny<Exception>(),
                It.Is<Func<It.IsAnyType, Exception, string>>((v, t) => true)),
            Times.Once);
    }
}
