namespace Scenic.Editor.Core

open System
open System.Numerics
open System.Runtime.InteropServices
open System.Windows.Input
open CommonResourceFormats.AssetStore.Core.Domain
open FsToolbox.GameDevelopment.Geometry.Types
open FsToolbox.OpenGL.Geometry
open FsToolbox.OpenGL.Types

module Domain =

    let scenicNS = "scenic"

    type EditorSceneInstance = { SceneId: Guid }

    type EditorSceneObjectInstance =
        { Data: SceneObject
          Model: Model3D option
          MaterialId: Guid
          Primitives: EditorSceneObjectPrimitive ResizeArray }

    and EditorSceneObjectPrimitive =
        { Mesh: ElementMesh
          MaterialId: EntityId }

    //type SceneObjectTreeViewItem(entityId: EntityId) =
    //    inherit TreeViewItem()
    //
    //    do
    //        base.Header <- "New object"
    //
    //    member _.EntityId = entityId

    type RelayCommand(action: unit -> unit, canExecute: unit -> bool) =
        interface ICommand with
            member this.CanExecute(parameter) = canExecute ()

            member this.add_CanExecuteChanged(value: EventHandler) = ()


            member this.remove_CanExecuteChanged(value: EventHandler) = ()

            member this.Execute(parameter) = action ()

    type ScenicEditorMaterialSlot =
        { Index: int
          MaterialId: EntityId option }

    type ScenicEditorMaterialSlots =
        private
            { Slots: ScenicEditorMaterialSlot array }

        static member Create(materialSlots: ScenicEditorMaterialSlot seq) =
            Array.init (materialSlots |> Seq.maxBy _.Index |> _.Index) (fun i ->
                match materialSlots |> Seq.tryFind (fun s -> s.Index = i) with
                | Some ms -> ms
                | None -> { Index = i; MaterialId = None })
