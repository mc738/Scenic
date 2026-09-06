namespace Scenic.Core.Workflows.Standard.V1

open CommonResourceFormats.AssetStore.Core.Domain
open FsToolbox.GLTF
open Scenic.Core.Workflows.Common


module ComponentWorkFlows =

    let tryLoadModel (comp: Component) =
        match comp.Metadata.TryGet Keys.Models.``model-asset-id`` with
        | None -> Error ""
        | Some mId ->
            match comp.Assets.TryGetValue (EntityId.Deserialize mId) with
            | false, _ -> Error ""
            | true, { Asset = asset } ->
                match asset.AssetType.Equals(Keys.AssetType.gltf) with
                | false -> failwith "todo"
                | true ->
                    GLTFLoader.loadModel (asset.Path.Serialize())
                    |> Ok
                    
              
    /// <summary>
    /// This will try and get all OpenGL materials.
    /// It will not load them.
    /// This is because each material will likely only be loaded once and reused.
    /// The results of this can be used to check if a material is already loaded and if not, load it.
    /// </summary>
    /// <param name="comp"></param>
    let tryGetOpenGLMaterials (comp: Component) =
        let materials = comp.VersionMetadata.GetScopedCollection Keys.Models.``material-slots-scope``
        
        for mdc in materials.Items do
            
            match mdc.Metadata.TryGetEntityId Keys.Models.``material-slot-asset-id`` with
            | None -> ()
            | Some assetId ->
                
                match comp.Assets.TryGetValue assetId with
                | false, _ -> failwith "todo"
                | true, { Asset = asset } ->
                    match asset.AssetType.NamespaceEquals(Keys.OpenGL.materialNs) with
                    | false -> failwith "todo"
                    | true ->
                        //({}: ScenicEditorMaterialSlot)
                        
                        
                        
                        
                        // This is a supported type of material.
                        
                        failwith "todo"
                    
            
            
            
            
            ()
        
        ()