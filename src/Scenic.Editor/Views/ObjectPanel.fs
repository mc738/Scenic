namespace Scenic.Editor.Views

open Avalonia.Controls
open Avalonia.Media
open CommonResourceFormats.AssetStore.Core.Domain
open Scenic.Editor.Controls
open Scenic.Editor.Core

type ObjectPanel(ctx: EditorContext) as this =
    inherit StackPanel()
    
    let mutable sceneObject = Operators.Unchecked.defaultof<SceneObject>

    let title = TextBlock(FontWeight = FontWeight.Bold)
    
    let transformControl = TransformControl()
    
    do
        //transformControl.SetValue(sceneObject.Transform)
        
        
        this.Children.Add(title)
        this.Children.Add(transformControl)
        
    member _.SetObject(newSceneObject: SceneObject) =
        sceneObject <- newSceneObject
        
        transformControl.SetValue(sceneObject.Transform)
        
        title.Text <- newSceneObject.Name
        
        ()
        
        
    member _.SaveObject() =
        ctx.AssetStoreContext.UpdateSceneObjectTransform(sceneObject.Id, transformControl.GetTransform())