namespace Scenic.Editor.Workflows.Standard.Assets

open CommonResourceFormats.AssetStore.Core.Domain
open Scenic.Core.Workflows.Common
open Scenic.Editor.Core.Workflows.Types

module GLTF =

    type NewGLTFAssetWorkflowControl() =
        inherit NewAssetWorkflowControl()

        override this.CreateMetadata() = EntityMetadata.Empty
        override this.GetAssetType() = Keys.AssetType.gltf

    type PreviewGLTFAssetWorkflowControl() =
        inherit PreviewAssetWorkflowControl()

        override this.CreateMetadata() = EntityMetadata.Empty

    let key = EntityKey.Namespace (assetsNS, "gltf")
    
    let factory =
        ({ Key = key
           Name = "GLTF (standard workflow)"
           CreateNewAssetWorkflowControl = fun () -> NewGLTFAssetWorkflowControl()
           CreatePreviewAssetWorkflowControl = fun ctx asset -> PreviewGLTFAssetWorkflowControl() }
        : AssetWorkflowFactory)

    let handler = key, factory