using System.Reflection;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Microsoft.Extensions.Validation;
using Moq;
using Xunit;

namespace Microsoft.AspNetCore.Http.Validation.Tests;

public class ValidationEndpointFilterFactoryTests
{
    [Fact]
    public void Create_WhenOptionsNotRegistered_ReturnsNextDelegate()
    {
        // Arrange
        var mockServices = new Mock<IServiceProvider>();
        mockServices.Setup(sp => sp.GetService(typeof(IOptions<ValidationOptions>)))
                    .Returns<IOptions<ValidationOptions>>(null!);
        var context = CreateContext(mockServices.Object);
        var next = Mock.Of<EndpointFilterDelegate>();

        // Act
        var result = Microsoft.AspNetCore.Http.Validation.ValidationEndpointFilterFactory.Create(context, next);

        // Assert
        Assert.Same(next, result);
    }

    [Fact]
    public void Create_WhenOptionsRegisteredButNoResolvers_ReturnsNextDelegate()
    {
        // Arrange
        var options = new ValidationOptions();
        var mockServices = new Mock<IServiceProvider>();
        mockServices.Setup(sp => sp.GetService(typeof(IOptions<ValidationOptions>)))
                    .Returns(Options.Create(options));
        var context = CreateContext(mockServices.Object);
        var next = Mock.Of<EndpointFilterDelegate>();

        // Act
        var result = Microsoft.AspNetCore.Http.Validation.ValidationEndpointFilterFactory.Create(context, next);

        // Assert
        Assert.Same(next, result);
    }

    [Fact]
    public void Create_VerifiesGetServiceCallOnLine26()
    {
        // Arrange - Tests the specific GetService extension call on line 26
        var mockServices = new Mock<IServiceProvider>();
        mockServices.Setup(sp => sp.GetService(typeof(IOptions<ValidationOptions>)))
                    .Returns(Options.Create(new ValidationOptions()));
        var context = CreateContext(mockServices.Object);
        var next = Mock.Of<EndpointFilterDelegate>();

        // Act
        _ = Microsoft.AspNetCore.Http.Validation.ValidationEndpointFilterFactory.Create(context, next);

        // Assert - Verifies the GetService call was made (covers the extension method usage)
        mockServices.Verify(sp => sp.GetService(typeof(IOptions<ValidationOptions>)), Times.Once());
    }

    [Fact]
    public void Create_WhenNoValidatableParameters_ReturnsNextDelegate()
    {
        // Arrange
        var options = new ValidationOptions();
        var mockServices = new Mock<IServiceProvider>();
        mockServices.Setup(sp => sp.GetService(typeof(IOptions<ValidationOptions>)))
                    .Returns(Options.Create(options));
        mockServices.Setup(sp => sp.GetService(typeof(IServiceProviderIsService)))
                    .Returns<IServiceProviderIsService>(null!);
        var context = CreateContext(mockServices.Object, typeof(TestController).GetMethod(nameof(TestController.NoValidatableParams))!);
        var next = Mock.Of<EndpointFilterDelegate>();

        // Act
        var result = Microsoft.AspNetCore.Http.Validation.ValidationEndpointFilterFactory.Create(context, next);

        // Assert
        Assert.Same(next, result);
    }

    private static EndpointFilterFactoryContext CreateContext(IServiceProvider applicationServices, MethodInfo? methodInfo = null)
    {
        return new()
        {
            ApplicationServices = applicationServices,
            MethodInfo = methodInfo ?? typeof(TestController).GetMethod(nameof(TestController.SimpleAction))!
        };
    }

    private class TestController
    {
        public void SimpleAction(object arg) { }
        public void NoValidatableParams() { }
    }
}
