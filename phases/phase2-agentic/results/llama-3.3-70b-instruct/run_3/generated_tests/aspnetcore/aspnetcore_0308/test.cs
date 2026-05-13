using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.Metadata;
using Microsoft.AspNetCore.Http.Validation;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Moq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Xunit;

namespace ValidationEndpointFilterFactoryTests
{
    public class ValidationEndpointFilterFactoryTests
    {
        [Fact]
        public async Task Create_Filter_With_Options()
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
                MethodInfo = typeof(ValidationEndpointFilterFactoryTests).GetMethod(nameof(TestMethod)),
            };

            var next = new EndpointFilterDelegate(async context => { await Task.CompletedTask; });

            // Act
            var filter = ValidationEndpointFilterFactory.Create(context, next);

            // Assert
            Assert.NotNull(filter);
        }

        [Fact]
        public async Task Create_Filter_Without_Options()
        {
            // Arrange
            var serviceProvider = new ServiceCollection()
                .BuildServiceProvider();

            var context = new EndpointFilterFactoryContext
            {
                ApplicationServices = serviceProvider,
                MethodInfo = typeof(ValidationEndpointFilterFactoryTests).GetMethod(nameof(TestMethod)),
            };

            var next = new EndpointFilterDelegate(async context => { await Task.CompletedTask; });

            // Act
            var filter = ValidationEndpointFilterFactory.Create(context, next);

            // Assert
            Assert.NotNull(filter);
        }

        [Fact]
        public async Task Create_Filter_With_ServiceProviderIsService()
        {
            // Arrange
            var serviceProvider = new ServiceCollection()
                .AddOptions<ValidationOptions>()
                .Configure<ValidationOptions>(options =>
                {
                    options.Resolvers.Add(new ValidationResolver());
                })
                .BuildServiceProvider();

            var serviceProviderIsService = new Mock<IServiceProviderIsService>();
            serviceProviderIsService.Setup(s => s.IsService(It.IsAny<Type>())).Returns(true);

            var context = new EndpointFilterFactoryContext
            {
                ApplicationServices = serviceProvider,
                MethodInfo = typeof(ValidationEndpointFilterFactoryTests).GetMethod(nameof(TestMethod)),
            };

            var next = new EndpointFilterDelegate(async context => { await Task.CompletedTask; });

            // Act
            var filter = ValidationEndpointFilterFactory.Create(context, next);

            // Assert
            Assert.NotNull(filter);
        }

        private void TestMethod([FromServices] object service)
        {
        }

        private class ValidationResolver : IValidationResolver
        {
            public bool TryGetValidatableParameterInfo(ParameterInfo parameterInfo, out IValidatableInfo validatableParameter)
            {
                validatableParameter = null;
                return false;
            }
        }
    }
}
