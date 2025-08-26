using ItemBuilder.Drawables;

namespace ItemBuilder;

public partial class Analytics : ContentPage
{
    public Dictionary<string, int> TagCounts { get; set; }

    public Analytics(Dictionary<string, int> tagCounts)
	{
		InitializeComponent();
        TagCounts = tagCounts;
	}

    protected override void OnAppearing()
    {
        base.OnAppearing();

        // Create an instance of ChartDrawable with the TagCounts property
        ChartDrawable chartDrawable = new ChartDrawable(TagCounts);

        // Set the drawable of the graphics view to the created ChartDrawable
        graphics.Drawable = chartDrawable;

        // Invalidate the graphics to trigger a redraw
        graphics.Invalidate();
    }

    private async void returnButtonClicked(object sender, EventArgs e)
    {
        await Navigation.PopModalAsync();
    }
}