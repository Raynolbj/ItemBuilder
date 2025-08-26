namespace ItemBuilder;

public partial class Settings : ContentPage
{
	public Settings()
	{
		InitializeComponent();

        // Preferences
        if (!Preferences.ContainsKey("MusicMode"))
        {
            Preferences.Set("MusicMode", true);
        }

        if (!Preferences.ContainsKey("sliderValue"))
        {
            Preferences.Set("sliderValue", 0.5);
        }

        musicSwitch.IsToggled = Preferences.Get("MusicMode", true);
        slider.Value = Preferences.Get("sliderValue", 0.5);
	}

    // Update volume within preferences and our player
    private void Slider_ValueChanged(object sender, ValueChangedEventArgs e)
    {
        double roundedValue = Math.Round(e.NewValue * 10) / 10; // Round to the nearest 0.1
        slider.Value = roundedValue; // Set the rounded value back to the slider

        Preferences.Set("sliderValue", slider.Value);

        // Check the state of the MusicSwitch toggle
        if (musicSwitch.IsToggled)
        {
            // Call a static method on MainPage to update the player state
            MainPage.UpdatePlayerState();
        }
    }

    // Update Player.playing based on preferences, and update preferences if changed
    private void MusicSwitch_Toggled(object sender, ToggledEventArgs e)
    {
        Preferences.Set("MusicMode", musicSwitch.IsToggled);

        // Call a static method on MainPage to update the player state
        MainPage.UpdatePlayerState();
    }
}