using System;
using System.Collections.Generic;
using System.Linq;
using Xunit;

namespace TestDataBuilderPattern.TheoryExamples
{
    // ===== Domain Models =====
    
    public class Customer
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string Email { get; set; }
        public CustomerType Type { get; set; }
        public decimal CreditLimit { get; set; }
        public bool IsVerified { get; set; }
    }
    
    public enum CustomerType
    {
        Regular,
        Premium,
        VIP
    }
    
    // ===== Customer Builder =====
    
    public class CustomerBuilder
    {
        private int _id = 1;
        private string _name = "Default Customer";
        private string _email = "customer@example.com";
        private CustomerType _type = CustomerType.Regular;
        private decimal _creditLimit = 1000m;
        private bool _isVerified = true;
        
        public CustomerBuilder WithId(int id)
        {
            _id = id;
            return this;
        }
        
        public CustomerBuilder WithName(string name)
        {
            _name = name;
            return this;
        }
        
        public CustomerBuilder WithEmail(string email)
        {
            _email = email;
            return this;
        }
        
        public CustomerBuilder OfType(CustomerType type)
        {
            _type = type;
            return this;
        }
        
        public CustomerBuilder WithCreditLimit(decimal limit)
        {
            _creditLimit = limit;
            return this;
        }
        
        public CustomerBuilder Unverified()
        {
            _isVerified = false;
            return this;
        }
        
        // Default creators
        public static CustomerBuilder ACustomer() => new();
        
        public static CustomerBuilder ARegularCustomer() => new CustomerBuilder()
            .OfType(CustomerType.Regular)
            .WithCreditLimit(1000m);
        
        public static CustomerBuilder APremiumCustomer() => new CustomerBuilder()
            .OfType(CustomerType.Premium)
            .WithCreditLimit(5000m);
        
        public static CustomerBuilder AVIPCustomer() => new CustomerBuilder()
            .OfType(CustomerType.VIP)
            .WithCreditLimit(10000m);
        
        public Customer Build()
        {
            return new Customer
            {
                Id = _id,
                Name = _name,
                Email = _email,
                Type = _type,
                CreditLimit = _creditLimit,
                IsVerified = _isVerified
            };
        }
    }
    
    // ===== Builder with xUnit Theory Tests =====
    
    public class CustomerServiceTests
    {
        // Example 1: Using MemberData with Builder to test different customer types
        [Theory]
        [MemberData(nameof(GetCustomerTypeScenarios))]
        public void CalculateDiscount_DifferentCustomerTypes_ShouldReturnCorrectDiscount(Customer customer, decimal expectedDiscount)
        {
            // Arrange
            var service = new CustomerService();
            
            // Act
            var discount = service.CalculateDiscount(customer);
            
            // Assert
            Assert.Equal(expectedDiscount, discount);
        }
        
        public static IEnumerable<object[]> GetCustomerTypeScenarios()
        {
            // Regular customer: No discount
            yield return new object[]
            {
                CustomerBuilder.ARegularCustomer()
                    .WithName("Regular John")
                    .Build(),
                0m
            };
            
            // Premium customer: 5% discount
            yield return new object[]
            {
                CustomerBuilder.APremiumCustomer()
                    .WithName("Premium Jane")
                    .Build(),
                0.05m
            };
            
            // VIP customer: 10% discount
            yield return new object[]
            {
                CustomerBuilder.AVIPCustomer()
                    .WithName("VIP Alice")
                    .Build(),
                0.10m
            };
        }
        
        // Example 2: Testing customer validation logic
        [Theory]
        [MemberData(nameof(GetCustomerValidationScenarios))]
        public void ValidateCustomer_DifferentScenarios_ShouldReturnCorrectValidationResult(Customer customer, bool expectedValid, string description)
        {
            // Arrange
            var validator = new CustomerValidator();
            
            // Act
            var isValid = validator.IsValid(customer);
            
            // Assert
            Assert.Equal(expectedValid, isValid);
        }
        
        public static IEnumerable<object[]> GetCustomerValidationScenarios()
        {
            // Valid customer
            yield return new object[]
            {
                CustomerBuilder.ACustomer()
                    .WithName("Valid Customer")
                    .WithEmail("valid@example.com")
                    .Build(),
                true,
                "Valid regular customer"
            };
            
            // Invalid customer - empty name
            yield return new object[]
            {
                CustomerBuilder.ACustomer()
                    .WithName("")
                    .WithEmail("test@example.com")
                    .Build(),
                false,
                "Name is empty"
            };
            
            // Invalid customer - invalid email
            yield return new object[]
            {
                CustomerBuilder.ACustomer()
                    .WithName("Test User")
                    .WithEmail("invalid-email")
                    .Build(),
                false,
                "Email format is invalid"
            };
            
            // Invalid customer - unverified
            yield return new object[]
            {
                CustomerBuilder.ACustomer()
                    .WithName("Unverified User")
                    .WithEmail("unverified@example.com")
                    .Unverified()
                    .Build(),
                false,
                "Customer is unverified"
            };
            
            // Invalid customer - negative credit limit
            yield return new object[]
            {
                CustomerBuilder.ACustomer()
                    .WithName("Negative Credit")
                    .WithCreditLimit(-100m)
                    .Build(),
                false,
                "Credit limit is negative"
            };
        }
        
        // Example 3: Testing credit limit approval logic
        [Theory]
        [MemberData(nameof(GetCreditApprovalScenarios))]
        public void ApproveCredit_DifferentLimitsAndCustomerTypes_ShouldReturnCorrectApprovalResult(
            Customer customer, 
            decimal requestAmount, 
            bool expectedApproved)
        {
            // Arrange
            var service = new CustomerService();
            
            // Act
            var approved = service.ApproveCreditRequest(customer, requestAmount);
            
            // Assert
            Assert.Equal(expectedApproved, approved);
        }
        
        public static IEnumerable<object[]> GetCreditApprovalScenarios()
        {
            // Regular customer - request amount within limit
            yield return new object[]
            {
                CustomerBuilder.ARegularCustomer().Build(),
                500m,
                true
            };
            
            // Regular customer - request amount exceeds limit
            yield return new object[]
            {
                CustomerBuilder.ARegularCustomer().Build(),
                1500m,
                false
            };
            
            // Premium customer - request higher amount
            yield return new object[]
            {
                CustomerBuilder.APremiumCustomer().Build(),
                4000m,
                true
            };
            
            // Premium customer - request amount exceeds limit
            yield return new object[]
            {
                CustomerBuilder.APremiumCustomer().Build(),
                6000m,
                false
            };
            
            // VIP customer - request high amount
            yield return new object[]
            {
                CustomerBuilder.AVIPCustomer().Build(),
                9000m,
                true
            };
            
            // Unverified customer - reject any request
            yield return new object[]
            {
                CustomerBuilder.ACustomer()
                    .Unverified()
                    .Build(),
                100m,
                false
            };
        }
        
        // Example 4: Using ClassData with Builder
        [Theory]
        [ClassData(typeof(CustomerUpgradeTestData))]
        public void UpgradeCustomer_WhenCriteriaMet_ShouldUpgradeCustomerLevel(
            Customer customer, 
            CustomerType expectedType)
        {
            // Arrange
            var service = new CustomerService();
            
            // Act
            var upgradedCustomer = service.UpgradeCustomer(customer);
            
            // Assert
            Assert.Equal(expectedType, upgradedCustomer.Type);
        }
    }
    
    // ===== ClassData Implementation with Builder =====
    
    public class CustomerUpgradeTestData : TheoryData<Customer, CustomerType>
    {
        public CustomerUpgradeTestData()
        {
            // Regular -> Premium upgrade condition: credit limit >= 2000
            Add(
                CustomerBuilder.ARegularCustomer()
                    .WithCreditLimit(2000m)
                    .Build(),
                CustomerType.Premium
            );
            
            // Premium -> VIP upgrade condition: credit limit >= 7000
            Add(
                CustomerBuilder.APremiumCustomer()
                    .WithCreditLimit(7000m)
                    .Build(),
                CustomerType.VIP
            );
            
            // Does not meet upgrade condition: maintain current level
            Add(
                CustomerBuilder.ARegularCustomer()
                    .WithCreditLimit(1000m)
                    .Build(),
                CustomerType.Regular
            );
        }
    }
    
    // ===== Mock Services (for demonstration) =====
    
    public class CustomerService
    {
        public decimal CalculateDiscount(Customer customer)
        {
            return customer.Type switch
            {
                CustomerType.Regular => 0m,
                CustomerType.Premium => 0.05m,
                CustomerType.VIP => 0.10m,
                _ => 0m
            };
        }
        
        public bool ApproveCreditRequest(Customer customer, decimal amount)
        {
            if (!customer.IsVerified)
                return false;
            
            return amount <= customer.CreditLimit;
        }
        
        public Customer UpgradeCustomer(Customer customer)
        {
            var newType = customer.Type;
            
            if (customer.Type == CustomerType.Regular && customer.CreditLimit >= 2000m)
                newType = CustomerType.Premium;
            else if (customer.Type == CustomerType.Premium && customer.CreditLimit >= 7000m)
                newType = CustomerType.VIP;
            
            return new Customer
            {
                Id = customer.Id,
                Name = customer.Name,
                Email = customer.Email,
                Type = newType,
                CreditLimit = customer.CreditLimit,
                IsVerified = customer.IsVerified
            };
        }
    }
    
    public class CustomerValidator
    {
        public bool IsValid(Customer customer)
        {
            if (string.IsNullOrWhiteSpace(customer.Name))
                return false;
            
            if (!IsValidEmail(customer.Email))
                return false;
            
            if (!customer.IsVerified)
                return false;
            
            if (customer.CreditLimit < 0)
                return false;
            
            return true;
        }
        
        private bool IsValidEmail(string email)
        {
            return !string.IsNullOrWhiteSpace(email) && email.Contains("@");
        }
    }
}
