namespace Scenic.Editor.Core

open System
open System.Numerics
open System.Runtime.InteropServices
open System.Windows.Input

module Domain =

    type EditorSceneInstance = { SceneId: Guid }
    
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

