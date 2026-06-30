using FluentAssertions;
using Koala.Yedpa.Core.Helpers;
using Xunit;

namespace Koala.Yedpa.Service.Tests.BulkInvoice
{
    public class PendingLinesMonthTests
    {
        [Theory]
        [InlineData(1, "OCAK")]
        [InlineData(2, "SUBAT")]
        [InlineData(7, "TEMMUZ")]
        [InlineData(8, "AGUSTOS")]
        [InlineData(9, "EYLUL")]
        [InlineData(12, "ARALIK")]
        public void ToLogoName_ReturnsUppercaseAsciiMonth(int month, string expected)
            => BulkInvoiceMonths.ToLogoName(month).Should().Be(expected);
    }
}
