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
    
    
    let mainView = Grid()
    
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
        
        listingsPanel.Width <- 250
        
        let layout = StackPanel()
        layout.HorizontalAlignment <- HorizontalAlignment.Stretch
        layout.VerticalAlignment <- VerticalAlignment.Stretch

        listingsPanel.VerticalAlignment <- VerticalAlignment.Stretch
        listingsPanel.HorizontalAlignment <- HorizontalAlignment.Stretch
        
        listingsPanel.Children.Add(listingsBox)
        
        listingsBox.SelectionChanged.Add(this.SelectionChanged)
        
        
        dockPanel.Children.Add(listingsPanel)

        layout.Children.Add(menu)
        layout.Children.Add(dockPanel)
        dockPanel.Children.Add(mainView)

        this.Child <- layout
        
        this.PopulateListings()

    member _.PopulateListings() =
        listings <- ctx.ScenicContext.AssetStore.GetComponentListings()
        listingsBox.Items.Clear()
        
        for listing in listings.Entities do
            listingsBox.Items.Add(new ListBoxItem(Content = listing.Name, Name = listing.Name, DataContext = listing)) |> ignore
       
         
    member _.AddNewComponent(e: RoutedEventArgs) =
         async {
            let newComponentWindow = NewComponentWindow(ctx.WorkflowHandlers.Components)
            
            let! newComponent = newComponentWindow.ShowDialog<NewComponent option>(parent) |> Async.AwaitTask
            
            match newComponent with
            | None -> ()
            | Some value ->
                ctx.ScenicContext.AssetStore.AddNewComponent value
            
                // TODO make better, just add to existing list?
                this.PopulateListings() 
         }
         |> Async.StartImmediate
    
    member _.SelectionChanged(e: SelectionChangedEventArgs) =
        if e.AddedItems.Count = 0 then

            ()
        else

            let lbi = e.AddedItems[0] :?> ListBoxItem

            let eli = lbi.DataContext :?> EntitiesListingItem

            // Get asset
            let versionId = eli.Versions |> List.maxBy (fun v -> v.Version) |> _.Id

            match ctx.ScenicContext.AssetStore.GetComponentVersion(versionId) with
            | Error errorValue -> printfn $"*********** {errorValue}"
            | Ok resultValue ->
                let comp = resultValue.Component

                match ctx.WorkflowHandlers.Components.Factories.TryFind comp.ComponentType with
                | None ->
                    // TODO: Display raw preview

                    failwith "todo"
                | Some wff ->
                    let control = wff.CreateEditWorkflowControl ctx.ScenicContext parent comp
                    mainView.Children.Clear()

                    mainView.Children.Add(control)
                    ()
