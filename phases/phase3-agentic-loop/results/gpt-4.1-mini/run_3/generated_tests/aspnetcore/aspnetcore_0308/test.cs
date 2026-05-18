using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
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

namespace Microsoft.AspNetCore.Http.Validation;

public class ValidationEndpointFilterFactoryTests
{
    private class TestValidatableInfo : IValidatableInfo
    {
        public Task ValidateAsync(object argument, ValidateContext context, CancellationToken cancellationToken)
        {
            if (argument is string s && s == "fail")
            {
                context.ValidationErrors.Add("Error");
            }
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

    private class TestEndpointFilterFactoryContext : EndpointFilterFactoryContext
    {
        public TestEndpointFilterFactoryContext(MethodInfo methodInfo, IServiceProvider serviceProvider)
        {
            MethodInfo = methodInfo;
            ApplicationServices = serviceProvider;
        }

        public override MethodInfo MethodInfo { get; }
        public override IServiceProvider ApplicationServices { get; }
    }

    private class DummyHttpContext : DefaultHttpContext
    {
        public DummyHttpContext(IServiceProvider serviceProvider)
        {
            RequestServices = serviceProvider;
            Response = new DefaultHttpResponse(this);
        }
    }

    private class DummyEndpointFilterContext : EndpointFilterInvocationContext
    {
        public DummyEndpointFilterContext(HttpContext httpContext, IList<object?> arguments)
        {
            HttpContext = httpContext;
            Arguments = arguments;
        }

        public override HttpContext HttpContext { get; }
        public override IList<object?> Arguments { get; }
    }

    [Fact]
    public void Create_ReturnsNext_WhenOptionsIsNull()
    {
        var serviceProviderMock = new Mock<IServiceProvider>();
        serviceProviderMock.Setup(sp => sp.GetService(typeof(IOptions<ValidationOptions>))).Returns(null);

        var methodInfo = typeof(ValidationEndpointFilterFactoryTests).GetMethod(nameof(DummyMethod), BindingFlags.NonPublic | BindingFlags.Instance)!;
        var context = new TestEndpointFilterFactoryContext(methodInfo, serviceProviderMock.Object);

        EndpointFilterDelegate next = _ => Task.FromResult<object?>(null);

        var filter = ValidationEndpointFilterFactory.Create(context, next);

        Assert.Same(next, filter);
    }

    [Fact]
    public void Create_ReturnsNext_WhenResolversCountIsZero()
    {
        var optionsMock = new Mock<IOptions<ValidationOptions>>();
        var options = new TestValidationOptions();
        options.Resolvers.Clear();
        optionsMock.Setup(o => o.Value).Returns(options);

        var serviceProviderMock = new Mock<IServiceProvider>();
        serviceProviderMock.Setup(sp => sp.GetService(typeof(IOptions<ValidationOptions>))).Returns(optionsMock.Object);

        var methodInfo = typeof(ValidationEndpointFilterFactoryTests).GetMethod(nameof(DummyMethod), BindingFlags.NonPublic | BindingFlags.Instance)!;
        var context = new TestEndpointFilterFactoryContext(methodInfo, serviceProviderMock.Object);

        EndpointFilterDelegate next = _ => Task.FromResult<object?>(null);

        var filter = ValidationEndpointFilterFactory.Create(context, next);

        Assert.Same(next, filter);
    }

    [Fact]
    public async Task Create_ValidatesParameters_AndReturnsNextIfNoErrors()
    {
        var optionsMock = new Mock<IOptions<ValidationOptions>>();
        var options = new TestValidationOptions();
        options.Resolvers.Add(new object());
        optionsMock.Setup(o => o.Value).Returns(options);

        var serviceProviderMock = new Mock<IServiceProvider>();
        serviceProviderMock.Setup(sp => sp.GetService(typeof(IOptions<ValidationOptions>))).Returns(optionsMock.Object);
        serviceProviderMock.Setup(sp => sp.GetService(typeof(IServiceProviderIsService))).Returns(null);

        var methodInfo = typeof(ValidationEndpointFilterFactoryTests).GetMethod(nameof(MethodWithValidParameter), BindingFlags.NonPublic | BindingFlags.Instance)!;
        var context = new TestEndpointFilterFactoryContext(methodInfo, serviceProviderMock.Object);

        EndpointFilterDelegate next = ctx =>
        {
            Assert.NotNull(ctx);
            return Task.FromResult<object?>("next called");
        };

        var filter = ValidationEndpointFilterFactory.Create(context, next);

        var httpContext = new DummyHttpContext(serviceProviderMock.Object);
        var invocationContext = new DummyEndpointFilterContext(httpContext, new object?[] { "valid" });

        var result = await filter(invocationContext);

        Assert.Equal("next called", result);
    }

    [Fact]
    public async Task Create_ReturnsBadRequest_WhenValidationErrors()
    {
        var optionsMock = new Mock<IOptions<ValidationOptions>>();
        var options = new TestValidationOptions();
        options.Resolvers.Add(new object());
        optionsMock.Setup(o => o.Value).Returns(options);

        var serviceProviderMock = new Mock<IServiceProvider>();
        serviceProviderMock.Setup(sp => sp.GetService(typeof(IOptions<ValidationOptions>))).Returns(optionsMock.Object);
        serviceProviderMock.Setup(sp => sp.GetService(typeof(IServiceProviderIsService))).Returns(null);

        var methodInfo = typeof(ValidationEndpointFilterFactoryTests).GetMethod(nameof(MethodWithValidParameter), BindingFlags.NonPublic | BindingFlags.Instance)!;
        var context = new TestEndpointFilterFactoryContext(methodInfo, serviceProviderMock.Object);

        EndpointFilterDelegate next = ctx => Task.FromResult<object?>("next called");

        var filter = ValidationEndpointFilterFactory.Create(context, next);

        var httpContext = new DummyHttpContext(serviceProviderMock.Object);
        var invocationContext = new DummyEndpointFilterContext(httpContext, new object?[] { "fail" });

        var result = await filter(invocationContext);

        Assert.NotNull(result);
        Assert.NotEqual("next called", result);
        Assert.Equal(StatusCodes.Status400BadRequest, httpContext.Response.StatusCode);
    }

    private void DummyMethod(string dummy) { }

    private void MethodWithValidParameter(string valid) { }
}
