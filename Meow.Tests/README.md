# Meow.Tests - Unit Testing Project

## Overview
This is a comprehensive unit testing project for the Meow .NET MAUI application using xUnit, Moq, and FluentAssertions.

## Test Framework
- **xUnit** 2.9.3 - Modern testing framework for .NET
- **Moq** 4.20.72 - Mocking framework for creating test doubles
- **FluentAssertions** 7.0.0 - Fluent API for assertions
- **CommunityToolkit.Mvvm** 8.4.0 - MVVM toolkit for ViewModels

## Project Structure
```
Meow.Tests/
├── ViewModels/
│   ├── BaseViewModelTests.cs      - Tests for base ViewModel functionality
│   ├── VoteViewModelTests.cs      - Tests for voting functionality
│   ├── BreedsViewModelTests.cs    - Tests for breeds functionality
│   └── FavoriteViewModelTests.cs  - Tests for favorites management
├── GlobalUsings.cs                - Global using directives
└── Meow.Tests.csproj             - Project configuration
```

## Test Coverage

### BaseViewModelTests
- Property change notifications (INotifyPropertyChanged)
- Theme selection command
- Observable properties (Title, IsBusy, Progress)

### VoteViewModelTests
- Cat loading and initialization
- Favorite management (add/remove)
- Animation control
- Error handling for null/empty data
- UI state management (ImageHeart icons, IsAnimation)

### BreedsViewModelTests
- Breeds loading from cache
- Breed selection
- Cats filtered by breed
- Loading states
- Error handling

### FavoriteViewModelTests
- Favorites loading
- Delete favorite operations
- Sync functionality
- Unsynced count tracking
- Refresh operations

## Running Tests

### Command Line
```bash
# Run all tests
dotnet test

# Run specific test class
dotnet test --filter "FullyQualifiedName~VoteViewModelTests"

# Run with detailed output
dotnet test --verbosity detailed

# Generate code coverage
dotnet test /p:CollectCoverage=true
```

### Visual Studio
- Open Test Explorer (Test > Test Explorer)
- Click "Run All" to execute all tests
- Right-click individual tests for specific execution

### VS Code
- Install "NET Core Test Explorer" extension
- Tests will appear in the Test Explorer panel
- Click the play button next to tests to run them

## Test Patterns

### AAA Pattern (Arrange-Act-Assert)
All tests follow the AAA pattern for clarity:
```csharp
[Fact]
public async Task MethodName_Scenario_ExpectedBehavior()
{
    // Arrange - Set up test data and mocks
    var mockService = new Mock<IService>();
    mockService.Setup(x => x.Method()).ReturnsAsync(expectedData);

    // Act - Execute the method under test
    var result = await _sut.MethodAsync();

    // Assert - Verify the results
    result.Should().NotBeNull();
    mockService.Verify(x => x.Method(), Times.Once);
}
```

### Mocking Services
Services are mocked using Moq to isolate ViewModels:
```csharp
_mockCacheService = new Mock<ICacheService>();
_mockCacheService.Setup(x => x.GetVotingCatsAsync(false))
    .ReturnsAsync(expectedCats);
```

### Fluent Assertions
Assertions use FluentAssertions for readable tests:
```csharp
result.Should().NotBeNull();
result.Should().HaveCount(5);
result.First().Name.Should().Be("Expected");
_sut.IsBusy.Should().BeFalse();
```

## Best Practices

1. **Test Naming**: Use descriptive names following the pattern `MethodName_Scenario_ExpectedBehavior`
2. **One Assert Per Test**: Focus each test on a single behavior
3. **Mock External Dependencies**: Always mock ICacheService and other services
4. **Async Tests**: Use `async Task` for asynchronous test methods
5. **Test Independence**: Each test should be independent and not rely on others

## CI/CD Integration

Tests are automatically executed in the GitHub Actions CI/CD pipeline on every push and pull request.

## Troubleshooting

### Tests Not Discovered
- Ensure all test classes are public
- Verify [Fact] or [Theory] attributes are present
- Rebuild the project

### Async Tests Hanging
- Make sure to await all async calls
- Check for deadlocks in tested code
- Verify CancellationTokens are handled properly

### Mock Setup Not Working
- Verify method signatures match exactly
- Use `It.IsAny<T>()` for flexible parameter matching
- Check that mocks are set up before the act phase

## Future Improvements

- [ ] Add integration tests for cache services
- [ ] Implement test data builders for complex models
- [ ] Add parameterized tests using [Theory] and [InlineData]
- [ ] Measure and improve code coverage (target: >80%)
- [ ] Add performance tests for critical paths
