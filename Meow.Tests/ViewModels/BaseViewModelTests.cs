namespace Meow.Tests.ViewModels;

/// <summary>
/// Unit tests for BaseViewModel functionality
/// </summary>
public class BaseViewModelTests
{
    #region Setup

    private readonly BaseViewModel _sut;

    public BaseViewModelTests()
    {
        _sut = new BaseViewModel();
    }

    #endregion

    #region Property Tests

    [Fact]
    public void Title_WhenSet_ShouldUpdateProperty()
    {
        // Arrange & Act
        _sut.Title = "Test Title";

        // Assert
        _sut.Title.Should().Be("Test Title");
    }

    [Fact]
    public void Title_DefaultValue_ShouldBeEmpty()
    {
        _sut.Title.Should().BeEmpty();
    }

    [Fact]
    public void IsBusy_WhenSet_ShouldUpdateProperty()
    {
        _sut.IsBusy = true;
        _sut.IsBusy.Should().BeTrue();
    }

    [Fact]
    public void IsBusy_DefaultValue_ShouldBeFalse()
    {
        _sut.IsBusy.Should().BeFalse();
    }

    [Fact]
    public void Progress_WhenSet_ShouldUpdateProperty()
    {
        var expected = TimeSpan.FromSeconds(5);
        _sut.Progress = expected;
        _sut.Progress.Should().Be(expected);
    }

    [Fact]
    public void IsOffline_WhenSet_ShouldUpdateProperty()
    {
        _sut.IsOffline = true;
        _sut.IsOffline.Should().BeTrue();
    }

    [Fact]
    public void IsOffline_DefaultValue_ShouldBeFalse()
    {
        _sut.IsOffline.Should().BeFalse();
    }

    [Fact]
    public void StatusMessage_WhenSet_ShouldUpdateProperty()
    {
        _sut.StatusMessage = "Test message";
        _sut.StatusMessage.Should().Be("Test message");
    }

    [Fact]
    public void StatusMessage_DefaultValue_ShouldBeNull()
    {
        _sut.StatusMessage.Should().BeNull();
    }

    #endregion

    #region PropertyChanged Tests

    [Fact]
    public void Title_WhenChanged_ShouldRaisePropertyChanged()
    {
        // Arrange
        var propertyNames = new List<string>();
        _sut.PropertyChanged += (_, e) => propertyNames.Add(e.PropertyName!);

        // Act
        _sut.Title = "New Title";

        // Assert
        propertyNames.Should().Contain(nameof(BaseViewModel.Title));
    }

    [Fact]
    public void IsBusy_WhenChanged_ShouldRaisePropertyChanged()
    {
        var propertyNames = new List<string>();
        _sut.PropertyChanged += (_, e) => propertyNames.Add(e.PropertyName!);

        _sut.IsBusy = true;

        propertyNames.Should().Contain(nameof(BaseViewModel.IsBusy));
    }

    [Fact]
    public void IsOffline_WhenChanged_ShouldRaisePropertyChanged()
    {
        var propertyNames = new List<string>();
        _sut.PropertyChanged += (_, e) => propertyNames.Add(e.PropertyName!);

        _sut.IsOffline = true;

        propertyNames.Should().Contain(nameof(BaseViewModel.IsOffline));
    }

    [Fact]
    public void StatusMessage_WhenChanged_ShouldRaisePropertyChanged()
    {
        var propertyNames = new List<string>();
        _sut.PropertyChanged += (_, e) => propertyNames.Add(e.PropertyName!);

        _sut.StatusMessage = "error";

        propertyNames.Should().Contain(nameof(BaseViewModel.StatusMessage));
    }

    #endregion

    #region Theme Command Tests

    [Fact]
    public void SelectThemeCommand_ShouldBeAvailable()
    {
        _sut.SelectThemeCommand.Should().NotBeNull();
    }

    [Fact]
    public void SelectThemeCommand_CanExecute_ShouldReturnTrue()
    {
        var canExecute = _sut.SelectThemeCommand.CanExecute(null);
        canExecute.Should().BeTrue();
    }

    #endregion
}
