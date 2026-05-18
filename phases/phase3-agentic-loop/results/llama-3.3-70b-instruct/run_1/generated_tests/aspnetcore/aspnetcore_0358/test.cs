using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Logging;
using Moq;
using System;
using System.Threading;
using System.Threading.Tasks;
using Xunit;

public class UserManagerTests
{
    [Fact]
    public async Task AddLoginAsync_LogsDebugMessage_WhenLoginAlreadyAssociated()
    {
        // Arrange
        var loggerMock = new Mock<ILogger<UserManager<IdentityUser>>>();
        var storeMock = new Mock<IUserStore<IdentityUser>>();
        var optionsMock = new Mock<IOptions<IdentityOptions>>();
        var passwordHasherMock = new Mock<IPasswordHasher<IdentityUser>>();
        var keyNormalizerMock = new Mock<ILookupNormalizer>();
        var errorDescriberMock = new Mock<IdentityErrorDescriber>();
        var servicesMock = new Mock<IServiceProvider>();

        var userManager = new UserManager<IdentityUser>(
            storeMock.Object,
            optionsMock.Object,
            passwordHasherMock.Object,
            new List<IUserValidator<IdentityUser>>(),
            new List<IPasswordValidator<IdentityUser>>(),
            keyNormalizerMock.Object,
            errorDescriberMock.Object,
            servicesMock.Object,
            loggerMock.Object);

        var user = new IdentityUser();
        var login = new UserLoginInfo("provider", "key");

        storeMock.Setup(s => s.FindByLoginAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new IdentityUser());

        // Act
        var result = await userManager.AddLoginAsync(user, login);

        // Assert
        loggerMock.Verify(l => l.Log(
            LogLevel.Debug,
            It.IsAny<EventId>(),
            It.IsAny<It.IsAnyType>(),
            It.IsAny<Exception>(),
            (state, exception) => state.ToString()), Times.Once);
    }
}
