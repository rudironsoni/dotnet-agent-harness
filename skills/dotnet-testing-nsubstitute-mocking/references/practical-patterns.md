# NSubstitute Practical Patterns

> This document is extracted from [SKILL.md](../SKILL.md) and contains complete practical code examples.

## Pattern 1: Dependency Injection and Test Setup

### Class Under Test

```csharp
public class FileBackupService
{
    private readonly IFileSystem _fileSystem;
    private readonly IDateTimeProvider _dateTimeProvider;
    private readonly IBackupRepository _backupRepository;
    private readonly ILogger<FileBackupService> _logger;
    
    public FileBackupService(
        IFileSystem fileSystem,
        IDateTimeProvider dateTimeProvider,
        IBackupRepository backupRepository,
        ILogger<FileBackupService> logger)
    {
        _fileSystem = fileSystem;
        _dateTimeProvider = dateTimeProvider;
        _backupRepository = backupRepository;
        _logger = logger;
    }
    
    public async Task<BackupResult> BackupFileAsync(string sourcePath, string destinationPath)
    {
        if (!_fileSystem.FileExists(sourcePath))
        {
            _logger.LogWarning("Source file not found: {Path}", sourcePath);
            return new BackupResult { Success = false, Message = "Source file not found" };
        }
        
        var fileInfo = _fileSystem.GetFileInfo(sourcePath);
        if (fileInfo.Length > 100 * 1024 * 1024)
        {
            return new BackupResult { Success = false, Message = "File too large" };
        }
        
        var timestamp = _dateTimeProvider.Now.ToString("yyyyMMdd_HHmmss");
        var backupFileName = $"{Path.GetFileNameWithoutExtension(sourcePath)}_{timestamp}{Path.GetExtension(sourcePath)}";
        var fullBackupPath = Path.Combine(destinationPath, backupFileName);
        
        _fileSystem.CopyFile(sourcePath, fullBackupPath);
        await _backupRepository.SaveBackupHistory(sourcePath, fullBackupPath, _dateTimeProvider.Now);
        
        _logger.LogInformation("Backup completed: {Path}", fullBackupPath);
        
        return new BackupResult { Success = true, BackupPath = fullBackupPath };
    }
}
```

### Test Class Setup

```csharp
public class FileBackupServiceTests
{
    private readonly IFileSystem _fileSystem;
    private readonly IDateTimeProvider _dateTimeProvider;
    private readonly IBackupRepository _backupRepository;
    private readonly ILogger<FileBackupService> _logger;
    private readonly FileBackupService _sut; // System Under Test
    
    public FileBackupServiceTests()
    {
        _fileSystem = Substitute.For<IFileSystem>();
        _dateTimeProvider = Substitute.For<IDateTimeProvider>();
        _backupRepository = Substitute.For<IBackupRepository>();
        _logger = Substitute.For<ILogger<FileBackupService>>();
        
        _sut = new FileBackupService(_fileSystem, _dateTimeProvider, _backupRepository, _logger);
    }
    
    [Fact]
    public async Task BackupFileAsync_FileExistsAndSizeReasonable_ShouldBackupSuccessfully()
    {
        // Arrange
        var sourcePath = @"C:\source\test.txt";
        var destinationPath = @"C:\backup";
        var testTime = new DateTime(2024, 1, 1, 12, 0, 0);
        
        _fileSystem.FileExists(sourcePath).Returns(true);
        _fileSystem.GetFileInfo(sourcePath).Returns(new FileInfo { Length = 1024 });
        _dateTimeProvider.Now.Returns(testTime);
        
        // Act
        var result = await _sut.BackupFileAsync(sourcePath, destinationPath);
        
        // Assert
        result.Success.Should().BeTrue();
        result.BackupPath.Should().Be(@"C:\backup\test_20240101_120000.txt");
        
        _fileSystem.Received(1).CopyFile(sourcePath, result.BackupPath);
        await _backupRepository.Received(1).SaveBackupHistory(
            sourcePath, result.BackupPath, testTime);
    }
}
```

## Pattern 2: Mock vs Stub in Practice

### Stub: Focus on State

```csharp
[Fact]
public void CalculateDiscount_PremiumMember_ShouldReturn20PercentDiscount()
{
    // Stub: Only care about return value, used to set up test scenario
    var stubCustomerService = Substitute.For<ICustomerService>();
    stubCustomerService.GetCustomerType(123).Returns(CustomerType.Premium);
    
    var service = new PricingService(stubCustomerService);
    
    // Act
    var discount = service.CalculateDiscount(123, 1000);
    
    // Assert - Only verify result state
    discount.Should().Be(200); // 20% of 1000
}
```

### Mock: Focus on Behavior

```csharp
[Fact]
public void ProcessPayment_SuccessfulPayment_ShouldRecordTransactionInfo()
{
    // Mock: Care about correct interaction
    var mockLogger = Substitute.For<ILogger<PaymentService>>();
    var stubPaymentGateway = Substitute.For<IPaymentGateway>();
    stubPaymentGateway.ProcessPayment(Arg.Any<decimal>()).Returns(PaymentResult.Success);
    
    var service = new PaymentService(stubPaymentGateway, mockLogger);
    
    // Act
    service.ProcessPayment(100);
    
    // Assert - Verify interaction behavior
    mockLogger.Received(1).LogInformation(
        "Payment processed: {Amount} - Result: {Result}", 
        100, 
        PaymentResult.Success);
}
```

## Pattern 3: Async Method Testing

```csharp
[Fact]
public async Task GetUserAsync_UserExists_ShouldReturnUserData()
{
    // Arrange
    var repository = Substitute.For<IUserRepository>();
    repository.GetByIdAsync(123).Returns(Task.FromResult(
        new User { Id = 123, Name = "John" }));
    
    var service = new UserService(repository);
    
    // Act
    var result = await service.GetUserAsync(123);
    
    // Assert
    result.Name.Should().Be("John");
    await repository.Received(1).GetByIdAsync(123);
}

[Fact]
public async Task SaveUserAsync_DatabaseError_ShouldThrowException()
{
    // Arrange
    var repository = Substitute.For<IUserRepository>();
    repository.SaveAsync(Arg.Any<User>())
              .Throws(new InvalidOperationException("Database error"));
    
    var service = new UserService(repository);
    
    // Act & Assert
    await service.SaveUserAsync(new User { Name = "John" })
                .Should().ThrowAsync<InvalidOperationException>()
                .WithMessage("Database error");
}
```

## Pattern 4: ILogger Validation

Due to the extension method nature of ILogger, need to verify the underlying Log method:

```csharp
[Fact]
public async Task BackupFileAsync_FileNotExists_ShouldLogWarning()
{
    // Arrange
    var sourcePath = @"C:\nonexistent\test.txt";
    _fileSystem.FileExists(sourcePath).Returns(false);
    
    // Act
    var result = await _sut.BackupFileAsync(sourcePath, @"C:\backup");
    
    // Assert
    result.Success.Should().BeFalse();
    
    // Verify ILogger.Log method was called correctly
    _logger.Received(1).Log(
        LogLevel.Warning,
        Arg.Any<EventId>(),
        Arg.Is<object>(v => v.ToString().Contains("Source file not found")),
        null,
        Arg.Any<Func<object, Exception, string>>());
}
```

## Pattern 5: Complex Setup Management

Use base test class to manage shared setup:

```csharp
public class OrderServiceTestsBase
{
    protected readonly IOrderRepository Repository;
    protected readonly IEmailService EmailService;
    protected readonly ILogger<OrderService> Logger;
    protected readonly OrderService Sut;
    
    protected OrderServiceTestsBase()
    {
        Repository = Substitute.For<IOrderRepository>();
        EmailService = Substitute.For<IEmailService>();
        Logger = Substitute.For<ILogger<OrderService>>();
        Sut = new OrderService(Repository, EmailService, Logger);
    }
    
    protected void SetupValidOrder(int orderId = 1)
    {
        Repository.GetById(orderId).Returns(
            new Order { Id = orderId, Status = OrderStatus.Pending });
    }
    
    protected void SetupEmailServiceSuccess()
    {
        EmailService.SendConfirmation(Arg.Any<string>()).Returns(true);
    }
}

public class OrderServiceTests : OrderServiceTestsBase
{
    [Fact]
    public void ProcessOrder_ValidOrder_ShouldProcessSuccessfully()
    {
        // Arrange
        SetupValidOrder();
        SetupEmailServiceSuccess();
        
        // Act
        var result = Sut.ProcessOrder(1);
        
        // Assert
        result.Success.Should().BeTrue();
    }
}
```
