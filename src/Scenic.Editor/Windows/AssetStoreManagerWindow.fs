namespace Scenic.Editor.Windows

open Avalonia.Controls
open Avalonia.Layout
open Avalonia.Markup.Xaml
open Scenic.Editor.Core
open Scenic.Editor.Views
open Scenic.Editor.Views.AssetStoreManager

type AssetStoreManagerWindow(ctx: EditorContext) as this =
    inherit Window()
    
    let grid = Grid()
    let tabControl = TabControl()
    
    do
        
        grid.VerticalAlignment <- VerticalAlignment.Stretch
        grid.HorizontalAlignment <- HorizontalAlignment.Stretch
        
        tabControl.VerticalAlignment <- VerticalAlignment.Stretch
        tabControl.HorizontalAlignment <- HorizontalAlignment.Stretch
        
        
        tabControl.Items.Add(new TabItem(Header = "Components", Content = ComponentManagerView(ctx, this))) |> ignore
        tabControl.Items.Add(new TabItem(Header = "Assets", Content = AssetManagerView(ctx, this))) |> ignore
        tabControl.Items.Add(new TabItem(Header = "Resources", Content = ResourceManagerView())) |> ignore
        tabControl.Items.Add(new TabItem(Header = "Settings", Content = SettingsView())) |> ignore
        
        
        grid.Children.Add(tabControl)
        
        this.Content <- grid
        


