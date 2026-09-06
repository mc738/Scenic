namespace Scenic.Editor.Workflows.Standard.Components

open Avalonia.Controls
open Avalonia.Interactivity
open CommonResourceFormats.AssetStore.Core.Domain
open Scenic.Core
open Scenic.Core.Workflows.Standard.V1
open Scenic.Editor.Core.Dsl
open Scenic.Editor.Core.Dsl.Presets
open Scenic.Editor.Core.Workflows.Types
open Scenic.Editor.Workflows.Standard.Dialogs
open Scenic.Core.Workflows.Standard

module Models =


    let key = EntityKey.Namespace(componentsNS, "model")

    let ``component-type-key`` = V1.Keys.Models.``model-type``


    type NewModelComponentWorkflowControl() as this =
        inherit NewComponentWorkflowControl()

        do this.Children.Add(TextBlock(Text = "Create a standard model component."))

        override this.CreateMetadata() = EntityMetadata.Empty
        override this.GetComponentType() = ``component-type-key``


    type PreviewModelComponentWorkflowControl() =
        inherit PreviewComponentWorkflowControl()

        override this.CreateMetadata() = EntityMetadata.Empty


    type EditModelComponentWorkflowControl(ctx: ScenicContext, parentWindow: Window, comp: Component) as this =
        inherit EditComponentWorkflowControl(ctx, parentWindow, comp)

        let mutable modelAccessChanged = false

        do
            this
            |> setChildren
                [ TextBlock(Text = "Model Selected.")
                  Buttons.general "Select model asset" this.ShowSelectAsset ]

        override _.Save() =


            ()

        member _.ShowSelectAsset(e: RoutedEventArgs) =
            async {
                let d = SelectAssetDialog(ctx.AssetStore.GetAssetListings())

                let! r = d.ShowDialog<EntityId option>(parentWindow) |> Async.AwaitTask

                match r with
                | None -> ()
                | Some assetVersionId ->
                    // Update.

                    let componentAssetId = EntityId.Create()

                    // First we need create the component asset.
                    ctx.AssetStore.AddComponentAsset(
                        { Id = componentAssetId
                          ComponentVersionId = comp.VersionId
                          AssetVersionId = assetVersionId
                          Metadata = EntityMetadata.Empty }
                    )

                    // Next update the component version's metadata.
                    ctx.AssetStore.UpsertComponentVersionMetadataItem(
                        comp.VersionId,
                        Keys.Models.``model-asset-id``,
                        componentAssetId.Serialize()
                    )

                    // TODO clear up any existing one.
                    ()

                ()


            }
            |> Async.StartImmediate

    let factory =
        ({ Key = key
           Name = "Model (standard workflow)"
           CreateNewWorkflowControl = fun () -> NewModelComponentWorkflowControl()
           CreatePreviewWorkflowControl = fun ctx asset -> PreviewModelComponentWorkflowControl()
           CreateEditWorkflowControl =
             fun ctx parentWindow asset -> EditModelComponentWorkflowControl(ctx, parentWindow, asset) }
        : ComponentWorkflowFactory)

    let handler = key, factory
