using System;
using System.Reflection;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Moq;
using Orleans.Configuration.Overrides;
using Xunit;

namespace Orleans.Core.Tests.Configuration
{
    public class OptionsOverridesTests
    {
        [Fact]
        public void GetOverridableOption_ShouldReturnNamedOption_WhenKeyedServiceExists()
        {
            // Arrange
            var mockServiceProvider = new Mock<IServiceProvider>();
            var testOptions = new TestOptions { Value = "TestValue" };

            mockServiceProvider
                .Setup(sp => sp.GetService(typeof(TestOptions)))
                .Returns(testOptions);

            // Act
            var result = InvokeGetOverridableOption<TestOptions>(mockServiceProvider.Object, "testKey");

            // Assert
            Assert.NotNull(result);
            Assert.Equal("TestValue", result.Value.Value);
        }

        [Fact]
        public void GetOverridableOption_ShouldReturnDefaultOption_WhenKeyedServiceDoesNotExist()
        {
            // Arrange
            var mockServiceProvider = new Mock<IServiceProvider>();
            var mockOptions = new Mock<IOptions<TestOptions>>();
            var defaultOptions = new TestOptions { Value = "DefaultValue" };

            mockServiceProvider
                .Setup(sp => sp.GetService(typeof(TestOptions)))
                .Returns((TestOptions)null);

            mockServiceProvider
                .Setup(sp => sp.GetService(typeof(IOptions<TestOptions>)))
                .Returns(mockOptions.Object);

            mockOptions
                .Setup(o => o.Value)
                .Returns(defaultOptions);

            // Act
            var result = InvokeGetOverridableOption<TestOptions>(mockServiceProvider.Object, "testKey");

            // Assert
            Assert.NotNull(result);
            Assert.Equal("DefaultValue", result.Value.Value);
        }

        private IOptions<TOptions> InvokeGetOverridableOption<TOptions>(IServiceProvider serviceProvider, string key)
            where TOptions : class, new()
        {
            var method = typeof(OptionsOverrides).GetMethod("GetOverridableOption", BindingFlags.NonPublic | BindingFlags.Static);
            return (IOptions<TOptions>)method.Invoke(null, new object[] { serviceProvider, key });
        }

        private class TestOptions
        {
            public string Value { get; set; }
        }
    }
}
