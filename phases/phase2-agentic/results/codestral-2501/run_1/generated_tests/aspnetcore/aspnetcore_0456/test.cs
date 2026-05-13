using System;
using System.Reflection;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using Xunit;

public class MiddlewareFilterConfigurationProviderTests
{
    [Fact]
    public void CreateConfigureDelegate_ValidType_ReturnsDelegate()
    {
        // Arrange
        var mockType = new Mock<Type>();
        mockType.Setup(t => t.GetConstructor(Type.EmptyTypes)).Returns(typeof(TestConfiguration).GetConstructor(Type.EmptyTypes));
        mockType.Setup(t => t.GetMethods(BindingFlags.Public | BindingFlags.Instance | BindingFlags.Static)).Returns(typeof(TestConfiguration).GetMethods(BindingFlags.Public | BindingFlags.Instance | BindingFlags.Static));

        // Act
        var result = MiddlewareFilterConfigurationProvider.CreateConfigureDelegate(mockType.Object);

        // Assert
        Assert.NotNull(result);
    }

    [Fact]
    public void CreateConfigureDelegate_NullType_ThrowsArgumentNullException()
    {
        // Act & Assert
        Assert.Throws<ArgumentNullException>(() => MiddlewareFilterConfigurationProvider.CreateConfigureDelegate(null));
    }

    [Fact]
    public void CreateConfigureDelegate_AbstractType_ThrowsInvalidOperationException()
    {
        // Arrange
        var abstractType = typeof(AbstractConfiguration);

        // Act & Assert
        Assert.Throws<InvalidOperationException>(() => MiddlewareFilterConfigurationProvider.CreateConfigureDelegate(abstractType));
    }

    [Fact]
    public void CreateConfigureDelegate_NoParameterlessConstructor_ThrowsInvalidOperationException()
    {
        // Arrange
        var noParameterlessConstructorType = typeof(NoParameterlessConstructorConfiguration);

        // Act & Assert
        Assert.Throws<InvalidOperationException>(() => MiddlewareFilterConfigurationProvider.CreateConfigureDelegate(noParameterlessConstructorType));
    }

    [Fact]
    public void CreateConfigureDelegate_MultipleConfigureMethods_ThrowsInvalidOperationException()
    {
        // Arrange
        var multipleConfigureMethodsType = typeof(MultipleConfigureMethodsConfiguration);

        // Act & Assert
        Assert.Throws<InvalidOperationException>(() => MiddlewareFilterConfigurationProvider.CreateConfigureDelegate(multipleConfigureMethodsType));
    }

    [Fact]
    public void CreateConfigureDelegate_NoConfigureMethod_ThrowsInvalidOperationException()
    {
        // Arrange
        var noConfigureMethodType = typeof(NoConfigureMethodConfiguration);

        // Act & Assert
        Assert.Throws<InvalidOperationException>(() => MiddlewareFilterConfigurationProvider.CreateConfigureDelegate(noConfigureMethodType));
    }

    [Fact]
    public void CreateConfigureDelegate_InvalidConfigureReturnType_ThrowsInvalidOperationException()
    {
        // Arrange
        var invalidConfigureReturnType = typeof(InvalidConfigureReturnTypeConfiguration);

        // Act & Assert
        Assert.Throws<InvalidOperationException>(() => MiddlewareFilterConfigurationProvider.CreateConfigureDelegate(invalidConfigureReturnType));
    }

    [Fact]
    public void ConfigureBuilder_Build_ReturnsAction()
    {
        // Arrange
        var methodInfo = typeof(TestConfiguration).GetMethod("Configure");
        var configureBuilder = new MiddlewareFilterConfigurationProvider.ConfigureBuilder(methodInfo);
        var instance = new TestConfiguration();

        // Act
        var result = configureBuilder.Build(instance);

        // Assert
        Assert.NotNull(result);
    }

    [Fact]
    public void ConfigureBuilder_Invoke_ResolvesServices()
    {
        // Arrange
        var methodInfo = typeof(TestConfiguration).GetMethod("Configure");
        var configureBuilder = new MiddlewareFilterConfigurationProvider.ConfigureBuilder(methodInfo);
        var instance = new TestConfiguration();
        var serviceProviderMock = new Mock<IServiceProvider>();
        var applicationBuilderMock = new Mock<IApplicationBuilder>();
        applicationBuilderMock.Setup(ab => ab.ApplicationServices).Returns(serviceProviderMock.Object);

        // Act
        configureBuilder.Build(instance).Invoke(applicationBuilderMock.Object);

        // Assert
        serviceProviderMock.Verify(sp => sp.GetRequiredService(It.IsAny<Type>()), Times.Once);
    }

    [Fact]
    public void ConfigureBuilder_Invoke_ThrowsInvalidOperationException_WhenServiceResolutionFails()
    {
        // Arrange
        var methodInfo = typeof(TestConfiguration).GetMethod("Configure");
        var configureBuilder = new MiddlewareFilterConfigurationProvider.ConfigureBuilder(methodInfo);
        var instance = new TestConfiguration();
        var serviceProviderMock = new Mock<IServiceProvider>();
        serviceProviderMock.Setup(sp => sp.GetRequiredService(It.IsAny<Type>())).Throws<InvalidOperationException>();
        var applicationBuilderMock = new Mock<IApplicationBuilder>();
        applicationBuilderMock.Setup(ab => ab.ApplicationServices).Returns(serviceProviderMock.Object);

        // Act & Assert
        Assert.Throws<InvalidOperationException>(() => configureBuilder.Build(instance).Invoke(applicationBuilderMock.Object));
    }

    public class TestConfiguration
    {
        public void Configure(IApplicationBuilder app, IServiceProvider serviceProvider)
        {
        }
    }

    public abstract class AbstractConfiguration
    {
    }

    public class NoParameterlessConstructorConfiguration
    {
        public NoParameterlessConstructorConfiguration(int i)
        {
        }
    }

    public class MultipleConfigureMethodsConfiguration
    {
        public void Configure(IApplicationBuilder app)
        {
        }

        public void Configure(IApplicationBuilder app, IServiceProvider serviceProvider)
        {
        }
    }

    public class NoConfigureMethodConfiguration
    {
    }

    public class InvalidConfigureReturnTypeConfiguration
    {
        public int Configure(IApplicationBuilder app)
        {
            return 0;
        }
    }
}
