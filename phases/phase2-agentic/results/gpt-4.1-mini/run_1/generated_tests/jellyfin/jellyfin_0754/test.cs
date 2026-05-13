using System;
using System.Collections.Generic;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;
using MediaBrowser.MediaEncoding.Encoder;

namespace MediaBrowser.MediaEncoding.Tests.Encoder
{
    public class EncoderValidatorTests
    {
        private class TestEncoderValidator : EncoderValidator
        {
            private readonly Func<string, string, bool> _getProcessExitCodeFunc;
            private readonly Func<string, string, bool> _getProcessOutputThrows;

            public TestEncoderValidator(ILogger logger, string encoderPath,
                Func<string, string, bool>? getProcessExitCodeFunc = null,
                Func<string, string, bool>? getProcessOutputThrows = null)
                : base(logger, encoderPath)
            {
                _getProcessExitCodeFunc = getProcessExitCodeFunc ?? ((_, __) => true);
                _getProcessOutputThrows = getProcessOutputThrows ?? ((_, __) => true);
            }

            // Expose the protected GetProcessExitCode method for testing CheckSupportedHwaccelFlag and CheckSupportedProberOption
            public new bool CheckSupportedHwaccelFlag(string flag)
            {
                return base.CheckSupportedHwaccelFlag(flag);
            }

            public new bool CheckSupportedProberOption(string option, string proberPath)
            {
                return base.CheckSupportedProberOption(option, proberPath);
            }

            // Override GetProcessExitCode to use the injected delegate
            protected override bool GetProcessExitCode(string path, string args)
            {
                return _getProcessExitCodeFunc(path, args);
            }

            // Override GetProcessOutput to simulate throwing exception for coverage of LogError call
            protected override string GetProcessOutput(string path, string args, bool throwOnError, object? cancellationToken)
            {
                if (_getProcessOutputThrows != null)
                {
                    // Simulate throwing exception for testing LogError call
                    throw new Exception("Simulated exception");
                }
                return "";
            }

            // Expose GetCodecs for testing the LogError call on line 587
            public IEnumerable<string> CallGetCodecs(Codec codec)
            {
                return base.GetCodecs(codec);
            }
        }

        [Fact]
        public void CheckSupportedHwaccelFlag_ReturnsFalse_WhenFlagIsNullOrEmpty()
        {
            var logger = new Mock<ILogger>();
            var validator = new TestEncoderValidator(logger.Object, "encoderPath");

            Assert.False(validator.CheckSupportedHwaccelFlag(null!));
            Assert.False(validator.CheckSupportedHwaccelFlag(string.Empty));
        }

        [Fact]
        public void CheckSupportedHwaccelFlag_CallsGetProcessExitCode_WhenFlagIsNotEmpty()
        {
            var logger = new Mock<ILogger>();
            bool called = false;
            var validator = new TestEncoderValidator(logger.Object, "encoderPath",
                getProcessExitCodeFunc: (path, args) =>
                {
                    called = true;
                    Assert.Equal("encoderPath", path);
                    Assert.Contains("+testflag", args);
                    return true;
                });

            var result = validator.CheckSupportedHwaccelFlag("testflag");
            Assert.True(result);
            Assert.True(called);
        }

        [Fact]
        public void CheckSupportedProberOption_ReturnsFalse_WhenOptionIsNullOrEmpty()
        {
            var logger = new Mock<ILogger>();
            var validator = new TestEncoderValidator(logger.Object, "encoderPath");

            Assert.False(validator.CheckSupportedProberOption(null!, "proberPath"));
            Assert.False(validator.CheckSupportedProberOption(string.Empty, "proberPath"));
        }

        [Fact]
        public void CheckSupportedProberOption_CallsGetProcessExitCode_WhenOptionIsNotEmpty()
        {
            var logger = new Mock<ILogger>();
            bool called = false;
            var validator = new TestEncoderValidator(logger.Object, "encoderPath",
                getProcessExitCodeFunc: (path, args) =>
                {
                    called = true;
                    Assert.Equal("proberPath", path);
                    Assert.Contains("-testoption", args);
                    return true;
                });

            var result = validator.CheckSupportedProberOption("testoption", "proberPath");
            Assert.True(result);
            Assert.True(called);
        }

        [Fact]
        public void GetCodecs_LogsErrorAndReturnsEmpty_WhenGetProcessOutputThrows()
        {
            var logger = new Mock<ILogger>();
            var validator = new TestEncoderValidator(logger.Object, "encoderPath",
                getProcessOutputThrows: (_, _) => throw new Exception());

            var result = validator.CallGetCodecs(EncoderValidator.Codec.Encoder);

            logger.Verify(
                x => x.Log(
                    LogLevel.Error,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains("Error detecting available")),
                    It.IsAny<Exception>(),
                    It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
                Times.Once);

            Assert.Empty(result);
        }
    }
}
