# Test Naming Examples

This document collects naming examples for various test scenarios, for reference and copying.

---

## 1. Basic Calculation Tests

### Calculator Related

```csharp
// Addition
Add_Input1And2_ShouldReturn3
Add_InputNegativeAndPositive_ShouldReturnCorrectResult
Add_Input0And0_ShouldReturn0
Add_InputVariousNumberCombinations_ShouldReturnCorrectResult

// Division
Divide_Input10And2_ShouldReturn5
Divide_Input10And0_ShouldThrowDivideByZeroException
Divide_InputVariousValidNumbers_ShouldReturnCorrectResult

// Multiplication
Multiply_Input3And4_ShouldReturn12
Multiply_InputVariousNumberCombinations_ShouldReturnCorrectResult
```

---

## 2. Validation Tests

### Email Validation

```csharp
// Valid input
IsValidEmail_InputValidEmail_ShouldReturnTrue
IsValidEmail_InputValidEmailFormat_ShouldReturnTrue

// Invalid input
IsValidEmail_InputNull_ShouldReturnFalse
IsValidEmail_InputEmptyString_ShouldReturnFalse
IsValidEmail_InputWhitespaceOnly_ShouldReturnFalse
IsValidEmail_InputInvalidEmailFormat_ShouldReturnFalse

// Domain extraction
GetDomain_InputValidEmail_ShouldReturnDomainName
GetDomain_InputInvalidEmail_ShouldReturnNull
GetDomain_InputNull_ShouldReturnNull
GetDomain_InputVariousValidEmails_ShouldReturnCorrespondingDomain
```

### Password Validation

```csharp
IsValidPassword_InputPasswordMeetingRules_ShouldReturnTrue
IsValidPassword_InputLessThan8Characters_ShouldReturnFalse
IsValidPassword_InputNoUppercaseLetter_ShouldReturnFalse
IsValidPassword_InputNoNumber_ShouldReturnFalse
```

---

## 3. Business Logic Tests

### Order Related

```csharp
// Process order
ProcessOrder_InputValidOrder_ShouldReturnProcessedOrder
ProcessOrder_InputNull_ShouldThrowArgumentNullException
ProcessOrder_CalledMultipleTimes_ShouldReturnNewObjectInstanceEachTime

// Order number
GetOrderNumber_InputValidOrder_ShouldReturnFormattedOrderNumber
GetOrderNumber_InputNull_ShouldThrowArgumentNullException
GetOrderNumber_InputVariousPrefixAndNumberCombinations_ShouldReturnCorrectFormat
```

### Price Calculation

```csharp
// Discount calculation
Calculate_Input100And10PercentDiscount_ShouldReturn90
Calculate_InputNegativePrice_ShouldThrowArgumentException
Calculate_InputInvalidDiscountRate_ShouldThrowArgumentException
Calculate_InputVariousValidCombinations_ShouldReturnCorrectResult
Calculate_Input0Price_ShouldHandleNormally

// Tax calculation
CalculateWithTax_Input100And5PercentTax_ShouldReturn105
CalculateWithTax_InputNegativePrice_ShouldThrowArgumentException
CalculateWithTax_InputNegativeTaxRate_ShouldThrowArgumentException
CalculateWithTax_InputVariousValidCombinations_ShouldReturnCorrectResult
CalculateWithTax_Input0Price_ShouldHandleNormally
```

---

## 4. State Change Tests

### Counter Related

```csharp
// Increment
Increment_From0_ShouldReturn1
Increment_From0Twice_ShouldReturn2
Increment_ExecutedMultipleTimes_ShouldProduceConsistentResults

// Decrement
Decrement_From0_ShouldReturnNegative1
Decrement_FromPositiveNumber_ShouldDecreaseCorrectly

// Reset
Reset_FromAnyValue_ShouldReturn0

// Set value
SetValue_InputAnyValue_ShouldSetCorrectNumber
```

---

## 5. Collection Operation Tests

```csharp
// Add
Add_AddItem_CollectionShouldContainItem
Add_AddDuplicateItem_ShouldThrowInvalidOperationException

// Remove
Remove_RemoveExistingItem_ShouldReturnTrue
Remove_RemoveNonExistingItem_ShouldReturnFalse

// Query
Find_InputExistingId_ShouldReturnCorrespondingItem
Find_InputNonExistingId_ShouldReturnNull
FindAll_InputCondition_ShouldReturnAllMatchingItems

// Count
Count_CollectionEmpty_ShouldReturn0
Count_CollectionHas3Items_ShouldReturn3
```

---

## 6. Async Operation Tests

```csharp
// Get data
GetAsync_InputValidId_ShouldReturnCorrespondingData
GetAsync_InputNonExistingId_ShouldReturnNull
GetAllAsync_WhenNoData_ShouldReturnEmptyCollection

// Save data
SaveAsync_InputValidEntity_ShouldSaveSuccessfully
SaveAsync_InputNull_ShouldThrowArgumentNullException

// Delete data
DeleteAsync_InputExistingId_ShouldDeleteSuccessfully
DeleteAsync_InputNonExistingId_ShouldReturnFalse
```

---

## 7. Exception Handling Test Naming

```csharp
// ArgumentNullException
MethodName_InputNull_ShouldThrowArgumentNullException

// ArgumentException
MethodName_InputInvalidParameter_ShouldThrowArgumentException
MethodName_InputNegativeNumber_ShouldThrowArgumentException

// InvalidOperationException
MethodName_CalledWhenStateIncorrect_ShouldThrowInvalidOperationException

// Custom exception
MethodName_BusinessRuleViolated_ShouldThrowBusinessRuleException

// Verify exception message
MethodName_InputInvalid_ShouldThrowExceptionWithCorrectMessage
```

---

## 8. Theory Test Naming

```csharp
// Multiple valid inputs
MethodName_InputVariousValidValues_ShouldReturnCorrectResult
MethodName_InputVariousValidCombinations_ShouldHandleNormally

// Multiple invalid inputs
MethodName_InputVariousInvalidValues_ShouldThrowException
MethodName_InputVariousInvalidFormats_ShouldReturnFalse

// Correspondence tests
MethodName_InputVariousAValues_ShouldReturnCorrespondingBValue
GetDomain_InputVariousValidEmails_ShouldReturnCorrespondingDomain
```

---

## Naming Templates

Copy the following templates, replace `{placeholder}` to use:

```csharp
// Happy path
{Method}_Input{ValidInput}_ShouldReturn{ExpectedResult}

// Null input
{Method}_InputNull_ShouldThrowArgumentNullException

// Empty input
{Method}_InputEmptyString_ShouldReturn{ExpectedResult}

// Boundary condition
{Method}_Input{BoundaryValue}_Should{ExpectedBehavior}

// Exception case
{Method}_Input{InvalidInput}_ShouldThrow{ExceptionType}

// State change
{Method}_From{InitialState}_Should{ExpectedState}

// Theory test
{Method}_InputVarious{InputType}_ShouldReturn{ExpectedPattern}
```
