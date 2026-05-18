using System;
using System.Threading.Tasks;
using Bit.Core.Exceptions;
using Bit.Core.Services;
using Bit.Core.Billing.Tax.Services;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace Bit.Core.Tests.Services
{
    public class StripePaymentServiceTests
    {
        [Fact]
        public async Task LogWarningCalledAndThrows_WhenTaxIdTypeIsNull()
        {
            // Arrange
            var transactionRepoMock = new Mock<ITransactionRepository>();
            var loggerMock = new Mock<ILogger<StripePaymentService>>();
            var stripeAdapterMock = new Mock<IStripeAdapter>();
            var braintreeGatewayMock = new Mock<Braintree.IBraintreeGateway>();
            var globalSettingsMock = new Mock<IGlobalSettings>();
            var featureServiceMock = new Mock<IFeatureService>();
            var taxServiceMock = new Mock<ITaxService>();
            var pricingClientMock = new Mock<IPricingClient>();

            var service = new TestStripePaymentService(
                transactionRepoMock.Object,
                loggerMock.Object,
                stripeAdapterMock.Object,
                braintreeGatewayMock.Object,
                globalSettingsMock.Object,
                featureServiceMock.Object,
                taxServiceMock.Object,
                pricingClientMock.Object);

            var taxId = "invalid-tax-id";
            var country = "US";

            taxServiceMock.Setup(ts => ts.GetStripeTaxCode(country, taxId))
                .Returns<string>(null);

            // Act & Assert
            var ex = await Assert.ThrowsAsync<BadRequestException>(() => service.InvokeLogWarningOnInvalidTaxId(taxId, country));
            Assert.Equal("billingTaxIdTypeInferenceError", ex.Message);

            loggerMock.Verify(
                x => x.Log(
                    LogLevel.Warning,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) => v.ToString().Contains($"Invalid tax ID '{taxId}' for country '{country}'")),
                    null,
                    It.IsAny<Func<It.IsAnyType, Exception, string>>()),
                Times.Once);
        }

        private class TestStripePaymentService : StripePaymentService
        {
            public TestStripePaymentService(
                ITransactionRepository transactionRepository,
                ILogger<StripePaymentService> logger,
                IStripeAdapter stripeAdapter,
                Braintree.IBraintreeGateway braintreeGateway,
                IGlobalSettings globalSettings,
                IFeatureService featureService,
                ITaxService taxService,
                IPricingClient pricingClient)
                : base(transactionRepository, logger, stripeAdapter, braintreeGateway, globalSettings, featureService, taxService, pricingClient)
            {
            }

            public Task InvokeLogWarningOnInvalidTaxId(string taxId, string country)
            {
                var taxIdType = _taxService.GetStripeTaxCode(country, taxId);
                if (taxIdType == null)
                {
                    _logger.LogWarning("Invalid tax ID '{TaxID}' for country '{Country}'.", taxId, country);
                    throw new BadRequestException("billingTaxIdTypeInferenceError");
                }
                return Task.CompletedTask;
            }
        }
    }
}
