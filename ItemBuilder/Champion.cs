using System;
using Newtonsoft.Json;
using SQLite;
using SQLiteNetExtensions.Attributes;

namespace ItemBuilder;

[Table("Champions")]
public class Champion
{
    [PrimaryKey, AutoIncrement]
    public int ChampionID { get; set; }

    [JsonProperty("name")]
    public string Name { get; set; }

    [JsonProperty("title")]
    public string Title { get; set; }

    [JsonProperty("key")]
    public string Key { get; set; }

    [JsonProperty("id")]
    public string Call { get; set; }

    // Property to store the image URL
    [JsonIgnore]
    public string ImageUrl { get; set; }

    [OneToMany(CascadeOperations = CascadeOperation.All)] // Specify the inverse relationship
    public List<Build> Builds { get; set; } = new List<Build>();

    public override string ToString()
    {
        return $"{Name} - {Title}";
    }

    public Champion() 
    {
    }
}

public class ChampionDataWrapper
{
    [JsonProperty("data")]
    [Ignore]
    public Dictionary<string, Champion> Data { get; set; }
}
