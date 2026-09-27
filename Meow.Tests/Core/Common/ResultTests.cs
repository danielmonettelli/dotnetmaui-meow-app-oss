namespace Meow.Tests.Core.Common;

/// <summary>
/// Unit tests for the Result&lt;T&gt; generic wrapper
/// </summary>
public class ResultTests
{
    #region Success Factory

    [Fact]
    public void Success_ShouldCreateSuccessfulResult()
    {
        var data = new List<string> { "a", "b" };

        var result = Result<List<string>>.Success(data);

        result.IsSuccess.Should().BeTrue();
        result.Data.Should().BeSameAs(data);
        result.ErrorMessage.Should().BeNull();
        result.Source.Should().Be(ResultSource.Api);
    }

    [Fact]
    public void Success_WithExplicitSource_ShouldSetSource()
    {
        var result = Result<int>.Success(42, ResultSource.Cache);

        result.IsSuccess.Should().BeTrue();
        result.Data.Should().Be(42);
        result.Source.Should().Be(ResultSource.Cache);
    }

    [Fact]
    public void Success_DefaultSource_ShouldBeApi()
    {
        var result = Result<string>.Success("test");
        result.Source.Should().Be(ResultSource.Api);
    }

    #endregion

    #region Failure Factory

    [Fact]
    public void Failure_WithErrorOnly_ShouldCreateFailedResult()
    {
        var result = Result<string>.Failure("Something went wrong");

        result.IsSuccess.Should().BeFalse();
        result.Data.Should().BeNull();
        result.ErrorMessage.Should().Be("Something went wrong");
        result.Source.Should().Be(ResultSource.None);
    }

    [Fact]
    public void Failure_WithFallbackData_ShouldBeSuccessfulWithError()
    {
        var fallback = new List<Cat> { new Cat { Id = "cat1" } };

        var result = Result<List<Cat>>.Failure("API error", fallback, ResultSource.Cache);

        // When fallback data is not null, IsSuccess is true (data is available, just from cache)
        result.IsSuccess.Should().BeTrue();
        result.Data.Should().BeSameAs(fallback);
        result.ErrorMessage.Should().Be("API error");
        result.Source.Should().Be(ResultSource.Cache);
    }

    [Fact]
    public void Failure_WithNullFallback_ShouldNotBeSuccess()
    {
        var result = Result<List<Cat>>.Failure("Error", null);

        result.IsSuccess.Should().BeFalse();
        result.Data.Should().BeNull();
    }

    #endregion

    #region FromCache Factory

    [Fact]
    public void FromCache_ShouldCreateCacheResult()
    {
        var data = new List<Breed> { new Breed { Id = "abys", Name = "Abyssinian" } };

        var result = Result<List<Breed>>.FromCache(data);

        result.IsSuccess.Should().BeTrue();
        result.Data.Should().BeSameAs(data);
        result.ErrorMessage.Should().BeNull();
        result.Source.Should().Be(ResultSource.Cache);
    }

    #endregion

    #region Offline Factory

    [Fact]
    public void Offline_WithCachedData_ShouldBeSuccessful()
    {
        var cached = new List<Cat> { new Cat { Id = "cat1" } };

        var result = Result<List<Cat>>.Offline(cached);

        result.IsSuccess.Should().BeTrue();
        result.Data.Should().BeSameAs(cached);
        result.ErrorMessage.Should().Be("No internet connection. Showing cached data.");
        result.Source.Should().Be(ResultSource.Cache);
    }

    [Fact]
    public void Offline_WithNullData_ShouldNotBeSuccessful()
    {
        var result = Result<List<Cat>>.Offline(null);

        result.IsSuccess.Should().BeFalse();
        result.Data.Should().BeNull();
        result.Source.Should().Be(ResultSource.Cache);
    }

    [Fact]
    public void Offline_WithEmptyList_ShouldBeSuccessful()
    {
        // An empty list is not null, so it should count as "success" (data available, just empty)
        var result = Result<List<Cat>>.Offline(new List<Cat>());

        result.IsSuccess.Should().BeTrue();
        result.Data.Should().NotBeNull();
        result.Data.Should().BeEmpty();
    }

    #endregion

    #region ResultSource Enum

    [Fact]
    public void ResultSource_ShouldHaveExpectedValues()
    {
        ResultSource.None.Should().Be((ResultSource)0);
        ResultSource.Api.Should().Be((ResultSource)1);
        ResultSource.Cache.Should().Be((ResultSource)2);
    }

    #endregion
}
