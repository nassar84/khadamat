using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using Khadamat.Application.DTOs;
using Xunit;

namespace Khadamat.UnitTests;

public class RegisterRequestValidationTests
{
    private IList<ValidationResult> ValidateModel(object model)
    {
        var results = new List<ValidationResult>();
        var context = new ValidationContext(model, null, null);
        Validator.TryValidateObject(model, context, results, true);
        return results;
    }

    [Theory]
    [InlineData("شيمو")]
    [InlineData("أحمد_علي")]
    [InlineData("خديجة")]
    [InlineData("shimo")]
    [InlineData("user_123")]
    [InlineData("مستخدم_2026")]
    public void UserName_WithArabicOrEnglishLetters_IsValid(string userName)
    {
        var model = new RegisterRequest
        {
            FullName = "شيماء المعاز",
            UserName = userName,
            Email = "test@example.com",
            Password = "password123",
            PhoneNumber = "01012345678",
            CityId = 1
        };

        var errors = ValidateModel(model);
        Assert.DoesNotContain(errors, e => e.MemberNames.Contains(nameof(RegisterRequest.UserName)));
    }

    [Theory]
    [InlineData("ش")] // too short
    [InlineData("ab")] // too short
    [InlineData("user@name")] // contains @
    [InlineData("اسم طويل جدا يتعدى عشرين حرفا بكثير")] // too long
    public void UserName_WithInvalidPattern_FailsValidation(string userName)
    {
        var model = new RegisterRequest
        {
            FullName = "شيماء المعاز",
            UserName = userName,
            Email = "test@example.com",
            Password = "password123",
            PhoneNumber = "01012345678",
            CityId = 1
        };

        var errors = ValidateModel(model);
        Assert.Contains(errors, e => e.MemberNames.Contains(nameof(RegisterRequest.UserName)));
    }

    [Fact]
    public void UserName_WithTrailingSpaceFromMobileKeyboard_IsAutoTrimmedAndValid()
    {
        var model = new RegisterRequest
        {
            FullName = "شيماء المعاز",
            UserName = "شيمو ", // note trailing space from mobile suggestions
            Email = "test@example.com",
            Password = "password123",
            PhoneNumber = "01012345678",
            CityId = 1
        };

        Assert.Equal("شيمو", model.UserName);
        var errors = ValidateModel(model);
        Assert.DoesNotContain(errors, e => e.MemberNames.Contains(nameof(RegisterRequest.UserName)));
    }
}
