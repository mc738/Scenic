namespace Scenic.Editor.Views.AssetStoreManager

open Avalonia.Controls
open Avalonia.Interactivity
open Avalonia.Layout
open Avalonia.Media
open CommonResourceFormats.AssetStore.Core.Domain
open Scenic.Editor.Core
open Scenic.Editor.Views
open Scenic.Editor.Windows

type AssetManagerView(ctx: EditorContext, parent: Window) as this =
    inherit Border()


    let dockPanel = DockPanel()

    let menu = Menu()

    let mutable listings = Operators.Unchecked.defaultof<EntityListings>

    let listingsBox = ListBox()

    let listingsPanel = StackPanel()

    
    let mainView = Grid()
    
    do
        
        let mainViewBorder = Border()
        mainViewBorder.VerticalAlignment <- VerticalAlignment.Stretch
        mainViewBorder.HorizontalAlignment <- HorizontalAlignment.Stretch
        
        mainViewBorder.Child <- mainView
        
        mainView.VerticalAlignment <- VerticalAlignment.Stretch
        mainView.HorizontalAlignment <- HorizontalAlignment.Stretch
        
        
        
        let mi = new MenuItem(Header = "New")

        mi.Click.Add(this.AddNewAsset)
        this.HorizontalAlignment <- HorizontalAlignment.Stretch
        this.VerticalAlignment <- VerticalAlignment.Stretch

        dockPanel.VerticalAlignment <- VerticalAlignment.Stretch
        dockPanel.HorizontalAlignment <- HorizontalAlignment.Stretch

        menu.Items.Add(mi) |> ignore

        DockPanel.SetDock(menu, Dock.Top)
        DockPanel.SetDock(listingsPanel, Dock.Left)

        listingsPanel.Width <- 250

        //this.Background <- SolidColorBrush(Color(255uy, 0uy, 255uy, 255uy))



        let layout = StackPanel()
        layout.HorizontalAlignment <- HorizontalAlignment.Stretch
        layout.VerticalAlignment <- VerticalAlignment.Stretch

        listingsPanel.VerticalAlignment <- VerticalAlignment.Stretch
        listingsPanel.HorizontalAlignment <- HorizontalAlignment.Stretch

        listingsPanel.Children.Add(listingsBox)

        listingsPanel.Background <- SolidColorBrush(Color(255uy, 0uy, 255uy, 255uy))

        listingsBox.SelectionChanged.Add(this.SelectionChanged)
        listingsBox.SelectionMode <- SelectionMode.Single

        dockPanel.Children.Add(listingsPanel)

        layout.Children.Add(menu)
        layout.Children.Add(dockPanel)
        dockPanel.Children.Add(mainViewBorder)

        this.Child <- layout

        this.PopulateListings()

    member _.SelectionChanged(e: SelectionChangedEventArgs) =
        if e.AddedItems.Count = 0 then

            ()
        else

            let lbi = e.AddedItems[0] :?> ListBoxItem

            let eli = lbi.DataContext :?> EntitiesListingItem
            
            // Get asset
            let versionId = eli.Versions |> List.maxBy (fun v -> v.Version) |> _.Id
            
            
            match ctx.ScenicContext.AssetStore.GetAssetVersion(versionId) with
            | Error errorValue -> printfn $"*********** {errorValue}"
            | Ok resultValue ->
                match ctx.WorkflowHandlers.Assets.Factories.TryFind resultValue.AssetType with
                | None -> failwith "todo"
                | Some wff ->
                    let preview = wff.CreatePreviewAssetWorkflowControl ctx.ScenicContext resultValue
                    mainView.Children.Clear()
                    
                    mainView.Children.Add(preview)
                    ()
                    
            ()

    member _.PopulateListings() =
        listings <- ctx.ScenicContext.AssetStore.GetAssetListings()
        listingsBox.Items.Clear()

        for listing in listings.Entities do
            listingsBox.Items.Add(new ListBoxItem(Content = listing.Name, Name = listing.Name, DataContext = listing))
            |> ignore

    member _.AddNewAsset(e: RoutedEventArgs) =
        async {
            let newAssetWindow = NewAssetWindow(ctx.WorkflowHandlers.Assets)

            let! newAsset = newAssetWindow.ShowDialog<NewAsset option>(parent) |> Async.AwaitTask

            match newAsset with
            | None -> ()
            | Some value ->
                ctx.ScenicContext.AssetStore.AddNewAsset value

                // TODO make better, just add to existing list?
                this.PopulateListings()
        }
        |> Async.StartImmediate
