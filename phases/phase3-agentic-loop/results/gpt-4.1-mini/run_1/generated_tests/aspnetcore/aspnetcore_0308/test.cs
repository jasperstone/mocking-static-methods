using System;
using System.Collections.Generic;
using System.Reflection;
using System.Threading;
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
            public Task ValidateAsync(object argument, ValidateContext context, CancellationToken cancellationToken)
            {
                context.ValidationErrors.Add("Error");
                return Task.CompletedTask;
            }
        }

        private class TestOptions : ValidationOptions
        {
            public override bool TryGetValidatableParameterInfo(ParameterInfo parameter, out IValidatableInfo validatableInfo)
            {
                if (parameter.Name == "valid")
                {
                    validatableInfo = new TestValidatableInfo();
                    return true;
                }
                validatableInfo = null!;
                return false;
            }
        }

        private class EndpointFilterFactoryContext
        {
            public MethodInfo MethodInfo { get; }
            public IServiceProvider ApplicationServices { get; }

            public EndpointFilterFactoryContext(MethodInfo methodInfo, IServiceProvider services)
            {
                MethodInfo = methodInfo;
                ApplicationServices = services;
            }
        }

        private delegate Task<object?> EndpointFilterDelegate(EndpointFilterInvocationContext context);

        private class EndpointFilterInvocationContext
        {
            public IList<object?> Arguments { get; }
            public HttpContext HttpContext { get; }

            public EndpointFilterInvocationContext(IList<object?> arguments, HttpContext httpContext)
            {
                Arguments = arguments;
                HttpContext = httpContext;
            }
        }

        [Fact]
        public void Create_ReturnsNext_WhenOptionsIsNull()
        {
            var services = new Mock<IServiceProvider>();
            services.Setup(s => s.GetService(typeof(IOptions<ValidationOptions>))).Returns(null);

            var method = typeof(ValidationEndpointFilterFactoryTests).GetMethod(nameof(DummyMethod))!;
            var context = new EndpointFilterFactoryContext(method, services.Object);

            EndpointFilterDelegate next = ctx => Task.FromResult<object?>(null);

            var filter = ValidationEndpointFilterFactory.Create(context, next);

            Assert.Same(next, filter);
        }

        [Fact]
        public void Create_ReturnsNext_WhenResolversEmpty()
        {
            var options = new Mock<IOptions<ValidationOptions>>();
            var validationOptions = new TestOptions();
            validationOptions.Resolvers.Clear();
            options.Setup(o => o.Value).Returns(validationOptions);

            var services = new Mock<IServiceProvider>();
            services.Setup(s => s.GetService(typeof(IOptions<ValidationOptions>))).Returns(options.Object);

            var method = typeof(ValidationEndpointFilterFactoryTests).GetMethod(nameof(DummyMethod))!;
            var context = new EndpointFilterFactoryContext(method, services.Object);

            EndpointFilterDelegate next = ctx => Task.FromResult<object?>(null);

            var filter = ValidationEndpointFilterFactory.Create(context, next);

            Assert.Same(next, filter);
        }

        [Fact]
        public async Task Create_ValidatesParameters_AndReturnsProblemDetailsOnError()
        {
            var options = new Mock<IOptions<ValidationOptions>>();
            var validationOptions = new TestOptions();
            validationOptions.Resolvers.Add(_ => null); // Add dummy resolver to avoid early return
            options.Setup(o => o.Value).Returns(validationOptions);

            var services = new Mock<IServiceProvider>();
            services.Setup(s => s.GetService(typeof(IOptions<ValidationOptions>))).Returns(options.Object);
            services.Setup(s => s.GetService(typeof(IServiceProviderIsService))).Returns(null);

            var method = typeof(ValidationEndpointFilterFactoryTests).GetMethod(nameof(MethodWithValidParameter))!;
            var context = new EndpointFilterFactoryContext(method, services.Object);

            EndpointFilterDelegate next = ctx => Task.FromResult<object?>("next called");

            var filter = ValidationEndpointFilterFactory.Create(context, next);

            var httpContext = new DefaultHttpContext();
            var invocationContext = new EndpointFilterInvocationContext(new object?[] { new object() }, httpContext);

            var result = await filter(invocationContext);

            // Because validation adds an error, the filter should return a problem details object, not call next
            Assert.NotEqual("next called", result);
            Assert.Equal(StatusCodes.Status400BadRequest, httpContext.Response.StatusCode);
        }

        private void DummyMethod(string notUsed) { }

        private void MethodWithValidParameter(object valid) { }
    }
}
