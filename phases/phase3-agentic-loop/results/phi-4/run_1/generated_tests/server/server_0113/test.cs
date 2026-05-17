        using System;
        using Microsoft.Extensions.DependencyInjection;
        using Microsoft.Extensions.Logging;
        using Moq;
        using Xunit;

        namespace SharedWeb.Utilities.Tests
        {
            public class ServiceCollectionExtensionsTests
            {
                [Fact]
                public void AddTokenizers_ShouldCallGetRequiredServiceWithCorrectType()
                {
                    // Arrange
                    var serviceProviderMock = new Mock<IServiceProvider>();
                    var loggerMock = new Mock<ILogger<DataProtectorTokenFactory<SsoEmail2faSessionTokenable>>>();
                    var dataProtectionProviderMock = new Mock<IDataProtectionProvider>();

                    serviceProviderMock
                        .Setup(sp => sp.GetRequiredService<ILogger<DataProtectorTokenFactory<SsoEmail2faSessionTokenable>>>())
                        .Returns(loggerMock.Object);

                    serviceProviderMock
                        .Setup(sp => sp.GetDataProtectionProvider())
                        .Returns(dataProtectionProviderMock.Object);

                    var services = new ServiceCollection();

                    // Act
                    ServiceCollectionExtensions.AddTokenizers(services);
                    var serviceProvider = services.BuildServiceProvider();

                    // Assert
                    serviceProviderMock.Verify(
                        sp => sp.GetRequiredService<ILogger<DataProtectorTokenFactory<SsoEmail2faSessionTokenable>>>(),
                        Times.Once);
                }
            }
        }
        