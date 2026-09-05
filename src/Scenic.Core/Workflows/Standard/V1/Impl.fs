namespace Scene.Core.Workflows.Standard.V1

open CommonResourceFormats.AssetStore.Core.Domain
open FsToolbox.GLTF


module ComponentWorkFlows =

    let tryLoadModel (comp: Component) =
        match comp.Metadata.TryGet Keys.Models.``model-asset-id`` with
        | None -> Error ""
        | Some mId ->
            match comp.Assets.TryGetValue (EntityId.Deserialize mId) with
            | false, _ -> Error ""
            | true, { Asset = asset } ->
                match asset.AssetType.Equals(Keys.GLTF.``asset-type``) with
                | false -> failwith "todo"
                | true ->
                    GLTFLoader.loadModel (asset.Path.Serialize())
                    |> Ok