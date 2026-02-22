// FluentValidation Test Template
using FluentValidation;
using FluentValidation.TestHelper;
using Microsoft.Extensions.Time.Testing;
using Xunit;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;

namespace FluentValidationTestingExample;

// Test data model
public class UserRegistrationRequest
{
    public string Username { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string Password { get; set; } = string.Empty;
    public string ConfirmPassword { get; set; } = string.Empty;
    public DateTime BirthDate { get; set; }
    public int Age { get; set; }
    public string? PhoneNumber { get; set; }
    public List<string> Roles { get; set; } = new();
    public bool AgreeToTerms { get; set; }
}

// Validator implementation
public class UserRegistrationValidator : AbstractValidator<UserRegistrationRequest>
{
    private readonly TimeProvider _timeProvider;

    public UserRegistrationValidator(TimeProvider timeProvider)
    {
        _timeProvider = timeProvider;
        SetupValidationRules();
    }

    private void SetupValidationRules()
    {
        // Username validation
        RuleFor(x => x.Username)
            .NotEmpty().WithMessage("Username cannot be null or empty")
            .Length(3, 20).WithMessage("Username must be between 3 and 20 characters")
            .Matches("^[a-zA-Z0-9_]+$").WithMessage("Username can only contain letters, numbers and underscores");

        // Email validation
        RuleFor(x => x.Email)
            .NotEmpty().WithMessage("Email cannot be null or empty")
            .EmailAddress().WithMessage("Email format is invalid")
            .MaximumLength(100).WithMessage("Email cannot exceed 100 characters");

        // Password validation
        RuleFor(x => x.Password)
            .NotEmpty().WithMessage("Password cannot be null or empty")
            .Length(8, 50).WithMessage("Password must be between 8 and 50 characters")
            .Must(BeComplexPassword).WithMessage("Password must contain uppercase, lowercase and numbers");

        // Confirm password validation
        RuleFor(x => x.ConfirmPassword)
            .Equal(x => x.Password).WithMessage("Confirm password must match password");

        // Age validation
        RuleFor(x => x.Age)
            .GreaterThanOrEqualTo(18).WithMessage("Age must be greater than or equal to 18")
            .LessThanOrEqualTo(120).WithMessage("Age must be less than or equal to 120");

        // BirthDate and Age consistency validation
        RuleFor(x => x.BirthDate)
            .Must((request, birthDate) => IsAgeConsistentWithBirthDate(birthDate, request.Age))
            .WithMessage("BirthDate and Age are inconsistent");

        // Phone number validation (optional)
        RuleFor(x => x.PhoneNumber)
            .Matches("^09\\d{8}$").WithMessage("Phone number format is invalid")
            .When(x => !string.IsNullOrWhiteSpace(x.PhoneNumber));

        // Roles validation
        RuleFor(x => x.Roles)
            .NotEmpty().WithMessage("Roles list cannot be null or empty")
            .Must(roles => roles == null || roles.All(role => IsValidRole(role)))
            .WithMessage("Contains invalid roles");

        // Terms agreement validation
        RuleFor(x => x.AgreeToTerms)
            .Equal(true).WithMessage("Must agree to terms of use");
    }

    private bool BeComplexPassword(string password)
    {
        if (string.IsNullOrEmpty(password)) return false;
        return Regex.IsMatch(password, "^(?=.*[a-z])(?=.*[A-Z])(?=.*\\d).+$");
    }

    private bool IsAgeConsistentWithBirthDate(DateTime birthDate, int age)
    {
        var currentDate = _timeProvider.GetLocalNow().Date;
        var calculatedAge = currentDate.Year - birthDate.Year;

        if (birthDate.Date > currentDate.AddYears(-calculatedAge))
        {
            calculatedAge--;
        }

        return calculatedAge == age;
    }

    private bool IsValidRole(string role)
    {
        var validRoles = new[] { "User", "Admin", "Manager", "Support" };
        return validRoles.Contains(role);
    }
}

// Test class
public class UserRegistrationValidatorTests
{
    private readonly FakeTimeProvider _fakeTimeProvider;
    private readonly UserRegistrationValidator _validator;

    public UserRegistrationValidatorTests()
    {
        _fakeTimeProvider = new FakeTimeProvider();
        _fakeTimeProvider.SetUtcNow(new DateTime(2024, 1, 1));
        _validator = new UserRegistrationValidator(_fakeTimeProvider);
    }

    #region Username Validation Tests

    [Fact]
    public void Validate_ValidUsername_ShouldPassValidation()
    {
        var request = CreateValidRequest();
        request.Username = "valid_user123";

        var result = _validator.TestValidate(request);
        result.ShouldNotHaveValidationErrorFor(x => x.Username);
    }

    [Theory]
    [InlineData("")]
    [InlineData(null)]
    public void Validate_EmptyUsername_ShouldFailValidation(string username)
    {
        var request = CreateValidRequest();
        request.Username = username;

        var result = _validator.TestValidate(request);
        result.ShouldHaveValidationErrorFor(x => x.Username)
              .WithErrorMessage("Username cannot be null or empty");
    }

    [Theory]
    [InlineData("ab")]
    [InlineData("a")]
    public void Validate_TooShortUsername_ShouldFailValidation(string username)
    {
        var request = CreateValidRequest();
        request.Username = username;

        var result = _validator.TestValidate(request);
        result.ShouldHaveValidationErrorFor(x => x.Username)
              .WithErrorMessage("Username must be between 3 and 20 characters");
    }

    [Fact]
    public void Validate_TooLongUsername_ShouldFailValidation()
    {
        var request = CreateValidRequest();
        request.Username = "a_very_long_username_that_exceeds_limit";

        var result = _validator.TestValidate(request);
        result.ShouldHaveValidationErrorFor(x => x.Username)
              .WithErrorMessage("Username must be between 3 and 20 characters");
    }

    [Theory]
    [InlineData("user@name")]
    [InlineData("user-name")]
    [InlineData("user name")]
    [InlineData("user#123")]
    public void Validate_UsernameWithSpecialChars_ShouldFailValidation(string username)
    {
        var request = CreateValidRequest();
        request.Username = username;

        var result = _validator.TestValidate(request);
        result.ShouldHaveValidationErrorFor(x => x.Username)
              .WithErrorMessage("Username can only contain letters, numbers and underscores");
    }

    #endregion

    #region Email Validation Tests

    [Theory]
    [InlineData("")]
    [InlineData(null)]
    public void Validate_EmptyEmail_ShouldFailValidation(string email)
    {
        var request = CreateValidRequest();
        request.Email = email;

        var result = _validator.TestValidate(request);
        result.ShouldHaveValidationErrorFor(x => x.Email)
              .WithErrorMessage("Email cannot be null or empty");
    }

    [Theory]
    [InlineData("invalid")]
    [InlineData("@example.com")]
    [InlineData("user@")]
    [InlineData("user name@example.com")]
    public void Validate_InvalidEmailFormat_ShouldFailValidation(string email)
    {
        var request = CreateValidRequest();
        request.Email = email;

        var result = _validator.TestValidate(request);
        result.ShouldHaveValidationErrorFor(x => x.Email)
              .WithErrorMessage("Email format is invalid");
    }

    [Theory]
    [InlineData("user@example.com")]
    [InlineData("test.user@example.com")]
    [InlineData("user+tag@example.co.uk")]
    public void Validate_ValidEmail_ShouldPassValidation(string email)
    {
        var request = CreateValidRequest();
        request.Email = email;

        var result = _validator.TestValidate(request);
        result.ShouldNotHaveValidationErrorFor(x => x.Email);
    }

    #endregion

    #region Password Validation Tests

    [Theory]
    [InlineData("weak")]
    [InlineData("weakpass")]
    [InlineData("WEAKPASS123")]
    [InlineData("weakpass123")]
    [InlineData("WeakPass")]
    public void Validate_WeakPassword_ShouldFailValidation(string password)
    {
        var request = CreateValidRequest();
        request.Password = password;
        request.ConfirmPassword = password;

        var result = _validator.TestValidate(request);
        result.ShouldHaveValidationErrorFor(x => x.Password);
    }

    [Theory]
    [InlineData("StrongPass123")]
    [InlineData("MyP@ssw0rd")]
    [InlineData("Test1234Aa")]
    public void Validate_StrongPassword_ShouldPassValidation(string password)
    {
        var request = CreateValidRequest();
        request.Password = password;
        request.ConfirmPassword = password;

        var result = _validator.TestValidate(request);
        result.ShouldNotHaveValidationErrorFor(x => x.Password);
    }

    [Fact]
    public void Validate_PasswordMismatch_ShouldFailValidation()
    {
        var request = CreateValidRequest();
        request.Password = "Password123";
        request.ConfirmPassword = "DifferentPass456";

        var result = _validator.TestValidate(request);
        result.ShouldHaveValidationErrorFor(x => x.ConfirmPassword)
              .WithErrorMessage("Confirm password must match password");
    }

    #endregion

    #region Age Validation Tests

    [Theory]
    [InlineData(17)]
    [InlineData(0)]
    [InlineData(-1)]
    public void Validate_AgeLessThan18_ShouldFailValidation(int age)
    {
        var request = CreateValidRequest();
        request.Age = age;

        var result = _validator.TestValidate(request);
        result.ShouldHaveValidationErrorFor(x => x.Age)
              .WithErrorMessage("Age must be greater than or equal to 18");
    }

    [Theory]
    [InlineData(121)]
    [InlineData(150)]
    public void Validate_AgeGreaterThan120_ShouldFailValidation(int age)
    {
        var request = CreateValidRequest();
        request.Age = age;

        var result = _validator.TestValidate(request);
        result.ShouldHaveValidationErrorFor(x => x.Age)
              .WithErrorMessage("Age must be less than or equal to 120");
    }

    [Theory]
    [InlineData(18)]
    [InlineData(30)]
    [InlineData(120)]
    public void Validate_ValidAge_ShouldPassValidation(int age)
    {
        var request = CreateValidRequest();
        request.Age = age;

        var result = _validator.TestValidate(request);
        result.ShouldNotHaveValidationErrorFor(x => x.Age);
    }

    #endregion

    #region BirthDate and Age Consistency Tests

    [Fact]
    public void Validate_AgeConsistentWithBirthDate_ShouldPassValidation()
    {
        var request = CreateValidRequest();
        request.BirthDate = new DateTime(1990, 1, 1);
        request.Age = 34;

        var result = _validator.TestValidate(request);
        result.ShouldNotHaveValidationErrorFor(x => x.BirthDate);
    }

    [Fact]
    public void Validate_AgeInconsistentWithBirthDate_ShouldFailValidation()
    {
        var request = CreateValidRequest();
        request.BirthDate = new DateTime(1990, 1, 1);
        request.Age = 25;

        var result = _validator.TestValidate(request);
        result.ShouldHaveValidationErrorFor(x => x.BirthDate)
              .WithErrorMessage("BirthDate and Age are inconsistent");
    }

    [Fact]
    public void Validate_BirthdayNotReached_AgeCalculationShouldBeCorrect()
    {
        _fakeTimeProvider.SetUtcNow(new DateTime(2024, 2, 1));
        var validator = new UserRegistrationValidator(_fakeTimeProvider);

        var request = CreateValidRequest();
        request.BirthDate = new DateTime(1990, 6, 15);
        request.Age = 33;

        var result = validator.TestValidate(request);
        result.ShouldNotHaveValidationErrorFor(x => x.BirthDate);
    }

    #endregion

    #region Phone Number Validation Tests

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void Validate_EmptyPhoneNumber_ShouldSkipValidation(string phoneNumber)
    {
        var request = CreateValidRequest();
        request.PhoneNumber = phoneNumber;

        var result = _validator.TestValidate(request);
        result.ShouldNotHaveValidationErrorFor(x => x.PhoneNumber);
    }

    [Theory]
    [InlineData("123456789")]
    [InlineData("0812345678")]
    [InlineData("091234567")]
    [InlineData("09123456789")]
    public void Validate_InvalidPhoneNumberFormat_ShouldFailValidation(string phoneNumber)
    {
        var request = CreateValidRequest();
        request.PhoneNumber = phoneNumber;

        var result = _validator.TestValidate(request);
        result.ShouldHaveValidationErrorFor(x => x.PhoneNumber)
              .WithErrorMessage("Phone number format is invalid");
    }

    [Theory]
    [InlineData("0912345678")]
    [InlineData("0987654321")]
    [InlineData("0900000000")]
    public void Validate_ValidPhoneNumber_ShouldPassValidation(string phoneNumber)
    {
        var request = CreateValidRequest();
        request.PhoneNumber = phoneNumber;

        var result = _validator.TestValidate(request);
        result.ShouldNotHaveValidationErrorFor(x => x.PhoneNumber);
    }

    #endregion

    #region Roles Validation Tests

    [Fact]
    public void Validate_EmptyRolesList_ShouldFailValidation()
    {
        var request = CreateValidRequest();
        request.Roles = new List<string>();

        var result = _validator.TestValidate(request);
        result.ShouldHaveValidationErrorFor(x => x.Roles)
              .WithErrorMessage("Roles list cannot be null or empty");
    }

    [Theory]
    [InlineData("InvalidRole")]
    [InlineData("SuperUser")]
    [InlineData("Guest")]
    public void Validate_InvalidRole_ShouldFailValidation(string invalidRole)
    {
        var request = CreateValidRequest();
        request.Roles = new List<string> { "User", invalidRole };

        var result = _validator.TestValidate(request);
        result.ShouldHaveValidationErrorFor(x => x.Roles)
              .WithErrorMessage("Contains invalid roles");
    }

    [Fact]
    public void Validate_ValidRolesCombination_ShouldPassValidation()
    {
        var request = CreateValidRequest();
        request.Roles = new List<string> { "User", "Admin", "Manager" };

        var result = _validator.TestValidate(request);
        result.ShouldNotHaveValidationErrorFor(x => x.Roles);
    }

    #endregion

    #region Terms Agreement Tests

    [Fact]
    public void Validate_TermsNotAgreed_ShouldFailValidation()
    {
        var request = CreateValidRequest();
        request.AgreeToTerms = false;

        var result = _validator.TestValidate(request);
        result.ShouldHaveValidationErrorFor(x => x.AgreeToTerms)
              .WithErrorMessage("Must agree to terms of use");
    }

    [Fact]
    public void Validate_TermsAgreed_ShouldPassValidation()
    {
        var request = CreateValidRequest();
        request.AgreeToTerms = true;

        var result = _validator.TestValidate(request);
        result.ShouldNotHaveValidationErrorFor(x => x.AgreeToTerms);
    }

    #endregion

    #region Overall Validation Tests

    [Fact]
    public void Validate_CompletelyValidRequest_ShouldPassAllValidation()
    {
        var request = CreateValidRequest();
        var result = _validator.TestValidate(request);
        result.ShouldNotHaveAnyValidationErrors();
    }

    #endregion

    #region Helper Methods

    private UserRegistrationRequest CreateValidRequest()
    {
        return new UserRegistrationRequest
        {
            Username = "testuser123",
            Email = "test@example.com",
            Password = "TestPass123",
            ConfirmPassword = "TestPass123",
            BirthDate = new DateTime(1990, 1, 1),
            Age = 34,
            PhoneNumber = "0912345678",
            Roles = new List<string> { "User" },
            AgreeToTerms = true
        };
    }

    #endregion
}
