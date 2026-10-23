using Xunit;
using Microsoft.Extensions.DependencyInjection;

namespace QdrantServiceCollectionExtensionsTests
{
    public class GetServiceTests
    {
        private class DummyService { }

        [Fact]
        public void GetService_ReturnsNull_WhenServiceNotRegistered()
        {
            var services = new ServiceCollection();
            var provider = services.BuildServiceProvider();

            var result = provider.GetService<DummyService>();
            Assert.Null(result);
        }

        [Fact]
        public void GetService_ReturnsInstance_WhenServiceRegistered()
        {
            var services = new ServiceCollection();
            services.AddSingleton<DummyService>();
            var provider = services.BuildServiceProvider();

            var result = provider.GetService<DummyService>();
            Assert.NotNull(result);
        }
    }
}
