using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ViewEngines;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Xunit;

namespace DefaultDisplayTemplatesTests
{
    public class DefaultDisplayTemplatesTests
    {
        [Fact]
        public void ObjectTemplate_GetRequiredService_ICompositeViewEngine()
        {
            // Arrange
            var serviceProvider = new ServiceCollection()
                .AddScoped<ICompositeViewEngine, CompositeViewEngine>()
                .BuildServiceProvider();

            var viewContext = new ViewContext();
            viewContext.HttpContext = new DefaultHttpContext();
            viewContext.HttpContext.RequestServices = serviceProvider;

            var htmlHelper = new HtmlHelper(viewContext);

            // Act
            var result = DefaultDisplayTemplates.ObjectTemplate(htmlHelper);

            // Assert
            Assert.NotNull(result);
        }

        [Fact]
        public void ObjectTemplate_GetRequiredService_IViewBufferScope()
        {
            // Arrange
            var serviceProvider = new ServiceCollection()
                .AddScoped<IViewBufferScope, ViewBufferScope>()
                .BuildServiceProvider();

            var viewContext = new ViewContext();
            viewContext.HttpContext = new DefaultHttpContext();
            viewContext.HttpContext.RequestServices = serviceProvider;

            var htmlHelper = new HtmlHelper(viewContext);

            // Act
            var result = DefaultDisplayTemplates.ObjectTemplate(htmlHelper);

            // Assert
            Assert.NotNull(result);
        }
    }
}
