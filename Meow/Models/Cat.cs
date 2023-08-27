namespace Meow.Models;

public class Cat
{
    [JsonPropertyName("breeds")]
    public List<Breed> Breeds { get; set; }

    [JsonPropertyName("id")]
    public string Id { get; set; }

    [JsonPropertyName("url")]
    public string Url { get; set; }

    [JsonPropertyName("width")]
    public int Width { get; set; }

    [JsonPropertyName("height")]
    public int Height { get; set; }
}

public class Breed
{
    [JsonPropertyName("weight")]
    public Weight Weight { get; set; }

    [JsonPropertyName("id")]
    public string Id { get; set; }

    [JsonPropertyName("name")]
    public string Name { get; set; }

    [JsonPropertyName("cfa_url")]
    public string Cfa_url { get; set; }

    [JsonPropertyName("vetstreet_url")]
    public string Vetstreet_url { get; set; }

    [JsonPropertyName("vcahospitals_url")]
    public string Vcahospitals_url { get; set; }

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

    [JsonPropertyName("cat_friendly")]
    public int Cat_friendly { get; set; }

    [JsonPropertyName("bidability")]
    public int Bidability { get; set; }
}

public class Weight
{
    [JsonPropertyName("imperial")]
    public string Imperial { get; set; }

    [JsonPropertyName("metric")]
    public string Metric { get; set; }
}
