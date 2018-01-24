using System;
using Xamarin.UITest;

public static class AppExtensions
{
    public static void PerformBack(this IApp app, string identifier)
    {
        app.Tap(identifier);
    }
}
