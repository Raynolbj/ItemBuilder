using Microsoft.Maui.Graphics;
using System;
using System.Collections.Generic;

namespace ItemBuilder.Drawables;

public class ChartDrawable : IDrawable
{
    public Dictionary<string, int> TagCounts { get; set; }
    private Color[] colors = { Colors.IndianRed, Colors.Khaki, Colors.Snow, Colors.SteelBlue, Colors.MediumAquamarine, Colors.LightGoldenrodYellow, Colors.SlateGrey, Colors.SeaShell };

    public ChartDrawable()
    {
    }

    public ChartDrawable(Dictionary<string, int> tagCounts)
    {
        TagCounts = tagCounts;
    }

    public void Draw(ICanvas canvas, RectF dirtyRect)
    {
        float W = dirtyRect.Width;
        float H = dirtyRect.Height;

        canvas.StrokeColor = Colors.Black;
        canvas.StrokeSize = 3;
        canvas.DrawRectangle(0, 0, W, H);

        if (TagCounts == null || TagCounts.Count == 0)
        {
            return;
        }

        int numTags = TagCounts.Count;

        float widthPerTag = W / numTags;
        int maxCount = TagCounts.Values.Max();

        float leftOfTag = 0;
        int colorIndex = 0;

        foreach (var tagCount in TagCounts)
        {
            const float BarPercOfInterval = 0.9f;
            const float EmptyPercOfInterval = 1.0f - BarPercOfInterval;

            canvas.FillColor = colors[colorIndex % colors.Length];

            float barWidth = widthPerTag * BarPercOfInterval;
            float barHeight = (float)tagCount.Value / maxCount * H;
            float barLeft = leftOfTag + EmptyPercOfInterval / 2 * widthPerTag;
            float barTop = H - barHeight;

            canvas.FillRectangle(barLeft, barTop, barWidth, barHeight);

            // Extract and display only the capital letters of the tag
            string abbreviatedLabel = new string(tagCount.Key.Where(char.IsUpper).ToArray());

            canvas.FontColor = Colors.Black;
            canvas.FontSize = 12;
            canvas.DrawString(abbreviatedLabel, barLeft, H - 20, barWidth, 20,
                              HorizontalAlignment.Center, VerticalAlignment.Center);

            leftOfTag += widthPerTag;
            colorIndex++;
        }
    }

}

