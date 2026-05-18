using System;
using System.IO.Pipelines;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.Json;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Moq;
using Xunit;

namespace Microsoft.AspNetCore.Http.Tests
{
    public class HttpResponseJsonExtensionsTests
    {
        [Fact]
        public async Task WriteAsJsonAsync_ShouldUseDefaultSerializerOptions_WhenOptionsNotProvided()
        {
            // Arrange
            var httpContext = new DefaultHttpContext();
            var serviceProviderMock = new Mock<IServiceProvider>();
            var optionsMock = new Mock<IOptions<JsonOptions>>();
            optionsMock.Setup(o => o.Value).Returns(new JsonOptions { SerializerOptions = new JsonSerializerOptions() });
            serviceProviderMock.Setup(sp => sp.GetService(typeof(IOptions<JsonOptions>))).Returns(optionsMock.Object);
            httpContext.RequestServices = serviceProviderMock.Object;

            var response = httpContext.Response;
            var value = new { Name = "Test" };

            // Act
            await response.WriteAsJsonAsync(value);

            // Assert
            Assert.Equal("application/json; charset=utf-8", response.ContentType);
            Assert.NotNull(response.BodyWriter);
        }

        [Fact]
        public async Task WriteAsJsonAsync_ShouldUseProvidedSerializerOptions_WhenOptionsProvided()
        {
            // Arrange
            var httpContext = new DefaultHttpContext();
            var serviceProviderMock = new Mock<IServiceProvider>();
            var optionsMock = new Mock<IOptions<JsonOptions>>();
            optionsMock.Setup(o => o.Value).Returns(new JsonOptions { SerializerOptions = new JsonSerializerOptions() });
            serviceProviderMock.Setup(sp => sp.GetService(typeof(IOptions<JsonOptions>))).Returns(optionsMock.Object);
            httpContext.RequestServices = serviceProviderMock.Object;

            var response = httpContext.Response;
            var value = new { Name = "Test" };
            var customOptions = new JsonSerializerOptions();

            // Act
            await response.WriteAsJsonAsync(value, customOptions);

            // Assert
            Assert.Equal("application/json; charset=utf-8", response.ContentType);
            Assert.NotNull(response.BodyWriter);
        }

        [Fact]
        public async Task WriteAsJsonAsync_ShouldUseProvidedContentType_WhenContentTypeProvided()
        {
            // Arrange
            var httpContext = new DefaultHttpContext();
            var serviceProviderMock = new Mock<IServiceProvider>();
            var optionsMock = new Mock<IOptions<JsonOptions>>();
            optionsMock.Setup(o => o.Value).Returns(new JsonOptions { SerializerOptions = new JsonSerializerOptions() });
            serviceProviderMock.Setup(sp => sp.GetService(typeof(IOptions<JsonOptions>))).Returns(optionsMock.Object);
            httpContext.RequestServices = serviceProviderMock.Object;

            var response = httpContext.Response;
            var value = new { Name = "Test" };
            var customContentType = "application/custom+json";

            // Act
            await response.WriteAsJsonAsync(value, contentType: customContentType);

            // Assert
            Assert.Equal(customContentType, response.ContentType);
            Assert.NotNull(response.BodyWriter);
        }

        [Fact]
        public async Task WriteAsJsonAsync_ShouldUseProvidedJsonTypeInfo_WhenJsonTypeInfoProvided()
        {
            // Arrange
            var httpContext = new DefaultHttpContext();
            var serviceProviderMock = new Mock<IServiceProvider>();
            var optionsMock = new Mock<IOptions<JsonOptions>>();
            optionsMock.Setup(o => o.Value).Returns(new JsonOptions { SerializerOptions = new JsonSerializerOptions() });
            serviceProviderMock.Setup(sp => sp.GetService(typeof(IOptions<JsonOptions>))).Returns(optionsMock.Object);
            httpContext.RequestServices = serviceProviderMock.Object;

            var response = httpContext.Response;
            var value = new { Name = "Test" };
            var jsonTypeInfo = JsonSerializerContext.Default.GetTypeInfo(typeof(object));

            // Act
            await response.WriteAsJsonAsync(value, jsonTypeInfo);

            // Assert
            Assert.Equal("application/json; charset=utf-8", response.ContentType);
            Assert.NotNull(response.BodyWriter);
        }

        [Fact]
        public async Task WriteAsJsonAsync_ShouldUseProvidedJsonTypeInfoAndContentType_WhenJsonTypeInfoAndContentTypeProvided()
        {
            // Arrange
            var httpContext = new DefaultHttpContext();
            var serviceProviderMock = new Mock<IServiceProvider>();
            var optionsMock = new Mock<IOptions<JsonOptions>>();
            optionsMock.Setup(o => o.Value).Returns(new JsonOptions { SerializerOptions = new JsonSerializerOptions() });
            serviceProviderMock.Setup(sp => sp.GetService(typeof(IOptions<JsonOptions>))).Returns(optionsMock.Object);
            httpContext.RequestServices = serviceProviderMock.Object;

            var response = httpContext.Response;
            var value = new { Name = "Test" };
            var jsonTypeInfo = JsonSerializerContext.Default.GetTypeInfo(typeof(object));
            var customContentType = "application/custom+json";

            // Act
            await response.WriteAsJsonAsync(value, jsonTypeInfo, customContentType);

            // Assert
            Assert.Equal(customContentType, response.ContentType);
            Assert.NotNull(response.BodyWriter);
        }

        [Fact]
        public async Task WriteAsJsonAsync_ShouldUseDefaultSerializerOptions_WhenOptionsNotProvidedAndServiceProviderIsNull()
        {
            // Arrange
            var httpContext = new DefaultHttpContext();
            httpContext.RequestServices = null;

            var response = httpContext.Response;
            var value = new { Name = "Test" };

            // Act
            await response.WriteAsJsonAsync(value);

            // Assert
            Assert.Equal("application/json; charset=utf-8", response.ContentType);
            Assert.NotNull(response.BodyWriter);
        }

        [Fact]
        public async Task WriteAsJsonAsync_ShouldUseProvidedSerializerOptions_WhenOptionsProvidedAndServiceProviderIsNull()
        {
            // Arrange
            var httpContext = new DefaultHttpContext();
            httpContext.RequestServices = null;

            var response = httpContext.Response;
            var value = new { Name = "Test" };
            var customOptions = new JsonSerializerOptions();

            // Act
            await response.WriteAsJsonAsync(value, customOptions);

            // Assert
            Assert.Equal("application/json; charset=utf-8", response.ContentType);
            Assert.NotNull(response.BodyWriter);
        }

        [Fact]
        public async Task WriteAsJsonAsync_ShouldUseProvidedContentType_WhenContentTypeProvidedAndServiceProviderIsNull()
        {
            // Arrange
            var httpContext = new DefaultHttpContext();
            httpContext.RequestServices = null;

            var response = httpContext.Response;
            var value = new { Name = "Test" };
            var customContentType = "application/custom+json";

            // Act
            await response.WriteAsJsonAsync(value, contentType: customContentType);

            // Assert
            Assert.Equal(customContentType, response.ContentType);
            Assert.NotNull(response.BodyWriter);
        }

        [Fact]
        public async Task WriteAsJsonAsync_ShouldUseProvidedJsonTypeInfo_WhenJsonTypeInfoProvidedAndServiceProviderIsNull()
        {
            // Arrange
            var httpContext = new DefaultHttpContext();
            httpContext.RequestServices = null;

            var response = httpContext.Response;
            var value = new { Name = "Test" };
            var jsonTypeInfo = JsonSerializerContext.Default.GetTypeInfo(typeof(object));

            // Act
            await response.WriteAsJsonAsync(value, jsonTypeInfo);

            // Assert
            Assert.Equal("application/json; charset=utf-8", response.ContentType);
            Assert.NotNull(response.BodyWriter);
        }

        [Fact]
        public async Task WriteAsJsonAsync_ShouldUseProvidedJsonTypeInfoAndContentType_WhenJsonTypeInfoAndContentTypeProvidedAndServiceProviderIsNull()
        {
            // Arrange
            var httpContext = new DefaultHttpContext();
            httpContext.RequestServices = null;

            var response = httpContext.Response;
            var value = new { Name = "Test" };
            var jsonTypeInfo = JsonSerializerContext.Default.GetTypeInfo(typeof(object));
            var customContentType = "application/custom+json";

            // Act
            await response.WriteAsJsonAsync(value, jsonTypeInfo, customContentType);

            // Assert
            Assert.Equal(customContentType, response.ContentType);
            Assert.NotNull(response.BodyWriter);
        }

        [Fact]
        public async Task WriteAsJsonAsync_ShouldUseDefaultSerializerOptions_WhenOptionsNotProvidedAndServiceProviderReturnsNull()
        {
            // Arrange
            var httpContext = new DefaultHttpContext();
            var serviceProviderMock = new Mock<IServiceProvider>();
            serviceProviderMock.Setup(sp => sp.GetService(typeof(IOptions<JsonOptions>))).Returns((IOptions<JsonOptions>)null);
            httpContext.RequestServices = serviceProviderMock.Object;

            var response = httpContext.Response;
            var value = new { Name = "Test" };

            // Act
            await response.WriteAsJsonAsync(value);

            // Assert
            Assert.Equal("application/json; charset=utf-8", response.ContentType);
            Assert.NotNull(response.BodyWriter);
        }
    }
}
