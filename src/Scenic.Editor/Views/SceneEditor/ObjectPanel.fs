namespace Scenic.Editor.Views.SceneEditor

open Avalonia.Controls
open Avalonia.Media
open CommonResourceFormats.AssetStore.Core.Domain
open Scenic.Editor.Controls
open Scenic.Editor.Core
open Scenic.Editor.Core.Domain
open Scenic.Editor.Views.SceneEditor.Dialogs
open Scenic.Editor.Core.Dsl

type ObjectPanel(ctx: EditorContext, parentWindow: Window) as this =
    inherit StackPanel()

    let mutable sceneObject = Operators.Unchecked.defaultof<SceneObject>

    let title = TextBlock(FontWeight = FontWeight.Bold)

    let transformControl = TransformControl()

    let componentsListBox =
        ListBox.create ControlStyle.Fill
        |> ListBox.withContextMenu (
            ContextMenu.create ControlStyle.Default
            |> ContextMenu.withItems
                [ MenuItem.createDefault ()
                  |> MenuItem.withHeader "Add component"
                  |> MenuItem.withCommand this.AddSceneObjectComponent ]
        )

    do this |> setChildren [ title; transformControl; componentsListBox ]

    member _.SetObject(newSceneObject: SceneObject) =
        sceneObject <- newSceneObject

        transformControl.SetValue(sceneObject.Transform)

        title.Text <- newSceneObject.Name

        componentsListBox.Items.Clear()

        for comp in sceneObject.Components do
            componentsListBox.Items.Add(ListBoxItem(Content = comp.Component.Name))
            |> ignore

        ()

    member _.SaveObject() =
        ctx.ScenicContext.AssetStore.UpdateSceneObjectTransform(sceneObject.Id, transformControl.GetTransform())

    member _.AddSceneObjectComponent() =
        async {

            let dialog =
                NewSceneObjectComponentDialog(ctx.ScenicContext.AssetStore.GetComponentListings())

            let! r = dialog.ShowDialog<EntityId option>(parentWindow) |> Async.AwaitTask

            match r with
            | None -> ()
            | Some componentVersionId ->
                ctx.ScenicContext.AssetStore.AddSceneObjectComponent(sceneObject.Id, componentVersionId, None)
                sceneObject.Components.Clear()
                sceneObject.Components.AddRange(ctx.ScenicContext.AssetStore.GetSceneObjectComponents(sceneObject.Id))
        }
        |> Async.StartImmediate
