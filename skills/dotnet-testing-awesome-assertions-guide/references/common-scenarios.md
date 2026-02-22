# Common Scenarios and Solutions

> This file is extracted from [SKILL.md](../SKILL.md), providing assertion examples for various common testing scenarios.

---

## Scenario 1: API Response Validation

```csharp
[Fact]
public void API_UserData_ShouldMatchSpecification()
{
    var response = apiClient.GetUserProfile(userId);
    
    response.StatusCode.Should().Be(200);
    response.Content.Should().NotBeNullOrEmpty();
    
    var user = JsonSerializer.Deserialize<User>(response.Content);
    
    user.Should().BeEquivalentTo(new
    {
        Id = userId,
        Email = expectedEmail
    }, options => options
        .Including(u => u.Id)
        .Including(u => u.Email)
    );
}
```

## Scenario 2: Database Entity Validation

```csharp
[Fact]
public void Database_SaveEntity_ShouldPersistCorrectly()
{
    var user = new User 
    { 
        Name = "John", 
        Email = "john@example.com" 
    };
    
    dbContext.Users.Add(user);
    dbContext.SaveChanges();
    
    var saved = dbContext.Users.Find(user.Id);
    
    saved.Should().BeEquivalentTo(user, options => options
        .Excluding(u => u.CreatedAt)
        .Excluding(u => u.UpdatedAt)
        .Excluding(u => u.RowVersion)
    );
}
```

## Scenario 3: Event Validation

```csharp
[Fact]
public void Event_PublishEvent_ShouldContainCorrectData()
{
    var eventRaised = false;
    OrderCreatedEvent? capturedEvent = null;
    
    eventBus.Subscribe<OrderCreatedEvent>(e => 
    {
        eventRaised = true;
        capturedEvent = e;
    });
    
    orderService.CreateOrder(orderRequest);
    
    eventRaised.Should().BeTrue("Order creation should raise event");
    capturedEvent.Should().NotBeNull();
    capturedEvent!.OrderId.Should().BeGreaterThan(0);
    capturedEvent.TotalAmount.Should().Be(expectedAmount);
}
```
