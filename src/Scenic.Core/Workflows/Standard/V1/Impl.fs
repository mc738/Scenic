namespace Scenic.Core.Workflows.Standard.V1

open CommonResourceFormats.AssetStore.Core.Domain
open FsToolbox.GLTF
open Scenic.Core.Workflows.Common


module ComponentWorkFlows =

    let tryLoadModel (comp: Component) =
        // Try and get the model asset id from the version first.
        // If this fails, get it from the component metadata as a fallback.
        let modelAssetIdKey = Keys.Models.``model-asset-id``

        match
            comp.VersionMetadata.TryGet modelAssetIdKey
            |> Option.orElse (comp.Metadata.TryGet modelAssetIdKey)
        with
        | None -> Error $"Could not find `{modelAssetIdKey.Serialize()}` metadata value"
        | Some mId ->
            match comp.Assets.TryGetValue(EntityId.Deserialize mId) with
            | false, _ -> Error $"Could not find component asset `{mId}`"
            | true, { Asset = asset } ->
                let sat = asset.AssetType.Serialize()

                match asset.AssetType.Serialize().Equals(Keys.Assets.gltf.Serialize()) with
                | _ when sat.Equals(Keys.Assets.gltf.Serialize()) -> GLTFLoader.loadModel (asset.Path.Serialize()) |> Ok
                | _ -> Error "Unsupported model type"


    /// <summary>
    /// This will try and get all OpenGL materials.
    /// It will not load them.
    /// This is because each material will likely only be loaded once and reused.
    /// The results of this can be used to check if a material is already loaded and if not, load it.
    /// </summary>
    /// <param name="comp"></param>
    let tryGetOpenGLMaterials (comp: Component) =
        let materials =
            comp.VersionMetadata.GetScopedCollection Keys.Models.``material-slots-scope``

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
