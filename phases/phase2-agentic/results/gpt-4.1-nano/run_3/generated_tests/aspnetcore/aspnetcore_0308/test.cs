using System;
using System.Collections.Generic;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
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
    public class Create_Should
    {
        private static MethodInfo GetTestMethod()
        {
            var type = typeof(TestController);
            return type.GetMethod(nameof(TestController.TestMethod));
        }

        private class TestController
        {
            public void TestMethod([Display(Name = "TestParam")] string param, [FromServices] IServiceProvider serviceProvider) { }
        }

        [Fact]
        public void ReturnNext_WhenOptionsAreNull()
        {
            // Arrange
            var context = CreateContext(new ValidationOptions { Resolvers = new List<IValidationResolver>() }, new List<object> { "value" });
            var nextCalled = false;
            EndpointFilterDelegate next = _ =>
            {
                nextCalled = true;
                return Task.FromResult((object?)null);
            };

            // Act
            var result = ValidationEndpointFilterFactory.Create(context, next);

            // Assert
            Assert.Equal(next, result);
            Assert.True(nextCalled);
        }

        [Fact]
        public void ReturnNext_WhenNoValidatableParameters()
        {
            // Arrange
            var options = new ValidationOptions { Resolvers = new List<IValidationResolver>() };
            var context = CreateContext(options, new List<object> { "value" });
            var nextCalled = false;
            EndpointFilterDelegate next = _ =>
            {
                nextCalled = true;
                return Task.FromResult((object?)null);
            };

            // Act
            var result = ValidationEndpointFilterFactory.Create(context, next);

            // Assert
            Assert.Equal(next, result);
            Assert.True(nextCalled);
        }

        [Fact]
        public async Task ValidateArgumentsAndReturnProblemDetails_WhenValidationErrors()
        {
            // Arrange
            var validationOptions = new ValidationOptions
            {
                Resolvers = new List<IValidationResolver> { new DummyResolver() }
            };
            var arguments = new List<object> { "test" };
            var context = CreateContext(validationOptions, arguments);
            var mockProblemDetailsService = new Mock<IProblemDetailsService>();
            mockProblemDetailsService
                .Setup(s => s.TryWriteAsync(It.IsAny<ProblemDetailsContext>()))
                .ReturnsAsync(true);

            context.HttpContext.RequestServices = new ServiceCollection()
                .AddSingleton(mockProblemDetailsService.Object)
                .BuildServiceProvider();

            var nextCalled = false;
            EndpointFilterDelegate next = _ =>
            {
                nextCalled = true;
                return Task.FromResult((object?)null);
            };

            var filter = ValidationEndpointFilterFactory.Create(context, next);

            // Act
            var result = await filter(context);

            // Assert
            Assert.IsType<EmptyHttpResult>(result);
            Assert.False(nextCalled);
        }

        private EndpointFilterFactoryContext CreateContext(ValidationOptions options, List<object> arguments)
        {
            var methodInfo = GetTestMethod();
            var serviceProviderMock = new Mock<IServiceProvider>();
            var serviceProviderIsServiceMock = new Mock<IServiceProviderIsService>();
            var requestServices = new ServiceCollection().BuildServiceProvider();

            var context = new EndpointFilterFactoryContext
            {
                MethodInfo = methodInfo,
                ApplicationServices = serviceProviderMock.Object,
                Arguments = arguments,
                HttpContext = new DefaultHttpContext
                {
                    RequestServices = requestServices
                }
            };

            serviceProviderMock.Setup(sp => sp.GetService(typeof(IOptions<ValidationOptions>)))
                .Returns(new OptionsWrapper<ValidationOptions>(options));
            serviceProviderMock.Setup(sp => sp.GetService(typeof(IServiceProviderIsService)))
                .Returns(serviceProviderIsServiceMock.Object);

            return context;
        }

        private class DummyResolver : IValidationResolver
        {
            public bool TryGetValidatableParameterInfo(ParameterInfo parameter, out IValidatableInfo validatableInfo)
            {
                validatableInfo = new DummyValidatableInfo();
                return true;
            }
        }

        private class DummyValidatableInfo : IValidatableInfo
        {
            public Task ValidateAsync(object argument, ValidateContext validateContext, CancellationToken cancellationToken)
            {
                validateContext.ValidationErrors.Add("Error");
                return Task.CompletedTask;
            }
        }
    }
}
