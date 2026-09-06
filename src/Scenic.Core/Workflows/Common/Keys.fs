namespace Scenic.Core.Workflows.Common

open CommonResourceFormats.AssetStore.Core.Domain

[<RequireQualifiedAccess>]
module Keys =

    
    let ``count-key`` = "count"
    
    [<RequireQualifiedAccess>]
    module AssetType =

        let gltf = EntityKey.Literal "gltf"


    