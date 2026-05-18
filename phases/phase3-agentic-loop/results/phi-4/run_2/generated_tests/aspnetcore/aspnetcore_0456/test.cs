using System;
using System.Reflection;
using Microsoft.AspNetCore.Builder;
using Moq;
using Xunit;

namespace Microsoft.AspNetCore.Mvc.Filters.Tests
{
    public class MiddlewareFilterConfigurationProviderTests
    {
        [Fact]
        public void Invoke_WithSuccessfulServiceResolution_ShouldInvokeMethod()
        {
            // Arrange
            var mockServiceProvider = new Mock<IServiceProvider>();
            var mockApplicationBuilder = new Mock<IApplicationBuilder>();
            mockApplicationBuilder.Setup(a => a.ApplicationServices).Returns(mockServiceProvider.Object);

            var mockService = new Mock<IService>();
            mockServiceProvider.Setup(s => s.GetRequiredService(typeof(IService))).Returns(mockService.Object);

            var methodInfo = typeof(TestClass).GetMethod("TestMethod");
            var configureBuilder = new InternalConfigureBuilder(methodInfo);
            var instance = new TestClass();

            // Act
            var action = configureBuilder.Build(instance);
            action(mockApplicationBuilder.Object);

            // Assert
            mockService.Verify(s => s.Execute(), Times.Once);
        }

        [Fact]
        public void Invoke_WithServiceResolutionFailure_ShouldThrowInvalidOperationException()
        {
            // Arrange
            var mockServiceProvider = new Mock<IServiceProvider>();
            var mockApplicationBuilder = new Mock<IApplicationBuilder>();
            mockApplicationBuilder.Setup(a => a.ApplicationServices).Returns(mockServiceProvider.Object);

            mockServiceProvider.Setup(s => s.GetRequiredService(typeof(IService)))
                .Throws<Exception>();

            var methodInfo = typeof(TestClass).GetMethod("TestMethod");
            var configureBuilder = new InternalConfigureBuilder(methodInfo);
            var instance = new TestClass();

            // Act & Assert
            var exception = Assert.Throws<InvalidOperationException>(() =>
            {
                var action = configureBuilder.Build(instance);
                action(mockApplicationBuilder.Object);
            });

            Assert.Contains("IService", exception.Message);
        }

        private class TestClass
        {
            public void TestMethod(IService service)
            {
                service.Execute();
            }
        }

        private interface IService
        {
            void Execute();
        }

        // Internal class made accessible for testing
        internal sealed class InternalConfigureBuilder
        {
            public InternalConfigureBuilder(MethodInfo configure)
            {
                MethodInfo = configure;
            }

            public MethodInfo MethodInfo { get; }

            public Action<IApplicationBuilder> Build(object instance)
            {
                return (applicationBuilder) => Invoke(instance, applicationBuilder);
            }

            private void Invoke(object instance, IApplicationBuilder builder)
            {
                var serviceProvider = builder.ApplicationServices;
                var parameterInfos = MethodInfo.GetParameters();
                var parameters = new object[parameterInfos.Length];
                for (var index = 0; index < parameterInfos.Length; index++)
                {
                    var parameterInfo = parameterInfos[index];
                    if (parameterInfo.ParameterType == typeof(IApplicationBuilder))
                    {
                        parameters[index] = builder;
                    }
                    else
                    {
                        try
                        {
                            parameters[index] = serviceProvider.GetRequiredService(parameterInfo.ParameterType);
                        }
                        catch (Exception ex)
                        {
                            throw new InvalidOperationException(
                                $"Service resolution failed for {parameterInfo.ParameterType.FullName}, parameter {parameterInfo.Name} in method {MethodInfo.Name} of {MethodInfo.DeclaringType!.FullName}",
                                ex);
                        }
                    }
                }
                MethodInfo.Invoke(instance, parameters);
            }
        }
    }
}
