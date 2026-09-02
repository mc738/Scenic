namespace Scenic.Editor.Views.AssetStoreManager

open Avalonia.Controls
open Avalonia.Interactivity
open Scenic.Editor.Views
open Scenic.Editor.Windows

type AssetManagerView(parent: Window) as this =
    inherit Border()

    let dockPanel = DockPanel()

    let menu = Menu()

    let listingsPanel = StackPanel()

    do
        let mi = new MenuItem(Header = "New")
        
        mi.Click.Add(this.AddNewAsset)

        menu.Items.Add(mi)
        |> ignore

        DockPanel.SetDock(menu, Dock.Top)
        DockPanel.SetDock(listingsPanel, Dock.Left)

        let layout = StackPanel()

        dockPanel.Children.Add(listingsPanel)

        layout.Children.Add(menu)
        layout.Children.Add(dockPanel)

        this.Child <- layout

    
    member _.AddNewAsset(e: RoutedEventArgs) =
         async {
            let newAssetWindow = NewAssetWindow()
            
            let! refreshRequired = newAssetWindow.ShowDialog<bool>(parent) |> Async.AwaitTask
            
            if refreshRequired then
                // TODO handle refresh
                
                ()
            
            ()
             
             
         }
         |> Async.StartImmediate