module Scenic.Editor.AssetStoreManager.Dialogs

open Avalonia.Controls
open Avalonia.Layout
open Avalonia.Media
open CommonResourceFormats.AssetStore.Core.Domain

type SupportedComponentType =
    | Model

    member sat.Serialize() =
        match sat with
        | Model -> "model"


type NewComponentWindow() as this =
    inherit Window()

    let mutable selectedAssetType =
        Operators.Unchecked.defaultof<SupportedComponentType>

    let nameBox = TextBox()


    let confirmButton = Button()
    let cancelButton = Button()


    do
        this.WindowDecorations <- WindowDecorations.None

        this.Height <- 200
        this.Width <- 400
        this.WindowStartupLocation <- WindowStartupLocation.CenterOwner
        this.Title <- "Create new component"

        let layout = StackPanel()

        let title = TextBlock(Text = "Create new component", FontWeight = FontWeight.Bold)

        let nameLabel = Label(Content = "Name", Target = nameBox)

        let componentTypeBox = ComboBox()

        let componentTypeLabel =
            Label(Content = "Component type", Target = componentTypeBox)

        componentTypeBox.Items.Add(new ComboBoxItem(Content = "Model", DataContext = SupportedComponentType.Model))
        |> ignore

        let assetPathLabel = Label(Content = "Path", Target = componentTypeBox)

        componentTypeBox.SelectionChanged.Add(fun e ->
            if e.AddedItems.Count = 0 then
                ()
            else
                let item = e.AddedItems[0]

                let cb = item :?> ComboBoxItem

                let sat = cb.DataContext :?> SupportedComponentType
                selectedAssetType <- sat

                ())

        confirmButton.Content <- "Create"
        cancelButton.Content <- "Cancel"

        confirmButton.Classes.Add("ok")
        cancelButton.Classes.Add("cancel")

        confirmButton.Click.Add(fun e ->
            printfn "Ok!!"

            let comp =
                ({ Id = EntityId.Create()
                   VersionId = EntityId.Create()
                   Name = nameBox.Text
                   ComponentType = selectedAssetType.Serialize()
                   Metadata = EntityMetadata.Empty
                   VersionMetadata = EntityMetadata.Empty
                   SerializedData = "" }
                : NewComponent)

            this.Close(Some comp))

        cancelButton.Click.Add(fun e ->
            printfn "Cancel"

            this.Close(None)
            ())

        let buttonsLayout = StackPanel()
        buttonsLayout.Orientation <- Orientation.Horizontal
        buttonsLayout.Children.Add(confirmButton)
        buttonsLayout.Children.Add(cancelButton)

        layout.Children.Add(title)
        layout.Children.Add(nameLabel)
        layout.Children.Add(nameBox)
        layout.Children.Add(componentTypeLabel)
        layout.Children.Add(componentTypeBox)

        layout.Children.Add(buttonsLayout)

        this.Content <- layout
