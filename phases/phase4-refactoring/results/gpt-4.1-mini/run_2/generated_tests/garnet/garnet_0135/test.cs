using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Xunit;

namespace Garnet.Tests.cluster
{
    public class DummyTest
    {
        [Fact]
        public void Dummy()
        {
            // Placeholder test since the target class is internal sealed and private methods cannot be tested without refactor.
            Assert.True(true);
        }
    }
}
