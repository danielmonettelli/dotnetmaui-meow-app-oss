namespace Meow.Controls;

public class CustomStringSplitterView : ContentView
{
    private StackLayout itemsLayout;

    public static readonly BindableProperty InputTextProperty =
        BindableProperty.Create(nameof(InputText), typeof(string), typeof(CustomStringSplitterView), null, propertyChanged: OnInputTextChanged);

    public string InputText
    {
        get { return (string)GetValue(InputTextProperty); }
        set { SetValue(InputTextProperty, value); }
    }

    public CustomStringSplitterView()
    {
        itemsLayout = new StackLayout
        {
            Spacing = 6,
            Orientation = StackOrientation.Horizontal
        };

        Content = new ScrollView
        {
            HorizontalScrollBarVisibility = ScrollBarVisibility.Never,
            Orientation = ScrollOrientation.Horizontal,
            Content = itemsLayout
        };
    }

    private static void OnInputTextChanged(BindableObject bindable, object oldValue, object newValue)
    {
        if (bindable is CustomStringSplitterView splitter)
        {
            splitter.UpdateItems();
        }
    }

    private void UpdateItems()
    {
        itemsLayout.Children.Clear();

        if (!string.IsNullOrWhiteSpace(InputText))
        {
            string cleanedText = InputText.Replace(",", "");
            string[] items = cleanedText.Split(' ');

            foreach (string item in items)
            {
                Label label = new()
                {
                    Text = item,
                    HorizontalTextAlignment = TextAlignment.Center,
                    VerticalTextAlignment = TextAlignment.Center

                };
                label.SetAppThemeColor(Label.TextColorProperty, Color.FromArgb("#703EDB"), Color.FromArgb("#9F79F1"));

                Border border = new()
                {
                    Content = label,
                    Padding = new Thickness(8, 6, 8, 6),
                    StrokeShape = new RoundRectangle
                    {
                        CornerRadius = new CornerRadius(7, 7, 7, 7)
                    },
                    StrokeThickness = 0,
                };
                border.SetAppThemeColor(Border.BackgroundColorProperty, Color.FromArgb("#F4F0FC"), Color.FromArgb("#1C0F37"));

                itemsLayout.Children.Add(border);
            }
        }
    }
}
