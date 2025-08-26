using System;
using Newtonsoft.Json;
using SQLite;
using SQLiteNetExtensions.Attributes;

namespace ItemBuilder;

[Table("Builds")]
public class Build
{

    [PrimaryKey, AutoIncrement]
    public int ID { get; set; }

    [Indexed]
    public int ChampID { get; set; }

    [ManyToOne(nameof(ChampID))]
    public Champion Champ { get; set; }

    public string ImageURL { get; set; }

    [ManyToMany(typeof(BuildItem))]
    public List<Item> Items { get; set; } = new List<Item>();

    public string Title { get; set; }

    public Build(string t, Champion c, List<Item> i)
    {
        this.Title = t;
        this.Champ = c;
        this.ChampID = c.ChampionID;
        this.Items = i;
    }

    public Build()
    {
    }

    public override string ToString()
    {
        return $"{Title}";
    }

}


[Table("BuildItems")]
public class BuildItem
{
    [ForeignKey(typeof(Build))]
    public int BuildId { get; set; }

    [ForeignKey(typeof(Item))]
    public int ItemId { get; set; }
}
