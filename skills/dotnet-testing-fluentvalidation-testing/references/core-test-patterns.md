# FluentValidation Core Test Patterns - Detailed Reference

> This document contains complete code examples and detailed explanations for FluentValidation validator testing.
> Main document: [SKILL.md](../SKILL.md)

---

## Pattern 1: Basic Field Validation

### Validator Example

```csharp
public class UserValidator : AbstractValidator<UserRegistrationRequest>
{
    public UserValidator()
    {
        RuleFor(x => x.Username)
            .NotEmpty().WithMessage("Username cannot be null or empty")
            .Length(3, 20).WithMessage("Username length must be between 3 and 20 characters")
            .Matches(@"^[a-zA-Z0-9_]+$").WithMessage("Username can only contain letters, numbers, and underscores");

        RuleFor(x => x.Email)
            .NotEmpty().WithMessage("Email cannot be null or empty")
            .EmailAddress().WithMessage("Email format is incorrect")
            .MaximumLength(100).WithMessage("Email length cannot exceed 100 characters");

        RuleFor(x => x.Age)
            .GreaterThanOrEqualTo(18).WithMessage("Age must be greater than or equal to 18")
            .LessThanOrEqualTo(120).WithMessage("Age must be less than or equal to 120");
    }
}
```

### Test Example

```csharp
public class UserValidatorTests
{
    private readonly UserValidator _validator;

    public UserValidatorTests()
    {
        _validator = new UserValidator();
    }

    [Fact]
    public void Validate_ValidUsername_ShouldPassValidation()
    {
        // Arrange
        var request = new UserRegistrationRequest { Username = "valid_user123" };

        // Act
        var result = _validator.TestValidate(request);

        // Assert
        result.ShouldNotHaveValidationErrorFor(x => x.Username);
    }

    [Fact]
    public void Validate_EmptyUsername_ShouldFailValidation()
    {
        // Arrange
        var request = new UserRegistrationRequest { Username = "" };

        // Act
        var result = _validator.TestValidate(request);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.Username)
              .WithErrorMessage("Username cannot be null or empty");
    }
}
```

---

## Pattern 2: Parameterized Tests

```csharp
[Theory]
[InlineData("", "Username cannot be null or empty")]
[InlineData("ab", "Username length must be between 3 and 20 characters")]
[InlineData("a_very_long_username_exceeds_limit", "Username length must be between 3 and 20 characters")]
[InlineData("user@name", "Username can only contain letters, numbers, and underscores")]
public void Validate_InvalidUsername_ShouldReturnCorrespondingError(string username, string expectedError)
{
    // Arrange
    var request = new UserRegistrationRequest { Username = username };

    // Act
    var result = _validator.TestValidate(request);

    // Assert
    result.ShouldHaveValidationErrorFor(x => x.Username)
          .WithErrorMessage(expectedError);
}

[Theory]
[InlineData("user123")]
[InlineData("valid_user")]
[InlineData("TEST_User_99")]
public void Validate_ValidUsername_ShouldPassValidation(string username)
{
    // Arrange
    var request = new UserRegistrationRequest { Username = username };

    // Act
    var result = _validator.TestValidate(request);

    // Assert
    result.ShouldNotHaveValidationErrorFor(x => x.Username);
}
```

---

## Pattern 3: Cross-Field Validation

### Password and Confirm Password

```csharp
public class UserValidator : AbstractValidator<UserRegistrationRequest>
{
    public UserValidator()
    {
        RuleFor(x => x.Password)
            .NotEmpty().WithMessage("Password cannot be null or empty")
            .Length(8, 50).WithMessage("Password length must be between 8 and 50 characters")
            .Must(BeComplexPassword).WithMessage("Password must contain uppercase, lowercase letters and numbers");

        RuleFor(x => x.ConfirmPassword)
            .Equal(x => x.Password).WithMessage("Confirm password must match password");
    }

    private bool BeComplexPassword(string password)
    {
        return !string.IsNullOrEmpty(password) && 
               Regex.IsMatch(password, @"^(?=.*[a-z])(?=.*[A-Z])(?=.*\d).+$");
    }
}
```

### Test Example

```csharp
[Fact]
public void Validate_PasswordAndConfirmPasswordMismatch_ShouldFailValidation()
{
    // Arrange
    var request = new UserRegistrationRequest
    {
        Password = "Password123",
        ConfirmPassword = "DifferentPass456"
    };

    // Act
    var result = _validator.TestValidate(request);

    // Assert
    result.ShouldHaveValidationErrorFor(x => x.ConfirmPassword)
          .WithErrorMessage("Confirm password must match password");
}

[Theory]
[InlineData("weak", "Password length must be between 8 and 50 characters")]
[InlineData("weakpass", "Password must contain uppercase, lowercase letters and numbers")]
[InlineData("WEAKPASS123", "Password must contain uppercase, lowercase letters and numbers")]
public void Validate_WeakPassword_ShouldFailValidation(string password, string expectedError)
{
    // Arrange
    var request = new UserRegistrationRequest
    {
        Password = password,
        ConfirmPassword = password
    };

    // Act
    var result = _validator.TestValidate(request);

    // Assert
    result.ShouldHaveValidationErrorFor(x => x.Password)
          .WithErrorMessage(expectedError);
}
```

---

## Pattern 4: Time-Dependent Validation

### Age and Birthdate Consistency Validation

```csharp
public class UserValidator : AbstractValidator<UserRegistrationRequest>
{
    private readonly TimeProvider _timeProvider;

    public UserValidator(TimeProvider timeProvider)
    {
        _timeProvider = timeProvider;

        RuleFor(x => x.BirthDate)
            .Must((request, birthDate) => IsAgeConsistentWithBirthDate(birthDate, request.Age))
            .WithMessage("Birth date and age are inconsistent");
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
}
```

### Test Example

```csharp
public class UserValidatorTests
{
    private readonly FakeTimeProvider _fakeTimeProvider;
    private readonly UserValidator _validator;

    public UserValidatorTests()
    {
        _fakeTimeProvider = new FakeTimeProvider();
        _fakeTimeProvider.SetUtcNow(new DateTime(2024, 1, 1));
        _validator = new UserValidator(_fakeTimeProvider);
    }

    [Fact]
    public void Validate_AgeAndBirthDateConsistent_ShouldPassValidation()
    {
        // Arrange
        var request = new UserRegistrationRequest
        {
            BirthDate = new DateTime(1990, 1, 1),
            Age = 34 // 2024 - 1990 = 34
        };

        // Act
        var result = _validator.TestValidate(request);

        // Assert
        result.ShouldNotHaveValidationErrorFor(x => x.BirthDate);
    }

    [Fact]
    public void Validate_AgeAndBirthDateInconsistent_ShouldFailValidation()
    {
        // Arrange
        var request = new UserRegistrationRequest
        {
            BirthDate = new DateTime(1990, 1, 1),
            Age = 25 // Wrong age
        };

        // Act
        var result = _validator.TestValidate(request);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.BirthDate)
              .WithErrorMessage("Birth date and age are inconsistent");
    }

    [Fact]
    public void Validate_BirthDateNotYetReached_AgeCalculationShouldBeCorrect()
    {
        // Arrange
        _fakeTimeProvider.SetUtcNow(new DateTime(2024, 2, 1));
        var validator = new UserValidator(_fakeTimeProvider);

        var request = new UserRegistrationRequest
        {
            BirthDate = new DateTime(1990, 6, 15), // Birthday not yet reached this year
            Age = 33 // 2024 - 1990 - 1 = 33
        };

        // Act
        var result = validator.TestValidate(request);

        // Assert
        result.ShouldNotHaveValidationErrorFor(x => x.BirthDate);
    }
}
```

---

## Pattern 5: Conditional Validation

### Validator Definition

```csharp
public class UserValidator : AbstractValidator<UserRegistrationRequest>
{
    public UserValidator()
    {
        // Phone number is optional, but if provided must be valid format
        RuleFor(x => x.PhoneNumber)
            .Matches(@"^09\d{8}$").WithMessage("Phone number format is incorrect")
            .When(x => !string.IsNullOrWhiteSpace(x.PhoneNumber));
    }
}
```

### Test Example

```csharp
[Fact]
public void Validate_EmptyPhoneNumber_ShouldSkipValidation()
{
    // Arrange
    var request = new UserRegistrationRequest { PhoneNumber = null };

    // Act
    var result = _validator.TestValidate(request);

    // Assert
    result.ShouldNotHaveValidationErrorFor(x => x.PhoneNumber);
}

[Fact]
public void Validate_InvalidPhoneNumberFormat_ShouldFailValidation()
{
    // Arrange
    var request = new UserRegistrationRequest { PhoneNumber = "123456789" };

    // Act
    var result = _validator.TestValidate(request);

    // Assert
    result.ShouldHaveValidationErrorFor(x => x.PhoneNumber)
          .WithErrorMessage("Phone number format is incorrect");
}

[Theory]
[InlineData("0912345678")]
[InlineData("0987654321")]
public void Validate_ValidPhoneNumber_ShouldPassValidation(string phoneNumber)
{
    // Arrange
    var request = new UserRegistrationRequest { PhoneNumber = phoneNumber };

    // Act
    var result = _validator.TestValidate(request);

    // Assert
    result.ShouldNotHaveValidationErrorFor(x => x.PhoneNumber);
}
```

---

## Pattern 6: Async Validation

### Validator Definition

```csharp
public interface IUserService
{
    Task<bool> IsUsernameAvailableAsync(string username);
    Task<bool> IsEmailRegisteredAsync(string email);
}

public class UserAsyncValidator : AbstractValidator<UserRegistrationRequest>
{
    private readonly IUserService _userService;

    public UserAsyncValidator(IUserService userService)
    {
        _userService = userService;

        RuleFor(x => x.Username)
            .MustAsync(async (username, cancellation) =>
                await _userService.IsUsernameAvailableAsync(username))
            .WithMessage("Username is already taken");

        RuleFor(x => x.Email)
            .MustAsync(async (email, cancellation) =>
                !await _userService.IsEmailRegisteredAsync(email))
            .WithMessage("This email is already registered");
    }
}
```

### Test Example

```csharp
public class UserAsyncValidatorTests
{
    private readonly IUserService _mockUserService;
    private readonly UserAsyncValidator _validator;

    public UserAsyncValidatorTests()
    {
        _mockUserService = Substitute.For<IUserService>();
        _validator = new UserAsyncValidator(_mockUserService);
    }

    [Fact]
    public async Task ValidateAsync_UsernameAvailable_ShouldPassValidation()
    {
        // Arrange
        var request = new UserRegistrationRequest { Username = "newuser123" };

        _mockUserService.IsUsernameAvailableAsync("newuser123")
                       .Returns(Task.FromResult(true));

        // Act
        var result = await _validator.TestValidateAsync(request);

        // Assert
        result.ShouldNotHaveValidationErrorFor(x => x.Username);
        await _mockUserService.Received(1).IsUsernameAvailableAsync("newuser123");
    }

    [Fact]
    public async Task ValidateAsync_UsernameAlreadyTaken_ShouldFailValidation()
    {
        // Arrange
        var request = new UserRegistrationRequest { Username = "existinguser" };

        _mockUserService.IsUsernameAvailableAsync("existinguser")
                       .Returns(Task.FromResult(false));

        // Act
        var result = await _validator.TestValidateAsync(request);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.Username)
              .WithErrorMessage("Username is already taken");
        await _mockUserService.Received(1).IsUsernameAvailableAsync("existinguser");
    }

    [Fact]
    public async Task ValidateAsync_ExternalServiceThrowsException_ShouldHandleCorrectly()
    {
        // Arrange
        var request = new UserRegistrationRequest { Username = "testuser" };

        _mockUserService.IsUsernameAvailableAsync("testuser")
                       .Returns(Task.FromException<bool>(new TimeoutException("Service timeout")));

        // Act & Assert
        await _validator.TestValidateAsync(request)
                       .Should().ThrowAsync<TimeoutException>();
    }
}
```

---

## Pattern 7: Collection Validation

### Validator Definition

```csharp
public class UserValidator : AbstractValidator<UserRegistrationRequest>
{
    public UserValidator()
    {
        RuleFor(x => x.Roles)
            .NotEmpty().WithMessage("Role list cannot be null or empty array")
            .Must(roles => roles == null || roles.All(role => IsValidRole(role)))
            .WithMessage("Contains invalid roles");
    }

    private bool IsValidRole(string role)
    {
        var validRoles = new[] { "User", "Admin", "Manager", "Support" };
        return validRoles.Contains(role);
    }
}
```

### Test Example

```csharp
[Fact]
public void Validate_EmptyRoleList_ShouldFailValidation()
{
    // Arrange
    var request = new UserRegistrationRequest { Roles = new List<string>() };

    // Act
    var result = _validator.TestValidate(request);

    // Assert
    result.ShouldHaveValidationErrorFor(x => x.Roles)
          .WithErrorMessage("Role list cannot be null or empty array");
}

[Theory]
[InlineData("InvalidRole")]
[InlineData("SuperUser")]
public void Validate_InvalidRole_ShouldFailValidation(string invalidRole)
{
    // Arrange
    var request = new UserRegistrationRequest
    {
        Roles = new List<string> { "User", invalidRole }
    };

    // Act
    var result = _validator.TestValidate(request);

    // Assert
    result.ShouldHaveValidationErrorFor(x => x.Roles)
          .WithErrorMessage("Contains invalid roles");
}

[Theory]
[InlineData(new[] { "User" })]
[InlineData(new[] { "Admin" })]
[InlineData(new[] { "User", "Manager" })]
public void Validate_ValidRole_ShouldPassValidation(string[] roles)
{
    // Arrange
    var request = new UserRegistrationRequest { Roles = roles.ToList() };

    // Act
    var result = _validator.TestValidate(request);

    // Assert
    result.ShouldNotHaveValidationErrorFor(x => x.Roles);
}
```
