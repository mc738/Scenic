module Scenic.Editor.AssetStoreManager.Dialogs

open Avalonia.Controls
open Avalonia.Controls.Presenters
open Avalonia.Interactivity
open Avalonia.Layout
open Avalonia.Media
open CommonResourceFormats.AssetStore.Core.Domain
open Scenic.Editor.Core.Workflows.Types
open Scenic.Editor.Core.Dsl

type SupportedComponentType =
    | Model

    member sat.Serialize() =
        match sat with
        | Model -> "model"


type NewComponentWindow(workflows: ComponentWorkflowsHandler) as this =
    inherit Window()

    let mutable selectedAssetType =
        Operators.Unchecked.defaultof<SupportedComponentType>

    let nameBox = TextBox()

    let mutable workflowControl: NewComponentWorkflowControl option = None

    let workflowSpecificContent = ContentPresenter()

    let componentTypeBox = ComboBox()

    do
        this.WindowDecorations <- WindowDecorations.None

        this.Height <- 300
        this.Width <- 400
        this.WindowStartupLocation <- WindowStartupLocation.CenterOwner
        this.Title <- "Create new component"

        this.Content <-
            StackPanel.create ControlStyle.Fill
            |> withChildren
                [ TextBlock(Text = "Create new component", FontWeight = FontWeight.Bold)

                  Label.createDefault ()
                  |> Label.withContent "Component name"
                  |> Label.withTarget nameBox
                  nameBox

                  Label.createDefault ()
                  |> Label.withContent "Component type"
                  |> Label.withTarget componentTypeBox

                  componentTypeBox
                  |> withItems
                      false
                      [ for listing in workflows.GetListings() do
                            ComboBoxItem(Content = listing.Name, DataContext = listing.Key) ]
                  |> ComboBox.withSelectionChanged this.OnTypeSelectionChanged

                  workflowSpecificContent

                  StackPanel.createDefault ()
                  |> StackPanel.withOrientation Orientation.Horizontal
                  |> withChildren
                      [ Button.create
                            { ControlStyle.Default with
                                Classes = [ "ok" ] }
                        |> Button.withContent "Add"
                        |> Button.onClick this.OnOk
                        Button.create
                            { ControlStyle.Default with
                                Classes = [ "cancel" ] }
                        |> Button.withContent "Cancel"
                        |> Button.onClick this.OnCancel ]

                  ]

    member _.OnOk(e: RoutedEventArgs) =
        match workflowControl with
        | None -> ()
        | Some wfc ->
            let comp =
                ({ Id = EntityId.Create()
                   VersionId = EntityId.Create()
                   Name = nameBox.Text
                   ComponentType = wfc.GetComponentType()
                   Metadata = EntityMetadata.Empty
                   VersionMetadata = EntityMetadata.Empty
                   SerializedData = "" }
                : NewComponent)

            this.Close(Some comp)

    member _.OnCancel(e: RoutedEventArgs) = this.Close(None)

    member _.OnTypeSelectionChanged(e: SelectionChangedEventArgs) =
        if e.AddedItems.Count = 0 then
            ()
        else
            let item = e.AddedItems[0]

            let cb = item :?> ComboBoxItem

            let sat = cb.DataContext :?> EntityKey

            match workflows.Factories.TryFind sat with
            | None -> failwith "todo"
            | Some value ->
                //  Clear the work flow specific stuff.
                let newControl = value.CreateNewWorkflowControl()

                workflowControl <- Some newControl
                workflowSpecificContent.Content <- newControl
