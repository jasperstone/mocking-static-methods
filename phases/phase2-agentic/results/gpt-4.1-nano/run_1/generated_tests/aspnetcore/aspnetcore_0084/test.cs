using System;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Xunit;
using Moq;

namespace DebugProxyLauncherTests
{
    public class DebugProxyLauncherTests
    {
        [Fact]
        public async Task EnsureLaunchedAndGetUrl_CallsGetRequiredServiceAndReturnsUrl()
        {
            // Arrange
            var serviceProviderMock = new Mock<IServiceProvider>();
            var webHostEnvMock = new Mock<IWebHostEnvironment>();
            var expectedUrl = "http://localhost:6000";

            // Setup IWebHostEnvironment
            webHostEnvMock.Setup(e => e.ApplicationName).Returns("TestApp");
            serviceProviderMock.Setup(sp => sp.GetRequiredService<IWebHostEnvironment>())
                .Returns(webHostEnvMock.Object);

            // Mock LocateDebugProxyExecutable to return a dummy path
            DebugProxyLauncher.GetType()
                .GetMethod("LocateDebugProxyExecutable", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Static)
                .CreateDelegate<Func<IWebHostEnvironment, string>>()
                .Invoke = (env) => "dummyPath";

            // Mock Process.Start to return a dummy process
            var processMock = new Mock<Process>();
            processMock.Setup(p => p.Start()).Returns(true);
            processMock.Setup(p => p.StandardOutput).Returns(new DummyStream());
            processMock.Setup(p => p.StandardError).Returns(new DummyStream());

            // Act
            var url = await DebugProxyLauncher.EnsureLaunchedAndGetUrl(serviceProviderMock.Object, "http://devtools", false);

            // Assert
            Assert.NotNull(url);
            Assert.IsType<string>(url);
        }
    }

    // Dummy stream for process output
    public class DummyStream : System.IO.Stream
    {
        public override bool CanRead => true;
        public override bool CanSeek => false;
        public override bool CanWrite => false;
        public override long Length => 0;
        public override long Position { get; set; }
        public override void Flush() { }
        public override int Read(byte[] buffer, int offset, int count) => 0;
        public override long Seek(long offset, System.IO.SeekOrigin origin) => 0;
        public override void SetLength(long value) { }
        public override void Write(byte[] buffer, int offset, int count) { }
    }
}
