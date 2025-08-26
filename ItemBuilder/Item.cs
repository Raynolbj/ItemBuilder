using System.Collections.Generic;
using System.Security.Cryptography.X509Certificates;
using Newtonsoft.Json;
using SQLite;
using SQLiteNetExtensions.Attributes;

namespace ItemBuilder;

[Table("Items")]
public class Item
{
    [PrimaryKey, AutoIncrement]
    public int ItemID { get; set; }

    [JsonProperty("id")]
    public string Id { get; set; }

    [JsonProperty("name")]
    public string Name { get; set; }

    [ManyToOne]
    public ItemGold Gold { get; set; }

    // Add other properties like Stats if needed
    [JsonProperty("tags")]
    [ManyToOne]
    public List<string> Tags { get; set; }

    [JsonIgnore]
    public string TagString { get; set; }

    // Property to store the image URL
    [JsonIgnore]
    public string ImageUrl { get; set; }

    [JsonIgnore]
    public int GoldCost { get; set; }

    [ManyToMany(typeof(BuildItem))]
    public List<Build> Builds { get; set; } = new List<Build>();

    public override string ToString()
    {
        return $"{Name} - Cost: {Gold?.Total} gold"; // Note the null-conditional operator here
    }

}

// Only using the gold total right now, but this could
// be useful for an expansion of the app that uses more attributes
public class ItemGold
{
    
    // Add properties for ItemGold
    [JsonProperty("base")]
    public int Base { get; set; }

    [JsonProperty("total")]
    public int Total { get; set; }

    [JsonProperty("sell")]
    public int Sell { get; set; }

    public ItemGold()
    {
    }
}

public class ItemDataWrapper
{
    [JsonProperty("data")]
    public Dictionary<string, Item> Data { get; set; }
}

