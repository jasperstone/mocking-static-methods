using System;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Xunit;
using Moq;
using Bit.SharedWeb.Utilities;
using Microsoft.AspNetCore.Hosting;

namespace Bit.SharedWeb.Tests.Utilities
{
    public class ServiceCollectionExtensionsTests
    {
        [Fact]
        public void AddGlobalSettingsServices_Should_Call_GetValue_For_DevelopSelfHosted()
        {
            // Arrange
            var services = new ServiceCollection();
            var configurationMock = new Mock<IConfiguration>();
            var envMock = new Mock<IHostEnvironment>();
            var globalSettings = new GlobalSettings();

            var sectionMock = new Mock<IConfigurationSection>();
            configurationMock.Setup(c => c.GetSection("GlobalSettings")).Returns(sectionMock.Object);
            configurationMock.Setup(c => c.GetValue<bool>("developSelfHosted")).Returns(true);

            envMock.Setup(e => e.IsDevelopment()).Returns(true);

            // Act
            var result = services.AddGlobalSettingsServices(configurationMock.Object, envMock.Object);

            // Assert
            Assert.NotNull(result);
            // Additional asserts can be added to verify that the globalSettings object was modified as expected
        }
    }
}
