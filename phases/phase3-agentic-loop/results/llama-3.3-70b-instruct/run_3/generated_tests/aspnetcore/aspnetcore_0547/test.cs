using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Infrastructure;
using Microsoft.AspNetCore.Mvc.TagHelpers;
using Microsoft.AspNetCore.Routing;
using Moq;
using System;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using Xunit;

namespace Microsoft.AspNetCore.Mvc.TagHelpers.Tests
{
    public class ScriptTagHelperTests
    {
        [Fact]
        public async Task EnsureFileVersionProvider_CallsGetRequiredService()
        {
            // Arrange
            var serviceProvider = new Mock<IServiceProvider>();
            var fileVersionProvider = new Mock<IFileVersionProvider>();
            serviceProvider.Setup(p => p.GetRequiredService<IFileVersionProvider>()).Returns(fileVersionProvider.Object);

            var viewContext = new ViewContext();
            viewContext.HttpContext = new DefaultHttpContext();
            viewContext.HttpContext.RequestServices = serviceProvider.Object;

            var scriptTagHelper = new ScriptTagHelper(
                Mock.Of<IWebHostEnvironment>(),
                Mock.Of<TagHelperMemoryCacheProvider>(),
                null,
                Mock.Of<HtmlEncoder>(),
                Mock.Of<JavaScriptEncoder>(),
                Mock.Of<IUrlHelperFactory>());

            scriptTagHelper.ViewContext = viewContext;

            // Act
            scriptTagHelper.EnsureFileVersionProvider();

            // Assert
            serviceProvider.Verify(p => p.GetRequiredService<IFileVersionProvider>(), Times.Once);
        }
    }
}
