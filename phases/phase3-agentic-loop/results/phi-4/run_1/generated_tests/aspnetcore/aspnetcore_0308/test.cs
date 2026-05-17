using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Http.Metadata;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Microsoft.Extensions.Validation;
using Moq;
using Xunit;

namespace Microsoft.AspNetCore.Http.Validation.Tests
{
    public class ValidationEndpointFilterFactoryTests
    {
        [Fact]
        public void Create_ShouldReturnNext_WhenValidationOptionsServiceIsNotAvailable()
        {
            // Arrange
            var context = new Mock<EndpointFilterFactoryContext>();
            context.Setup(c => c.ApplicationServices.GetService<IOptions<ValidationOptions>>())
                   .Returns<IOptions<ValidationOptions>>(null);
            context.Setup(c => c.MethodInfo).Returns(typeof(object).GetMethod("ToString"));
            var serviceProviderIsService = new Mock<IServiceProviderIsService>();
            context.Setup(c => c.ApplicationServices.GetService<IServiceProviderIsService>()).Returns(serviceProviderIsService.Object);
            var next = new EndpointFilterDelegate((c, e) => Task.CompletedTask);

            // Act
            var result = ValidationEndpointFilterFactory.Create(context.Object, next);

            // Assert
            Assert.Same(next, result);
        }

        [Fact]
        public void Create_ShouldReturnNext_WhenValidationOptionsResolversIsEmpty()
        {
            // Arrange
            var options = new Mock<IOptions<ValidationOptions>>();
            options.Setup(o => o.Value).Returns(new ValidationOptions { Resolvers = Array.Empty<IValidationMetadataResolver>() });
            var context = new Mock<EndpointFilterFactoryContext>();
            context.Setup(c => c.ApplicationServices.GetService<IOptions<ValidationOptions>>()).Returns(options.Object);
            context.Setup(c => c.MethodInfo).Returns(typeof(object).GetMethod("ToString"));
            var serviceProviderIsService = new Mock<IServiceProviderIsService>();
            context.Setup(c => c.ApplicationServices.GetService<IServiceProviderIsService>()).Returns(serviceProviderIsService.Object);
            var next = new EndpointFilterDelegate((c, e) => Task.CompletedTask);

            // Act
            var result = ValidationEndpointFilterFactory.Create(context.Object, next);

            // Assert
            Assert.Same(next, result);
        }

        [Fact]
        public void Create_ShouldNotReturnNext_WhenValidationOptionsServiceIsAvailableAndResolversIsNotEmpty()
        {
            // Arrange
            var options = new Mock<IOptions<ValidationOptions>>();
            options.Setup(o => o.Value).Returns(new ValidationOptions { Resolvers = new List<IValidationMetadataResolver> { Mock.Of<IValidationMetadataResolver>() } });
            var context = new Mock<EndpointFilterFactoryContext>();
            context.Setup(c => c.ApplicationServices.GetService<IOptions<ValidationOptions>>()).Returns(options.Object);
            context.Setup(c => c.MethodInfo).Returns(typeof(object).GetMethod("ToString"));
            var serviceProviderIsService = new Mock<IServiceProviderIsService>();
            context.Setup(c => c.ApplicationServices.GetService<IServiceProviderIsService>()).Returns(serviceProviderIsService.Object);
            var next = new EndpointFilterDelegate((c, e) => Task.CompletedTask);

            // Act
            var result = ValidationEndpointFilterFactory.Create(context.Object, next);

            // Assert
            Assert.NotSame(next, result);
        }
    }
}
