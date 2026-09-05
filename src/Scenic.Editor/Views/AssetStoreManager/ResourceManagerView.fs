namespace Scenic.Editor.Views.AssetStoreManager

open Avalonia.Controls

type ResourceManagerView() as this =
    inherit Border()
    
    let dockPanel = DockPanel()
    
    let menu = Menu()
    
    let listingsPanel = StackPanel()
    
    do
        
        menu.Items.Add(new MenuItem(Header = "New")) |> ignore
        
        DockPanel.SetDock(menu, Dock.Top)
        DockPanel.SetDock(listingsPanel, Dock.Left)
        
        let layout = StackPanel()
        
        dockPanel.Children.Add(listingsPanel)
        
        layout.Children.Add(menu)
        layout.Children.Add(dockPanel)
        
        this.Child <- layout


