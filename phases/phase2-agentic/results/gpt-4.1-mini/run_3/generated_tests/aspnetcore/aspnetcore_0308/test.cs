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
            public Task ValidateAsync(object instance, ValidateContext context, CancellationToken cancellationToken)
            {
                context.ValidationErrors.Add("Error");
                return Task.CompletedTask;
            }
        }

        private class TestValidationOptions : ValidationOptions
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

        private class TestEndpointFilterContext : EndpointFilterInvocationContext
        {
            public TestEndpointFilterContext(object[] args, HttpContext httpContext)
            {
                Arguments = new List<object>(args);
                HttpContext = httpContext;
            }
        }

        private delegate Task<object?> EndpointFilterDelegate(EndpointFilterInvocationContext context);

        private class EndpointFilterFactoryContext
        {
            public IServiceProvider ApplicationServices { get; set; } = null!;
            public MethodInfo MethodInfo { get; set; } = null!;
        }

        [Fact]
        public void Create_ReturnsNext_WhenOptionsIsNull()
        {
            var services = new Mock<IServiceProvider>();
            services.Setup(s => s.GetService(typeof(IOptions<ValidationOptions>))).Returns(null);

            var context = new EndpointFilterFactoryContext
            {
                ApplicationServices = services.Object,
                MethodInfo = typeof(ValidationEndpointFilterFactoryTests).GetMethod(nameof(DummyMethod))!
            };

            EndpointFilterDelegate next = _ => Task.FromResult<object?>(null);

            var filter = ValidationEndpointFilterFactory.Create(context, next);

            Assert.Same(next, filter);
        }

        [Fact]
        public void Create_ReturnsNext_WhenResolversEmpty()
        {
            var options = new TestValidationOptions();
            options.Resolvers.Clear();

            var mockOptions = new Mock<IOptions<ValidationOptions>>();
            mockOptions.Setup(o => o.Value).Returns(options);

            var services = new Mock<IServiceProvider>();
            services.Setup(s => s.GetService(typeof(IOptions<ValidationOptions>))).Returns(mockOptions.Object);

            var context = new EndpointFilterFactoryContext
            {
                ApplicationServices = services.Object,
                MethodInfo = typeof(ValidationEndpointFilterFactoryTests).GetMethod(nameof(DummyMethod))!
            };

            EndpointFilterDelegate next = _ => Task.FromResult<object?>(null);

            var filter = ValidationEndpointFilterFactory.Create(context, next);

            Assert.Same(next, filter);
        }

        [Fact]
        public async Task Create_ValidatesParameterAndReturnsNext_WhenNoValidationErrors()
        {
            var options = new TestValidationOptions();
            options.Resolvers.Add(new ValidationOptions.DefaultResolver());

            var mockOptions = new Mock<IOptions<ValidationOptions>>();
            mockOptions.Setup(o => o.Value).Returns(options);

            var services = new Mock<IServiceProvider>();
            services.Setup(s => s.GetService(typeof(IOptions<ValidationOptions>))).Returns(mockOptions.Object);
            services.Setup(s => s.GetService(typeof(IServiceProviderIsService))).Returns(null);

            var context = new EndpointFilterFactoryContext
            {
                ApplicationServices = services.Object,
                MethodInfo = typeof(ValidationEndpointFilterFactoryTests).GetMethod(nameof(MethodWithValidParameter))!
            };

            var httpContext = new DefaultHttpContext();
            var invocationContext = new EndpointFilterInvocationContext(new object[] { new object() }, httpContext);

            EndpointFilterDelegate next = ctx =>
            {
                return Task.FromResult<object?>(new object());
            };

            var filter = ValidationEndpointFilterFactory.Create(context, next);

            var result = await filter(invocationContext);

            Assert.NotNull(result);
        }

        public void DummyMethod(string notValid) { }

        public void MethodWithValidParameter(object valid) { }
    }
}
