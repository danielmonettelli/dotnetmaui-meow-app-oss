namespace Meow.Models;

/// <summary>
/// Cached breed entity for local storage
/// </summary>
[Table("CachedBreeds")]
public class CachedBreed
{
    [PrimaryKey]
    [JsonPropertyName("id")]
    public string Id { get; set; }

    [JsonPropertyName("name")]
    public string Name { get; set; }

    [JsonPropertyName("temperament")]
    public string Temperament { get; set; }

    [JsonPropertyName("origin")]
    public string Origin { get; set; }

    [JsonPropertyName("country_codes")]
    public string Country_codes { get; set; }

    [JsonPropertyName("country_code")]
    public string Country_code { get; set; }

    [JsonPropertyName("description")]
    public string Description { get; set; }

    [JsonPropertyName("life_span")]
    public string Life_span { get; set; }

    [JsonPropertyName("indoor")]
    public int Indoor { get; set; }

    [JsonPropertyName("lap")]
    public int Lap { get; set; }

    [JsonPropertyName("alt_names")]
    public string Alt_names { get; set; }

    [JsonPropertyName("adaptability")]
    public int Adaptability { get; set; }

    [JsonPropertyName("affection_level")]
    public int Affection_level { get; set; }

    [JsonPropertyName("child_friendly")]
    public int Child_friendly { get; set; }

    [JsonPropertyName("dog_friendly")]
    public int Dog_friendly { get; set; }

    [JsonPropertyName("energy_level")]
    public int Energy_level { get; set; }

    [JsonPropertyName("grooming")]
    public int Grooming { get; set; }

    [JsonPropertyName("health_issues")]
    public int Health_issues { get; set; }

    [JsonPropertyName("intelligence")]
    public int Intelligence { get; set; }

    [JsonPropertyName("shedding_level")]
    public int Shedding_level { get; set; }

    [JsonPropertyName("social_needs")]
    public int Social_needs { get; set; }

    [JsonPropertyName("stranger_friendly")]
    public int Stranger_friendly { get; set; }

    [JsonPropertyName("vocalisation")]
    public int Vocalisation { get; set; }

    [JsonPropertyName("experimental")]
    public int Experimental { get; set; }

    [JsonPropertyName("hairless")]
    public int Hairless { get; set; }

    [JsonPropertyName("natural")]
    public int Natural { get; set; }

    [JsonPropertyName("rare")]
    public int Rare { get; set; }

    [JsonPropertyName("rex")]
    public int Rex { get; set; }

    [JsonPropertyName("suppressed_tail")]
    public int Suppressed_tail { get; set; }

    [JsonPropertyName("short_legs")]
    public int Short_legs { get; set; }

    [JsonPropertyName("wikipedia_url")]
    public string Wikipedia_url { get; set; }

    [JsonPropertyName("hypoallergenic")]
    public int Hypoallergenic { get; set; }

    [JsonPropertyName("reference_image_id")]
    public string Reference_image_id { get; set; }

    /// <summary>
    /// Serialized JSON of weight information
    /// </summary>
    public string WeightJson { get; set; }

    /// <summary>
    /// When this breed was cached
    /// </summary>
    public DateTime CachedAt { get; set; }

    /// <summary>
    /// Deserialized weight information
    /// </summary>
    [Ignore]
    public Weight Weight
    {
        get => string.IsNullOrEmpty(WeightJson) 
            ? new Weight() 
            : JsonSerializer.Deserialize<Weight>(WeightJson);
        set => WeightJson = JsonSerializer.Serialize(value);
    }

    /// <summary>
    /// Converts to Breed model
    /// </summary>
    public Breed ToBreed()
    {
        return new Breed
        {
            Weight = this.Weight,
            Id = this.Id,
            Name = this.Name,
            Temperament = this.Temperament,
            Origin = this.Origin,
            Country_codes = this.Country_codes,
            Country_code = this.Country_code,
            Description = this.Description,
            Life_span = this.Life_span,
            Indoor = this.Indoor,
            Lap = this.Lap,
            Alt_names = this.Alt_names,
            Adaptability = this.Adaptability,
            Affection_level = this.Affection_level,
            Child_friendly = this.Child_friendly,
            Dog_friendly = this.Dog_friendly,
            Energy_level = this.Energy_level,
            Grooming = this.Grooming,
            Health_issues = this.Health_issues,
            Intelligence = this.Intelligence,
            Shedding_level = this.Shedding_level,
            Social_needs = this.Social_needs,
            Stranger_friendly = this.Stranger_friendly,
            Vocalisation = this.Vocalisation,
            Experimental = this.Experimental,
            Hairless = this.Hairless,
            Natural = this.Natural,
            Rare = this.Rare,
            Rex = this.Rex,
            Suppressed_tail = this.Suppressed_tail,
            Short_legs = this.Short_legs,
            Wikipedia_url = this.Wikipedia_url,
            Hypoallergenic = this.Hypoallergenic,
            Reference_image_id = this.Reference_image_id
        };
    }

    /// <summary>
    /// Creates from Breed model
    /// </summary>
    public static CachedBreed FromBreed(Breed breed)
    {
        return new CachedBreed
        {
            Weight = breed.Weight,
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
}
