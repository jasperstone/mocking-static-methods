using System;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.Services;
using Xunit;

public class KernelServiceSelectorTests
{
    private class DummyAIService : IAIService { }

    private class DummyServiceSelector : IAIServiceSelector
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
            if (typeof(T) == typeof(DummyAIService))
            {
                service = new DummyAIService() as T;
                return true;
            }
            return false;
        }
    }

    [Fact]
    public void ServiceSelector_ReturnsServiceFromProvider_WhenAvailable()
    {
        var services = new ServiceCollection();
        var dummySelector = new DummyServiceSelector();
        services.AddSingleton<IAIServiceSelector>(dummySelector);
        var provider = services.BuildServiceProvider();

        var kernel = new Kernel(provider);

        Assert.Same(dummySelector, kernel.ServiceSelector);
    }

    [Fact]
    public void ServiceSelector_ReturnsDefaultInstance_WhenServiceNotAvailable()
    {
        var services = new ServiceCollection();
        var provider = services.BuildServiceProvider();

        var kernel = new Kernel(provider);

        Assert.Same(OrderedAIServiceSelector.Instance, kernel.ServiceSelector);
    }
}
