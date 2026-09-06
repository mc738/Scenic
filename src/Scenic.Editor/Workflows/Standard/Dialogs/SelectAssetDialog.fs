namespace Scenic.Editor.Workflows.Standard.Dialogs

open Avalonia.Controls
open Avalonia.Interactivity
open CommonResourceFormats.AssetStore.Core.Domain
open Scenic.Editor.Controls
open Scenic.Editor.Core.Workflows.Types
open Scenic.Editor.Core.Dsl
open Scenic.Editor.Core.Dsl.Presets

type SelectAssetDialog((*workflows: AssetWorkflowsHandler,*)assetListing: EntityListings) as this =
    inherit Window()

    let selectAssetControl = SelectAssetControl(assetListing)

    do
        this.Content <-
            StackPanel.create ControlStyle.Fill
            |> withChildren
                [ Titles.dialog "Select asset"

                  selectAssetControl

                  Buttons.group [ Buttons.success "Save" this.OnOk; Buttons.cancel "Cancel" this.OnCancel ] ]

    member _.OnOk(e: RoutedEventArgs) =
        match selectAssetControl.SelectedAssetVersionId with
        | None -> ()
        | Some v -> this.Close(Some v)

    member _.OnCancel(e: RoutedEventArgs) = this.Close(None)
