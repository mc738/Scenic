namespace Scenic.Editor

open Avalonia
open Avalonia.Controls.ApplicationLifetimes
open Avalonia.Markup.Xaml
open CommonResourceFormats.AssetStore
open CommonResourceFormats.AssetStore.Store
open Microsoft.Extensions.DependencyInjection
open Microsoft.Extensions.Logging
open Scene.Core
open Scenic.Editor.Core

type App() =
    inherit Application()

    override this.Initialize() = AvaloniaXamlLoader.Load(this)

    override this.OnFrameworkInitializationCompleted() =
        let serviceProvider =
            ServiceCollection()
                .AddLogging(fun builder -> builder.SetMinimumLevel(LogLevel.Trace).AddConsole() |> ignore)
                .AddSingleton<ScenicContext>(fun builder ->
                    ScenicContext.Initialize(@"C:\Users\mclif\Projects\data\Scenic\Dev"))
                .BuildServiceProvider()

        let ctx =
            { ServiceProvider = serviceProvider
              LoggerFactory = serviceProvider.GetService<ILoggerFactory>()
              ScenicContext = serviceProvider.GetService<ScenicContext>() }

        match this.ApplicationLifetime with
        | :? IClassicDesktopStyleApplicationLifetime as desktop -> desktop.MainWindow <- MainWindow(ctx)
        | _ -> ()

        base.OnFrameworkInitializationCompleted()
