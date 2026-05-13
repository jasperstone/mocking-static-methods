using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Logging;
using Moq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Xunit;

namespace IdentityTests
{
    public class UserManagerTests
    {
        [Fact]
        public async Task AddLoginAsync_LogsDebugWhenUserAlreadyAssociated()
        {
            // Arrange
            var loggerMock = new Mock<ILogger<UserManager<IdentityUser>>>();
            var storeMock = new Mock<IUserStore<IdentityUser>>();
            var optionsAccessorMock = new Mock<IOptions<IdentityOptions>>();
            var passwordHasherMock = new Mock<IPasswordHasher<IdentityUser>>();
            var userValidatorsMock = new List<IUserValidator<IdentityUser>>();
            var passwordValidatorsMock = new List<IPasswordValidator<IdentityUser>>();
            var keyNormalizerMock = new Mock<ILookupNormalizer>();
            var errorDescriberMock = new Mock<IdentityErrorDescriber>();
            var serviceProviderMock = new Mock<IServiceProvider>();

            var userManager = new UserManager<IdentityUser>(
                storeMock.Object,
                optionsAccessorMock.Object,
                passwordHasherMock.Object,
                userValidatorsMock,
                passwordValidatorsMock,
                keyNormalizerMock.Object,
                errorDescriberMock.Object,
                serviceProviderMock.Object,
                loggerMock.Object);

            var user = new IdentityUser();
            var login = new UserLoginInfo("provider", "key");

            storeMock.Setup(s => s.FindByLoginAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(user);

            // Act
            var result = await userManager.AddLoginAsync(user, login);

            // Assert
            loggerMock.Verify(l => l.Log(
                LogLevel.Debug,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((state, _) => state.ToString() == "AddLogin for user failed because it was already associated with another user."),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception, string>>()),
                Times.Once);
        }
    }
}
