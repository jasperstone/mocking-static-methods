using Microsoft.Extensions.DependencyInjection;
using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.Abstractions;
using Microsoft.SemanticKernel.Services;
using Moq;
using System;
using System.Collections.Generic;
using Xunit;

namespace SemanticKernel.Tests
{
    public class KernelTests
    {
        [Fact]
        public void ServiceSelector_GetService_ReturnsService()
        {
            // Arrange
            var serviceProvider = new ServiceCollection()
                .AddSingleton<IAIServiceSelector, MockAIServiceSelector>()
                .BuildServiceProvider();

            var kernel = new Kernel(serviceProvider);

            // Act
            var serviceSelector = kernel.ServiceSelector;

            // Assert
            Assert.IsType<MockAIServiceSelector>(serviceSelector);
        }

        [Fact]
        public void ServiceSelector_GetService_ReturnsDefaultService()
        {
            // Arrange
            var serviceProvider = new ServiceCollection()
                .BuildServiceProvider();

            var kernel = new Kernel(serviceProvider);

            // Act
            var serviceSelector = kernel.ServiceSelector;

            // Assert
            Assert.IsType<OrderedAIServiceSelector>(serviceSelector);
        }

        private class MockAIServiceSelector : IAIServiceSelector
        {
            public IEnumerable<IAIService> GetServices()
            {
                throw new NotImplementedException();
            }
        }
    }
}
