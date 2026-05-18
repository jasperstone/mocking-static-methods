using System;
using System.Reflection;
using Microsoft.SemanticKernel.Connectors.Google.Core;
using Xunit;

namespace Microsoft.SemanticKernel.Connectors.Google.Core.Tests
{
    public class GeminiChatCompletionClientStaticTests
    {
        [Fact]
        public void ValidateGeminiResponse_Throws_WhenPromptFeedbackBlockReasonIsNotNull()
        {
            // Arrange
            var geminiResponse = new GeminiResponse
            {
                PromptFeedback = new GeminiPromptFeedback
                {
                    BlockReason = "blocked"
                }
            };

            // Act & Assert
            var method = typeof(GeminiChatCompletionClient).GetMethod("ValidateGeminiResponse", BindingFlags.NonPublic | BindingFlags.Static);
            var ex = Assert.Throws<TargetInvocationException>(() => method.Invoke(null, new object[] { geminiResponse }));
            Assert.IsType<KernelException>(ex.InnerException);
            Assert.Equal("Prompt was blocked due to Gemini API safety reasons.", ex.InnerException.Message);
        }

        [Fact]
        public void ValidateGeminiResponse_DoesNotThrow_WhenPromptFeedbackIsNull()
        {
            // Arrange
            var geminiResponse = new GeminiResponse
            {
                PromptFeedback = null
            };

            // Act & Assert
            var method = typeof(GeminiChatCompletionClient).GetMethod("ValidateGeminiResponse", BindingFlags.NonPublic | BindingFlags.Static);
            var exception = Record.Exception(() => method.Invoke(null, new object[] { geminiResponse }));
            Assert.Null(exception);
        }

        [Fact]
        public void ValidateGeminiResponse_DoesNotThrow_WhenBlockReasonIsNull()
        {
            // Arrange
            var geminiResponse = new GeminiResponse
            {
                PromptFeedback = new GeminiPromptFeedback
                {
                    BlockReason = null
                }
            };

            // Act & Assert
            var method = typeof(GeminiChatCompletionClient).GetMethod("ValidateGeminiResponse", BindingFlags.NonPublic | BindingFlags.Static);
            var exception = Record.Exception(() => method.Invoke(null, new object[] { geminiResponse }));
            Assert.Null(exception);
        }
    }

    // Minimal stubs for dependent types to allow compilation of tests
    internal class GeminiResponse
    {
        public GeminiPromptFeedback? PromptFeedback { get; set; }
    }

    internal class GeminiPromptFeedback
    {
        public string? BlockReason { get; set; }
    }

    internal class KernelException : Exception
    {
        public KernelException(string message) : base(message) { }
    }
}
