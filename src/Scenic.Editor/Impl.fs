namespace Scenic.Editor.Workflows

open Scenic.Editor.Core.Workflows.Types

[<AutoOpen>]
module Extensions =

    type WorkflowsBuilder with

        member this.WithStandardWorkflows() =
            this
                .WithAssetHandlers(
                    [
                        Standard.Assets.GLTF.handler
                    ]
                )
                .WithComponentHandlers(
                    [

                    ]
                )
