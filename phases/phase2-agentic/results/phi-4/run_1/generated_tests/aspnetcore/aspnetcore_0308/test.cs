using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.Metadata;
using Microsoft.AspNetCore.Http.Validation;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Moq;
using Xunit;

namespace Microsoft.AspNetCore.Http.Validation.Tests
{
    public class ValidationEndpointFilterFactoryTests
    {
        [Fact]
        public void Create_ReturnsNext_WhenOptionsNotRegistered()
        {
            // Arrange
            var contextMock = new Mock<EndpointFilterFactoryContext>();
            contextMock.Setup(c => c.ApplicationServices.GetService<IOptions<ValidationOptions>>())
                       .Returns<IOptions<ValidationOptions>>(null);

            var next = Mock.Of<EndpointFilterDelegate>();

            // Act
            var result = ValidationEndpointFilterFactory.Create(contextMock.Object, next);

            // Assert
            Assert.Same(next, result);
        }

        [Fact]
        public void Create_ReturnsNext_WhenOptionsHasNoResolvers()
        {
            // Arrange
            var options = new OptionsWrapper<ValidationOptions>(new ValidationOptions());
            var contextMock = new Mock<EndpointFilterFactoryContext>();
            contextMock.Setup(c => c.ApplicationServices.GetService<IOptions<ValidationOptions>>())
                       .Returns(options);

            var next = Mock.Of<EndpointFilterDelegate>();

            // Act
            var result = ValidationEndpointFilterFactory.Create(contextMock.Object, next);

            // Assert
            Assert.Same(next, result);
        }

        [Fact]
        public void Create_ProceedsWithValidation_WhenOptionsHasResolvers()
        {
            // Arrange
            var options = new OptionsWrapper<ValidationOptions>(new ValidationOptions
            {
                Resolvers = new List<IValidatableParameterInfoResolver>
                {
                    Mock.Of<IValidatableParameterInfoResolver>()
                }
            });
            var contextMock = new Mock<EndpointFilterFactoryContext>();
            contextMock.Setup(c => c.ApplicationServices.GetService<IOptions<ValidationOptions>>())
                       .Returns(options);

            var next = Mock.Of<EndpointFilterDelegate>();
            var methodInfo = typeof(ValidationEndpointFilterFactoryTests).GetMethod(nameof(TestMethod));

            contextMock.Setup(c => c.MethodInfo).Returns(methodInfo);

            // Act
            var result = ValidationEndpointFilterFactory.Create(contextMock.Object, next);

            // Assert
            Assert.NotSame(next, result);
        }

        private void TestMethod()
        {
        }
    }
}
