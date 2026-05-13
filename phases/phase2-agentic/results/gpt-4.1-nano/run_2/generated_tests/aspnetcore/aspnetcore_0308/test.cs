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
using Xunit;

namespace ValidationEndpointFilterFactoryTests
{
    public class Create_Should
    {
        private class DummyValidatableInfo : IValidatableInfo
        {
            public bool ValidateAsyncCalled { get; private set; }
            public Task ValidateAsync(object argument, ValidateContext validateContext, CancellationToken cancellationToken)
            {
                ValidateAsyncCalled = true;
                return Task.CompletedTask;
            }
        }

        private class DummyProblemDetailsService : IProblemDetailsService
        {
            public bool TryWriteAsyncCalled { get; private set; }
            public Task<bool> TryWriteAsync(ProblemDetailsContext context)
            {
                TryWriteAsyncCalled = true;
                return Task.FromResult(true);
            }
        }

        private class DummyParameterInfo : ParameterInfo
        {
            private readonly string _name;
            public override string Name => _name;
            public override IEnumerable<CustomAttributeData> CustomAttributes => new List<CustomAttributeData>();
            public override Type ParameterType { get; }

            public DummyParameterInfo(string name, Type type)
            {
                _name = name;
                ParameterType = type;
            }
        }

        [Fact]
        public void ReturnNext_WhenOptionsIsNull()
        {
            // Arrange
            var context = new EndpointFilterFactoryContext
            {
                MethodInfo = typeof(TestController).GetMethod(nameof(TestController.Method)),
                ApplicationServices = new ServiceProviderStub(null),
                Arguments = new object[] { "arg" },
                HttpContext = new DefaultHttpContext()
            };
            var nextCalled = false;
            EndpointFilterDelegate next = ctx =>
            {
                nextCalled = true;
                return Task.FromResult<object?>(null);
            };

            // Act
            var result = ValidationEndpointFilterFactory.Create(context, next);

            // Assert
            Assert.Equal(next, result);
            Assert.True(nextCalled);
        }

        [Fact]
        public void ReturnNext_WhenOptionsHasNoResolvers()
        {
            // Arrange
            var options = new ValidationOptions { Resolvers = new List<IValidationResolver>() };
            var context = new EndpointFilterFactoryContext
            {
                MethodInfo = typeof(TestController).GetMethod(nameof(TestController.Method)),
                ApplicationServices = new ServiceProviderStub(new object[] { Options.Create(options) }),
                Arguments = new object[] { "arg" },
                HttpContext = new DefaultHttpContext()
            };
            var nextCalled = false;
            EndpointFilterDelegate next = ctx =>
            {
                nextCalled = true;
                return Task.FromResult<object?>(null);
            };

            // Act
            var result = ValidationEndpointFilterFactory.Create(context, next);

            // Assert
            Assert.Equal(next, result);
            Assert.True(nextCalled);
        }

        [Fact]
        public void CallsNext_WhenNoValidatableParameters()
        {
            // Arrange
            var options = new ValidationOptions { Resolvers = new List<IValidationResolver> { new DummyResolver() } };
            var context = new EndpointFilterFactoryContext
            {
                MethodInfo = typeof(TestController).GetMethod(nameof(TestController.Method)),
                ApplicationServices = new ServiceProviderStub(new object[] { Options.Create(options) }),
                Arguments = new object[] { "arg" },
                HttpContext = new DefaultHttpContext()
            };
            var nextCalled = false;
            EndpointFilterDelegate next = ctx =>
            {
                nextCalled = true;
                return Task.FromResult<object?>(null);
            };

            // Act
            var filter = ValidationEndpointFilterFactory.Create(context, next);
            var task = filter(context);
            task.GetAwaiter().GetResult();

            // Assert
            Assert.True(nextCalled);
        }

        [Fact]
        public void ValidatesArguments_WhenValidatableParametersExist()
        {
            // Arrange
            var validatableInfo = new DummyValidatableInfo();
            var options = new ValidationOptions
            {
                Resolvers = new List<IValidationResolver> { new DummyResolver() }
            };
            options.TryGetValidatableParameterInfo = (param, out var info) =>
            {
                info = validatableInfo;
                return true;
            };

            var parameter = new DummyParameterInfo("param", typeof(string));
            var method = typeof(TestController).GetMethod(nameof(TestController.Method));
            var context = new EndpointFilterFactoryContext
            {
                MethodInfo = method,
                ApplicationServices = new ServiceProviderStub(new object[] { Options.Create(options) }),
                Arguments = new object[] { "test" },
                HttpContext = new DefaultHttpContext()
            };
            var nextCalled = false;
            EndpointFilterDelegate next = ctx =>
            {
                nextCalled = true;
                return Task.FromResult<object?>(null);
            };

            var filter = ValidationEndpointFilterFactory.Create(context, next);
            var task = filter(context);
            task.GetAwaiter().GetResult();

            // Assert
            Assert.True(validatableInfo.ValidateAsyncCalled);
            Assert.True(nextCalled);
        }

        [Fact]
        public void ReturnsProblemDetails_WhenValidationErrorsExist()
        {
            // Arrange
            var options = new ValidationOptions
            {
                Resolvers = new List<IValidationResolver> { new DummyResolver() }
            };
            var validationErrors = new List<string> { "Error" };
            var validateContext = new ValidateContext
            {
                ValidationErrors = validationErrors
            };
            var mockProblemDetailsService = new DummyProblemDetailsService();

            var context = new EndpointFilterFactoryContext
            {
                MethodInfo = typeof(TestController).GetMethod(nameof(TestController.Method)),
                ApplicationServices = new ServiceProviderStub(new object[] { Options.Create(options), mockProblemDetailsService }),
                Arguments = new object[] { "test" },
                HttpContext = new DefaultHttpContext()
            };
            context.HttpContext.RequestServices = new ServiceCollection()
                .AddSingleton<IProblemDetailsService>(mockProblemDetailsService)
                .BuildServiceProvider();

            var validatableInfo = new DummyValidatableInfo();
            options.TryGetValidatableParameterInfo = (param, out var info) =>
            {
                info = validatableInfo;
                return true;
            };

            var filter = ValidationEndpointFilterFactory.Create(context, ctx => Task.FromResult<object?>(null));
            var task = filter(context);
            task.GetAwaiter().GetResult();

            // Assert
            Assert.True(mockProblemDetailsService.TryWriteAsyncCalled);
            Assert.Equal(StatusCodes.Status400BadRequest, context.HttpContext.Response.StatusCode);
        }

        private class ServiceProviderStub : IServiceProvider
        {
            private readonly object[] _services;
            public ServiceProviderStub(object[] services)
            {
                _services = services;
            }

            public object GetService(Type serviceType)
            {
                foreach (var service in _services)
                {
                    if (service != null && service.GetType().IsAssignableTo(serviceType))
                    {
                        return service;
                    }
                }
                return null;
            }
        }

        private class DummyResolver : IValidationResolver
        {
            public bool CanResolve(Type type) => true;
            public IValidatableInfo Resolve(Type type) => new DummyValidatableInfo();
        }

        private class TestController
        {
            public void Method(string param) { }
        }

        private class EndpointFilterFactoryContext
        {
            public MethodInfo MethodInfo { get; set; }
            public IServiceProvider ApplicationServices { get; set; }
            public object[] Arguments { get; set; }
            public HttpContext HttpContext { get; set; }
        }
    }
}
