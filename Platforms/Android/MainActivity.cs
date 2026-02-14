using Android.App;
using Android.Content.PM;
using Android.OS;
using Android.Views;
using AndroidX.Activity;

namespace HospitalTriageAI;

[Activity(Theme = "@style/Maui.SplashTheme", MainLauncher = true, LaunchMode = LaunchMode.SingleTop, ConfigurationChanges = ConfigChanges.ScreenSize | ConfigChanges.Orientation | ConfigChanges.UiMode | ConfigChanges.ScreenLayout | ConfigChanges.SmallestScreenSize | ConfigChanges.Density)]
public class MainActivity : MauiAppCompatActivity
{
    private Android.Webkit.WebView? _webView;

    protected override void OnCreate(Bundle? savedInstanceState)
    {
        base.OnCreate(null);

        // Register back button callback using modern API
        OnBackPressedDispatcher.AddCallback(this, new BackPressedCallback(this));
    }

    protected override void OnResume()
    {
        base.OnResume();
        // Find WebView after the view is fully loaded
        _webView ??= FindWebViewInLayout();
    }

    public Android.Webkit.WebView? GetWebView() => _webView ??= FindWebViewInLayout();

    private Android.Webkit.WebView? FindWebViewInLayout()
    {
        try
        {
            var content = FindViewById<ViewGroup>(Android.Resource.Id.Content);
            return FindWebViewRecursive(content);
        }
        catch
        {
            return null;
        }
    }

    private Android.Webkit.WebView? FindWebViewRecursive(ViewGroup? parent)
    {
        if (parent == null) return null;

        for (int i = 0; i < parent.ChildCount; i++)
        {
            var child = parent.GetChildAt(i);

            if (child is Android.Webkit.WebView webView)
                return webView;

            if (child is ViewGroup viewGroup)
            {
                var found = FindWebViewRecursive(viewGroup);
                if (found != null) return found;
            }
        }

        return null;
    }

    private class BackPressedCallback : OnBackPressedCallback
    {
        private readonly MainActivity _activity;

        public BackPressedCallback(MainActivity activity) : base(true)
        {
            _activity = activity;
        }

        public override void HandleOnBackPressed()
        {
            var webView = _activity.GetWebView();

            if (webView != null && webView.CanGoBack())
            {
                webView.GoBack();
            }
            else
            {
                // Disable this callback temporarily and trigger default back behavior
                Enabled = false;
                _activity.OnBackPressedDispatcher.OnBackPressed();
                Enabled = true;
            }
        }
    }
}
