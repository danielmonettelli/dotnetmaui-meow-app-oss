using FluentAssertions;
using Meow.Controls;
using Microsoft.Maui.Graphics;
using Moq;

namespace Meow.Tests.Controls;

public class RatingCanvasTests
{
    private const string PawShapePath = "M9.41914 9.33216C11.2145 10.2051 13.0015 7.46695 13.076 5.48238C13.1422 3.49035 12.5124 1.83354 11.4783 0.841256C10.4441 -0.151031 9.40379 -0.0974449 8.37858 0.158078C7.61571 0.348216 7.01159 0.737309 6.68893 1.66245C6.51519 2.1474 6.39109 2.78903 6.39109 3.61718C6.39109 4.32596 6.49865 5.02727 6.68893 5.69874C7.15224 7.31774 8.14504 8.72037 9.41914 9.33216ZM16.8569 9.83203C18.3626 10.2125 19.8353 8.53385 20.7288 6.82533C21.1425 6.02702 21.432 5.22871 21.5396 4.61693C21.7299 3.55003 21.611 1.51075 20.6021 0.707804C19.5971 -0.091996 17.8659 0.150044 16.7198 0.769698C15.5059 1.426 15.0615 2.25932 14.7141 4.43041C14.3749 6.6015 14.6562 9.27247 16.8569 9.83203ZM20.7371 11.2048C21.6555 9.9514 22.9958 9.08595 22.9958 9.08595C25.544 7.59379 26.9918 9.45899 27.0001 11.8763C27.0001 14.301 24.4519 16.9123 22.4497 16.9123C21.7134 16.9123 21.1508 16.7183 20.7371 16.3528C20.0174 15.7261 19.7609 14.6069 19.8932 13.1894C19.9594 12.4732 20.3069 11.7942 20.7371 11.2048ZM6.68069 14.1518C7.00335 13.5848 7.06126 12.7641 6.8627 11.5704C6.82133 11.3167 6.75515 11.078 6.68069 10.8392C5.98572 8.77259 3.91954 7.08248 2.39394 7.06694C0.30804 7.04571 -0.508869 10.1379 0.318468 12.0031C1.1458 13.8683 2.73429 15.547 4.66198 15.2336C5.66306 15.077 6.3332 14.7636 6.68069 14.1518ZM16.0297 13.1222C17.0639 13.9504 19.3804 16.3304 20.729 18.8596C21.002 19.3744 21.2336 19.8892 21.4074 20.3965C21.8459 21.7096 21.9585 22.9354 21.1725 23.4875C20.1218 24.2261 17.8416 23.9926 15.9552 23.3212C13.5395 22.4552 12.0488 22.9703 10.641 23.4568C10.1203 23.6367 9.61093 23.8127 9.0703 23.9134C9.02881 23.9211 8.93256 23.9489 8.79555 23.9884C8.00352 24.2165 5.8491 24.8373 5.03324 24.2514C3.7607 23.3376 3.37144 22.8213 4.72827 19.837C5.16676 18.882 5.87827 17.927 6.68078 17.0392C7.51077 16.124 8.00602 15.413 8.40229 14.844C9.01725 13.9611 9.39386 13.4203 10.4132 12.9903C12.1105 12.2743 14.7887 12.13 16.0297 13.1222Z";

    [Fact]
    public void PawPath_ShouldHaveValidGeometry()
    {
        var pathBuilder = new PathBuilder();
        var path = pathBuilder.BuildPath(PawShapePath);

        path.Should().NotBeNull();
        path.Bounds.Width.Should().BeGreaterThan(0);
        path.Bounds.Height.Should().BeGreaterThan(0);

        // Record bounds for inspection
        var b = path.Bounds;
        b.Width.Should().BeGreaterThan(20);
    }

    [Theory]
    [InlineData(0.0, 0)]
    [InlineData(1.0, 1)]
    [InlineData(3.0, 3)]
    [InlineData(4.0, 4)]
    [InlineData(5.0, 5)]
    public void Draw_ShouldFillExactNumberOfRatedPaws(double ratingValue, int expectedFilledPaws)
    {
        var canvasMock = new Mock<ICanvas>();
        var filledColors = new List<Color>();

        canvasMock.SetupSet(c => c.FillColor = It.IsAny<Color>())
            .Callback<Color>(color => filledColors.Add(color));

        var ratingCanvas = new RatingCanvas
        {
            ShapePath = PawShapePath,
            ItemCount = 5,
            ItemSize = 16f,
            ItemSpacing = 6f,
            Value = ratingValue,
            RatedFillColor = Color.FromArgb("#FF703EDB"),
            UnRatedFillColor = Color.FromArgb("#D3D3D3")
        };

        ratingCanvas.Draw(canvasMock.Object, new RectF(0, 0, 106, 18));

        // In the original control, all items render unrated background, then rated items render on top
        int ratedFillCalls = filledColors.Count(c => c == ratingCanvas.RatedFillColor);
        ratedFillCalls.Should().Be(expectedFilledPaws);

        int unratedFillCalls = filledColors.Count(c => c == ratingCanvas.UnRatedFillColor);
        unratedFillCalls.Should().Be(5);
    }

    [Fact]
    public void Draw_Translations_ShouldExecuteForAllItems()
    {
        var canvasMock = new Mock<ICanvas>();
        var translations = new List<(float X, float Y)>();

        canvasMock.Setup(c => c.Translate(It.IsAny<float>(), It.IsAny<float>()))
            .Callback<float, float>((x, y) => translations.Add((x, y)));

        var ratingCanvas = new RatingCanvas
        {
            ShapePath = PawShapePath,
            ItemCount = 5,
            ItemSize = 16f,
            ItemSpacing = 6f,
            Value = 5,
            RatedFillColor = Color.FromArgb("#FF703EDB"),
            UnRatedFillColor = Color.FromArgb("#D3D3D3")
        };

        ratingCanvas.Draw(canvasMock.Object, new RectF(0, 0, 104, 16));

        translations.Should().HaveCount(5);
    }

    [Fact]
    public void Draw_PartialRating_ShouldClipRectangle()
    {
        var canvasMock = new Mock<ICanvas>();
        int clipRectCalls = 0;

        canvasMock.Setup(c => c.ClipRectangle(It.IsAny<float>(), It.IsAny<float>(), It.IsAny<float>(), It.IsAny<float>()))
            .Callback<float, float, float, float>((x, y, w, h) =>
            {
                clipRectCalls++;
                w.Should().BeGreaterThan(0);
                h.Should().BeGreaterThan(0);
            });

        var ratingCanvas = new RatingCanvas
        {
            ShapePath = PawShapePath,
            ItemCount = 5,
            ItemSize = 16f,
            ItemSpacing = 6f,
            Value = 3.5, // 3 full + 0.5 partial
            RatedFillColor = Color.FromArgb("#FF703EDB"),
            UnRatedFillColor = Color.FromArgb("#D3D3D3")
        };

        ratingCanvas.Draw(canvasMock.Object, new RectF(0, 0, 104, 16));

        // Full items 0, 1, 2 are drawn directly without clipping
        // Only partial item 3 triggers ClipRectangle!
        clipRectCalls.Should().Be(1);
    }
}
