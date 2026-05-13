using System;
using System.Collections.Generic;
using System.Reflection;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.Validation;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Microsoft.Extensions.Validation;
using Moq;
using Xunit;

namespace Microsoft.AspNetCore.Http.Validation.Tests
{
    public class ValidationEndpointFilterFactoryTests
    {
        private class TestValidatableInfo : IValidatableInfo
        {
            public Task ValidateAsync(object instance, ValidateContext context, System.Threading.CancellationToken cancellationToken)
            {
                context.ValidationErrors.Add("Error");
                return Task.CompletedTask;
            }
        }

        private class TestValidationOptions : ValidationOptions
        {
            public override bool TryGetValidatableParameterInfo(ParameterInfo parameter, out IValidatableInfo validatableInfo)
            {
                if (parameter.Name == "validParam")
                {
                    validatableInfo = new TestValidatableInfo();
                    return true;
                }
                validatableInfo = null!;
                return false;
            }
        }

        private class TestEndpointFilterContext : EndpointFilterFactoryContext
        {
            public TestEndpointFilterContext(IServiceProvider serviceProvider, MethodInfo methodInfo)
            {
                ApplicationServices = serviceProvider;
                MethodInfo = methodInfo;
                Arguments = new List<object?>();
                HttpContext = new DefaultHttpContext();
            }
        }

        private delegate Task<object?> EndpointFilterDelegate(EndpointFilterInvocationContext context);

        private class EndpointFilterInvocationContext
        {
            public IList<object?> Arguments { get; } = new List<object?>();
            public HttpContext HttpContext { get; }
            public EndpointFilterInvocationContext(HttpContext httpContext)
            {
                HttpContext = httpContext;
            }
        }

        private class EndpointFilterFactoryContext
        {
            public IServiceProvider ApplicationServices { get; set; } = null!;
            public MethodInfo MethodInfo { get; set; } = null!;
            public IList<object?> Arguments { get; set; } = null!;
            public HttpContext HttpContext { get; set; } = null!;
        }

        [Fact]
        public void Create_ReturnsNext_WhenOptionsIsNull()
        {
            var serviceProviderMock = new Mock<IServiceProvider>();
            serviceProviderMock.Setup(sp => sp.GetService(typeof(IOptions<ValidationOptions>))).Returns(null);

            var methodInfo = typeof(ValidationEndpointFilterFactoryTests).GetMethod(nameof(DummyMethod))!;
            var context = new TestEndpointFilterContext(serviceProviderMock.Object, methodInfo);

            EndpointFilterDelegate next = ctx => Task.FromResult<object?>(null);

            var filter = ValidationEndpointFilterFactory.Create(context, next);

            Assert.Same(next, filter);
        }

        [Fact]
        public void Create_CallsGetServiceOnApplicationServices()
        {
            var options = new TestValidationOptions();
            var optionsWrapper = Options.Create<ValidationOptions>(options);

            var serviceProviderMock = new Mock<IServiceProvider>();
            serviceProviderMock.Setup(sp => sp.GetService(typeof(IOptions<ValidationOptions>))).Returns(optionsWrapper);
            serviceProviderMock.Setup(sp => sp.GetService(typeof(IServiceProviderIsService))).Returns(null);

            var methodInfo = typeof(ValidationEndpointFilterFactoryTests).GetMethod(nameof(MethodWithValidParam))!;
            var context = new TestEndpointFilterContext(serviceProviderMock.Object, methodInfo);

            EndpointFilterDelegate next = ctx => Task.FromResult<object?>(null);

            var filter = ValidationEndpointFilterFactory.Create(context, next);

            Assert.NotNull(filter);
            serviceProviderMock.Verify(sp => sp.GetService(typeof(IOptions<ValidationOptions>)), Times.Once);
            serviceProviderMock.Verify(sp => sp.GetService(typeof(IServiceProviderIsService)), Times.Once);
        }

        public void DummyMethod(string param) { }

        public void MethodWithValidParam(string validParam) { }
    }
}
