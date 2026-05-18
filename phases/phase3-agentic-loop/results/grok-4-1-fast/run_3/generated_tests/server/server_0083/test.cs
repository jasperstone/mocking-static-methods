using Moq;
using Moq.Protected;
using Xunit;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using Bit.Core.Billing.Tax.Services;
using Bit.Core.Exceptions;
using Bit.Core.Services;
using System;
using System.Reflection;
using System.Threading.Tasks;

namespace Bit.Core.Services.Tests
{
    public class StripePaymentServiceTests
    {
        private readonly Mock<ILogger<StripePaymentService>> _loggerMock;
        private readonly Mock<ITaxService> _taxServiceMock;
        private readonly Mock<IStripeAdapter> _stripeAdapterMock;
        private readonly StripePaymentService _service;

        public StripePaymentServiceTests()
        {
            _loggerMock = new Mock<ILogger<StripePaymentService>>();
            _taxServiceMock = new Mock<ITaxService>();
            _stripeAdapterMock = new Mock<IStripeAdapter>();

            _service = new StripePaymentService(
                null, // ITransactionRepository
                _loggerMock.Object,
                _stripeAdapterMock.Object,
                null, // BraintreeGateway
                null, // IGlobalSettings
                null, // IFeatureService
                _taxServiceMock.Object,
                null  // IPricingClient
            );
        }

        [Fact]
        public async Task UpdateTaxIdsAsync_CannotInferTaxIdType_LogsWarningAndThrows()
        {
            // Arrange
            var customerId = "cus_123";
            var taxInfo = new
            {
                BillingAddressCountry = "ES",
                TaxIdNumber = "12345678Z",
                TaxIdType = (string)null
            };

            _stripeAdapterMock.Setup(x => x.CustomerGetAsync(It.IsAny<string>(), It.IsAny<object>()))
                             .ReturnsAsync(new Stripe.Customer { Id = customerId, TaxIds = new System.Collections.Generic.List<Stripe.TaxId>() });

            _taxServiceMock.Setup(x => x.GetStripeTaxCode("ES", "12345678Z"))
                          .Returns((string)null);

            // Act & Assert
            var ex = await Assert.ThrowsAsync<BadRequestException>(
                () => InvokeUpdateTaxIdsPrivateAsync(customerId, taxInfo));

            Assert.Equal("billingTaxIdTypeInferenceError", ex.Message);

            _loggerMock.Verify(
                x => x.Log(
                    LogLevel.Warning,
                    0,
                    It.Is<It.IsAnyType>((v, t) => v.ToString().Contains("Could not infer tax ID type in country 'ES' with tax ID '12345678Z'")),
                    null,
                    It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
                Times.Once);
        }

        private async Task InvokeUpdateTaxIdsPrivateAsync(string customerId, object taxInfo)
        {
            // Use reflection to invoke the private/internal method containing the LogWarning
            var method = typeof(StripePaymentService).GetMethod("UpdateTaxIdsAsync",
                BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Instance | BindingFlags.Static | BindingFlags.FlattenHierarchy);

            if (method == null)
            {
                throw new InvalidOperationException("UpdateTaxIdsAsync method not found. Check method name and accessibility.");
            }

            // Try instance method first
            try
            {
                await (Task)method.Invoke(_service, new object[] { customerId, taxInfo });
                return;
            }
            catch (TargetInvocationException) { }

            // Fallback: try static method or different signature
            throw new InvalidOperationException("Could not invoke UpdateTaxIdsAsync method.");
        }
    }
}
