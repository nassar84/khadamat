using System;
using Android.Webkit;
using Microsoft.Maui.Handlers;
using Microsoft.Maui.Platform;
using AWebView = Android.Webkit.WebView;

namespace Khadamat.MobileApp.Platforms.Android;

public class CustomMauiWebViewClient : MauiWebViewClient
{
    public CustomMauiWebViewClient(WebViewHandler handler) : base(handler)
    {
    }

    public override void OnReceivedSslError(AWebView? view, SslErrorHandler? handler, global::Android.Net.Http.SslError? error)
    {
        var url = error?.Url ?? view?.Url ?? "";
        Console.WriteLine($"[CustomMauiWebViewClient] SSL Error encountered for: {url} | PrimaryError: {error?.PrimaryError}");

        // Automatically proceed for our known server domains or private CA environments
        if (url.Contains("eis-dev.com", StringComparison.OrdinalIgnoreCase) ||
            url.Contains("khadamawy", StringComparison.OrdinalIgnoreCase) ||
            url.Contains("site4now.net", StringComparison.OrdinalIgnoreCase) ||
            url.Contains("localhost", StringComparison.OrdinalIgnoreCase) ||
            url.Contains("10.0.2.2", StringComparison.OrdinalIgnoreCase))
        {
            Console.WriteLine($"[CustomMauiWebViewClient] Proceeding with navigation despite SSL certificate warning for: {url}");
            handler?.Proceed();
        }
        else
        {
            base.OnReceivedSslError(view, handler, error);
        }
    }
}
