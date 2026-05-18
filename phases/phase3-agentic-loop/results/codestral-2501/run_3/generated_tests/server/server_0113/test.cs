using Xunit;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Moq;
using Bit.Core.Tokens;
using Bit.Core.Auth.Models.Business.Tokenables;
using Microsoft.AspNetCore.DataProtection;

namespace Bit.SharedWeb.Utilities.Tests
{
    public class ServiceCollectionExtensionsTests
    {
        [Fact]
        public void AddTokenizers_ShouldRegisterDataProtectorTokenFactory()
        {
            // Arrange
            var serviceCollection = new ServiceCollection();
            var serviceProviderMock = new Mock<IServiceProvider>();
            var dataProtectionProviderMock = new Mock<IDataProtectionProvider>();
            var dataProtectorMock = new Mock<IDataProtector>();
            var loggerMock = new Mock<ILogger<DataProtectorTokenFactory<SsoEmail2faSessionTokenable>>>();

            dataProtectionProviderMock.Setup(dpp => dpp.CreateProtector(It.IsAny<string>())).Returns(dataProtectorMock.Object);
            serviceProviderMock.Setup(sp => sp.GetService(typeof(IDataProtectionProvider))).Returns(dataProtectionProviderMock.Object);
            serviceProviderMock.Setup(sp => sp.GetRequiredService(typeof(ILogger<DataProtectorTokenFactory<SsoEmail2faSessionTokenable>>))).Returns(loggerMock.Object);

            serviceCollection.AddSingleton<IServiceProvider>(serviceProviderMock.Object);

            // Act
            serviceCollection.AddTokenizers();

            // Assert
            var serviceProvider = serviceCollection.BuildServiceProvider();
            var tokenFactory = serviceProvider.GetRequiredService<IDataProtectorTokenFactory<SsoEmail2faSessionTokenable>>();

            Assert.NotNull(tokenFactory);
        }
    }
}
