namespace Scenic.Core.Components

open System
open FsToolbox.GLTF
open FsToolbox.GameDevelopment.Geometry.Types
open FsToolbox.OpenGL.Materials
open CommonResourceFormats.AssetStore.Core.Domain
open Scenic.Core

(*
type ModelComponent() =
    
    let mutable material: OpenGLMaterial option = None
    
    let mutable model: Model3D option = None
    
    static member ComponentTypeName = "scenic:model"
    
    
    static member Load(ctx: ScenicContext, comp: Component) =
        
        let modelComp = ModelComponent()
        
        let materialAsset =
            comp.Assets.Values |> Seq.tryFind (fun ca -> ca.Asset.AssetType.Equals("opengl:material", StringComparison.OrdinalIgnoreCase))
        
        let loadModelResult =
            match 
                comp.Assets.Values
                |> Seq.tryFind (fun ca -> ca.Asset.AssetType.Equals("crf:model", StringComparison.OrdinalIgnoreCase))
            with
            | None -> Error ""
            | Some ma ->
                match ctx.TryResolvePath ma.Asset.Path with
                | Error errorValue -> Error errorValue
                | Ok path ->
                    let m3d = GLTFLoader.loadModel path
                    
                    modelComp.SetModel(m3d)
                    Ok ()
            
        match loadModelResult with
        | Ok resultValue -> ()
        | Error errorValue -> printfn $"Error: {errorValue}"
            
        modelComp
    
    static member Deserialize(jsonString: string) =
        // Build from json.
        
        
        
        ()
    
    member _.SetModel(newModel: Model3D) =
        model <- Some newModel
        
    member _.SetMaterial(newMaterial: OpenGLMaterial) =
        material <- Some newMaterial
        
    
    member _.Serialize() =
        
        ""
*)        
        