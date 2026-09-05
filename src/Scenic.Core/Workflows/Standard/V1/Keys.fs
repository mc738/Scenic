namespace Scene.Core.Workflows.Standard.V1

open CommonResourceFormats.AssetStore.Core.Domain

/// Various EntityKeys used in this work flow.
/// They are defined here for ease of use.
[<RequireQualifiedAccess>]
module Keys =
    
    let nsPrefix = "scenic-std-v1"
    
    [<RequireQualifiedAccess>]    
    module OpenGL =

        let ns = $"{nsPrefix}.opengl"
          
        let shaderNs = $"{ns}.shader"
        
        let materialNs = $"{ns}.material"
    
    [<RequireQualifiedAccess>]
    module Textures =
        
        let ns = $"{nsPrefix}.textures"
        
        let ``albedo-map`` = EntityKey.Namespace (ns, "albedo-map") 

    [<RequireQualifiedAccess>]
    module GLTF =
        
        let ``asset-type`` = "gltf"
    
    [<RequireQualifiedAccess>]
    module Models =
        let ns = $"{nsPrefix}.model"
        
        let gltf = EntityKey.Namespace (ns, "gltf")
        
        let ``model-type`` = EntityKey.Literal ns
        
        /// This is the id of the component asset (from the component_assets table)
        let ``model-asset-id`` = EntityKey.Namespace(ns, "asset-id")