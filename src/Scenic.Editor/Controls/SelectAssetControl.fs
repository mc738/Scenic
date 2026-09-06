namespace Scenic.Editor.Controls

open Avalonia.Controls
open CommonResourceFormats.AssetStore.Core.Domain
open Scenic.Editor.Core.Dsl
open Scenic.Editor.Core.Dsl.Presets

type SelectAssetControl(assetListing: EntityListings) as this =
    inherit StackPanel()

    let mutable selectedAssetVersionId: EntityId option = None
    
    let assetComboBox =
        ComboBox.createDefault ()
        |> withItems
            false
            [ for listing in assetListing.Entities do
                  ComboBoxItem.createDefault ()
                  |> ComboBoxItem.withContent listing.Name
                  |> withDataContext listing ]
        |> ComboBox.withSelectionChanged this.AssetSelectionChanged

    let assetVersionBox =
        ComboBox.createDefault ()
        |> ComboBox.withSelectionChanged this.AssetVersionSelectionChanged

    do

        this
        |> setChildren
            [ Labels.input "Asset" assetComboBox
              assetComboBox

              Labels.input "Version" assetVersionBox
              assetVersionBox ]
    
    member _.SelectedAssetVersionId = selectedAssetVersionId
    
    member _.AssetSelectionChanged(e: SelectionChangedEventArgs) =
        match e.AddedItems.Count with
        | 0 -> ()
        | _ ->
            let item = e.AddedItems[0] :?> ComboBoxItem
            
            match item |> tryGetDataContext<EntitiesListingItem> with
            | Error errorValue ->
                printfn $"{errorValue}"
                failwith "todo"
            | Ok resultValue ->
                assetVersionBox
                |> setItems true [
                    for listing in resultValue.Versions do
                      ComboBoxItem.createDefault ()
                      |> ComboBoxItem.withContent listing.Version
                      |> withDataContext listing
                ]

    member _.AssetVersionSelectionChanged(e: SelectionChangedEventArgs) =
        match e.AddedItems.Count with
        | 0 -> ()
        | _ ->
            let item = e.AddedItems[0] :?> ComboBoxItem
            
            match item |> tryGetDataContext<EntityVersionListingItem> with
            | Error errorValue ->
                printfn $"{errorValue}"
                failwith "todo"
            | Ok version ->
                selectedAssetVersionId <- Some version.Id