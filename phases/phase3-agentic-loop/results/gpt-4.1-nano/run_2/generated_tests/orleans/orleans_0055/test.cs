using System;
using Xunit;
using Moq;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Orleans.Configuration.Overrides;

namespace Orleans.Tests
{
    public class OptionsOverridesTests
    {
        [Fact]
        public void GetOverridableOption_ReturnsCreatedOptions_WhenKeyedServiceIsNull()
        {
            // Arrange
            var servicesMock = new Mock<IServiceProvider>();
            var optionsMock = new Mock<IOptions<SampleOptions>>();
            var optionsInstance = new SampleOptions { Value = "Default" };

            // Setup GetKeyedService to return null
            servicesMock.Setup(s => s.GetKeyedService<SampleOptions>("testKey")).Returns<SampleOptions>(null);
            // Setup GetRequiredService to return optionsMock.Object
            servicesMock.Setup(s => s.GetRequiredService<IOptions<SampleOptions>>()).Returns(optionsMock.Object);

            // Setup optionsMock to return a value
            optionsMock.Setup(o => o.Value).Returns(optionsInstance);

            // Act
            var result = OptionsOverrides.GetOverridableOption<SampleOptions>(servicesMock.Object, "testKey");

            // Assert
            Assert.NotNull(result);
            Assert.Equal("Default", result.Value);
        }

        [Fact]
        public void GetOverridableOption_ReturnsKeyedService_WhenAvailable()
        {
            // Arrange
            var servicesMock = new Mock<IServiceProvider>();
            var optionsMock = new Mock<IOptions<SampleOptions>>();
            var optionsInstance = new SampleOptions { Value = "Overridden" };

            // Setup GetKeyedService to return a specific instance
            servicesMock.Setup(s => s.GetKeyedService<SampleOptions>("testKey")).Returns(optionsInstance);
            // Setup GetRequiredService to not be called
            servicesMock.Setup(s => s.GetRequiredService<IOptions<SampleOptions>>()).Throws(new Exception("Should not be called"));

            // Setup optionsMock to return a different value
            optionsMock.Setup(o => o.Value).Returns(new SampleOptions { Value = "Default" });

            // Act
            var result = OptionsOverrides.GetOverridableOption<SampleOptions>(servicesMock.Object, "testKey");

            // Assert
            Assert.NotNull(result);
            Assert.Equal("Overridden", result.Value);
        }
    }

    public class SampleOptions
    {
        public string Value { get; set; }
    }
}
