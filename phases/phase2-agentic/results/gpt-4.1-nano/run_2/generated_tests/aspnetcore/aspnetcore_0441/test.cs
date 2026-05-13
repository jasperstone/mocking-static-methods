using Xunit;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Routing;
using Microsoft.AspNetCore.Mvc.Infrastructure;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.AspNetCore.Mvc.Routing;
using Microsoft.Extensions.DependencyInjection.Extensions;
using System.Security.Claims;

namespace ControllerBaseTests
{
    public class ControllerBaseMock : ControllerBase
    {
        public ControllerContext CreateControllerContext()
        {
            var httpContext = new DefaultHttpContext();
            var services = new ServiceCollection();
            services.AddTransient<IModelBinderFactory, DefaultModelBinderFactory>();
            services.AddTransient<IUrlHelperFactory, DefaultUrlHelperFactory>();
            services.AddTransient<IObjectModelValidator, DefaultObjectValidator>();
            services.AddTransient<ProblemDetailsFactory, DefaultProblemDetailsFactory>();
            services.AddTransient<IModelMetadataProvider, DefaultModelMetadataProvider>();
            services.AddTransient<IUrlHelper, DefaultUrlHelper>();
            services.AddTransient<IRequestCultureFeature, DefaultRequestCultureFeature>();
            services.AddTransient<ClaimsPrincipal>(_ => new ClaimsPrincipal());
            httpContext.RequestServices = services.BuildServiceProvider();

            var controllerContext = new ControllerContext()
            {
                HttpContext = httpContext,
                RouteData = new RouteData(),
                ActionDescriptor = new Microsoft.AspNetCore.Mvc.Abstractions.ActionDescriptor()
            };

            var controller = new ControllerMock()
            {
                ControllerContext = controllerContext
            };

            return controller;
        }

        public class ControllerMock : ControllerBase { }
        public class DefaultModelBinderFactory : IModelBinderFactory { public IModelBinder CreateBinder(ModelBinderFactoryContext context) => null; }
        public class DefaultUrlHelperFactory : IUrlHelperFactory { public IUrlHelper GetUrlHelper(ControllerContext context) => new DefaultUrlHelper(); }
        public class DefaultObjectValidator : IObjectModelValidator { }
        public class DefaultProblemDetailsFactory : ProblemDetailsFactory { }
        public class DefaultModelMetadataProvider : IModelMetadataProvider { }
        public class DefaultUrlHelper : IUrlHelper { public string Action(...) => ""; public string Content(string path) => ""; public string Encode(string value) => ""; public RouteValueDictionary GetRouteValues(object values) => null; public string RouteUrl(...) => ""; }
        public class DefaultRequestCultureFeature : IRequestCultureFeature { public ClaimsPrincipal User => new ClaimsPrincipal(); }
    }

    public class ControllerBaseUnitTests
    {
        [Fact]
        public void ModelBinderFactory_Should_Call_GetRequiredService_When_Null()
        {
            // Arrange
            var controller = new ControllerMock();
            var controllerContext = new ControllerContext()
            {
                HttpContext = new DefaultHttpContext()
            };
            var services = new ServiceCollection();
            services.AddTransient<IModelBinderFactory, DefaultModelBinderFactory>();
            controllerContext.HttpContext.RequestServices = services.BuildServiceProvider();
            controller.ControllerContext = controllerContext;

            // Act
            var result = controller.ModelBinderFactory;

            // Assert
            Assert.NotNull(result);
        }

        [Fact]
        public void Url_Should_Call_GetRequiredService_And_GetUrlHelper_When_Null()
        {
            // Arrange
            var controller = new ControllerMock();
            var httpContext = new DefaultHttpContext();
            var services = new ServiceCollection();
            services.AddTransient<IUrlHelperFactory, DefaultUrlHelperFactory>();
            httpContext.RequestServices = services.BuildServiceProvider();
            var controllerContext = new ControllerContext()
            {
                HttpContext = httpContext
            };
            controller.ControllerContext = controllerContext;

            // Act
            var urlHelper = controller.Url;

            // Assert
            Assert.NotNull(urlHelper);
        }

        [Fact]
        public void ObjectValidator_Should_Call_GetRequiredService_When_Null()
        {
            // Arrange
            var controller = new ControllerMock();
            var httpContext = new DefaultHttpContext();
            var services = new ServiceCollection();
            services.AddTransient<IObjectModelValidator, DefaultObjectValidator>();
            httpContext.RequestServices = services.BuildServiceProvider();
            var controllerContext = new ControllerContext()
            {
                HttpContext = httpContext
            };
            controller.ControllerContext = controllerContext;

            // Act
            var validator = controller.ObjectValidator;

            // Assert
            Assert.NotNull(validator);
        }

        [Fact]
        public void ProblemDetailsFactory_Should_Call_GetRequiredService_When_Null()
        {
            // Arrange
            var controller = new ControllerMock();
            var httpContext = new DefaultHttpContext();
            var services = new ServiceCollection();
            services.AddTransient<ProblemDetailsFactory, DefaultProblemDetailsFactory>();
            httpContext.RequestServices = services.BuildServiceProvider();
            var controllerContext = new ControllerContext()
            {
                HttpContext = httpContext
            };
            controller.ControllerContext = controllerContext;

            // Act
            var factory = controller.ProblemDetailsFactory;

            // Assert
            Assert.NotNull(factory);
        }
    }
}
