using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using System.Text.RegularExpressions;
using System.Diagnostics;

namespace ItemBuilder;

public class ApiManager
{
    HttpClient _client;

    public ApiManager()
    {
        _client = new HttpClient();
    }

    private const string ChampDataUrl = "https://ddragon.leagueoflegends.com/cdn/14.9.1/data/en_US/champion.json";
    private const string ItemDataUrl = "https://ddragon.leagueoflegends.com/cdn/14.9.1/data/en_US/item.json";

    private Dictionary<string, Champion> champData;
    private ChampionDataWrapper champDataWrapper; // Declare champDataWrapper at the class level

    private Dictionary<string, Item> itemData;
    private ItemDataWrapper itemDataWrapper;  // Declare itemDataWrapper at the class level



    // ****** Champion Related *********

    public async Task InitializeChampDataAsync()
    {
        var json = await _client.GetStringAsync(ChampDataUrl);
        champDataWrapper = JsonConvert.DeserializeObject<ChampionDataWrapper>(json);
        champData = champDataWrapper.Data;
    }
    public string GetChampImageUrl(string champName)
    {   
        return $"https://ddragon.leagueoflegends.com/cdn/14.9.1/img/champion/{(StringUtilities.CleanseString(champName))}.png";
    }

    public string GetChampKey(string champName)
    {
        if (champData != null)
        {
            // Find the champion with the specified name (case-insensitive)
            var matchingChamp = champData.FirstOrDefault(pair =>
                string.Equals(pair.Value.Name, champName, StringComparison.OrdinalIgnoreCase) ||
                string.Equals(pair.Value.Name.Replace("'", ""), champName, StringComparison.OrdinalIgnoreCase)).Value;

            // Check if matchingChamp is not null
            if (matchingChamp != null)
            {
                // Use the original casing of the key from the dictionary
                return champData.First(pair => pair.Value == matchingChamp).Key;
            }
        }

        // Return null or empty string if the champion is not found
        return null;
    }

    public Champion CreateChampion(string championName)
    {
        // Ensure champData is not null
        if (champData != null)
        {
            // Find the champion key with the specified name (case-insensitive)
            var championKey = GetChampKey(championName);

            // Check if championKey is not null
            if (!string.IsNullOrEmpty(championKey) && champData.TryGetValue(championKey, out var matchingChamp))
            {
                // Create a new Champion instance with the matching champion's data
                var newChamp = new Champion
                {
                    Name = matchingChamp.Name,
                    Key = matchingChamp.Key,
                    ImageUrl = GetChampImageUrl(matchingChamp.Call),
                    Title = matchingChamp.Title,
                    Call = matchingChamp.Call,
                    // Add other properties if needed
                };

                return newChamp;
            }
        }

        // Return null if the champion or its key is not found
        return null;
    }



    // ****** Item Related *********
    public async Task InitializeItemDataAsync()
    { 
        var json = await _client.GetStringAsync(ItemDataUrl);
        itemDataWrapper = JsonConvert.DeserializeObject<ItemDataWrapper>(json);
        itemData = itemDataWrapper.Data;
    }

    public string getItemKey(string itemName)
    {
        if (itemData != null)
        {
            // Find the item with the specified name
            var matchingItem = itemData.Values.FirstOrDefault(item =>
                StringUtilities.CleanseString(item.Name).Equals(StringUtilities.CleanseString(itemName), StringComparison.OrdinalIgnoreCase));

            // Check if matchingItem is not null
            if (matchingItem != null)
            {
                // Use the Key property to get the key from the dictionary
                return itemData.First(pair => pair.Value == matchingItem).Key;
            }
        }

        // Return null or empty string if the item is not found
        return null;
    }

    public string GetItemImageUrl(string itemName)
    {
       // Use the key (Id) from the dictionary to build the image URL
       return $"https://ddragon.leagueoflegends.com/cdn/14.9.1/img/item/{getItemKey(itemName)}.png";
            
    }

    public Item createItem(string itemName)
    {
        // Ensure itemData is not null
        if (itemData != null)
        {
            // Get the key (Id) associated with the input item name
            var itemId = getItemKey(itemName);

            // Check if itemId is not null
            if (itemId != null && itemData.TryGetValue(itemId, out var matchingItem))
            {
                // Create a new Item instance with the matching item's data
                var newItem = new Item
                {
                    Id = itemId,
                    Name = matchingItem.Name,
                    ImageUrl = GetItemImageUrl(matchingItem.Name),
                    Gold = matchingItem.Gold,
                    Tags = matchingItem.Tags,
                    TagString = string.Join(", ", matchingItem.Tags),
                    GoldCost = matchingItem.Gold.Total,
                    // Add other properties if needed
                };

                return newItem;
            }
        }

        // Return null if the item or its key is not found
        return null;
    }

    public List<string> GetAllItemNames()
    {

        if (itemData != null)
        {
            // Extract the names from the itemData dictionary
            var itemNames = itemData.Values.Select(item => item.Name).ToList();
            return itemNames;
        }

        // Return an empty list if itemData is null
        return new List<string>();
        
    }

}

public static class StringUtilities
{
    public static string CleanseString(string input)
    {
        // Remove spaces and punctuation
        return Regex.Replace(input, @"[\s\p{P}]", "");
    }
}



