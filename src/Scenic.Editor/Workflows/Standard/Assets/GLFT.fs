namespace Scenic.Editor.Workflows.Standard.Assets

open Avalonia.Controls
open CommonResourceFormats.AssetStore.Core.Domain
open Scenic.Core
open Scenic.Core.Workflows.Common
open Scenic.Editor.Core.Workflows.Types

module GLTF =

    
    let key = EntityKey.Namespace (assetsNS, "gltf")
    
    let ``asset-type-key`` = key
    
    type NewGLTFAssetWorkflowControl() as this =
        inherit NewAssetWorkflowControl()

        do
            this.Children.Add(TextBlock(Text = "Import a .gltf file as an asset."))
        
        override this.CreateMetadata() = EntityMetadata.Empty
        override this.GetAssetType() = ``asset-type-key``

    type PreviewGLTFAssetWorkflowControl() =
        inherit PreviewAssetWorkflowControl()

        override this.CreateMetadata() = EntityMetadata.Empty
        
    
    type EditGLTFAssetWorkflowControl(ctx: ScenicContext, parentWindow: Window, asset: Asset) =
        inherit EditAssetWorkflowControl(ctx, parentWindow, asset)
        
        override _.Save() = ()

    
    let factory =
        ({ Key = key
           Name = "GLTF (standard workflow)"
           CreateNewWorkflowControl = fun () -> NewGLTFAssetWorkflowControl()
           CreatePreviewWorkflowControl = fun ctx asset -> PreviewGLTFAssetWorkflowControl()
           CreateEditWorkflowControl = fun ctx parentWindow asset -> EditGLTFAssetWorkflowControl(ctx, parentWindow, asset) }
        : AssetWorkflowFactory)

    let handler = key, factory