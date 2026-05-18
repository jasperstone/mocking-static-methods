using System;
using System.Collections.Generic;
using System.Reflection;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Http.Metadata;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Microsoft.Extensions.Validation;
using Moq;
using Xunit;

namespace ValidationEndpointFilterFactoryTests
{
    public class CreateMethodTests
    {
        [Fact]
        public void CallsGetServiceForValidationOptions()
        {
            // Arrange
            var methodInfoMock = new Mock<MethodInfo>();
            var parameterMock = new Mock<ParameterInfo>();
            parameterMock.Setup(p => p.CustomAttributes).Returns(new List<CustomAttributeData>());
            parameterMock.Setup(p => p.GetCustomAttribute<DisplayAttribute>()).Returns((DisplayAttribute)null);
            parameterMock.Setup(p => p.Name).Returns("param");
            var parameters = new ParameterInfo[] { parameterMock.Object };
            methodInfoMock.Setup(m => m.GetParameters()).Returns(parameters);

            var contextMock = new Mock<EndpointFilterFactoryContext>();
            contextMock.Setup(c => c.MethodInfo).Returns(methodInfoMock.Object);
            var applicationServicesMock = new Mock<IServiceProvider>();
            var validationOptions = new ValidationOptions
            {
                Resolvers = new List<IValidationResolver> { }
            };
            var optionsMock = new Mock<IOptions<ValidationOptions>>();
            optionsMock.Setup(o => o.Value).Returns(validationOptions);
            applicationServicesMock.Setup(s => s.GetService<IOptions<ValidationOptions>>()).Returns(optionsMock.Object);
            applicationServicesMock.Setup(s => s.GetService<IServiceProviderIsService>()).Returns((IServiceProviderIsService)null);
            contextMock.Setup(c => c.ApplicationServices).Returns(applicationServicesMock.Object);
            contextMock.Setup(c => c.Arguments).Returns(new object[] { "arg" });
            var nextCalled = false;
            EndpointFilterDelegate next = ctx =>
            {
                nextCalled = true;
                return System.Threading.Tasks.Task.FromResult<object>(null);
            };

            // Act
            var result = ValidationEndpointFilterFactory.Create(contextMock.Object, next);

            // Assert
            Assert.NotNull(result);
            Assert.False(nextCalled);
        }
    }
}
