# Prompt for Generating Unit & Integration Tests for WebApiShop

## Framework & Tools
- Use xUnit with [Fact] attributes for test methods
- Use Moq 4.20.72 for mocking dependencies
- Use Moq.EntityFrameworkCore for mocking DbSet operations
- Async/await for all async operations

## Naming Convention
Follow the pattern: `MethodName_Scenario_ExpectedResult`
Examples:
- GetUsers_ReturnsAllUsersWithOrders
- GetUserById_ReturnsNull_WhenIdDoesNotExist
- AddOrder_ReturnsNull_WhenSumIsIncorrect

## Unit Tests (Repository Level)
Structure:
1. Mock the ApiDBContext
2. Use `.Setup(x => x.DbSet).ReturnsDbSet(testData)` for Moq.EntityFrameworkCore
3. Use `.FindAsync()` setup for single entity lookups
4. Follow AAA Pattern (Arrange, Act, Assert)
5. Test both happy paths and edge cases (empty lists, null, duplicates)

Example Pattern:
```csharp
[Fact]
public async Task GetUsers_ReturnsAllUsersWithOrders()
{
    // Arrange
    var users = new List<User> { /* test data */ };
    var mockContext = new Mock<ApiDBContext>();
    mockContext.Setup(x => x.Users).ReturnsDbSet(users);
    var repository = new UserRepository(mockContext.Object);

    // Act
    var result = await repository.GetUsers();

    // Assert
    Assert.NotNull(result);
    Assert.Equal(2, result.Count());
}
```

## Unit Tests (Service Level)
Structure:
1. Mock IRepository dependencies
2. Mock IMapper for DTO transformations
3. Mock ILogger<T> for logging
4. Verify method calls with `.Verify()`
5. Test business logic validation

Example Pattern:
```csharp
[Fact]
public async Task AddOrder_ReturnsOrderDTO_WhenSumIsCorrect()
{
    // Arrange
    var mockOrderRepo = new Mock<IOrderRepository>();
    var mockProductRepo = new Mock<IProductRepository>();
    var mockMapper = new Mock<IMapper>();
    var mockLogger = new Mock<ILogger<OrderService>>();

    // Setup mocks
    mockProductRepo.Setup(x => x.GetProductById(1)).ReturnsAsync(new Product { Id = 1, Price = 50.0 });
    mockMapper.Setup(m => m.Map<OrderDTO, Order>(It.IsAny<OrderDTO>())).Returns(new Order());

    var service = new OrderService(mockOrderRepo.Object, mockProductRepo.Object, mockMapper.Object, mockLogger.Object);

    // Act
    var result = await service.AddOrder(orderDto);

    // Assert
    Assert.NotNull(result);
    mockOrderRepo.Verify(x => x.AddOrder(It.IsAny<Order>()), Times.Once);
}
```

## Integration Tests
Structure:
1. Inherit from `IDisposable` for cleanup
2. Use `DatabaseFixture` with real SQL Server (creates unique test DB)
3. Add/update actual data in database
4. Test real repository behavior against database
5. Clean up with Dispose method

Example Pattern:
```csharp
public class UserRepositoryIntegrationTests : IDisposable
{
    private readonly DatabaseFixture _fixture;
    private readonly ApiDBContext _dbContext;
    private readonly UserRepository _userRepository;

    public UserRepositoryIntegrationTests()
    {
        _fixture = new DatabaseFixture();
        _dbContext = _fixture.Context;
        _userRepository = new UserRepository(_dbContext);
    }

    public void Dispose()
    {
        _fixture.Dispose();
    }

    [Fact]
    public async Task UpdateUser_ActuallyPersistsChangesInDatabase()
    {
        // Arrange
        var user = new User { FirstName = "Before", Email = "before@test.com", Password = "123" };
        await _dbContext.Users.AddAsync(user);
        await _dbContext.SaveChangesAsync();

        // Act
        user.FirstName = "After";
        await _userRepository.UpdateUser(user);

        // Assert
        var updated = await _userRepository.GetUserById(user.Id);
        Assert.Equal("After", updated.FirstName);
    }
}
```

## Test Coverage Areas
For each Repository:
- GetAll() - returns data, returns empty
- GetById() - returns valid item, returns null
- Add() - adds successfully, handles duplicates
- Update() - updates data, persists changes
- Delete() - removes item, handles non-existent

For each Service:
- Business logic validation (sums, passwords, uniqueness)
- Correct DTO mapping
- Repository method calls verified
- Error handling (null returns, invalid data)

## Key Conventions
- Use `IEnumerable<T>` for GetAll results
- Test with `DateOnly.FromDateTime(DateTime.Now)` for date fields
- Use record-based DTOs (if applicable)
- Verify async method calls: `.Verify(x => x.Method(It.IsAny<T>()), Times.Once)`
- Always await async operations
- Use `Assert.Equal()`, `Assert.NotNull()`, `Assert.Empty()`, etc.
- Include multiple test cases per method (happy path + edge cases)
