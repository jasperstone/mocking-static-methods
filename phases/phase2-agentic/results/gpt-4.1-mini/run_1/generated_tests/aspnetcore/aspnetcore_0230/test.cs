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
            var optionsWrapper = new TestJsonOptions(expectedOptions);

            var serviceProviderMock = new Mock<IServiceProvider>();
            serviceProviderMock
                .Setup(sp => sp.GetService(typeof(IOptions<JsonOptions>)))
                .Returns(optionsWrapper);

            var httpContextMock = new Mock<HttpContext>();
            httpContextMock.SetupGet(c => c.RequestServices).Returns(serviceProviderMock.Object);

            var httpRequestMock = new Mock<HttpRequest>();
            httpRequestMock.SetupGet(r => r.HttpContext).Returns(httpContextMock.Object);

            // Use reflection to call private static method ResolveSerializerOptions
            var method = typeof(HttpRequestJsonExtensions).GetMethod("ResolveSerializerOptions", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Static);

            // Act
            var result = method.Invoke(null, new object[] { httpContextMock.Object });

            // Assert
            Assert.Same(expectedOptions.SerializerOptions, result);
        }

        [Fact]
        public void ResolveSerializerOptions_ReturnsDefaultOptionsWhenNoDI()
        {
            // Arrange
            var httpContextMock = new Mock<HttpContext>();
            httpContextMock.SetupGet(c => c.RequestServices).Returns((IServiceProvider)null!);

            // Use reflection to call private static method ResolveSerializerOptions
            var method = typeof(HttpRequestJsonExtensions).GetMethod("ResolveSerializerOptions", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Static);

            // Act
            var result = method.Invoke(null, new object[] { httpContextMock.Object });

            // Assert
            var defaultOptionsProperty = typeof(JsonOptions).GetProperty("DefaultSerializerOptions", System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Static);
            var defaultOptions = defaultOptionsProperty.GetValue(null);
            Assert.Same(defaultOptions, result);
        }
    }
}
