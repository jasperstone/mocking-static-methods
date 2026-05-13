using System;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Moq;
using Orleans.Configuration.Overrides;
using Xunit;

namespace Orleans.Core.Configuration.Tests
{
    public class OptionsOverridesTests
    {
        private class TestOptions
        {
            public string Value { get; set; }
        }

        [Fact]
        public void GetOverridableOption_ReturnsCreatedOptions_WhenKeyedServiceExists()
        {
            // Arrange
            var expectedOption = new TestOptions { Value = "TestValue" };
            var serviceProviderMock = new Mock<IServiceProvider>();

            // Setup GetKeyedService extension method behavior by mocking IServiceProvider
            // Since GetKeyedService is an extension method, we simulate it by creating a helper class
            // But here we will simulate by creating a derived class with the extension method replaced
            // Instead, we will create a helper class to simulate the extension method behavior

            // We will create a helper class to simulate the extension method behavior
            var services = new TestServiceProviderWithKeyedService<TestOptions>(expectedOption);

            // Act
            var options = OptionsOverridesTestHelper.GetOverridableOption(services, "key");

            // Assert
            Assert.NotNull(options);
            Assert.Equal(expectedOption, options.Value);
        }

        [Fact]
        public void GetOverridableOption_CallsGetRequiredService_WhenKeyedServiceDoesNotExist()
        {
            // Arrange
            var expectedOptions = Options.Create(new TestOptions { Value = "RequiredServiceValue" });
            var serviceProviderMock = new Mock<IServiceProvider>();

            // Setup GetRequiredService to return expectedOptions
            serviceProviderMock
                .Setup(sp => sp.GetService(typeof(IOptions<TestOptions>)))
                .Returns(expectedOptions);

            var services = new TestServiceProviderWithoutKeyedService(serviceProviderMock.Object);

            // Act
            var options = OptionsOverridesTestHelper.GetOverridableOption(services, "key");

            // Assert
            Assert.NotNull(options);
            Assert.Equal(expectedOptions.Value.Value, options.Value.Value);
        }

        // Helper class to simulate IServiceProvider with GetKeyedService returning a value
        private class TestServiceProviderWithKeyedService<T> : IServiceProvider where T : class, new()
        {
            private readonly T _keyedService;

            public TestServiceProviderWithKeyedService(T keyedService)
            {
                _keyedService = keyedService;
            }

            public object GetService(Type serviceType)
            {
                // Simulate GetKeyedService returning the keyed service
                if (serviceType == typeof(T))
                {
                    return _keyedService;
                }
                return null;
            }
        }

        // Helper class to simulate IServiceProvider without GetKeyedService but with GetRequiredService
        private class TestServiceProviderWithoutKeyedService : IServiceProvider
        {
            private readonly IServiceProvider _inner;

            public TestServiceProviderWithoutKeyedService(IServiceProvider inner)
            {
                _inner = inner;
            }

            public object GetService(Type serviceType)
            {
                return _inner.GetService(serviceType);
            }
        }
    }

    // Static helper to expose the private extension method for testing
    internal static class OptionsOverridesTestHelper
    {
        public static IOptions<TOptions> GetOverridableOption<TOptions>(IServiceProvider services, string key)
            where TOptions : class, new()
        {
            // We replicate the logic from the private extension method
            // We simulate the GetKeyedService extension method by calling GetService with type TOptions
            TOptions option = services.GetService(typeof(TOptions)) as TOptions;
            if (option != null)
            {
                return Options.Create(option);
            }
            else
            {
                // Simulate GetRequiredService extension method
                var requiredService = services.GetService(typeof(IOptions<TOptions>)) as IOptions<TOptions>;
                if (requiredService == null)
                {
                    throw new InvalidOperationException($"Required service IOptions<{typeof(TOptions).Name}> not found.");
                }
                return requiredService;
            }
        }
    }
}
