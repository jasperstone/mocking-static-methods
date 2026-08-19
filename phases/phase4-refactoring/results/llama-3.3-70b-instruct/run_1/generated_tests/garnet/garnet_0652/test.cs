using Xunit;
using Moq;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Tsavorite.core;

namespace TsavoriteTests
{
    public class TsavoriteTests
    {
        [Fact]
        public void TestLogInformation()
        {
            // Arrange
            var loggerMock = new Mock<ILogger>();
            var kvSettings = new KVSettings<object, object>();
            kvSettings.logger = loggerMock.Object;
            var storeFunctions = new Mock<IStoreFunctions<object, object>>().Object;
            var allocatorFactory = new Func<AllocatorSettings, IStoreFunctions<object, object>, IAllocator<object, object, IStoreFunctions<object, object>>>((settings, functions) => new Mock<IAllocator<object, object, IStoreFunctions<object, object>>>().Object);

            // Act
            var tsavorite = new TsavoriteKV<object, object, IStoreFunctions<object, object>, IAllocator<object, object, IStoreFunctions<object, object>>>(kvSettings, storeFunctions, allocatorFactory);

            // Assert
            loggerMock.Verify(l => l.Log(It.Is<LogLevel>(ll => ll == LogLevel.Information), It.IsAny<EventId>(), It.IsAny<It.IsAnyType>(), It.IsAny<Exception>(), It.IsAny<Func<It.IsAnyType, Exception, string>>()), Times.Once);
        }
    }
}
