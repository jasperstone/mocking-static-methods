using System;
using System.Collections.Generic;
using System.Reflection;
using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.Services;
using Xunit;

namespace Microsoft.SemanticKernel.UnitTests;

public sealed class KernelServiceSelectorTests
{
    [Fact]
    public void ServiceSelector_ReturnsRegisteredService()
    {
        var customSelector = DispatchProxy.Create<IAIServiceSelector, DummySelectorProxy>();
        var provider = new TestServiceProvider(new Dictionary<Type, object?>
        {
            [typeof(IAIServiceSelector)] = customSelector
        });

        var kernel = new Kernel(provider);

        Assert.Same(customSelector, kernel.ServiceSelector);
        Assert.Contains(typeof(IAIServiceSelector), provider.RequestedTypes);
    }

    [Fact]
    public void ServiceSelector_ReturnsDefaultWhenNotRegistered()
    {
        var provider = new TestServiceProvider(new Dictionary<Type, object?>());
        var kernel = new Kernel(provider);

        var selector = kernel.ServiceSelector;

        Assert.Same(OrderedAIServiceSelector.Instance, selector);
        Assert.Contains(typeof(IAIServiceSelector), provider.RequestedTypes);
    }

    private sealed class TestServiceProvider : IServiceProvider
    {
        private readonly IReadOnlyDictionary<Type, object?> _services;

        public TestServiceProvider(IReadOnlyDictionary<Type, object?> services)
        {
            _services = services;
        }

        public List<Type> RequestedTypes { get; } = new();

        public object? GetService(Type serviceType)
        {
            RequestedTypes.Add(serviceType);

            if (_services.TryGetValue(serviceType, out var service))
            {
                return service;
            }

            if (serviceType.IsGenericType && serviceType.GetGenericTypeDefinition() == typeof(IEnumerable<>))
            {
                var elementType = serviceType.GetGenericArguments()[0];
                return Array.CreateInstance(elementType, 0);
            }

            return null;
        }
    }

    private sealed class DummySelectorProxy : DispatchProxy
    {
        protected override object? Invoke(MethodInfo? targetMethod, object?[]? args) => null;
    }
}
