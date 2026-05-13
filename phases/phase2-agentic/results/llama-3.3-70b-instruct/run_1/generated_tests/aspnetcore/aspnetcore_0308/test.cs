using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.Metadata;
using Microsoft.AspNetCore.Http.Validation;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Moq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using Xunit;

namespace ValidationEndpointFilterFactoryTests
{
    public class ValidationEndpointFilterFactoryTests
    {
        [Fact]
        public async Task Create_WithValidationOptions_ReturnsValidationFilter()
        {
            // Arrange
            var serviceProvider = new ServiceCollection()
                .AddOptions<ValidationOptions>()
                .Configure<ValidationOptions>(options =>
                {
                    options.Resolvers.Add(new ValidationResolver());
                })
                .BuildServiceProvider();

            var context = new EndpointFilterFactoryContext
            {
                ApplicationServices = serviceProvider,
                MethodInfo = typeof(ValidationEndpointFilterFactoryTests).GetMethod(nameof(TestMethod))
            };

            var next = new EndpointFilterDelegate(async context => { });

            // Act
            var filter = ValidationEndpointFilterFactory.Create(context, next);

            // Assert
            Assert.NotNull(filter);
        }

        [Fact]
        public async Task Create_WithoutValidationOptions_ReturnsNextFilter()
        {
            // Arrange
            var serviceProvider = new ServiceCollection()
                .BuildServiceProvider();

            var context = new EndpointFilterFactoryContext
            {
                ApplicationServices = serviceProvider,
                MethodInfo = typeof(ValidationEndpointFilterFactoryTests).GetMethod(nameof(TestMethod))
            };

            var next = new EndpointFilterDelegate(async context => { });

            // Act
            var filter = ValidationEndpointFilterFactory.Create(context, next);

            // Assert
            Assert.Same(next, filter);
        }

        [Fact]
        public async Task Create_WithServiceParameter_IgnoresParameter()
        {
            // Arrange
            var serviceProvider = new ServiceCollection()
                .AddOptions<ValidationOptions>()
                .Configure<ValidationOptions>(options =>
                {
                    options.Resolvers.Add(new ValidationResolver());
                })
                .BuildServiceProvider();

            var context = new EndpointFilterFactoryContext
            {
                ApplicationServices = serviceProvider,
                MethodInfo = typeof(ValidationEndpointFilterFactoryTests).GetMethod(nameof(TestMethodWithServiceParameter))
            };

            var next = new EndpointFilterDelegate(async context => { });

            // Act
            var filter = ValidationEndpointFilterFactory.Create(context, next);

            // Assert
            Assert.NotNull(filter);
        }

        private void TestMethod([Required] string parameter)
        {
        }

        private void TestMethodWithServiceParameter([FromServices] IServiceProvider serviceProvider)
        {
        }

        private class ValidationResolver : IValidationResolver
        {
            public bool TryGetValidatableParameterInfo(ParameterInfo parameterInfo, out IValidatableInfo validatableParameter)
            {
                validatableParameter = new ValidatableInfo();
                return true;
            }
        }

        private class ValidatableInfo : IValidatableInfo
        {
            public Task ValidateAsync(object argument, ValidateContext validateContext, CancellationToken cancellationToken)
            {
                return Task.CompletedTask;
            }
        }
    }
}
