// MAUI Rating View Control is the work of Naweed Akram.
// The repository of his project is available at the following link:
// https://github.com/naweed/Maui.Controls.RatingView
using Microsoft.Maui.Graphics;

namespace Meow.Controls;

internal class RatingCanvas : IDrawable
{
    #region Properties

    public int ItemCount { get; set; } = 5;
    public float ItemSize { get; set; } = 16f;
    public float ItemSpacing { get; set; } = 6f;
    public double Value { get; set; }

    public Color RatedFillColor { get; set; } = Colors.Yellow;
    public Color UnRatedFillColor { get; set; } = Colors.LightGray;

    public Color StrokeColor { get; set; } = Colors.Transparent;
    public float StrokeWidth { get; set; }

    public string ShapePath { get; set; } = string.Empty;

    #endregion

    public void Draw(ICanvas canvas, RectF dirtyRect)
    {
        canvas.Antialias = true;

        if (ItemCount <= 0 || string.IsNullOrEmpty(ShapePath))
            return;

        //Draw each rating item
        for (int itemIndex = 0; itemIndex < ItemCount; itemIndex++)
        {
            DrawRatingItem(canvas, dirtyRect, itemIndex);
        }
    }

    private void DrawRatingItem(ICanvas canvas, RectF dirtyRect, int itemIndex)
    {
        canvas.SaveState();

        //Position the Shape in the Canvas
        canvas.Translate(itemIndex * ItemSize + itemIndex * ItemSpacing + StrokeWidth, StrokeWidth);

        //Build the shape
        var pathBuilder = new PathBuilder();
        var shapePath = pathBuilder.BuildPath(ShapePath);
        var divisor = shapePath.Bounds.Width < shapePath.Bounds.Height ? shapePath.Bounds.Width : shapePath.Bounds.Height;
        var scale = divisor > 0 ? (ItemSize - StrokeWidth) / divisor : 1f;
        var scaledShapePath = shapePath.AsScaledPath(scale);

        //Draw Empty Star as background
        DrawShape(canvas, scaledShapePath, StrokeColor, UnRatedFillColor, StrokeWidth);

        //Draw Filled Star
        if (itemIndex < Value)
        {
            if (itemIndex + 1 <= Value)
            {
                // Full paw - draw rated shape directly without any clipping
                DrawShape(canvas, scaledShapePath, StrokeColor, RatedFillColor, StrokeWidth);
            }
            else
            {
                // Partial fill - clip rectangle to fractional width
                canvas.SaveState();
                float fraction = Convert.ToSingle(Value - itemIndex);
                var bounds = scaledShapePath.Bounds;
                canvas.ClipRectangle(bounds.X, bounds.Y, bounds.Width * fraction, bounds.Height);
                DrawShape(canvas, scaledShapePath, StrokeColor, RatedFillColor, StrokeWidth);
                canvas.RestoreState();
            }
        }

        canvas.RestoreState();
    }

    private void DrawShape(ICanvas canvas, PathF shapePath, Color strokeColor, Color fillColor, float strokeWidth)
    {
        //Set item colors and strokes
        canvas.StrokeColor = strokeColor;
        canvas.StrokeSize = strokeWidth;
        canvas.FillColor = fillColor;

        //Draw the shape
        canvas.DrawPath(shapePath);
        canvas.FillPath(shapePath);
    }
}
