using System;
using System.IO;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.Json;
using Microsoft.Extensions.Options;
using Xunit;

namespace Microsoft.AspNetCore.Http.Tests;

public class HttpResponseJsonExtensionsTests
{
    [Fact]
    public async Task WriteAsJsonAsync_UsesSerializerOptionsFromRequestServices()
    {
        // Arrange
        var httpContext = new DefaultHttpContext();
        var memoryStream = new MemoryStream();
        httpContext.Response.Body = memoryStream;

        var jsonOptions = new JsonOptions();
        jsonOptions.SerializerOptions.WriteIndented = true;

        var provider = new TrackingServiceProvider
        {
            ServiceToReturn = Options.Create(jsonOptions)
        };

        httpContext.RequestServices = provider;

        var payload = new TestPayload { Greeting = "hello", Number = 42 };

        // Act
        await httpContext.Response.WriteAsJsonAsync(payload);

        // Assert
        Assert.Equal(1, provider.GetServiceCallCount);
        Assert.Equal(typeof(IOptions<JsonOptions>), provider.LastRequestedServiceType);

        var json = Encoding.UTF8.GetString(memoryStream.ToArray());
        var expected = JsonSerializer.Serialize(payload, jsonOptions.SerializerOptions);
        Assert.Equal(expected, json);
        Assert.Equal("application/json; charset=utf-8", httpContext.Response.ContentType);
    }

    [Fact]
    public async Task WriteAsJsonAsync_FallsBackToDefaultSerializerOptionsWhenServiceNotRegistered()
    {
        // Arrange
        var httpContext = new DefaultHttpContext();
        var memoryStream = new MemoryStream();
        httpContext.Response.Body = memoryStream;

        var provider = new TrackingServiceProvider();
        httpContext.RequestServices = provider;

        var payload = new TestPayload { Greeting = "hi", Number = 7 };

        // Act
        await httpContext.Response.WriteAsJsonAsync(payload);

        // Assert
        Assert.Equal(1, provider.GetServiceCallCount);
        Assert.Equal(typeof(IOptions<JsonOptions>), provider.LastRequestedServiceType);

        var json = Encoding.UTF8.GetString(memoryStream.ToArray());
        var expected = JsonSerializer.Serialize(payload, JsonOptions.DefaultSerializerOptions);
        Assert.Equal(expected, json);
        Assert.Equal("application/json; charset=utf-8", httpContext.Response.ContentType);
    }

    private sealed class TrackingServiceProvider : IServiceProvider
    {
        public int GetServiceCallCount { get; private set; }
        public Type? LastRequestedServiceType { get; private set; }
        public object? ServiceToReturn { get; set; }

        public object? GetService(Type serviceType)
        {
            GetServiceCallCount++;
            LastRequestedServiceType = serviceType;

            if (serviceType == typeof(IOptions<JsonOptions>))
            {
                return ServiceToReturn;
            }

            return null;
        }
    }

    private sealed class TestPayload
    {
        public string Greeting { get; set; } = string.Empty;
        public int Number { get; set; }
    }
}
