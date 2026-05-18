using System;
using System.IO;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Microsoft.Extensions.Primitives;
using Microsoft.Net.Http.Headers;
using Xunit;
using Moq;
using System.Text.Json;
using Microsoft.AspNetCore.Http.Json;

namespace Microsoft.AspNetCore.Http
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
            var optionsWrapper = new TestJsonOptions(expectedOptions);

            var services = new ServiceCollection();
            services.AddSingleton<IOptions<JsonOptions>>(optionsWrapper);
            var serviceProvider = services.BuildServiceProvider();

            var context = new DefaultHttpContext();
            context.RequestServices = serviceProvider;

            // Act
            var result = InvokeResolveSerializerOptions(context);

            // Assert
            Assert.Same(expectedOptions.SerializerOptions, result);
        }

        [Fact]
        public void ResolveSerializerOptions_ReturnsDefaultOptionsWhenNoDI()
        {
            // Arrange
            var context = new DefaultHttpContext();
            context.RequestServices = null;

            // Act
            var result = InvokeResolveSerializerOptions(context);

            // Assert
            Assert.Same(JsonOptions.DefaultSerializerOptions, result);
        }

        private static JsonSerializerOptions InvokeResolveSerializerOptions(HttpContext httpContext)
        {
            // Use reflection to invoke the private static method ResolveSerializerOptions
            var method = typeof(HttpRequestJsonExtensions).GetMethod("ResolveSerializerOptions", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Static);
            return (JsonSerializerOptions)method.Invoke(null, new object[] { httpContext });
        }
    }
}
