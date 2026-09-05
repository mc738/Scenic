namespace Scenic.Editor.Views.SceneEditor.Dialogs

open System.Collections
open Avalonia.Controls
open Avalonia.Interactivity
open Avalonia.Layout
open CommonResourceFormats.AssetStore.Core.Domain
open Scenic.Editor.Core.Dsl

type NewSceneObjectComponentDialog(componentListings: EntityListings) as this =
    inherit Window()

    let textBox =
        TextBox.create ControlStyle.Default |> TextBox.withPlaceholderText "placeholder"

    let mutable selectedComponentVersionId: EntityId option = None

    let componentsComboBox =
        ComboBox.createDefault ()
        |> ComboBox.withSelectionChanged this.ComponentSelectionChanged
        |> withItems
            true
            [ for comp in componentListings.Entities do
                  ComboBoxItem.createDefault ()
                  |> ComboBoxItem.withContent comp.Name
                  |> withDataContext comp

              ]

    let componentsVersionComboBox =
        ComboBox.createDefault ()
        |> ComboBox.withSelectionChanged (fun e ->
            if e.AddedItems.Count = 0 then
                ()
            else
                match
                    (e.AddedItems[0] :?> ComboBoxItem)
                    |> tryGetDataContext<EntityVersionListingItem>
                with
                | Error errorValue -> failwith "todo"
                | Ok resultValue -> selectedComponentVersionId <- Some resultValue.Id)

    do
        this.Content <-
            StackPanel.create ControlStyle.Fill
            |> withChildren
                [ Label.createDefault ()
                  |> Label.withContent "Component"
                  |> Label.withTarget componentsComboBox

                  componentsComboBox

                  Label.createDefault ()
                  |> Label.withContent "Version"
                  |> Label.withTarget componentsVersionComboBox

                  componentsVersionComboBox

                  StackPanel.createDefault ()
                  |> StackPanel.withOrientation Orientation.Horizontal
                  |> withChildren
                      [ Button.create
                            { ControlStyle.Default with
                                Classes = [ "ok" ] }
                        |> Button.withContent "Add"
                        |> Button.onClick (fun _ -> this.Close(Some selectedComponentVersionId.Value))
                        Button.create
                            { ControlStyle.Default with
                                Classes = [ "cancel" ] }
                        |> Button.withContent "Cancel"
                        |> Button.onClick (fun _ -> this.Close(None)) ] ]

    member _.ComponentSelectionChanged(e: SelectionChangedEventArgs) =
        if e.AddedItems.Count = 0 then
            ()
        else
            componentsVersionComboBox.Items.Clear()

            let i = e.AddedItems.[0] :?> ComboBoxItem

            match tryGetDataContext<EntitiesListingItem> i with
            | Error errorValue -> failwith errorValue
            | Ok resultValue ->
                componentsVersionComboBox
                |> setItems
                    true
                    [ for version in resultValue.Versions |> List.sortByDescending _.Version do
                          ComboBoxItem.createDefault ()
                          |> ComboBoxItem.withContent version.Version
                          |> withDataContext version ]
