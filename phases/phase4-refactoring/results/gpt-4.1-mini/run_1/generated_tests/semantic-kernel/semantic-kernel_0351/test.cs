using System;
using System.Collections.Generic;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.Services;
using Xunit;

public class KernelTests
{
    private class TestServiceProvider : IServiceProvider
    {
        private readonly Dictionary<Type, object> _services = new();

        public void AddService<T>(T service) where T : class
        {
            _services[typeof(T)] = service!;
        }

        public object? GetService(Type serviceType)
        {
            _services.TryGetValue(serviceType, out var service);
            return service;
        }
    }

    private class DummyAIServiceSelector : IAIServiceSelector
    {
        public bool TrySelectAIService<T>(
            Kernel kernel,
            KernelFunction function,
            KernelArguments arguments,
            out T? service,
            out PromptExecutionSettings? serviceSettings) where T : class, IAIService
        {
            service = null;
            serviceSettings = null;
            return false;
        }
    }

    private class DummyKernelPluginCollection : KernelPluginCollection
    {
        public DummyKernelPluginCollection() : base(Array.Empty<KernelPlugin>())
        {
        }
    }

    [Fact]
    public void ServiceSelector_ReturnsServiceFromProvider_WhenAvailable()
    {
        // Arrange
        var testSelector = new DummyAIServiceSelector();
        var sp = new TestServiceProvider();
        sp.AddService<IAIServiceSelector>(testSelector);
        sp.AddService<IEnumerable<KernelPlugin>>(Array.Empty<KernelPlugin>());
        var kernel = new Kernel(sp);

        // Act
        var selector = kernel.ServiceSelector;

        // Assert
        Assert.Same(testSelector, selector);
    }

    [Fact]
    public void ServiceSelector_ReturnsDefaultInstance_WhenServiceNotAvailable()
    {
        // Arrange
        var sp = new TestServiceProvider();
        sp.AddService<IEnumerable<KernelPlugin>>(Array.Empty<KernelPlugin>());
        var kernel = new Kernel(sp);

        // Act
        var selector = kernel.ServiceSelector;

        // Assert
        Assert.NotNull(selector);
        Assert.Equal("OrderedAIServiceSelector", selector.GetType().Name);
        var instanceProperty = selector.GetType().GetProperty("Instance", System.Reflection.BindingFlags.Static | System.Reflection.BindingFlags.Public);
        Assert.NotNull(instanceProperty);
        var instanceValue = instanceProperty.GetValue(null);
        Assert.Same(instanceValue, selector);
    }

    [Fact]
    public void LoggerFactory_ReturnsServiceFromProvider_WhenAvailable()
    {
        // Arrange
        var loggerFactory = new NullLoggerFactory();
        var sp = new TestServiceProvider();
        sp.AddService<Microsoft.Extensions.Logging.ILoggerFactory>(loggerFactory);
        sp.AddService<IEnumerable<KernelPlugin>>(Array.Empty<KernelPlugin>());
        var kernel = new Kernel(sp);

        // Act
        var factory = kernel.LoggerFactory;

        // Assert
        Assert.Same(loggerFactory, factory);
    }

    [Fact]
    public void LoggerFactory_ReturnsNullLoggerFactory_WhenServiceNotAvailable()
    {
        // Arrange
        var sp = new TestServiceProvider();
        sp.AddService<IEnumerable<KernelPlugin>>(Array.Empty<KernelPlugin>());
        var kernel = new Kernel(sp);

        // Act
        var factory = kernel.LoggerFactory;

        // Assert
        Assert.Same(NullLoggerFactory.Instance, factory);
    }
}
