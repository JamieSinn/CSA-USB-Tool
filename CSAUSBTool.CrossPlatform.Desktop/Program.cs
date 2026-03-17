using System;
using System.Linq;

using Avalonia;
using Avalonia.ReactiveUI;
using CSAUSBTool.CrossPlatform.Core;

namespace CSAUSBTool.CrossPlatform.Desktop;

class Program
{
    // Initialization code. Don't use any Avalonia, third-party APIs or any 
    // SynchronizationContext-reliant code before AppMain is called: things aren't initialized
    // yet and stuff might break.
    [STAThread]
    public static void Main(string[] args)
    {
        RuntimeOverrides.EnableSettingFromArg = args.Any(a =>
            string.Equals(a, "--showsetting", StringComparison.OrdinalIgnoreCase));

        BuildAvaloniaApp().StartWithClassicDesktopLifetime(args);
    }

    // Avalonia configuration, don't remove; also used by visual designer.
    public static AppBuilder BuildAvaloniaApp()
        => AppBuilder.Configure<App>()
            .UsePlatformDetect()
            .WithInterFont()
            .LogToTrace()
            .UseReactiveUI();
}
