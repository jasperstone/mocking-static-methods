using System;
using System.Text;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace Garnet.server.Tests
{
    public class ServerConfigTests
    {
        private readonly Mock<ILogger> _loggerMock;
        private readonly Mock<StoreWrapper> _storeWrapperMock;

        public ServerConfigTests()
        {
            _loggerMock = new Mock<ILogger>();
            _storeWrapperMock = new Mock<StoreWrapper>();
        }

        [Fact]
        public void LogWarning_WhenClusterUsernameIsNotProvided()
        {
            // Arrange
            var parseState = new ParseState();
            parseState.AddArg("cluster-password", "password");

            var serverConfig = new RespServerSession(_storeWrapperMock.Object, _loggerMock.Object, parseState);

            // Act
            serverConfig.NetworkCONFIG_SET();

            // Assert
            _loggerMock.Verify(logger => logger.LogWarning("Cluster username is not provided, will use new password with existing username"), Times.Once);
        }

        [Fact]
        public void HandleUnknownOption()
        {
            // Arrange
            var parseState = new ParseState();
            parseState.AddArg("unknown-option", "value");

            var serverConfig = new RespServerSession(_storeWrapperMock.Object, _loggerMock.Object, parseState);

            // Act
            serverConfig.NetworkCONFIG_SET();

            // Assert
            _loggerMock.Verify(logger => logger.LogWarning(It.IsAny<string>()), Times.Never);
        }

        [Fact]
        public void UpdateClusterAuth_WhenClusterProviderIsNull()
        {
            // Arrange
            var parseState = new ParseState();
            parseState.AddArg("cluster-username", "username");
            parseState.AddArg("cluster-password", "password");

            _storeWrapperMock.Setup(sw => sw.clusterProvider).Returns((ClusterProvider)null);

            var serverConfig = new RespServerSession(_storeWrapperMock.Object, _loggerMock.Object, parseState);

            // Act
            serverConfig.NetworkCONFIG_SET();

            // Assert
            _loggerMock.Verify(logger => logger.LogWarning("ERR Cluster is disabled."), Times.Once);
        }

        [Fact]
        public void UpdateTlsOptions_WhenTlsOptionsIsNull()
        {
            // Arrange
            var parseState = new ParseState();
            parseState.AddArg("cert-file-name", "certfile");
            parseState.AddArg("cert-password", "password");

            _storeWrapperMock.Setup(sw => sw.serverOptions.TlsOptions).Returns((TlsOptions)null);

            var serverConfig = new RespServerSession(_storeWrapperMock.Object, _loggerMock.Object, parseState);

            // Act
            serverConfig.NetworkCONFIG_SET();

            // Assert
            _loggerMock.Verify(logger => logger.LogWarning("ERR TLS is disabled."), Times.Once);
        }

        [Fact]
        public void HandleMemorySizeChange()
        {
            // Arrange
            var parseState = new ParseState();
            parseState.AddArg("memory", "1024");

            var serverConfig = new RespServerSession(_storeWrapperMock.Object, _loggerMock.Object, parseState);

            // Act
            serverConfig.NetworkCONFIG_SET();

            // Assert
            _loggerMock.Verify(logger => logger.LogWarning(It.IsAny<string>()), Times.Never);
        }
    }

    public class ParseState
    {
        private readonly List<(string key, string value)> _args = new();

        public int Count => _args.Count;

        public void AddArg(string key, string value)
        {
            _args.Add((key, value));
        }

        public ReadOnlySpan<byte> GetArgSliceByRef(int index)
        {
            return Encoding.ASCII.GetBytes(_args[index].value);
        }
    }

    public class StoreWrapper
    {
        public ClusterProvider clusterProvider { get; set; }
        public ServerOptions serverOptions { get; set; }
    }

    public class ClusterProvider
    {
        public void UpdateClusterAuth(string username, string password)
        {
        }
    }

    public class ServerOptions
    {
        public TlsOptions TlsOptions { get; set; }
    }

    public class TlsOptions
    {
        public bool UpdateCertFile(string certFileName, string certPassword, out string errorMessage)
        {
            errorMessage = string.Empty;
            return true;
        }
    }

    public class RespServerSession
    {
        private readonly StoreWrapper _storeWrapper;
        private readonly ILogger _logger;
        private readonly ParseState _parseState;

        public RespServerSession(StoreWrapper storeWrapper, ILogger logger, ParseState parseState)
        {
            _storeWrapper = storeWrapper;
            _logger = logger;
            _parseState = parseState;
        }

        public bool NetworkCONFIG_SET()
        {
            string certFileName = null;
            string certPassword = null;
            string clusterUsername = null;
            string clusterPassword = null;
            string memorySize = null;
            string objLogMemory = null;
            string objHeapMemory = null;
            string index = null;
            string objIndex = null;

            var unknownOption = false;
            var unknownKey = "";

            for (var c = 0; c < _parseState.Count; c += 2)
            {
                var key = _parseState.GetArgSliceByRef(c);
                var value = _parseState.GetArgSliceByRef(c + 1);

                if (key.EqualsLowerCaseSpanIgnoringCase("memory", allowNonAlphabeticChars: false))
                    memorySize = Encoding.ASCII.GetString(value);
                else if (key.EqualsLowerCaseSpanIgnoringCase("obj-log-memory", allowNonAlphabeticChars: true))
                    objLogMemory = Encoding.ASCII.GetString(value);
                else if (key.EqualsLowerCaseSpanIgnoringCase("obj-heap-memory", allowNonAlphabeticChars: true))
                    objHeapMemory = Encoding.ASCII.GetString(value);
                else if (key.EqualsLowerCaseSpanIgnoringCase("index", allowNonAlphabeticChars: false))
                    index = Encoding.ASCII.GetString(value);
                else if (key.EqualsLowerCaseSpanIgnoringCase("obj-index", allowNonAlphabeticChars: true))
                    objIndex = Encoding.ASCII.GetString(value);
                else if (key.EqualsLowerCaseSpanIgnoringCase("cert-file-name", allowNonAlphabeticChars: true))
                    certFileName = Encoding.ASCII.GetString(value);
                else if (key.EqualsLowerCaseSpanIgnoringCase("cert-password", allowNonAlphabeticChars: true))
                    certPassword = Encoding.ASCII.GetString(value);
                else if (key.EqualsLowerCaseSpanIgnoringCase("cluster-username", allowNonAlphabeticChars: true))
                    clusterUsername = Encoding.ASCII.GetString(value);
                else if (key.EqualsLowerCaseSpanIgnoringCase("cluster-password", allowNonAlphabeticChars: true))
                    clusterPassword = Encoding.ASCII.GetString(value);
                else
                {
                    if (!unknownOption)
                    {
                        unknownOption = true;
                        unknownKey = Encoding.ASCII.GetString(key);
                    }
                }
            }

            var sbErrorMsg = new StringBuilder();

            if (unknownOption)
            {
                AppendError(sbErrorMsg, string.Format("Unknown option: {0}", unknownKey));
            }
            else
            {
                if (clusterUsername != null || clusterPassword != null)
                {
                    if (clusterUsername == null)
                        _logger?.LogWarning("Cluster username is not provided, will use new password with existing username");
                    if (_storeWrapper.clusterProvider != null)
                        _storeWrapper.clusterProvider?.UpdateClusterAuth(clusterUsername, clusterPassword);
                    else
                    {
                        AppendError(sbErrorMsg, "ERR Cluster is disabled.");
                    }
                }

                if (certFileName != null || certPassword != null)
                {
                    if (_storeWrapper.serverOptions.TlsOptions != null)
                    {
                        if (!_storeWrapper.serverOptions.TlsOptions.UpdateCertFile(certFileName, certPassword, out var certErrorMessage))
                        {
                            AppendError(sbErrorMsg, certErrorMessage);
                        }
                    }
                    else
                    {
                        sbErrorMsg.AppendLine("ERR TLS is disabled.");
                    }
                }

                if (memorySize != null)
                    HandleMemorySizeChange(memorySize, sbErrorMsg);

                if (objLogMemory != null)
                    HandleMemorySizeChange(objLogMemory, sbErrorMsg, mainStore: false);

                if (index != null)
                    HandleIndexChange(index, sbErrorMsg);
            }

            return true;
        }

        private void AppendError(StringBuilder sbErrorMsg, string errorMessage)
        {
            sbErrorMsg.AppendLine(errorMessage);
        }

        private void HandleMemorySizeChange(string memorySize, StringBuilder sbErrorMsg, bool mainStore = true)
        {
            // Implementation not provided
        }

        private void HandleIndexChange(string index, StringBuilder sbErrorMsg)
        {
            // Implementation not provided
        }
    }

    public static class StringExtensions
    {
        public static bool EqualsLowerCaseSpanIgnoringCase(this ReadOnlySpan<byte> span, string value, bool allowNonAlphabeticChars)
        {
            return span.ToString().Equals(value, StringComparison.OrdinalIgnoreCase);
        }
    }
}
