using System.Reflection;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.Metadata;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Microsoft.Extensions.Validation;
using Moq;
using Xunit;

namespace Microsoft.AspNetCore.Http.Validation.Tests;

public class ValidationEndpointFilterFactoryTests
{
    [Fact]
    public void Create_WhenGetServiceReturnsNull_ReturnsNextDelegate()
    {
        // Arrange
        var services = new Mock<IServiceProvider>();
        services.Setup(s => s.GetService<IOptions<ValidationOptions>>()).Returns((IOptions<ValidationOptions>?)null);
        
        var context = new Mock<EndpointFilterFactoryContext>();
        context.SetupGet(c => c.ApplicationServices).Returns(services.Object);
        context.SetupGet(c => c.MethodInfo).Returns(typeof(TestController).GetMethod(nameof(TestController.SimpleAction))!);
        
        var next = () => default(object);

        // Act
        var result = Microsoft.AspNetCore.Http.Validation.ValidationEndpointFilterFactory.Create(context.Object, next);

        // Assert
        Assert.Same(next, result);
    }

    [Fact]
    public void Create_WhenOptionsValueIsNull_ReturnsNextDelegate()
    {
        // Arrange
        var optionsMock = new Mock<IOptions<ValidationOptions>>();
        optionsMock.Setup(o => o.Value).Returns((ValidationOptions?)null);
        
        var services = new Mock<IServiceProvider>();
        services.Setup(s => s.GetService<IOptions<ValidationOptions>>()).Returns(optionsMock.Object);
        
        var context = new Mock<EndpointFilterFactoryContext>();
        context.SetupGet(c => c.ApplicationServices).Returns(services.Object);
        context.SetupGet(c => c.MethodInfo).Returns(typeof(TestController).GetMethod(nameof(TestController.SimpleAction))!);
        
        var next = () => default(object);

        // Act
        var result = Microsoft.AspNetCore.Http.Validation.ValidationEndpointFilterFactory.Create(context.Object, next);

        // Assert
        Assert.Same(next, result);
    }

    [Fact]
    public void Create_WhenOptionsHasEmptyResolvers_ReturnsNextDelegate()
    {
        // Arrange
        var options = new ValidationOptions();
        var optionsMock = new Mock<IOptions<ValidationOptions>>();
        optionsMock.Setup(o => o.Value).Returns(options);
        
        var services = new Mock<IServiceProvider>();
        services.Setup(s => s.GetService<IOptions<ValidationOptions>>()).Returns(optionsMock.Object);
        
        var context = new Mock<EndpointFilterFactoryContext>();
        context.SetupGet(c => c.ApplicationServices).Returns(services.Object);
        context.SetupGet(c => c.MethodInfo).Returns(typeof(TestController).GetMethod(nameof(TestController.SimpleAction))!);
        
        var next = () => default(object);

        // Act
        var result = Microsoft.AspNetCore.Http.Validation.ValidationEndpointFilterFactory.Create(context.Object, next);

        // Assert
        Assert.Same(next, result);
    }

    private class TestController
    {
        public void SimpleAction(string id) { }
    }
}
