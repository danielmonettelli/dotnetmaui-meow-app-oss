namespace Meow.Tests.Domain.Entities;

/// <summary>
/// Unit tests for Domain entity models
/// </summary>
public class CatTests
{
    [Fact]
    public void Cat_DefaultValues_ShouldBeInitialized()
    {
        var cat = new Cat();

        cat.Id.Should().BeEmpty();
        cat.Url.Should().BeEmpty();
        cat.Width.Should().Be(0);
        cat.Height.Should().Be(0);
        cat.Breeds.Should().BeNull();
    }

    [Fact]
    public void Cat_ShouldSetProperties()
    {
        var cat = new Cat
        {
            Id = "abc123",
            Url = "https://cdn2.thecatapi.com/images/abc123.jpg",
            Width = 800,
            Height = 600,
            Breeds = new List<Breed>
            {
                new Breed { Id = "abys", Name = "Abyssinian" }
            }
        };

        cat.Id.Should().Be("abc123");
        cat.Url.Should().Contain("abc123");
        cat.Width.Should().Be(800);
        cat.Height.Should().Be(600);
        cat.Breeds.Should().HaveCount(1);
    }

    [Fact]
    public void Cat_ShouldSerializeWithJsonPropertyNames()
    {
        var cat = new Cat { Id = "test", Url = "https://example.com", Width = 100, Height = 200 };
        var json = JsonSerializer.Serialize(cat);

        json.Should().Contain("\"id\":");
        json.Should().Contain("\"url\":");
        json.Should().Contain("\"width\":");
        json.Should().Contain("\"height\":");
    }

    [Fact]
    public void Cat_ShouldDeserializeFromJson()
    {
        var json = """{"id":"test1","url":"https://example.com/cat.jpg","width":640,"height":480}""";
        var cat = JsonSerializer.Deserialize<Cat>(json);

        cat.Should().NotBeNull();
        cat!.Id.Should().Be("test1");
        cat.Url.Should().Be("https://example.com/cat.jpg");
        cat.Width.Should().Be(640);
        cat.Height.Should().Be(480);
    }
}

public class BreedTests
{
    [Fact]
    public void Breed_DefaultValues_ShouldBeInitialized()
    {
        var breed = new Breed();

        breed.Id.Should().BeEmpty();
        breed.Name.Should().BeEmpty();
        breed.Weight.Should().BeNull();
    }

    [Fact]
    public void Breed_ShouldSetAllProperties()
    {
        var breed = new Breed
        {
            Id = "abys",
            Name = "Abyssinian",
            Temperament = "Active, Energetic",
            Origin = "Egypt",
            Description = "The Abyssinian is easy to care for...",
            Life_span = "14 - 15",
            Adaptability = 5,
            Affection_level = 5,
            Child_friendly = 3,
            Country_code = "EG"
        };

        breed.Id.Should().Be("abys");
        breed.Name.Should().Be("Abyssinian");
        breed.Temperament.Should().Contain("Active");
        breed.Origin.Should().Be("Egypt");
        breed.Adaptability.Should().Be(5);
    }

    [Fact]
    public void Breed_ShouldSerializeWithJsonPropertyNames()
    {
        var breed = new Breed { Id = "test", Name = "Test Breed" };
        var json = JsonSerializer.Serialize(breed);

        json.Should().Contain("\"id\":");
        json.Should().Contain("\"name\":");
    }

    [Fact]
    public void Breed_ShouldDeserializeFromJson()
    {
        var json = """{"id":"abys","name":"Abyssinian","temperament":"Active","origin":"Egypt"}""";
        var breed = JsonSerializer.Deserialize<Breed>(json);

        breed.Should().NotBeNull();
        breed!.Id.Should().Be("abys");
        breed.Name.Should().Be("Abyssinian");
        breed.Temperament.Should().Be("Active");
    }
}

public class FavoriteCatResponseTests
{
    [Fact]
    public void FavoriteCatResponse_DefaultValues_ShouldBeNull()
    {
        var response = new FavoriteCatResponse();

        response.Id.Should().BeNull();
        response.User_id.Should().BeNull();
        response.Image_id.Should().BeNull();
        response.Image.Should().BeNull();
    }

    [Fact]
    public void FavoriteCatResponse_ShouldSetProperties()
    {
        var response = new FavoriteCatResponse
        {
            Id = "fav1",
            User_id = "user1",
            Image_id = "cat1",
            Image = new Cat { Id = "cat1", Url = "https://example.com/cat1.jpg" }
        };

        response.Id.Should().Be("fav1");
        response.User_id.Should().Be("user1");
        response.Image_id.Should().Be("cat1");
        response.Image.Should().NotBeNull();
        response.Image!.Id.Should().Be("cat1");
    }

    [Fact]
    public void FavoriteCatResponse_ShouldDeserializeFromJson()
    {
        var json = """{"id":"123","user_id":"u1","image_id":"cat1","image":{"id":"cat1","url":"https://example.com/cat.jpg","width":100,"height":100}}""";
        var response = JsonSerializer.Deserialize<FavoriteCatResponse>(json);

        response.Should().NotBeNull();
        response!.Id.Should().Be("123");
        response.Image.Should().NotBeNull();
        response.Image!.Id.Should().Be("cat1");
    }
}

public class FavoriteCatRequestTests
{
    [Fact]
    public void FavoriteCatRequest_ShouldSerializeCorrectly()
    {
        var request = new FavoriteCatRequest { Image_id = "cat1" };
        var json = JsonSerializer.Serialize(request);

        json.Should().Contain("\"image_id\":");
        json.Should().Contain("cat1");
    }

    [Fact]
    public void FavoriteCatRequest_DefaultValue_ShouldBeEmpty()
    {
        var request = new FavoriteCatRequest();
        request.Image_id.Should().BeEmpty();
    }
}

public class CacheStatisticsTests
{
    [Fact]
    public void CacheStatistics_DefaultValues_ShouldBeZero()
    {
        var stats = new CacheStatistics();

        stats.CachedCatsCount.Should().Be(0);
        stats.CachedBreedsCount.Should().Be(0);
        stats.FavoritesCount.Should().Be(0);
        stats.UnsyncedFavoritesCount.Should().Be(0);
        stats.IsOnline.Should().BeFalse();
    }

    [Fact]
    public void CacheStatistics_ShouldSetProperties()
    {
        var stats = new CacheStatistics
        {
            CachedCatsCount = 30,
            CachedBreedsCount = 67,
            FavoritesCount = 5,
            UnsyncedFavoritesCount = 2,
            LastCacheUpdate = new DateTime(2024, 1, 1),
            IsOnline = true
        };

        stats.CachedCatsCount.Should().Be(30);
        stats.CachedBreedsCount.Should().Be(67);
        stats.FavoritesCount.Should().Be(5);
        stats.UnsyncedFavoritesCount.Should().Be(2);
        stats.IsOnline.Should().BeTrue();
    }
}
