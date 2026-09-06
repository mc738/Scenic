namespace Scenic.Core.Workflows.Standard.V1

open CommonResourceFormats.AssetStore.Core.Domain
open Scenic.Core.Workflows.Common

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

        let ``albedo-map`` = EntityKey.Namespace(ns, "albedo-map")


    [<RequireQualifiedAccess>]
    module Models =
        let ns = $"{nsPrefix}.model"
        
        
        let materialsNS = $"{ns}.materials"

        let gltf = EntityKey.Namespace(ns, "gltf")

        let ``model-type`` = EntityKey.Literal ns

        /// This is the id of the component asset (from the component_assets table)
        let ``model-asset-id`` = EntityKey.Namespace(ns, "asset-id")

        let ``material-slots-scope`` = "material-slots"

        let ``material-slot-count`` = EntityKey.ScopeDefinition(``material-slots-scope``, EntityMetadata.``count-key``)
              
        /// This is an unscoped version of the 
        let ``material-slot-asset-id`` = EntityKey.Namespace(materialsNS, "asset-id")
        
        let ``material-slot-1-asset-id`` =
            EntityKey.RepeatNamespace(``material-slots-scope``, 0, materialsNS, "asset-id")

        let ``material-slot-2-asset-id`` =
            EntityKey.RepeatNamespace(``material-slots-scope``, 1, materialsNS, "asset-id")

        let ``material-slot-3-asset-id`` =
            EntityKey.RepeatNamespace(``material-slots-scope``, 2, materialsNS, "asset-id")

        let ``material-slot-4-asset-id`` =
            EntityKey.RepeatNamespace(``material-slots-scope``, 3, materialsNS, "asset-id")

        let ``material-slot-5-asset-id`` =
            EntityKey.RepeatNamespace(``material-slots-scope``, 4, materialsNS, "asset-id")

        let ``material-slot-6-asset-id`` =
            EntityKey.RepeatNamespace(``material-slots-scope``, 5, materialsNS, "asset-id")

        let ``material-slot-7-asset-id`` =
            EntityKey.RepeatNamespace(``material-slots-scope``, 6, materialsNS, "asset-id")

        let ``material-slot-8-asset-id`` =
            EntityKey.RepeatNamespace(``material-slots-scope``, 7, materialsNS, "asset-id")
