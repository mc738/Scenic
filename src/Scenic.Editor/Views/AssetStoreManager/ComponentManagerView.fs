namespace Scenic.Editor.Views.AssetStoreManager

open Avalonia.Controls
open Avalonia.Interactivity
open Avalonia.Layout
open CommonResourceFormats.AssetStore.Core.Domain
open Scenic.Editor.AssetStoreManager.Dialogs
open Scenic.Editor.Core

type ComponentManagerView(ctx: EditorContext, parent: Window) as this =
    inherit Border()
    
    let dockPanel = DockPanel()
    
    let menu = Menu()
    
    let mutable listings = Operators.Unchecked.defaultof<EntityListings>
    
      
    let listingsBox = ListBox()
    
    let listingsPanel = StackPanel()
    
    do
        
        let mi = new MenuItem(Header = "New")
        
        mi.Click.Add(this.AddNewComponent)
        this.HorizontalAlignment <- HorizontalAlignment.Stretch
        this.VerticalAlignment <- VerticalAlignment.Stretch
        
        dockPanel.VerticalAlignment <- VerticalAlignment.Stretch
        dockPanel.HorizontalAlignment <- HorizontalAlignment.Stretch

        menu.Items.Add(mi)
        |> ignore
        
        
        DockPanel.SetDock(menu, Dock.Top)
        DockPanel.SetDock(listingsPanel, Dock.Left)
        
        //let layout = StackPanel()
        
        //dockPanel.Children.Add(listingsPanel)
        
        //layout.Children.Add(menu)
        //layout.Children.Add(dockPanel)
        
        listingsPanel.Width <- 250
        
        //this.Background <- SolidColorBrush(Color(255uy, 0uy, 255uy, 255uy))
        
        let layout = StackPanel()
        layout.HorizontalAlignment <- HorizontalAlignment.Stretch
        layout.VerticalAlignment <- VerticalAlignment.Stretch

        listingsPanel.VerticalAlignment <- VerticalAlignment.Stretch
        listingsPanel.HorizontalAlignment <- HorizontalAlignment.Stretch
        
        listingsPanel.Children.Add(listingsBox)
        
        dockPanel.Children.Add(listingsPanel)

        layout.Children.Add(menu)
        layout.Children.Add(dockPanel)
        dockPanel.Children.Add(TextBlock(Text=  "Hello, World"))

        this.Child <- layout
        
        this.PopulateListings()

    member _.PopulateListings() =
        listings <- ctx.ScenicContext.AssetStore.GetComponentListings()
        listingsBox.Items.Clear()
        
        for listing in listings.Entities do
            listingsBox.Items.Add(new ListBoxItem(Content = listing.Name, Name = listing.Name, DataContext = listing.Id)) |> ignore
       
         
    member _.AddNewComponent(e: RoutedEventArgs) =
         async {
            let newComponentWindow = NewComponentWindow()
            
            let! newComponent = newComponentWindow.ShowDialog<NewComponent option>(parent) |> Async.AwaitTask
            
            match newComponent with
            | None -> ()
            | Some value ->
                ctx.ScenicContext.AssetStore.AddNewComponent value
            
                // TODO make better, just add to existing list?
                this.PopulateListings() 
         }
         |> Async.StartImmediate 