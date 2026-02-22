using System;
using System.Collections.Generic;
using System.Linq;

namespace TestDataBuilderPattern.Examples
{
    // ===== Domain Models =====
    
    public class User
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string Email { get; set; }
        public int Age { get; set; }
        public string[] Roles { get; set; }
        public UserSettings Settings { get; set; }
        public bool IsActive { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime ModifiedAt { get; set; }
    }
    
    public class UserSettings
    {
        public string Theme { get; set; }
        public string Language { get; set; }
        public string[] FeatureFlags { get; set; }
    }
    
    // ===== User Builder Implementation =====
    
    public class UserBuilder
    {
        // Default values: Provide reasonable defaults for all properties
        private int _id = 1;
        private string _name = "Default User";
        private string _email = "default@example.com";
        private int _age = 25;
        private List<string> _roles = new();
        private UserSettings _settings = new() 
        { 
            Theme = "Light", 
            Language = "en-US",
            FeatureFlags = Array.Empty<string>()
        };
        private bool _isActive = true;
        private DateTime _createdAt = DateTime.UtcNow;
        private DateTime _modifiedAt = DateTime.UtcNow;
        
        // With* methods: Fluent interface for setting individual properties
        public UserBuilder WithId(int id)
        {
            _id = id;
            return this;
        }
        
        public UserBuilder WithName(string name)
        {
            _name = name;
            return this;
        }
        
        public UserBuilder WithEmail(string email)
        {
            _email = email;
            return this;
        }
        
        public UserBuilder WithAge(int age)
        {
            _age = age;
            return this;
        }
        
        public UserBuilder WithRole(string role)
        {
            _roles.Add(role);
            return this;
        }
        
        public UserBuilder WithRoles(params string[] roles)
        {
            _roles.AddRange(roles);
            return this;
        }
        
        public UserBuilder WithSettings(UserSettings settings)
        {
            _settings = settings;
            return this;
        }
        
        public UserBuilder IsInactive()
        {
            _isActive = false;
            return this;
        }
        
        public UserBuilder CreatedOn(DateTime createdAt)
        {
            _createdAt = createdAt;
            _modifiedAt = createdAt;
            return this;
        }
        
        public UserBuilder ModifiedOn(DateTime modifiedAt)
        {
            _modifiedAt = modifiedAt;
            return this;
        }
        
        // Semantic default creators: Provide quick creation methods for common scenarios
        public static UserBuilder AUser() => new();
        
        public static UserBuilder AnAdminUser() => new UserBuilder()
            .WithRoles("Admin", "User");
        
        public static UserBuilder ARegularUser() => new UserBuilder()
            .WithRole("User");
        
        public static UserBuilder AnInactiveUser() => new UserBuilder()
            .IsInactive();
        
        public static UserBuilder APremiumUser() => new UserBuilder()
            .WithRoles("Premium", "User")
            .WithSettings(new UserSettings 
            { 
                Theme = "Dark", 
                Language = "en-US",
                FeatureFlags = new[] { "AdvancedSearch", "PrioritySupport" }
            });
        
        // Semantic composition methods
        public UserBuilder WithValidEmail()
        {
            _email = $"{_name.Replace(" ", ".").ToLower()}@example.com";
            return this;
        }
        
        public UserBuilder WithAdminRights()
        {
            return WithRoles("Admin", "User");
        }
        
        public UserBuilder WithDarkTheme()
        {
            _settings.Theme = "Dark";
            return this;
        }
        
        // Build method: Create the final object
        public User Build()
        {
            return new User
            {
                Id = _id,
                Name = _name,
                Email = _email,
                Age = _age,
                Roles = _roles.ToArray(),
                Settings = _settings,
                IsActive = _isActive,
                CreatedAt = _createdAt,
                ModifiedAt = _modifiedAt
            };
        }
    }
    
    // ===== Usage Examples =====
    
    public class UserBuilderExamples
    {
        public void Example1_BasicUsage()
        {
            // Basic usage: Create a user with default values
            var user = UserBuilder.AUser().Build();
            
            Console.WriteLine($"User: {user.Name}, Email: {user.Email}");
            // Output: User: Default User, Email: default@example.com
        }
        
        public void Example2_CustomizeProperties()
        {
            // Customize properties: Only modify needed properties
            var user = UserBuilder.AUser()
                .WithName("John Doe")
                .WithEmail("john.doe@company.com")
                .WithAge(30)
                .Build();
            
            Console.WriteLine($"User: {user.Name}, Age: {user.Age}");
            // Output: User: John Doe, Age: 30
        }
        
        public void Example3_PresetScenarios()
        {
            // Use preset scenarios: Quickly create specific types of users
            var adminUser = UserBuilder.AnAdminUser()
                .WithName("Admin Smith")
                .Build();
            
            Console.WriteLine($"Roles: {string.Join(", ", adminUser.Roles)}");
            // Output: Roles: Admin, User
            
            var premiumUser = UserBuilder.APremiumUser()
                .WithName("Premium Jones")
                .Build();
            
            Console.WriteLine($"Features: {string.Join(", ", premiumUser.Settings.FeatureFlags)}");
            // Output: Features: AdvancedSearch, PrioritySupport
        }
        
        public void Example4_FluentChaining()
        {
            // Fluent interface chaining: Multiple settings chained together
            var user = UserBuilder.AUser()
                .WithName("Alice")
                .WithValidEmail()  // Auto-generate email based on name
                .WithAge(28)
                .WithRole("Manager")
                .WithDarkTheme()
                .Build();
            
            Console.WriteLine($"Email: {user.Email}, Theme: {user.Settings.Theme}");
            // Output: Email: alice@example.com, Theme: Dark
        }
        
        public void Example5_InvalidScenarios()
        {
            // Create invalid data for testing validation logic
            var userWithEmptyName = UserBuilder.AUser()
                .WithName("")
                .Build();
            
            var userWithInvalidEmail = UserBuilder.AUser()
                .WithEmail("invalid-email")
                .Build();
            
            var tooYoungUser = UserBuilder.AUser()
                .WithAge(10)
                .Build();
            
            // These objects can be used to test validator error handling
        }
    }
}
