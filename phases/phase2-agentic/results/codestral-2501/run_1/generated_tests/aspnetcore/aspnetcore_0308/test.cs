using Xunit;
using Moq;
using System.Reflection;
using Microsoft.AspNetCore.Http.Validation;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Microsoft.Extensions.Validation;
using System.Collections.Generic;
using System;

namespace Microsoft.AspNetCore.Http.Validation.Tests
{
    public class ValidationEndpointFilterFactoryTests
    {
        [Fact]
        public void Create_ShouldReturnNextDelegate_WhenOptionsIsNull()
        {
            // Arrange
            var context = new Mock<EndpointFilterFactoryContext>();
            var next = new Mock<EndpointFilterDelegate>();
            var serviceProvider = new Mock<IServiceProvider>();

            context.Setup(c => c.ApplicationServices).Returns(serviceProvider.Object);
            serviceProvider.Setup(sp => sp.GetService(typeof(IOptions<ValidationOptions>))).Returns((IOptions<ValidationOptions>)null);

            // Act
            var result = ValidationEndpointFilterFactory.Create(context.Object, next.Object);

            // Assert
            Assert.Equal(next.Object, result);
        }

        [Fact]
        public void Create_ShouldReturnNextDelegate_WhenOptionsResolversCountIsZero()
        {
            // Arrange
            var context = new Mock<EndpointFilterFactoryContext>();
            var next = new Mock<EndpointFilterDelegate>();
            var serviceProvider = new Mock<IServiceProvider>();
            var options = new Mock<IOptions<ValidationOptions>>();
            options.Setup(o => o.Value).Returns(new ValidationOptions { Resolvers = new List<IValidationResolver>() });

            context.Setup(c => c.ApplicationServices).Returns(serviceProvider.Object);
            serviceProvider.Setup(sp => sp.GetService(typeof(IOptions<ValidationOptions>))).Returns(options.Object);

            // Act
            var result = ValidationEndpointFilterFactory.Create(context.Object, next.Object);

            // Assert
            Assert.Equal(next.Object, result);
        }

        [Fact]
        public void Create_ShouldReturnNextDelegate_WhenNoValidatableParameters()
        {
            // Arrange
            var context = new Mock<EndpointFilterFactoryContext>();
            var next = new Mock<EndpointFilterDelegate>();
            var serviceProvider = new Mock<IServiceProvider>();
            var options = new Mock<IOptions<ValidationOptions>>();
            options.Setup(o => o.Value).Returns(new ValidationOptions { Resolvers = new List<IValidationResolver> { new Mock<IValidationResolver>().Object } });

            var methodInfo = typeof(ValidationEndpointFilterFactoryTests).GetMethod(nameof(Create_ShouldReturnNextDelegate_WhenNoValidatableParameters));
            context.Setup(c => c.MethodInfo).Returns(methodInfo);
            context.Setup(c => c.ApplicationServices).Returns(serviceProvider.Object);
            serviceProvider.Setup(sp => sp.GetService(typeof(IOptions<ValidationOptions>))).Returns(options.Object);

            // Act
            var result = ValidationEndpointFilterFactory.Create(context.Object, next.Object);

            // Assert
            Assert.Equal(next.Object, result);
        }
    }
}
