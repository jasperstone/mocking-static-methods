using System;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Xunit;

namespace HttpExtensionsTests
{
    public class HttpRequestJsonExtensionsTests
    {
        private class DummyOptions
        {
            public JsonSerializerOptions SerializerOptions { get; set; } = new JsonSerializerOptions();
        }

        private class DummyService
        {
        }

        private class DummyServiceProvider : IServiceProvider
        {
            public object? GetService(Type serviceType)
            {
                if (serviceType == typeof(IOptions<JsonOptions>))
                {
                    var options = new OptionsWrapper<JsonOptions>(new JsonOptions());
                    return options;
                }
                if (serviceType == typeof(IOptions<DummyOptions>))
                {
                    var options = new OptionsWrapper<DummyOptions>(new DummyOptions());
                    return options;
                }
                if (serviceType == typeof(DummyService))
                {
                    return new DummyService();
                }
                return null;
            }
        }

        private HttpContext CreateHttpContextWithRequest(string contentType, string contentCharset, string bodyContent)
        {
            var context = new DefaultHttpContext();
            var request = context.Request;
            request.ContentType = $"{contentType}; charset={contentCharset}";
            var bodyBytes = Encoding.UTF8.GetBytes(bodyContent);
            request.Body = new System.IO.MemoryStream(bodyBytes);
            request.BodyReader = System.IO.Pipelines.PipeReader.Create(request.Body);
            var services = new ServiceCollection();
            services.AddTransient<IOptions<JsonOptions>, OptionsWrapper<JsonOptions>>(sp => new OptionsWrapper<JsonOptions>(new JsonOptions()));
            request.HttpContext.RequestServices = services.BuildServiceProvider();
            return context;
        }

        [Fact]
        public async Task ReadFromJsonAsync_UsesServiceProvider_GetService()
        {
            // Arrange
            var context = CreateHttpContextWithRequest("application/json", "utf-8", "{\"Name\":\"Test\"}");
            var request = context.Request;

            // Inject a dummy service into the request's service provider
            var serviceProvider = new DummyServiceProvider();
            request.HttpContext.RequestServices = serviceProvider;

            // Act
            var result = await request.ReadFromJsonAsync<DummyOptions>();

            // Assert
            Assert.NotNull(result);
            Assert.IsType<JsonOptions>(result);
        }

        [Fact]
        public async Task ReadFromJsonAsync_WithCustomOptions_UsesProvidedOptions()
        {
            // Arrange
            var context = CreateHttpContextWithRequest("application/json", "utf-8", "{\"SerializerOptions\":{\"PropertyNamingPolicy\":\"CamelCase\"}}");
            var request = context.Request;

            var customOptions = new JsonSerializerOptions
            {
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase
            };

            // Act
            var result = await request.ReadFromJsonAsync<DummyOptions>(customOptions);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(JsonNamingPolicy.CamelCase, result.SerializerOptions.PropertyNamingPolicy);
        }

        [Fact]
        public async Task ReadFromJsonAsync_NonJsonContentType_Throws()
        {
            // Arrange
            var context = CreateHttpContextWithRequest("text/plain", "utf-8", "not json");
            var request = context.Request;

            // Act & Assert
            await Assert.ThrowsAsync<InvalidOperationException>(() => request.ReadFromJsonAsync<DummyOptions>());
        }
    }
}
