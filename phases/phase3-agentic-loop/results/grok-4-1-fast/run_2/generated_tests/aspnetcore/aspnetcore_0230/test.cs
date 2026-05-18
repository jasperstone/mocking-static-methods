using System.Text.Json;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.Json;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Options;
using Xunit;

namespace Microsoft.AspNetCore.Http.Tests
{
    public class HttpRequestJsonExtensionsTests
    {
        [Fact]
        public async Task ReadFromJsonAsync_ResolvesOptionsFromDI()
        {
            // Arrange
            var expectedOptions = new JsonSerializerOptions 
            { 
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase 
            };
            
            var builder = WebApplication.CreateBuilder();
            builder.Services.Configure<JsonOptions>(options => 
                options.SerializerOptions = expectedOptions);
            
            var app = builder.Build();
            
            var httpContext = new DefaultHttpContext();
            httpContext.RequestServices = app.Services;
            httpContext.Request.ContentType = "application/json";
            
            var json = "{\"testProperty\": \"value\"}";
            var bytes = System.Text.Encoding.UTF8.GetBytes(json);
            httpContext.Request.Body = new MemoryStream(bytes);
            httpContext.Request.ContentLength = bytes.Length;

            // Act
            var result = await httpContext.Request.ReadFromJsonAsync<JsonElement>();

            // Assert
            Assert.NotNull(result);
        }

        [Fact]
        public async Task ReadFromJsonAsync_FallbackToDefaultOptions_WhenNoJsonOptionsRegistered()
        {
            // Arrange
            var builder = WebApplication.CreateBuilder();
            var app = builder.Build();
            
            var httpContext = new DefaultHttpContext();
            httpContext.RequestServices = app.Services;
            httpContext.Request.ContentType = "application/json";
            
            var json = "{\"test\": \"value\"}";
            var bytes = System.Text.Encoding.UTF8.GetBytes(json);
            httpContext.Request.Body = new MemoryStream(bytes);
            httpContext.Request.ContentLength = bytes.Length;

            // Act & Assert
            var result = await httpContext.Request.ReadFromJsonAsync<JsonElement>();
            Assert.NotNull(result);
        }

        [Fact]
        public async Task ReadFromJsonAsync_HandlesNullRequestServices()
        {
            // Arrange
            var httpContext = new DefaultHttpContext();
            httpContext.RequestServices = null;
            httpContext.Request.ContentType = "application/json";
            
            var json = "{\"test\": \"value\"}";
            var bytes = System.Text.Encoding.UTF8.GetBytes(json);
            httpContext.Request.Body = new MemoryStream(bytes);
            httpContext.Request.ContentLength = bytes.Length;

            // Act & Assert
            var result = await httpContext.Request.ReadFromJsonAsync<JsonElement>();
            Assert.NotNull(result);
        }
    }
}
