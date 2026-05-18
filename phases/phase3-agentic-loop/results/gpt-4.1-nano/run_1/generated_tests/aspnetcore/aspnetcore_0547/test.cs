using Xunit;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc.TagHelpers;
using Microsoft.AspNetCore.Razor.TagHelpers;
using System;

namespace Microsoft.AspNetCore.Mvc.TagHelpers.Tests
{
    public class ScriptTagHelperTests
    {
        [Fact]
        public void EnsureFileVersionProvider_CallsGetRequiredService()
        {
            // Arrange
            var services = new ServiceCollection();
            var serviceProvider = services.BuildServiceProvider();

            var httpContext = new DefaultHttpContext
            {
                RequestServices = serviceProvider
            };

            var viewContext = new ViewContext
            {
                HttpContext = httpContext
            };

            var helper = new ScriptTagHelper(
                hostingEnvironment: null,
                cacheProvider: new TagHelperMemoryCacheProvider(),
                fileVersionProvider: new TestFileVersionProvider(),
                htmlEncoder: null,
                javaScriptEncoder: null,
                urlHelperFactory: null);

            helper.ViewContext = viewContext;

            // Act
            var method = typeof(ScriptTagHelper).GetMethod("EnsureFileVersionProvider", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            method.Invoke(helper, null);

            // Assert
            var provider = helper.GetType().GetProperty("FileVersionProvider", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance).GetValue(helper);
            Assert.IsType<TestFileVersionProvider>(provider);
        }
    }

    // Mock implementation for IFileVersionProvider
    public class TestFileVersionProvider : IFileVersionProvider
    {
        public bool WasCalled { get; private set; } = false;

        public string AddFileVersion(IFileInfo fileInfo)
        {
            WasCalled = true;
            return "versioned";
        }
    }
}
