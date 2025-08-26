using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Maui.Controls;
using SQLiteNetExtensions.Extensions;

namespace ItemBuilder
{
    public partial class AddPage : ContentPage
    {
        Build oldBuild;
        List<Item> defaultItems;
        ApiManager apiManager; // Create an instance of ApiManager
        bool deleteMode = false;

        public AddPage()
        {
            InitializeComponent();
            apiManager = new ApiManager();
            defaultItems = new List<Item>();
            RefreshCollectionView();
            addButton.Text = "ADD";
        }

        // All of the champion data needs to be initialized prior to the page creations, 
        // regardless of whether we are updating or adding anew
        public static async Task<AddPage> CreateAsync(Build oldBuild)
        {
            var addPage = new AddPage();
            await addPage.InitializeAsync(oldBuild);
            return addPage;
        }

        public static async Task<AddPage> CreateAsync()
        {
            var addPage = new AddPage();
            await addPage.InitializeAsync();
            return addPage;
        }

        private async Task InitializeAsync(Build oldBuild)
        {
            // Check if apiManager is null before initializing
            if (apiManager == null)
            {
                apiManager = new ApiManager();
            }

            await apiManager.InitializeItemDataAsync();
            await apiManager.InitializeChampDataAsync();
            this.oldBuild = oldBuild;

            // Fetch Champion data from the database using oldBuild.ChampID
            var champion = DB.conn.Find<Champion>(oldBuild.ChampID);
            champEntry.Text = champion?.Name;

            // Fetch Item data from the database using oldBuild.Items
            defaultItems = oldBuild.Items;

            // Set the name entry based on oldBuild.Title
            nameEntry.Text = oldBuild.Title;

            addButton.Text = "UPDATE";

            itemPicker.ItemsSource = apiManager.GetAllItemNames();
            itemPicker.SelectedIndex = 0;
            analyticsButton.IsVisible = true;
            // Refresh the CollectionView
            RefreshCollectionView();
        }


        // Initializes the item and champion data, and sets the itemPicker 
        private async Task InitializeAsync()
        {
            // Check if apiManager is null before initializing
            if (apiManager == null)
            {
                apiManager = new ApiManager();
            }
            await apiManager.InitializeItemDataAsync();
            await apiManager.InitializeChampDataAsync();
            itemPicker.ItemsSource = apiManager.GetAllItemNames();
            itemPicker.SelectedIndex = 0;

        }

        // Refresh the collection view 
        private void RefreshCollectionView()
        {
            // Set the items list as the ItemsSource for the CollectionView
            itemCollectionView.ItemsSource = defaultItems;

            // Set the ItemTemplate for the CollectionView
            itemCollectionView.ItemTemplate = new DataTemplate(() =>
            {
                var stackLayout = new StackLayout
                {
                    VerticalOptions = LayoutOptions.CenterAndExpand, // Center vertically
                    HorizontalOptions = LayoutOptions.CenterAndExpand, // Center horizontally
                    Spacing = 5 // Set spacing between elements
                };

                // Bind directly to the Name property of the Item
                var nameLabel = new Label
                {
                    HorizontalOptions = LayoutOptions.Center,
                    TextColor = Color.FromArgb("#F0E6D2"),
                    LineBreakMode = LineBreakMode.WordWrap, // Set LineBreakMode to WordWrap
                    MaxLines = 2 // Set the maximum number of lines
                };
                nameLabel.SetBinding(Label.TextProperty, "Name");
                stackLayout.Children.Add(nameLabel); // Add to the stack layout

                // Use the ImageUrl property to display the item image
                var itemImage = new Microsoft.Maui.Controls.Image();
                itemImage.SetBinding(Microsoft.Maui.Controls.Image.SourceProperty, new Binding("ImageUrl"));
                itemImage.WidthRequest = 40;
                itemImage.HeightRequest = 40;
                stackLayout.Children.Add(itemImage); // Add to the stack layout

                // Bind directly to the Gold property of the Item
                var goldLayout = new StackLayout
                {
                    Orientation = StackOrientation.Horizontal,
                    HorizontalOptions = LayoutOptions.Center
                };

                // Similar stuff to above, except for the rest of our data
                var goldImage = new Microsoft.Maui.Controls.Image
                {
                    Source = "gold.png",
                    WidthRequest = 15,
                    HeightRequest = 15,
                    HorizontalOptions = LayoutOptions.Start
                };
                goldLayout.Children.Add(goldImage);

                var goldCostLabel = new Label
                {
                    HorizontalOptions = LayoutOptions.Start
                };

                goldCostLabel.SetBinding(Label.TextProperty, new Binding("GoldCost", stringFormat: " {0}"));
                goldCostLabel.TextColor = Color.FromArgb("#C89B3C");
                goldLayout.Children.Add(goldCostLabel);

                stackLayout.Children.Add(goldLayout);

                return stackLayout;
            });
        }



        // Add to the CollectionView based on Search or Picker Control
        private async void OnSearchButtonPressed(object sender, EventArgs e)
        {
            Button clickedButton = new Button();
            if (sender is Button)
            {
                clickedButton = (Button)sender;
            }

            if (clickedButton.Text == "ADD ITEM")
            {
                // Ensure an item is selected in the itemPicker
                if (itemPicker.SelectedIndex == -1)
                {
                    await DisplayAlert("Item Selection", "Please select an item from the list.", "OK");
                    return;
                }

                Item newItem = apiManager.createItem(itemPicker.SelectedItem.ToString());

                if (newItem != null)
                {
                    // Save the new item to the database
                    DB.conn.Insert(newItem);

                    // Update the defaultItems
                    defaultItems.Add(newItem);
                }
                else
                {
                    // Display a popup with item details
                    await DisplayAlert("Item Mismatch", "Please enter a valid item", "OK");
                    return;
                }

            }

            else
            {
                Item newItem = apiManager.createItem(itemSearch.Text);
                if (newItem != null)
                {
                    // Save the new item to the database
                    DB.conn.Insert(newItem);

                    // Update the defaultItems
                    defaultItems.Add(newItem);
                }
                else
                {
                    // Display a popup with item details
                    await DisplayAlert("Item Mismatch", "Please enter a valid item", "OK");
                    return;
                }
            }
                itemSearch.Text = null;
                RefreshCollectionView();
        }

        // Allow for Viewing pf Item Info or Deletion
        private async void OnCollectionViewSelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            var selectedItem = itemCollectionView.SelectedItem as Item;

            if (deleteMode)
            {
                // Check if the user actually wishes to delete the item
                bool accepted = await DisplayAlert("Warning", "Do you wish to delete this item?", "Yes", "No");

                if (accepted)
                {
                    DeleteSelectedItem(selectedItem);
                }

            }
            else if (!deleteMode)
            {

                // Display a popup with item details
                DisplayItemDetailsPopup(selectedItem);

            }
        }


        private async void DisplayItemDetailsPopup(Item selectedItem)
        {
            if (selectedItem == null)
            {
                return;
            }
            // Create a formatted string with item details
            string formattedTagString = System.Text.RegularExpressions.Regex.Replace(selectedItem.TagString, "([a-z])([A-Z])", "$1 $2");
            string itemDetails = $"Name: {selectedItem.Name}\nTags: {formattedTagString}";

            // Display a popup with item details
            await DisplayAlert("Item Details", itemDetails, "OK");
            itemCollectionView.SelectedItem = null;
        }


        // Button functionality

        // Add and navigate back
        private async void AddButton_Clicked(object sender, EventArgs e)
        {

            Button button = (Button)sender;
            string buttonText = button.Text;


            // Create or retrieve the Champion
            Champion newChamp = apiManager.CreateChampion(champEntry.Text);

            // Create or retrieve the Items
            List<Item> itemsCollection = itemCollectionView.ItemsSource as List<Item>;

            if (newChamp == null)
            {
                // Display a popup with item details
                await DisplayAlert("Champion Mismatch", "Please enter a valid Champion", "OK");
                return;
            }

            bool accepted;

            if (addButton.Text == "ADD")
            {
                accepted = await DisplayAlert("Warning", "Do you wish to save this build?", "Yes", "No");
            } 
            else {
                accepted = await DisplayAlert("Warning", "Do you wish to save these changes?", "Yes", "No");
            }

            if (!accepted)
            {
                return;
            }

            // Create the new Build using the CreateBuild method
            Build newBuild = CreateBuild(nameEntry.Text, newChamp, itemsCollection);

            // Add or Update Existing Build
            if (buttonText == "ADD")
            {
                // Insert the new build to the database
                DB.conn.InsertWithChildren(newBuild);
                await Navigation.PopModalAsync();
            }
            else
            {
                // Update the existing build in the database
                newBuild.ID = oldBuild.ID;
                DB.conn.UpdateWithChildren(newBuild);
                await Navigation.PopModalAsync();
            }
        }

        // Discard changes and navigate back
        private async void CancelButton_Clicked(object sender, EventArgs e)
        {
            bool accepted = await DisplayAlert("Warning", "Do you wish to discard these changes?", "Yes", "No");
            if (accepted)
            {
                await Navigation.PopModalAsync();
            }
        }

        // Functions related to deleting
        private void DeleteSelectedItem(Item selectedItem)
        {
            // Remove the selected items from the defaultItems list
            defaultItems.Remove(selectedItem);

            // Refresh the CollectionView
            RefreshCollectionView();
        }

        private void deleteSwitch_Toggled(object sender, ToggledEventArgs e)
        {
            deleteMode = deleteMode ? false : true;
        }

        // Create the build object
        public static Build CreateBuild(string title, Champion champion, List<Item> items)
        {
            // Insert or update the champion in the database
            DB.conn.Insert(champion);

            // Set the foreign key ID in the build
            Build newBuild = new Build
            {
                Title = title,
                Champ = champion,
                ChampID = champion.ChampionID,
                ImageURL = champion.ImageUrl,
            };

            // Add items to the build
            newBuild.Items.AddRange(items);

            // Update the champion to include the new build
            champion.Builds.Add(newBuild);
           
            return newBuild;
        }

        // Create an item object
        public Item Item(string name)
        {
            Item newItem = apiManager.createItem(name);
            return newItem;
        }

        // Dictionary we pass to our drawable to do analytics
        private Dictionary<string, int> CountTagsInDefaultItems()
        {
            Dictionary<string, int> tagCount = new Dictionary<string, int>();

            foreach (var item in defaultItems)
            {
                // Split the tags using comma as the separator
                var tagString = item.TagString;

                // Split the TagString into individual tags
                var tags = tagString.Split(',');

                // Iterate through each tag and update the count in the dictionary
                foreach (var tag in tags)
                {
                    // Trim the tag to remove leading or trailing spaces
                    var trimmedTag = tag.Trim();
                    if (tagCount.ContainsKey(trimmedTag))
                    {
                        tagCount[trimmedTag]++;
                    } 
                    else {
                        tagCount[trimmedTag] = 1;
                    }
                }
            }

            return tagCount;
        }


        // Create our analytics/graphics page
        private async void analyticsButton_Clicked(object sender, EventArgs e)
        {
            Dictionary<string, int> itemTags = CountTagsInDefaultItems();
            Analytics analyticsPage = new Analytics(itemTags);
            await Navigation.PushModalAsync(analyticsPage);
        }
    }
}




