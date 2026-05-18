using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http.Features;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Xunit;

namespace Microsoft.AspNetCore.Mvc.Filters.Tests;

public class MiddlewareFilterConfigurationProviderTests
{
    [Fact]
    public void ConfigureBuilder_Invoke_ResolvesServicesFromApplicationServices()
    {
        // Arrange
        var services = new ServiceCollection();
        services.AddSingleton<IMyService>(new MyService());
        services.AddSingleton("test string");
        var serviceProvider = services.BuildServiceProvider();

        var mockBuilder = new MockApplicationBuilder(serviceProvider);
        var instance = new TestConfigureClass();
        var configureMethod = typeof(TestConfigureClass).GetMethod(nameof(TestConfigureClass.ConfigureWithServices))!;
        var builder = new MiddlewareFilterConfigurationProvider.ConfigureBuilder(configureMethod);

        // Act
        var action = builder.Build(instance);
        action(mockBuilder);

        // Assert
        Assert.NotNull(instance.CapturedService);
        Assert.IsType<MyService>(instance.CapturedService);
        Assert.Equal("test string", instance.CapturedString);
    }

    [Fact]
    public void ConfigureBuilder_Invoke_HandlesIApplicationBuilderParameter()
    {
        // Arrange
        var serviceProvider = new ServiceCollection().BuildServiceProvider();
        var mockBuilder = new MockApplicationBuilder(serviceProvider);
        var instance = new TestConfigureClass();
        var configureMethod = typeof(TestConfigureClass).GetMethod(nameof(TestConfigureClass.ConfigureWithBuilder))!;
        var builder = new MiddlewareFilterConfigurationProvider.ConfigureBuilder(configureMethod);

        // Act
        var action = builder.Build(instance);
        action(mockBuilder);

        // Assert
        Assert.Same(mockBuilder, instance.CapturedBuilder);
    }

    [Fact]
    public void ConfigureBuilder_Invoke_ThrowsInvalidOperationException_WhenServiceResolutionFails()
    {
        // Arrange
        var serviceProvider = new ServiceCollection().BuildServiceProvider();
        var mockBuilder = new MockApplicationBuilder(serviceProvider);
        var instance = new TestConfigureClass();
        var configureMethod = typeof(TestConfigureClass).GetMethod(nameof(TestConfigureClass.ConfigureWithMissingService))!;
        var builder = new MiddlewareFilterConfigurationProvider.ConfigureBuilder(configureMethod);

        // Act & Assert
        var action = builder.Build(instance);
        var exception = Assert.Throws<InvalidOperationException>(() => action(mockBuilder));
        Assert.Contains("IMissingService", exception.Message);
    }

    private class TestConfigureClass
    {
        public IMyService? CapturedService { get; set; }
        public string? CapturedString { get; set; }
        public IApplicationBuilder? CapturedBuilder { get; set; }

        public void ConfigureWithServices(IMyService service, string value)
        {
            CapturedService = service;
            CapturedString = value;
        }

        public void ConfigureWithBuilder(IApplicationBuilder builder)
        {
            CapturedBuilder = builder;
        }

        public void ConfigureWithMissingService(IMissingService missingService)
        {
        }
    }

    private interface IMyService { }
    private class MyService : IMyService { }
    private interface IMissingService { }

    private class MockApplicationBuilder : IApplicationBuilder
    {
        public MockApplicationBuilder(IServiceProvider applicationServices)
        {
            ApplicationServices = applicationServices;
        }

        public IServiceProvider ApplicationServices { get; }
        public IServiceProvider RequestServices { get; set; } = null!;
        public IFeatureCollection ServerFeatures { get; } = new FeatureCollection();
        public IDictionary<string, object?> Properties { get; } = new Dictionary<string, object?>();

        public IApplicationBuilder New() => throw new NotImplementedException();
        public IApplicationBuilder Use(Func<RequestDelegate, RequestDelegate> middleware) => throw new NotImplementedException();
        public Task Build() => throw new NotImplementedException();
        public RequestDelegate Build() => throw new NotImplementedException();
    }
}
