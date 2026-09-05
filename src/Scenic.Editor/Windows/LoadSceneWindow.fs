namespace Scenic.Editor.Windows

open Avalonia.Controls
open Avalonia.Layout
open CommonResourceFormats.AssetStore.Core.Domain

type LoadSceneWindow(sceneListings: EntityListings) as this =
    inherit Window()
    
    let scenesList = ListBox()
    let versionBox = ComboBox()
    
    let openButton = Button()
    let cancelButton = Button()
    
    let layout = StackPanel()
    
    do  
        this.WindowDecorations <- WindowDecorations.None
        
        for listing in sceneListings.Entities do
            let item = ListBoxItem()
            
            match listing.Versions |> List.sortByDescending (fun v -> v.Version) |> List.tryHead with
            | None ->
                // No version
                ()
            | Some lv ->
                item.DataContext <- lv.Id
                
                item.Content <- listing.Name
                
                scenesList.Items.Add(item) |> ignore
     
        openButton.Classes.Add("ok")
        openButton.Content <- "Open"
        openButton.Click.Add(fun e ->
            //printfn $"{scenesList.SelectedIndex}"
            //printfn $"{(scenesList.Items[scenesList.SelectedIndex] :?> ListBoxItem).DataContext :?> EntityId}"
            
            let eId = (scenesList.Items[scenesList.SelectedIndex] :?> ListBoxItem).DataContext :?> EntityId
            this.Close(Some eId))
        
        cancelButton.Classes.Add("cancel")
        cancelButton.Content <- "Cancel"
        cancelButton.Click.Add(fun e -> this.Close(None))
        
        layout.Children.Add(scenesList)
        
        let btnLayout = StackPanel()
        btnLayout.Orientation <- Orientation.Horizontal
        
        btnLayout.Children.Add(openButton)
        btnLayout.Children.Add(cancelButton)
        
        layout.Children.Add(btnLayout)
        
        this.Content <- layout
 