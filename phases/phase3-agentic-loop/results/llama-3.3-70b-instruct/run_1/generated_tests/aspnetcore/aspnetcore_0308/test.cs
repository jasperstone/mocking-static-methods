using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.Metadata;
using Microsoft.AspNetCore.Http.Validation;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Moq;
using System;
using System.Reflection;
using System.Threading.Tasks;
using Xunit;

namespace ValidationEndpointFilterFactoryTests
{
    public class ValidationEndpointFilterFactoryTests
    {
        [Fact]
        public async Task Create_WithNoValidationOptions_ReturnsNext()
        {
            // Arrange
            var context = new EndpointFilterFactoryContext
            {
                ApplicationServices = new ServiceCollection().BuildServiceProvider()
            };
            var next = new EndpointFilterDelegate(async context => { await Task.CompletedTask; });

            // Act
            var result = ValidationEndpointFilterFactory.Create(context, next);

            // Assert
            Assert.Same(next, result);
        }

        [Fact]
        public async Task Create_WithValidationOptionsButNoResolvers_ReturnsNext()
        {
            // Arrange
            var context = new EndpointFilterFactoryContext
            {
                ApplicationServices = new ServiceCollection()
                    .AddOptions<ValidationOptions>()
                    .Services
                    .BuildServiceProvider()
            };
            var next = new EndpointFilterDelegate(async context => { await Task.CompletedTask; });

            // Act
            var result = ValidationEndpointFilterFactory.Create(context, next);

            // Assert
            Assert.Same(next, result);
        }

        [Fact]
        public async Task Create_WithValidationOptionsAndResolvers_DoesNotReturnNext()
        {
            // Arrange
            var context = new EndpointFilterFactoryContext
            {
                ApplicationServices = new ServiceCollection()
                    .AddOptions<ValidationOptions>()
                    .Configure<ValidationOptions>(options =>
                    {
                        options.Resolvers = new System.Collections.Generic.List<IValidationResolver>();
                        options.Resolvers.Add(new Mock<IValidationResolver>().Object);
                    })
                    .Services
                    .BuildServiceProvider()
            };
            var next = new EndpointFilterDelegate(async context => { await Task.CompletedTask; });

            // Act
            var result = ValidationEndpointFilterFactory.Create(context, next);

            // Assert
            Assert.NotSame(next, result);
        }

        [Fact]
        public async Task Create_WithServiceParameter_DoesNotValidate()
        {
            // Arrange
            var methodInfo = typeof(ValidationEndpointFilterFactoryTests).GetMethod(nameof(ValidationEndpointFilterFactoryTests.TestMethodWithServiceParameter));
            var context = new EndpointFilterFactoryContext
            {
                MethodInfo = methodInfo,
                ApplicationServices = new ServiceCollection()
                    .AddOptions<ValidationOptions>()
                    .Configure<ValidationOptions>(options =>
                    {
                        options.Resolvers = new System.Collections.Generic.List<IValidationResolver>();
                        options.Resolvers.Add(new Mock<IValidationResolver>().Object);
                    })
                    .Services
                    .BuildServiceProvider()
            };
            var next = new EndpointFilterDelegate(async context => { await Task.CompletedTask; });

            // Act
            var result = ValidationEndpointFilterFactory.Create(context, next);

            // Assert
            Assert.NotSame(next, result);
        }

        [Fact]
        public async Task Create_WithValidatableParameter_Validates()
        {
            // Arrange
            var methodInfo = typeof(ValidationEndpointFilterFactoryTests).GetMethod(nameof(ValidationEndpointFilterFactoryTests.TestMethodWithValidatableParameter));
            var context = new EndpointFilterFactoryContext
            {
                MethodInfo = methodInfo,
                ApplicationServices = new ServiceCollection()
                    .AddOptions<ValidationOptions>()
                    .Configure<ValidationOptions>(options =>
                    {
                        options.Resolvers = new System.Collections.Generic.List<IValidationResolver>();
                        options.Resolvers.Add(new Mock<IValidationResolver>().Object);
                    })
                    .Services
                    .BuildServiceProvider()
            };
            var next = new EndpointFilterDelegate(async context => { await Task.CompletedTask; });

            // Act
            var result = ValidationEndpointFilterFactory.Create(context, next);

            // Assert
            Assert.NotSame(next, result);
        }

        private void TestMethodWithServiceParameter([FromServices] object service)
        {
        }

        private void TestMethodWithValidatableParameter([Required] string parameter)
        {
        }
    }
}
