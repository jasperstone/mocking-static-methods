using System;
using System.Collections.Generic;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Microsoft.Extensions.Validation;
using Xunit;

namespace ValidationEndpointFilterFactoryTests
{
    public class CreateMethodTests
    {
        private class DummyParameterInfo : ParameterInfo
        {
            public override object[] GetCustomAttributes(Type attributeType, bool inherit) => new object[0];
            public override bool IsDefined(Type attributeType, bool inherit) => false;
            public override string Name => "param";
            public override ParameterInfo[] GetParameters() => new ParameterInfo[0];
        }

        private class DummyValidatableInfo : IValidatableInfo
        {
            public Func<object, ValidateContext, CancellationToken, ValueTask> ValidateAsyncDelegate { get; set; }

            public ValueTask ValidateAsync(object argument, ValidateContext validateContext, CancellationToken cancellationToken)
            {
                return ValidateAsyncDelegate(argument, validateContext, cancellationToken);
            }
        }

        private class DummyController
        {
            public void Method(string param) { }
        }

        private class DummyServiceProviderIsService : IServiceProviderIsService
        {
            private readonly bool _isService;

            public DummyServiceProviderIsService(bool isService)
            {
                _isService = isService;
            }

            public bool IsService(Type serviceType)
            {
                return _isService;
            }
        }

        [Fact]
        public void Create_ReturnsNext_WhenOptionsIsNull()
        {
            // Arrange
            var context = new EndpointFilterFactoryContext
            {
                MethodInfo = typeof(DummyController).GetMethod(nameof(DummyController.Method)),
                ApplicationServices = new ServiceCollection().BuildServiceProvider(),
                Arguments = new object[] { "test" },
                HttpContext = new DefaultHttpContext()
            };
            var nextCalled = false;
            EndpointFilterDelegate next = ctx =>
            {
                nextCalled = true;
                return ValueTask.CompletedTask;
            };

            // Act
            var result = ValidationEndpointFilterFactory.Create(context, next);

            // Assert
            Assert.Equal(next, result);
            Assert.True(nextCalled);
        }

        [Fact]
        public void Create_ReturnsNext_WhenOptionsHasNoResolvers()
        {
            // Arrange
            var options = new ValidationOptions { Resolvers = new List<object>() };
            var services = new ServiceCollection()
                .AddSingleton<IOptions<ValidationOptions>>(Options.Create(options))
                .BuildServiceProvider();

            var context = new EndpointFilterFactoryContext
            {
                MethodInfo = typeof(DummyController).GetMethod(nameof(DummyController.Method)),
                ApplicationServices = services,
                Arguments = new object[] { "test" },
                HttpContext = new DefaultHttpContext()
            };
            var nextCalled = false;
            EndpointFilterDelegate next = ctx =>
            {
                nextCalled = true;
                return ValueTask.CompletedTask;
            };

            // Act
            var result = ValidationEndpointFilterFactory.Create(context, next);

            // Assert
            Assert.Equal(next, result);
            Assert.True(nextCalled);
        }

        [Fact]
        public void Create_CallsGetService_IServiceProviderIsService()
        {
            // Arrange
            var services = new ServiceCollection()
                .AddSingleton<IOptions<ValidationOptions>>(Options.Create(new ValidationOptions
                {
                    Resolvers = new List<object> { new object() }
                }))
                .AddSingleton<IServiceProviderIsService>(new DummyServiceProviderIsService(true))
                .BuildServiceProvider();

            var context = new EndpointFilterFactoryContext
            {
                MethodInfo = typeof(DummyController).GetMethod(nameof(DummyController.Method)),
                ApplicationServices = services,
                Arguments = new object[] { "test" },
                HttpContext = new DefaultHttpContext()
            };
            var nextCalled = false;
            EndpointFilterDelegate next = ctx =>
            {
                nextCalled = true;
                return ValueTask.CompletedTask;
            };

            // Act
            var result = ValidationEndpointFilterFactory.Create(context, next);

            // Assert
            Assert.NotEqual(next, result);
        }
    }
}
