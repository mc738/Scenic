namespace Scenic.Editor.Rendering

open System.Collections.Generic
open CommonResourceFormats.AssetStore.Core.Domain
open FsToolbox.OpenGL.Geometry
open FsToolbox.OpenGL.Materials
open System.Numerics


type RenderSettings() =

    let mutable lightingEnabled = false

    member _.LightingEnabled = lightingEnabled

    member _.SetLightingEnabled(value: bool) = lightingEnabled <- value

type RenderBatch =
    { Material: OpenGLMaterial
      Items: Dictionary<SceneObjectId, RenderBatchItem> }

and RenderBatchItem(objectId: SceneObjectId, mesh: ElementMesh) =

    let mutable distanceToCamera = 0f

    let mutable modelMatrix = Matrix4x4.Identity

    member _.ObjectId = objectId

    member _.Mesh = mesh

    member _.DistanceToCamera = distanceToCamera

    member _.ModelMatrix = modelMatrix

    member _.SetDistanceToCamera(newDistance) = distanceToCamera <- newDistance

    member _.SetModelMatrix(mm: Matrix4x4) = modelMatrix <- mm

type MaterialId = EntityId

type RenderBatches =
    { StandardOpaque: ResizeArray<RenderBatchItem>
      StandardTransparent: ResizeArray<RenderBatchItem>
      Opaque: Dictionary<MaterialId, RenderBatch>
      Transparent: Dictionary<MaterialId, RenderBatch> }

    static member Empty =
        { StandardOpaque = ResizeArray<RenderBatchItem>()
          StandardTransparent = ResizeArray<RenderBatchItem>()
          Opaque = Dictionary<MaterialId, RenderBatch>()
          Transparent = Dictionary<MaterialId, RenderBatch>() }
