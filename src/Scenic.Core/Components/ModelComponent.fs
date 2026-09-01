namespace Scene.Core.Components

open System
open FsToolbox.GLTF
open FsToolbox.GameDevelopment.Geometry.Types
open FsToolbox.OpenGL.Materials
open CommonResourceFormats.AssetStore.Core.Domain

type ModelComponent() =
    
    let mutable material: OpenGLMaterial option = None
    
    let mutable model: Model3D option = None
    
    static member ComponentTypeName = "scenic:model"
    
    
    static member Load(comp: Component) =
        let materialAsset =
            comp.Assets |> Seq.tryFind (fun ca -> ca.Asset.AssetType.Equals("opengl:material", StringComparison.OrdinalIgnoreCase))
        
        let modelAsset =
            comp.Assets
            |> Seq.tryFind (fun ca -> ca.Asset.AssetType.Equals("crf:model", StringComparison.OrdinalIgnoreCase))
            |> Option.bind (fun ma ->
                ma.Asset.Resources
                |> Seq.tryFind (fun r ->
                    match r.FileType with
                    | "gltf" -> true
                    //| "model" -> true
                    | _ -> false))
            |> Option.map (fun ma ->
                match ma.FileType with
                | "gltf" -> GLTFLoader.loadModel
                
                
                
                ()
                
                )
        
        
        let modelComp = ModelComponent()
        
        //materialAsset |> Option.iter (fun ma -> modelComp.SetMaterial ma)
        modelAsset |> Option.iter (fun ma -> modelComp.SetModel ma)
        
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
        
        