using System;
using System.IO;
using System.Text;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.Json;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Microsoft.Extensions.Primitives;
using Microsoft.Net.Http.Headers;
using Xunit;
using Moq;

namespace Microsoft.AspNetCore.Http.Tests
{
    public class HttpRequestJsonExtensionsTests
    {
        private class TestJsonOptions : IOptions<JsonOptions>
        {
            public JsonOptions Value { get; }

            public TestJsonOptions(JsonOptions options)
            {
                Value = options;
            }
        }

        [Fact]
        public void ResolveSerializerOptions_ReturnsOptionsFromDI()
        {
            // Arrange
            var expectedOptions = new JsonOptions();
            expectedOptions.SerializerOptions.PropertyNameCaseInsensitive = true;

            var optionsWrapper = new TestJsonOptions(expectedOptions);

            var services = new ServiceCollection();
            services.AddSingleton<IOptions<JsonOptions>>(optionsWrapper);
            var serviceProvider = services.BuildServiceProvider();

            var context = new DefaultHttpContext();
            context.RequestServices = serviceProvider;

            // Act
            var actualOptions = InvokeResolveSerializerOptions(context);

            // Assert
            Assert.NotNull(actualOptions);
            Assert.True(actualOptions.PropertyNameCaseInsensitive);
        }

        [Fact]
        public void ResolveSerializerOptions_ReturnsDefaultOptionsWhenNoDI()
        {
            // Arrange
            var context = new DefaultHttpContext();
            context.RequestServices = null;

            // Act
            var actualOptions = InvokeResolveSerializerOptions(context);

            // Assert
            Assert.NotNull(actualOptions);
            Assert.Same(JsonOptions.DefaultSerializerOptions, actualOptions);
        }

        [Fact]
        public void ResolveSerializerOptions_ReturnsDefaultOptionsWhenDIHasNoOptions()
        {
            // Arrange
            var services = new ServiceCollection();
            // Register a service provider that returns null for IOptions<JsonOptions>
            var spMock = new Mock<IServiceProvider>();
            spMock.Setup(sp => sp.GetService(typeof(IOptions<JsonOptions>))).Returns(null);
            var context = new DefaultHttpContext();
            context.RequestServices = spMock.Object;

            // Act
            var actualOptions = InvokeResolveSerializerOptions(context);

            // Assert
            Assert.NotNull(actualOptions);
            Assert.Same(JsonOptions.DefaultSerializerOptions, actualOptions);
        }

        private static JsonSerializerOptions InvokeResolveSerializerOptions(HttpContext httpContext)
        {
            // Use reflection to invoke the private static method ResolveSerializerOptions
            var method = typeof(HttpRequestJsonExtensions).GetMethod("ResolveSerializerOptions", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Static);
            Assert.NotNull(method);
            var result = method.Invoke(null, new object[] { httpContext });
            return (JsonSerializerOptions)result!;
        }
    }
}
