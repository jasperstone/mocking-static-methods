using System;
using System.Collections.Generic;
using System.Globalization;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
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

    private class TestServiceProviderWithPlugins : TestServiceProvider
    {
        public TestServiceProviderWithPlugins()
        {
            // Provide an empty list for IEnumerable<KernelPlugin> to avoid exceptions in Kernel constructor
            AddService<IEnumerable<KernelPlugin>>(new List<KernelPlugin>());
        }
    }

    [Fact]
    public void LoggerFactory_ReturnsRegisteredLoggerFactory()
    {
        var testLoggerFactory = new NullLoggerFactory();
        var sp = new TestServiceProviderWithPlugins();
        sp.AddService<ILoggerFactory>(testLoggerFactory);

        var kernel = new Kernel(sp);

        Assert.Same(testLoggerFactory, kernel.LoggerFactory);
    }

    [Fact]
    public void LoggerFactory_ReturnsNullLoggerFactoryWhenNoneRegistered()
    {
        var sp = new TestServiceProviderWithPlugins();

        var kernel = new Kernel(sp);

        Assert.Same(NullLoggerFactory.Instance, kernel.LoggerFactory);
    }

    [Fact]
    public void ServiceSelector_ReturnsRegisteredServiceSelector()
    {
        var testSelector = new TestAIServiceSelector();
        var sp = new TestServiceProviderWithPlugins();
        sp.AddService<IAIServiceSelector>(testSelector);

        var kernel = new Kernel(sp);

        Assert.Same(testSelector, kernel.ServiceSelector);
    }

    [Fact]
    public void ServiceSelector_ReturnsDefaultWhenNoneRegistered()
    {
        var sp = new TestServiceProviderWithPlugins();

        var kernel = new Kernel(sp);

        // The default fallback is internal, so we check that the returned instance is not null and implements IAIServiceSelector
        Assert.NotNull(kernel.ServiceSelector);
        Assert.IsAssignableFrom<IAIServiceSelector>(kernel.ServiceSelector);
    }

    private class TestAIServiceSelector : IAIServiceSelector
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
}
