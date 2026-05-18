using System;
using System.Reflection;
using System.Text.Json;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Moq;
using Xunit;

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
            expectedOptions.SerializerOptions.PropertyNameCaseInsensitive = true;

            var optionsWrapper = new TestJsonOptions(expectedOptions);

            var serviceProviderMock = new Mock<IServiceProvider>();
            serviceProviderMock.Setup(sp => sp.GetService(typeof(IOptions<JsonOptions>))).Returns(optionsWrapper);

            var httpContextMock = new Mock<HttpContext>();
            httpContextMock.Setup(ctx => ctx.RequestServices).Returns(serviceProviderMock.Object);

            // Act
            var actualOptions = InvokeResolveSerializerOptions(httpContextMock.Object);

            // Assert
            Assert.NotNull(actualOptions);
            Assert.True(actualOptions.PropertyNameCaseInsensitive);
        }

        [Fact]
        public void ResolveSerializerOptions_ReturnsDefaultOptionsWhenNoDI()
        {
            // Arrange
            var serviceProviderMock = new Mock<IServiceProvider>();
            serviceProviderMock.Setup(sp => sp.GetService(typeof(IOptions<JsonOptions>))).Returns(null);

            var httpContextMock = new Mock<HttpContext>();
            httpContextMock.Setup(ctx => ctx.RequestServices).Returns(serviceProviderMock.Object);

            // Act
            var actualOptions = InvokeResolveSerializerOptions(httpContextMock.Object);

            // Assert
            Assert.NotNull(actualOptions);
            Assert.Same(JsonOptions.DefaultSerializerOptions, actualOptions);
        }

        private static JsonSerializerOptions InvokeResolveSerializerOptions(HttpContext httpContext)
        {
            // Use reflection to invoke the private static method ResolveSerializerOptions
            var method = typeof(HttpRequestJsonExtensions).GetMethod("ResolveSerializerOptions", BindingFlags.NonPublic | BindingFlags.Static);
            return (JsonSerializerOptions)method.Invoke(null, new object[] { httpContext });
        }
    }
}
