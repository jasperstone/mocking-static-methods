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
            public TestEndpointFilterContext(IList<object?> args, HttpContext httpContext)
                : base(args, httpContext)
            {
            }
        }

        private class DummyServiceProvider : IServiceProvider
        {
            private readonly Dictionary<Type, object> _services;

            public DummyServiceProvider(Dictionary<Type, object> services)
            {
                _services = services;
            }

            public object? GetService(Type serviceType)
            {
                _services.TryGetValue(serviceType, out var service);
                return service;
            }
        }

        [Fact]
        public void Create_ReturnsNext_WhenOptionsIsNull()
        {
            var services = new DummyServiceProvider(new Dictionary<Type, object>());
            var method = typeof(ValidationEndpointFilterFactoryTests).GetMethod(nameof(DummyMethod), BindingFlags.NonPublic | BindingFlags.Instance)!;
            var context = new EndpointFilterFactoryContext(method, services);

            EndpointFilterDelegate next = _ => Task.FromResult<object?>(null);
            var filter = ValidationEndpointFilterFactory.Create(context, next);

            Assert.Same(next, filter);
        }

        [Fact]
        public async Task Create_ValidatesParameterAndReturnsProblemDetails_WhenValidationErrors()
        {
            var options = new TestValidationOptions();
            options.Resolvers.Add(new object()); // Add dummy resolver to pass check

            var servicesDict = new Dictionary<Type, object>
            {
                { typeof(IOptions<ValidationOptions>), Options.Create<ValidationOptions>(options) },
                { typeof(IServiceProviderIsService), Mock.Of<IServiceProviderIsService>() }
            };
            var services = new DummyServiceProvider(servicesDict);

            var method = typeof(ValidationEndpointFilterFactoryTests).GetMethod(nameof(MethodWithValidParameter), BindingFlags.NonPublic | BindingFlags.Instance)!;
            var context = new EndpointFilterFactoryContext(method, services);

            var httpContext = new DefaultHttpContext();
            context.HttpContext = httpContext;

            EndpointFilterDelegate next = _ => Task.FromResult<object?>(null);
            var filter = ValidationEndpointFilterFactory.Create(context, next);

            var invocationContext = new EndpointFilterInvocationContext(new object?[] { new object() }, httpContext);

            var result = await filter(invocationContext);

            Assert.NotNull(result);
            Assert.Equal(StatusCodes.Status400BadRequest, httpContext.Response.StatusCode);
        }

        private void DummyMethod(object notValid)
        {
        }

        private void MethodWithValidParameter(object valid)
        {
        }
    }

    // Minimal stubs for required types from ASP.NET Core
    internal delegate Task<object?> EndpointFilterDelegate(EndpointFilterInvocationContext context);

    internal class EndpointFilterFactoryContext
    {
        public MethodInfo MethodInfo { get; }
        public IServiceProvider ApplicationServices { get; }
        public HttpContext HttpContext { get; set; } = new DefaultHttpContext();

        public EndpointFilterFactoryContext(MethodInfo methodInfo, IServiceProvider services)
        {
            MethodInfo = methodInfo;
            ApplicationServices = services;
        }
    }

    internal class EndpointFilterInvocationContext
    {
        public IList<object?> Arguments { get; }
        public HttpContext HttpContext { get; }

        public EndpointFilterInvocationContext(IList<object?> arguments, HttpContext httpContext)
        {
            Arguments = arguments;
            HttpContext = httpContext;
        }
    }

    internal class ValidateContext
    {
        public ValidationOptions ValidationOptions { get; set; } = null!;
        public ValidationContext ValidationContext { get; set; } = null!;
        public List<string> ValidationErrors { get; } = new();
    }

    internal interface IValidatableInfo
    {
        Task ValidateAsync(object argument, ValidateContext context, CancellationToken cancellationToken);
    }

    internal class ValidationOptions
    {
        public List<object> Resolvers { get; } = new();

        public virtual bool TryGetValidatableParameterInfo(ParameterInfo parameter, out IValidatableInfo validatableInfo)
        {
            validatableInfo = null!;
            return false;
        }
    }

    internal interface IServiceProviderIsService
    {
        bool IsService(Type serviceType);
    }
}
