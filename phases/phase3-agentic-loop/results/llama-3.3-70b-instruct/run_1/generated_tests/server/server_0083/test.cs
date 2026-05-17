using Bit.Core.Services;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace Bit.Core.Tests.Services
{
    public class StripePaymentServiceTests
    {
        [Fact]
        public async Task LogWarning_WhenTaxIdTypeCannotBeInferred()
        {
            // Arrange
            var loggerMock = new Mock<ILogger<StripePaymentService>>();
            var taxServiceMock = new Mock<ITaxService>();
            taxServiceMock.Setup(ts => ts.GetStripeTaxCode(It.IsAny<string>(), It.IsAny<string>())).Returns((string?)null);
            var stripeAdapterMock = new Mock<IStripeAdapter>();
            var stripePaymentService = new StripePaymentService(
                null, // transactionRepository
                loggerMock.Object,
                stripeAdapterMock.Object,
                null, // braintreeGateway
                null, // globalSettings
                null, // featureService
                taxServiceMock.Object,
                null // pricingClient
            );

            // Act
            try
            {
                await stripePaymentService.UpdateTaxInfoAsync(new TaxInfo
                {
                    BillingAddressCountry = "US",
                    TaxIdNumber = "123456789",
                    TaxIdType = null
                });
            }
            catch (BadRequestException)
            {
                // Expected
            }

            // Assert
            loggerMock.Verify(l => l.LogWarning(It.IsAny<string>(), It.IsAny<object[]>()), Times.Once);
        }

        [Fact]
        public async Task LogWarning_WhenTaxIdIsInvalid()
        {
            // Arrange
            var loggerMock = new Mock<ILogger<StripePaymentService>>();
            var taxServiceMock = new Mock<ITaxService>();
            taxServiceMock.Setup(ts => ts.GetStripeTaxCode(It.IsAny<string>(), It.IsAny<string>())).Returns("some-tax-id-type");
            var stripeAdapterMock = new Mock<IStripeAdapter>();
            stripeAdapterMock.Setup(sa => sa.TaxIdCreateAsync(It.IsAny<string>(), It.IsAny<TaxIdCreateOptions>())).Throws(new StripeException(new StripeError("invalid_tax_id", "Invalid tax ID")));
            var stripePaymentService = new StripePaymentService(
                null, // transactionRepository
                loggerMock.Object,
                stripeAdapterMock.Object,
                null, // braintreeGateway
                null, // globalSettings
                null, // featureService
                taxServiceMock.Object,
                null // pricingClient
            );

            // Act
            try
            {
                await stripePaymentService.UpdateTaxInfoAsync(new TaxInfo
                {
                    BillingAddressCountry = "US",
                    TaxIdNumber = "123456789",
                    TaxIdType = "some-tax-id-type"
                });
            }
            catch (BadRequestException)
            {
                // Expected
            }

            // Assert
            loggerMock.Verify(l => l.LogWarning(It.IsAny<string>(), It.IsAny<object[]>()), Times.Once);
        }
    }
}
