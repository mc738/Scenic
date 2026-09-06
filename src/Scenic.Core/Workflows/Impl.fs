namespace Scenic.Core.Workflows

open System
open CommonResourceFormats.AssetStore.Core.Domain
open Scenic.Core.Workflows.Standard

[<AutoOpen>]
module private Internal =

    let equals (strA: string) (strB: string) =
        strA.Equals(strB, StringComparison.OrdinalIgnoreCase)

/// <summary>
/// Workflows for interacting with components (from CommonResourceFormats.AssetStore.Core.Domain.Component).
/// These are key to linking assets with scene components and build pipelines, even if the target uses a difference,
/// none component based architecture.
/// </summary>
module ComponentWorkflows =

    let tryLoadModel (comp: Component) =
        match comp.ComponentType with
        // TODO fix keys for standard work flow
        | ct when equals (ct.Serialize()) (V1.Keys.Components.``model-type``.Serialize()) ->
        //| ct when equals (ct.Serialize()) (V1.Keys.Models.``model-type``.Serialize()) ->
            // Send to v1 to load.
            V1.ComponentWorkFlows.tryLoadModel comp
        | ct -> Error $"Unknown model type: {ct}"

    let loadOpenGLMaterial (comp: Component) = ()
