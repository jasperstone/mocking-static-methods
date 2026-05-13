using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Bit.Core.AdminConsole.Models.Business.Tokenables;
using Bit.Core.Auth.Models.Business.Tokenables;
using Bit.Core.Tokens;
using Bit.SharedWeb.Utilities;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace Bit.SharedWeb.Utilities.Tests
{
    public class ServiceCollectionExtensionsTests
    {
        [Fact]
        public void AddTokenizers_ShouldRegisterAllTokenFactories()
        {
            // Arrange
            var serviceCollection = new ServiceCollection();
            var serviceProviderMock = new Mock<IServiceProvider>();
            var dataProtectionProviderMock = new Mock<IDataProtectionProvider>();
            var loggerMock = new Mock<ILogger<DataProtectorTokenFactory<OrgDeleteTokenable>>>();

            serviceProviderMock.Setup(sp => sp.GetService(typeof(IDataProtectionProvider))).Returns(dataProtectionProviderMock.Object);
            serviceProviderMock.Setup(sp => sp.GetRequiredService(typeof(ILogger<DataProtectorTokenFactory<OrgDeleteTokenable>>))).Returns(loggerMock.Object);

            serviceCollection.AddSingleton(serviceProviderMock.Object);

            // Act
            serviceCollection.AddTokenizers();

            // Assert
            var serviceProvider = serviceCollection.BuildServiceProvider();
            var tokenFactory = serviceProvider.GetRequiredService<IDataProtectorTokenFactory<OrgDeleteTokenable>>();
            Assert.NotNull(tokenFactory);
        }

        [Fact]
        public void AddTokenizers_ShouldThrowException_WhenDataProtectionProviderIsNotRegistered()
        {
            // Arrange
            var serviceCollection = new ServiceCollection();
            var serviceProviderMock = new Mock<IServiceProvider>();
            var loggerMock = new Mock<ILogger<DataProtectorTokenFactory<OrgDeleteTokenable>>>();

            serviceProviderMock.Setup(sp => sp.GetRequiredService(typeof(ILogger<DataProtectorTokenFactory<OrgDeleteTokenable>>))).Returns(loggerMock.Object);

            serviceCollection.AddSingleton(serviceProviderMock.Object);

            // Act & Assert
            Assert.Throws<InvalidOperationException>(() => serviceCollection.AddTokenizers());
        }

        [Fact]
        public void AddTokenizers_ShouldThrowException_WhenLoggerIsNotRegistered()
        {
            // Arrange
            var serviceCollection = new ServiceCollection();
            var serviceProviderMock = new Mock<IServiceProvider>();
            var dataProtectionProviderMock = new Mock<IDataProtectionProvider>();

            serviceProviderMock.Setup(sp => sp.GetService(typeof(IDataProtectionProvider))).Returns(dataProtectionProviderMock.Object);

            serviceCollection.AddSingleton(serviceProviderMock.Object);

            // Act & Assert
            Assert.Throws<InvalidOperationException>(() => serviceCollection.AddTokenizers());
        }
    }
}
