using Xunit;
using Moq;
using Microsoft.AspNetCore.Identity;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;

namespace IdentityTests
{
    public class UserManagerTests
    {
        [Fact]
        public async Task AddLoginAsync_LogsDebugWhenLoginAlreadyAssociated()
        {
            // Arrange
            var storeMock = new Mock<IUserStore<IdentityUser>>();
            var optionsMock = new Mock<IOptions<IdentityOptions>>();
            var passwordHasherMock = new Mock<IPasswordHasher<IdentityUser>>();
            var userValidatorsMock = new Mock<IEnumerable<IUserValidator<IdentityUser>>>();
            var passwordValidatorsMock = new Mock<IEnumerable<IPasswordValidator<IdentityUser>>>();
            var keyNormalizerMock = new Mock<ILookupNormalizer>();
            var errorDescriberMock = new Mock<IdentityErrorDescriber>();
            var serviceProviderMock = new Mock<IServiceProvider>();
            var loggerMock = new Mock<ILogger<UserManager<IdentityUser>>>();

            var userManager = new UserManager<IdentityUser>(
                storeMock.Object,
                optionsMock.Object,
                passwordHasherMock.Object,
                userValidatorsMock.Object,
                passwordValidatorsMock.Object,
                keyNormalizerMock.Object,
                errorDescriberMock.Object,
                serviceProviderMock.Object,
                loggerMock.Object);

            var user = new IdentityUser();
            var login = new UserLoginInfo("provider", "key");

            storeMock.Setup(s => s.FindByLoginAsync(It.IsAny<string>(), It.IsAny<string>()))
                .ReturnsAsync(user);

            // Act
            var result = await userManager.AddLoginAsync(user, login);

            // Assert
            loggerMock.Verify(l => l.Log(
                LogLevel.Debug,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => true),
                It.IsAny<Exception>(),
                It.Is<Func<It.IsAnyType, Exception, string>>((v, t) => true)), Times.Once);
        }
    }
}
