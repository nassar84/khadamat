using Khadamat.Domain.Common;
using Xunit;

namespace Khadamat.UnitTests;

public class PhoneNumberHelperTests
{
    [Theory]
    [InlineData("٠١٠١٢٣٤٥٦٧٨", "01012345678")]
    [InlineData("۰۱۲۳۴۵۶۷۸۹", "0123456789")]
    [InlineData("01012345678", "01012345678")]
    [InlineData("رقم تليفون ٠١٠-١٢٣٤", "رقم تليفون 010-1234")]
    public void ToEnglishDigits_ConvertsCorrectly(string input, string expected)
    {
        var result = PhoneNumberHelper.ToEnglishDigits(input);
        Assert.Equal(expected, result);
    }

    [Theory]
    [InlineData("٠١٠١٢٣٤٥٦٧٨", "01012345678")]
    [InlineData(" ٠١٠-١٢٣٤-٥٦٧٨ ", "01012345678")]
    [InlineData("+٢٠ ١٠١ ٢٣٤ ٥٦٧٨", "+201012345678")]
    [InlineData("010 1234 5678", "01012345678")]
    [InlineData(null, null)]
    [InlineData("   ", null)]
    [InlineData("abc", null)]
    public void NormalizeDigits_CleansAndStandardizes(string? input, string? expected)
    {
        var result = PhoneNumberHelper.NormalizeDigits(input);
        Assert.Equal(expected, result);
    }

    [Theory]
    [InlineData("٠١٠١٢٣٤٥٦٧٨", "201012345678")]
    [InlineData("01012345678", "201012345678")]
    [InlineData("+201012345678", "201012345678")]
    [InlineData("+٢٠١٠١٢٣٤٥٦٧٨", "201012345678")]
    [InlineData("00201012345678", "201012345678")]
    [InlineData(" 011 2345 6789 ", "201123456789")]
    [InlineData("966501234567", "966501234567")]
    public void NormalizeForWhatsApp_ProducesCorrectWaDigits(string input, string expected)
    {
        var result = PhoneNumberHelper.NormalizeForWhatsApp(input);
        Assert.Equal(expected, result);
    }

    [Fact]
    public void GetWhatsAppUrl_WithArabicDigitsAndMessage_ProducesValidUrl()
    {
        var url = PhoneNumberHelper.GetWhatsAppUrl("٠١٠١٢٣٤٥٦٧٨", "مرحبا");
        Assert.Equal("https://wa.me/201012345678?text=%D9%85%D8%B1%D8%AD%D8%A8%D8%A7", url);
    }

    [Fact]
    public void GetTelUrl_WithArabicDigits_ProducesValidTelLink()
    {
        var url = PhoneNumberHelper.GetTelUrl("٠١٠-١٢٣٤-٥٦٧٨");
        Assert.Equal("tel:01012345678", url);
    }
}
