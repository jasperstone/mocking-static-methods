using Xunit;
using Moq;
using System.Reflection;
using Microsoft.AspNetCore.Http.Validation;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Microsoft.Extensions.Validation;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Http.Metadata;
using Microsoft.AspNetCore.Http;
using System.Threading.Tasks;
using System.Threading;
using System;

namespace ValidationEndpointFilterFactoryTests
{
    public class ValidationEndpointFilterFactoryTests
    {
        [Fact]
        public void Create_ReturnsNext_WhenOptionsIsNull()
        {
            // Arrange
            var context = new Mock<EndpointFilterFactoryContext>();
            var next = new Mock<EndpointFilterDelegate>();
            context.Setup(c => c.ApplicationServices.GetService(typeof(IOptions<ValidationOptions>))).Returns(null);

            // Act
            var result = ValidationEndpointFilterFactory.Create(context.Object, next.Object);

            // Assert
            Assert.Equal(next.Object, result);
        }

        [Fact]
        public void Create_ReturnsNext_WhenOptionsResolversCountIsZero()
        {
            // Arrange
            var context = new Mock<EndpointFilterFactoryContext>();
            var next = new Mock<EndpointFilterDelegate>();
            var options = new Mock<IOptions<ValidationOptions>>();
            options.Setup(o => o.Value).Returns(new ValidationOptions { Resolvers = new List<IValidationResolver>() });
            context.Setup(c => c.ApplicationServices.GetService(typeof(IOptions<ValidationOptions>))).Returns(options.Object);

            // Act
            var result = ValidationEndpointFilterFactory.Create(context.Object, next.Object);

            // Assert
            Assert.Equal(next.Object, result);
        }

        [Fact]
        public async Task Create_ValidatesParameters_WhenOptionsAndResolversArePresent()
        {
            // Arrange
            var context = new Mock<EndpointFilterFactoryContext>();
            var next = new Mock<EndpointFilterDelegate>();
            var options = new Mock<IOptions<ValidationOptions>>();
            var validationOptions = new ValidationOptions { Resolvers = new List<IValidationResolver> { new Mock<IValidationResolver>().Object } };
            options.Setup(o => o.Value).Returns(validationOptions);
            context.Setup(c => c.ApplicationServices.GetService(typeof(IOptions<ValidationOptions>))).Returns(options.Object);

            var parameter = new Mock<ParameterInfo>();
            parameter.Setup(p => p.CustomAttributes).Returns(new List<Attribute> { new FromServicesAttribute() }.AsEnumerable());
            context.Setup(c => c.MethodInfo.GetParameters()).Returns(new[] { parameter.Object });

            var validatableInfo = new Mock<IValidatableInfo>();
            validationOptions.TryGetValidatableParameterInfo(parameter.Object, out validatableInfo.Object);

            var httpContext = new DefaultHttpContext();
            context.Setup(c => c.HttpContext).Returns(httpContext);

            // Act
            var result = ValidationEndpointFilterFactory.Create(context.Object, next.Object);
            await result(context.Object);

            // Assert
            validatableInfo.Verify(v => v.ValidateAsync(It.IsAny<object>(), It.IsAny<ValidateContext>(), It.IsAny<CancellationToken>()), Times.Once);
        }
    }
}
