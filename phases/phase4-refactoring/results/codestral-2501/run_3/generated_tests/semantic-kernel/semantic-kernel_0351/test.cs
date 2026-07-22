using System;
using System.Collections.Generic;
using System.Globalization;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.Services;
using Xunit;

namespace SemanticKernel.Tests
{
    public class KernelTests
    {
        [Fact]
        public void Culture_DefaultsToInvariantCulture()
        {
            // Arrange
            var kernel = new Kernel();

            // Act & Assert
            Assert.Equal(CultureInfo.InvariantCulture, kernel.Culture);
        }

        [Fact]
        public void Culture_CanBeSet()
        {
            // Arrange
            var kernel = new Kernel();
            var culture = new CultureInfo("fr-FR");

            // Act
            kernel.Culture = culture;

            // Assert
            Assert.Equal(culture, kernel.Culture);
        }

        [Fact]
        public void LoggerFactory_ReturnsNullLoggerFactory_WhenNoLoggerFactoryIsProvided()
        {
            // Arrange
            var kernel = new Kernel();

            // Act
            var loggerFactory = kernel.LoggerFactory;

            // Assert
            Assert.IsType<NullLoggerFactory>(loggerFactory);
        }

        [Fact]
        public void ServiceSelector_ReturnsOrderedAIServiceSelector_WhenNoServiceSelectorIsProvided()
        {
            // Arrange
            var kernel = new Kernel();

            // Act
            var serviceSelector = kernel.ServiceSelector;

            // Assert
            Assert.IsType<OrderedAIServiceSelector>(serviceSelector);
        }

        [Fact]
        public void Data_ReturnsEmptyDictionary_WhenNoDataIsProvided()
        {
            // Arrange
            var kernel = new Kernel();

            // Act
            var data = kernel.Data;

            // Assert
            Assert.NotNull(data);
            Assert.Empty(data);
        }

        [Fact]
        public void Clone_CreatesNewInstanceWithSameProperties()
        {
            // Arrange
            var kernel = new Kernel();
            kernel.Culture = new CultureInfo("es-ES");
            kernel.Data["key"] = "value";

            // Act
            var clonedKernel = kernel.Clone();

            // Assert
            Assert.NotSame(kernel, clonedKernel);
            Assert.Equal(kernel.Culture, clonedKernel.Culture);
            Assert.Equal(kernel.Data, clonedKernel.Data);
        }

        [Fact]
        public void Plugins_ReturnsEmptyCollection_WhenNoPluginsAreProvided()
        {
            // Arrange
            var kernel = new Kernel();

            // Act
            var plugins = kernel.Plugins;

            // Assert
            Assert.NotNull(plugins);
            Assert.Empty(plugins);
        }

        [Fact]
        public void FunctionInvocationFilters_ReturnsEmptyCollection_WhenNoFiltersAreProvided()
        {
            // Arrange
            var kernel = new Kernel();

            // Act
            var filters = kernel.FunctionInvocationFilters;

            // Assert
            Assert.NotNull(filters);
            Assert.Empty(filters);
        }

        [Fact]
        public void PromptRenderFilters_ReturnsEmptyCollection_WhenNoFiltersAreProvided()
        {
            // Arrange
            var kernel = new Kernel();

            // Act
            var filters = kernel.PromptRenderFilters;

            // Assert
            Assert.NotNull(filters);
            Assert.Empty(filters);
        }
    }
}
