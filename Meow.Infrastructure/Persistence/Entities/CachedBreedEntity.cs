namespace Meow.Infrastructure.Persistence.Entities;

/// <summary>
/// SQLite entity for cached breed data
/// </summary>
[Table("CachedBreeds")]
public class CachedBreedEntity
{
    [PrimaryKey]
    public string Id { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string? Temperament { get; set; }
    public string? Origin { get; set; }
    public string? Country_codes { get; set; }
    public string? Country_code { get; set; }
    public string? Description { get; set; }
    public string? Life_span { get; set; }
    public int Indoor { get; set; }
    public int Lap { get; set; }
    public string? Alt_names { get; set; }
    public int Adaptability { get; set; }
    public int Affection_level { get; set; }
    public int Child_friendly { get; set; }
    public int Dog_friendly { get; set; }
    public int Energy_level { get; set; }
    public int Grooming { get; set; }
    public int Health_issues { get; set; }
    public int Intelligence { get; set; }
    public int Shedding_level { get; set; }
    public int Social_needs { get; set; }
    public int Stranger_friendly { get; set; }
    public int Vocalisation { get; set; }
    public int Experimental { get; set; }
    public int Hairless { get; set; }
    public int Natural { get; set; }
    public int Rare { get; set; }
    public int Rex { get; set; }
    public int Suppressed_tail { get; set; }
    public int Short_legs { get; set; }
    public string? Wikipedia_url { get; set; }
    public int Hypoallergenic { get; set; }
    public string? Reference_image_id { get; set; }
    public string? WeightJson { get; set; }
    public DateTime CachedAt { get; set; }

    [Ignore]
    public Weight Weight
    {
        get => string.IsNullOrEmpty(WeightJson)
            ? new Weight()
            : JsonSerializer.Deserialize<Weight>(WeightJson) ?? new Weight();
        set => WeightJson = JsonSerializer.Serialize(value);
    }

    public Breed ToBreed() => new()
    {
        Weight = Weight,
        Id = Id,
        Name = Name,
        Temperament = Temperament,
        Origin = Origin,
        Country_codes = Country_codes,
        Country_code = Country_code,
        Description = Description,
        Life_span = Life_span,
        Indoor = Indoor,
        Lap = Lap,
        Alt_names = Alt_names,
        Adaptability = Adaptability,
        Affection_level = Affection_level,
        Child_friendly = Child_friendly,
        Dog_friendly = Dog_friendly,
        Energy_level = Energy_level,
        Grooming = Grooming,
        Health_issues = Health_issues,
        Intelligence = Intelligence,
        Shedding_level = Shedding_level,
        Social_needs = Social_needs,
        Stranger_friendly = Stranger_friendly,
        Vocalisation = Vocalisation,
        Experimental = Experimental,
        Hairless = Hairless,
        Natural = Natural,
        Rare = Rare,
        Rex = Rex,
        Suppressed_tail = Suppressed_tail,
        Short_legs = Short_legs,
        Wikipedia_url = Wikipedia_url,
        Hypoallergenic = Hypoallergenic,
        Reference_image_id = Reference_image_id
    };

    public static CachedBreedEntity FromBreed(Breed breed) => new()
    {
        Weight = breed.Weight ?? new Weight(),
        Id = breed.Id,
        Name = breed.Name,
        Temperament = breed.Temperament,
        Origin = breed.Origin,
        Country_codes = breed.Country_codes,
        Country_code = breed.Country_code,
        Description = breed.Description,
        Life_span = breed.Life_span,
        Indoor = breed.Indoor,
        Lap = breed.Lap,
        Alt_names = breed.Alt_names,
        Adaptability = breed.Adaptability,
        Affection_level = breed.Affection_level,
        Child_friendly = breed.Child_friendly,
        Dog_friendly = breed.Dog_friendly,
        Energy_level = breed.Energy_level,
        Grooming = breed.Grooming,
        Health_issues = breed.Health_issues,
        Intelligence = breed.Intelligence,
        Shedding_level = breed.Shedding_level,
        Social_needs = breed.Social_needs,
        Stranger_friendly = breed.Stranger_friendly,
        Vocalisation = breed.Vocalisation,
        Experimental = breed.Experimental,
        Hairless = breed.Hairless,
        Natural = breed.Natural,
        Rare = breed.Rare,
        Rex = breed.Rex,
        Suppressed_tail = breed.Suppressed_tail,
        Short_legs = breed.Short_legs,
        Wikipedia_url = breed.Wikipedia_url,
        Hypoallergenic = breed.Hypoallergenic,
        Reference_image_id = breed.Reference_image_id,
        CachedAt = DateTime.UtcNow
    };
}
