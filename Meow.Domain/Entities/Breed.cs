namespace Meow.Domain.Entities;

/// <summary>
/// Breed entity representing a cat breed
/// </summary>
public class Breed
{
    [JsonPropertyName("weight")]
    public Weight? Weight { get; set; }

    [JsonPropertyName("id")]
    public string Id { get; set; } = string.Empty;

    [JsonPropertyName("name")]
    public string Name { get; set; } = string.Empty;

    [JsonPropertyName("cfa_url")]
    public string? Cfa_url { get; set; }

    [JsonPropertyName("vetstreet_url")]
    public string? Vetstreet_url { get; set; }

    [JsonPropertyName("vcahospitals_url")]
    public string? Vcahospitals_url { get; set; }

    [JsonPropertyName("temperament")]
    public string? Temperament { get; set; }

    [JsonPropertyName("origin")]
    public string? Origin { get; set; }

    [JsonPropertyName("country_codes")]
    public string? Country_codes { get; set; }

    [JsonPropertyName("country_code")]
    public string? Country_code { get; set; }

    [JsonPropertyName("description")]
    public string? Description { get; set; }

    [JsonPropertyName("life_span")]
    public string? Life_span { get; set; }

    [JsonPropertyName("indoor")]
    public int Indoor { get; set; }

    [JsonPropertyName("lap")]
    public int Lap { get; set; }

    [JsonPropertyName("alt_names")]
    public string? Alt_names { get; set; }

    [JsonPropertyName("adaptability")]
    public int Adaptability { get; set; }

    [JsonPropertyName("affection_level")]
    public int Affection_level { get; set; }
    public int AffectionLevel { get => Affection_level; set => Affection_level = value; }

    [JsonPropertyName("child_friendly")]
    public int Child_friendly { get; set; }
    public int ChildFriendly { get => Child_friendly; set => Child_friendly = value; }

    [JsonPropertyName("dog_friendly")]
    public int Dog_friendly { get; set; }
    public int DogFriendly { get => Dog_friendly; set => Dog_friendly = value; }

    [JsonPropertyName("energy_level")]
    public int Energy_level { get; set; }
    public int EnergyLevel { get => Energy_level; set => Energy_level = value; }

    [JsonPropertyName("grooming")]
    public int Grooming { get; set; }

    [JsonPropertyName("health_issues")]
    public int Health_issues { get; set; }
    public int HealthIssues { get => Health_issues; set => Health_issues = value; }

    [JsonPropertyName("intelligence")]
    public int Intelligence { get; set; }

    [JsonPropertyName("shedding_level")]
    public int Shedding_level { get; set; }
    public int SheddingLevel { get => Shedding_level; set => Shedding_level = value; }

    [JsonPropertyName("social_needs")]
    public int Social_needs { get; set; }
    public int SocialNeeds { get => Social_needs; set => Social_needs = value; }

    [JsonPropertyName("stranger_friendly")]
    public int Stranger_friendly { get; set; }
    public int StrangerFriendly { get => Stranger_friendly; set => Stranger_friendly = value; }

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
    public string? Wikipedia_url { get; set; }

    [JsonPropertyName("hypoallergenic")]
    public int Hypoallergenic { get; set; }

    [JsonPropertyName("reference_image_id")]
    public string? Reference_image_id { get; set; }

    [JsonPropertyName("cat_friendly")]
    public int Cat_friendly { get; set; }

    [JsonPropertyName("bidability")]
    public int Bidability { get; set; }
}

/// <summary>
/// Weight measurement for a breed
/// </summary>
public class Weight
{
    [JsonPropertyName("imperial")]
    public string? Imperial { get; set; }

    [JsonPropertyName("metric")]
    public string? Metric { get; set; }
}
