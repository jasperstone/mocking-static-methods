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
        public void GetOverridableOption_ReturnsRequiredService_When_KeyedServiceIsNull()
        {
            // Arrange
            var serviceProviderMock = new Mock<IServiceProvider>();
            var optionsInstance = Options.Create(new TestOptions { Value = "default" });

            // Setup GetKeyedService to return null
            serviceProviderMock.Setup(sp => sp.GetKeyedService<TestOptions>("testKey"))
                .Returns((TestOptions)null);

            // Setup GetRequiredService to return the options instance
            serviceProviderMock.Setup(sp => sp.GetRequiredService<IOptions<TestOptions>>())
                .Returns(optionsInstance);

            // Act
            var result = OptionsOverrides.GetOverridableOption<TestOptions>(serviceProviderMock.Object, "testKey");

            // Assert
            Assert.NotNull(result);
            Assert.IsType<OptionsWrapper<TestOptions>>(result);
            Assert.Equal("default", result.Value.Value);
        }

        private class TestOptions
        {
            public string Value { get; set; }
        }
    }
}
