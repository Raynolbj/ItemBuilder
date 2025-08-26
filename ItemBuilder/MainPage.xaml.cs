using SQLiteNetExtensions.Extensions;
using System.Collections.ObjectModel;
using Plugin.Maui.Audio;

namespace ItemBuilder;

public partial class MainPage : ContentPage
{

    // Static instance variable to hold the current instance of MainPage
    private static MainPage _instance;

    public ObservableCollection<Build> Builds = new ObservableCollection<Build>();
    bool playSong;
    public IAudioManager audioManager;
    public IAudioPlayer player;
    ApiManager apiManager;

    public MainPage()
    {
        InitializeComponent();
        _instance = this;
        apiManager = new ApiManager();
        InitializeAsync();
    }

    public static MainPage Instance
    {
        get { return _instance; }
    }

    // Load up the data
    private async Task InitializeAsync()
    {
        // Initialize champion data asynchronously
        await apiManager.InitializeChampDataAsync();

        if (audioManager == null)
        {
            audioManager = new AudioManager();
        }
        if (player == null)
        {
            player = audioManager.CreatePlayer(await FileSystem.OpenAppPackageFileAsync("BrassV3ST.wav"));
        }

        playSong = Preferences.Get("MusicMode", true);
        player.Volume = Preferences.Get("sliderValue", 0.0);
        player.Loop = true;

        if (playSong)
        {
            player.Play();
        }

        // Fetch builds from the database with associated champions
        var buildsWithChampions = DB.conn.GetAllWithChildren<Build>(recursive: true);

        // Populate Builds ObservableCollection with the fetched data
        Builds = new ObservableCollection<Build>(buildsWithChampions);

        // Set the ListView's ItemsSource to the populated ObservableCollection
        Lv.ItemsSource = Builds;
    }

    // Reset on appearing
    protected override void OnAppearing()
    {
        base.OnAppearing();
        ResetListViewSources();
    }

    // Reset the listview with updated data
    private void ResetListViewSources()
    {
        var builds = DB.conn.GetAllWithChildren<Build>(recursive: true);
        Builds = new ObservableCollection<Build>(builds.ToList());
        Lv.ItemsSource = Builds;
    }

    // Listview selections; takes you to update or delete builds
    private async void Lv_ItemSelected(object sender, SelectedItemChangedEventArgs e)
    {

        if (Lv.SelectedItem == null)
        {
            return;
        }

        string input = await DisplayActionSheet("Select Action", "Cancel", null, "Update Build", "Delete Build");
        Build oldBuild = Lv.SelectedItem as Build;

        // Ask the user if they wish to update or delete the selected build
        if (input == "Update Build")
        {
            await Navigation.PushModalAsync(await AddPage.CreateAsync(oldBuild));
        } 
        else if (input == "Delete Build")
        {
            bool accepted = await DisplayAlert("Warning", "Are you sure you want to delete this build?", "Yes", "No");

            if (accepted)
            {
                if (oldBuild != null)
                {
                    int v = DB.conn.Delete(oldBuild);
                    if (v > 0)
                    {
                        ResetListViewSources();
                    }
                }
                await DisplayAlert("Confirmed", "Build Deleted", "OK");
            }
        }
    }

    // Searchbar functionality
    private void OnSearchButtonPressed(object sender, EventArgs e)
    {
        string searchText = searchBar.Text.Trim(); // Trim leading and trailing spaces

        if (string.IsNullOrWhiteSpace(searchText))
        {
            ResetListViewSources(); // Use the original data if the search text is empty
        }
        else
        {
            // Fetch builds from the database with associated champions
            var buildsWithChampions = DB.conn.GetAllWithChildren<Build>(recursive: true);

            // Trim leading and trailing spaces from build title and champion name
            var filteredBuilds = buildsWithChampions
                .Where(build =>
                    (build.Title != null && build.Title.Trim().Contains(searchText, StringComparison.OrdinalIgnoreCase)) ||
                    (build.Champ != null && build.Champ.Name.Replace("'", "").Trim().Contains(searchText, StringComparison.OrdinalIgnoreCase)) ||
                    (build.Champ != null && build.Champ.Name.Trim().Contains(searchText, StringComparison.OrdinalIgnoreCase)))
                .ToList();

            Lv.ItemsSource = new ObservableCollection<Build>(filteredBuilds);
        }

        searchBar.Text = null;
    }


    // ButtonClick functions, mostly navigators
    private async void AddButton_Clicked(object sender, EventArgs e)
    {
        var addPage = await AddPage.CreateAsync();
        await Navigation.PushModalAsync(addPage);
    }
    private async void settingsButton_Clicked(object sender, EventArgs e)
    {
        await Navigation.PushAsync(new Settings(), true);
    }


    // Update the player based on user preference
    public static void UpdatePlayerState()
    {
        if (Instance != null)
        {
            Instance.UpdatePlayerStateInternal();
        }
    }

    private void UpdatePlayerStateInternal()
    {
        playSong = Preferences.Get("MusicMode", true);
        player.Volume = Preferences.Get("sliderValue", 0.0);

        if (playSong && !player.IsPlaying)
        {
            player.Play();
        }
        else
        {
            player.Pause();
        }
    }

    // Create Champions for the build
    public Champion Champion(string name)
    {
        Champion newChamp = apiManager.CreateChampion(name);
        return newChamp;
    }

}

