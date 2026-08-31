namespace Scenic.Editor

open Avalonia
open Avalonia.Controls.ApplicationLifetimes
open Avalonia.Markup.Xaml
open CommonResourceFormats.AssetStore
open CommonResourceFormats.AssetStore.Store
open Microsoft.Extensions.DependencyInjection
open Microsoft.Extensions.Logging
open Scenic.Editor.Core

type App() =
    inherit Application()

    override this.Initialize() = AvaloniaXamlLoader.Load(this)

    override this.OnFrameworkInitializationCompleted() =
        let serviceProvider =
            ServiceCollection()
                .AddLogging(fun builder -> builder.SetMinimumLevel(LogLevel.Trace).AddConsole() |> ignore)
                .AddSingleton<AssetStoreContext>(fun builder ->
                    AssetStoreContext(@"C:\Users\mclif\Projects\data\Scenic\Dev\asset_store.db"))
                .BuildServiceProvider()

        let ctx =
            { ServiceProvider = serviceProvider
              LoggerFactory = serviceProvider.GetService<ILoggerFactory>()
              AssetStoreContext = serviceProvider.GetService<AssetStoreContext>() }

        match this.ApplicationLifetime with
        | :? IClassicDesktopStyleApplicationLifetime as desktop -> desktop.MainWindow <- MainWindow(ctx)
        | _ -> ()

        base.OnFrameworkInitializationCompleted()
