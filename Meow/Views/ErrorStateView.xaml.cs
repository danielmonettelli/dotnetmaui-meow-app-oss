using System.Windows.Input;

namespace Meow.Views;

public partial class ErrorStateView : ContentView
{
    public static readonly BindableProperty ErrorTypeProperty =
        BindableProperty.Create(nameof(ErrorType), typeof(ErrorDisplayType), typeof(ErrorStateView),
            ErrorDisplayType.NoInternet, propertyChanged: OnErrorTypeChanged);

    public static readonly BindableProperty RetryCommandProperty =
        BindableProperty.Create(nameof(RetryCommand), typeof(ICommand), typeof(ErrorStateView),
            propertyChanged: OnRetryCommandChanged);

    public static readonly BindableProperty ErrorMessageProperty =
        BindableProperty.Create(nameof(ErrorMessage), typeof(string), typeof(ErrorStateView),
            default(string), propertyChanged: OnErrorMessageChanged);

    public ErrorStateView()
    {
        InitializeComponent();
        UpdateVisualState(ErrorDisplayType.NoInternet);
    }

    public ErrorDisplayType ErrorType
    {
        get => (ErrorDisplayType)GetValue(ErrorTypeProperty);
        set => SetValue(ErrorTypeProperty, value);
    }

    public ICommand RetryCommand
    {
        get => (ICommand)GetValue(RetryCommandProperty);
        set => SetValue(RetryCommandProperty, value);
    }

    public string ErrorMessage
    {
        get => (string)GetValue(ErrorMessageProperty);
        set => SetValue(ErrorMessageProperty, value);
    }

    private static void OnErrorTypeChanged(BindableObject bindable, object oldValue, object newValue)
    {
        if (bindable is ErrorStateView view && newValue is ErrorDisplayType errorType)
            view.UpdateVisualState(errorType);
    }

    private static void OnRetryCommandChanged(BindableObject bindable, object oldValue, object newValue)
    {
        if (bindable is ErrorStateView view && newValue is ICommand command && view.RetryButton != null)
            view.RetryButton.Command = command;
    }

    private static void OnErrorMessageChanged(BindableObject bindable, object oldValue, object newValue)
    {
        if (bindable is ErrorStateView view && newValue is string message && !string.IsNullOrEmpty(message) && view.DescriptionLabel != null)
            view.DescriptionLabel.Text = message;
    }

    private void UpdateVisualState(ErrorDisplayType errorType)
    {
        if (OfflinePill == null || ErrorPill == null || TitleLabel == null || DescriptionLabel == null)
            return;
        switch (errorType)
        {
            case ErrorDisplayType.NoInternet:
                OfflinePill.IsVisible = true;
                ErrorPill.IsVisible = false;
                TitleLabel.Text = "No Connection";
                DescriptionLabel.Text = "It looks like you're offline. Check your connection and try again to see adorable cats!";
                break;

            case ErrorDisplayType.ApiError:
                OfflinePill.IsVisible = false;
                ErrorPill.IsVisible = true;
                TitleLabel.Text = "Something Went Wrong";
                DescriptionLabel.Text = "We couldn't reach the Cat API right now. The cats are taking a nap! Please try again in a moment.";
                break;

            case ErrorDisplayType.Empty:
                OfflinePill.IsVisible = false;
                ErrorPill.IsVisible = false;
                TitleLabel.Text = "No Cats Found";
                DescriptionLabel.Text = "We couldn't find any cats to show you. Try again later!";
                break;
        }
    }
}

public enum ErrorDisplayType
{
    NoInternet,
    ApiError,
    Empty
}
