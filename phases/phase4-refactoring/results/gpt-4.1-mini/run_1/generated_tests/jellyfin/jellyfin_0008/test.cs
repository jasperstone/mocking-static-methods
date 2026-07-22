using System;
using System.Collections.Generic;
using System.Reflection;
using Emby.Server.Implementations;
using Microsoft.Extensions.Logging;
using Xunit;

namespace Emby.Server.Implementations.Tests
{
    public class ApplicationHostTests
    {
        private class TestApplicationHost : ApplicationHost
        {
            public TestApplicationHost()
                : base(
                    applicationPaths: null,
                    loggerFactory: null,
                    options: null,
                    startupConfig: null)
            {
            }

            protected override IEnumerable<Assembly> GetAssembliesWithPartsInternal()
            {
                return Array.Empty<Assembly>();
            }

            public new object CreateInstanceSafe(Type type) => base.CreateInstanceSafe(type);
        }

        [Fact]
        public void CreateInstanceSafe_DiLoop_ThrowsTypeLoadException()
        {
            var host = new TestApplicationHost();

            var testType = typeof(string);

            // Add the type to _creatingInstances to simulate DI loop
            var creatingInstancesField = typeof(ApplicationHost).GetField("_creatingInstances", BindingFlags.NonPublic | BindingFlags.Instance);
            creatingInstancesField.SetValue(host, new List<Type> { testType });

            var ex = Assert.Throws<TypeLoadException>(() => host.CreateInstanceSafe(testType));

            Assert.Equal("DI Loop detected", ex.Message);
        }

        [Fact]
        public void CreateInstanceSafe_ExceptionDuringCreation_ReturnsNull()
        {
            var host = new TestApplicationHost();

            var testType = typeof(TypeThatThrowsOnCreate);

            // _creatingInstances initially empty
            var creatingInstancesField = typeof(ApplicationHost).GetField("_creatingInstances", BindingFlags.NonPublic | BindingFlags.Instance);
            creatingInstancesField.SetValue(host, new List<Type>());

            var result = host.CreateInstanceSafe(testType);

            Assert.Null(result);
        }

        private class TypeThatThrowsOnCreate
        {
            public TypeThatThrowsOnCreate()
            {
                throw new InvalidOperationException("Constructor failure");
            }
        }
    }
}
