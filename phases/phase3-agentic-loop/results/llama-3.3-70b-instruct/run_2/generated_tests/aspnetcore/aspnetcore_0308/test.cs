using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.Metadata;
using Microsoft.AspNetCore.Http.Validation;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Moq;
using System;
using System.Reflection;
using System.Collections.Generic;
using Xunit;

namespace ValidationEndpointFilterFactoryTests
{
    public class ValidationEndpointFilterFactoryTests
    {
        [Fact]
        public void Create_WhenOptionsAreNull_ReturnsNext()
        {
            // Arrange
            var context = new EndpointFilterFactoryContext
            {
                MethodInfo = typeof(ValidationEndpointFilterFactoryTests).GetMethod(nameof(TestMethod)),
                ApplicationServices = new ServiceCollection().BuildServiceProvider()
            };
            var next = Mock.Of<EndpointFilterDelegate>();

            // Act
            var result = ValidationEndpointFilterFactory.Create(context, next);

            // Assert
            Assert.Same(next, result);
        }

        [Fact]
        public void Create_WhenOptionsAreNotNullAndResolversAreEmpty_ReturnsNext()
        {
            // Arrange
            var context = new EndpointFilterFactoryContext
            {
                MethodInfo = typeof(ValidationEndpointFilterFactoryTests).GetMethod(nameof(TestMethod)),
                ApplicationServices = new ServiceCollection()
                    .AddOptions<ValidationOptions>()
                    .Configure<ValidationOptions>(options =>
                    {
                        options.Resolvers = new List<IValidationResolver>();
                    })
                    .Services
                    .BuildServiceProvider()
            };
            var next = Mock.Of<EndpointFilterDelegate>();

            // Act
            var result = ValidationEndpointFilterFactory.Create(context, next);

            // Assert
            Assert.Same(next, result);
        }

        [Fact]
        public void Create_WhenOptionsAreNotNullAndResolversAreNotEmpty_ReturnsFilter()
        {
            // Arrange
            var context = new EndpointFilterFactoryContext
            {
                MethodInfo = typeof(ValidationEndpointFilterFactoryTests).GetMethod(nameof(TestMethod)),
                ApplicationServices = new ServiceCollection()
                    .AddOptions<ValidationOptions>()
                    .Configure<ValidationOptions>(options =>
                    {
                        options.Resolvers = new List<IValidationResolver> { Mock.Of<IValidationResolver>() };
                    })
                    .Services
                    .BuildServiceProvider()
            };
            var next = Mock.Of<EndpointFilterDelegate>();

            // Act
            var result = ValidationEndpointFilterFactory.Create(context, next);

            // Assert
            Assert.NotSame(next, result);
        }

        private void TestMethod([FromServices] object service)
        {
        }
    }
}
