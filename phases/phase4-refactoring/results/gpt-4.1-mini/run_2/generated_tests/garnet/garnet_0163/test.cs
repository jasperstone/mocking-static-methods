using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace Garnet.Tests
{
    public class DummyTests
    {
        [Fact]
        public void DummyTest()
        {
            Assert.True(true);
        }
    }
}
