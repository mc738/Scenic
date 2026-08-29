namespace Scenic.Component

open System
open System.Drawing
open System.IO
open System.Numerics
open System.Runtime.InteropServices
open Avalonia
open Avalonia.Controls
open Avalonia.Input
open Avalonia.Layout
open Avalonia.OpenGL
open Avalonia.Rendering
open Avalonia.Platform
open Avalonia.OpenGL
open Avalonia.OpenGL.Controls
open Avalonia.Threading
open Scenic.Core
open Silk.NET.OpenGL
open StbImageSharp
open FSharp.NativeInterop

[<AutoOpen>]
module private Internal =

    let drawModel
        (gl: GL)
        (contentManager: ContentManager)
        (modelId: Guid)
        (view: Matrix4x4)
        (projection: Matrix4x4)
        (transform: Transform)
        =
        let model = contentManager.GetModel(modelId)

        for mesh in model.Meshes do
            for prim in mesh.Primitives do
                let material = contentManager.GetMaterial(model.MaterialIds[prim.MaterialId])

                material.Bind(view, projection, transform.ViewMatrix)

                prim.VertexArrayObject.Bind()

                Fiket.Engine.Rendering.RenderOperations.drawPrimitive
                    gl
                    prim
                    view
                    projection
                    transform.ViewMatrix
    
    let drawObjects
        (gl: GL)
        (contentManager: ContentManager)
        (objects: EditorSceneObject array)
        (view: Matrix4x4)
        (projection: Matrix4x4)
        =
        gl.Enable(EnableCap.DepthTest)
        gl.DepthFunc(DepthFunction.Less)
        gl.DepthMask(true)

        gl.BindVertexArray(0u)

        for i, sceneObject in objects |> Seq.indexed do
            match sceneObject.Type with
            | Model modelId ->
                drawModel gl contentManager modelId view projection sceneObject.Transform

    let drawScene (gl: GL) (scene: EditorSceneInstance) (view: Matrix4x4) (projection: Matrix4x4) =
        let contentManager = scene.ContentManager
        let objects = scene.GetSceneModels()

        gl.Enable(EnableCap.DepthTest)
        gl.DepthFunc(DepthFunction.Less)
        gl.DepthMask(true)

        gl.BindVertexArray(0u)
        
        drawObjects gl contentManager objects view projection
        
    let drawDebug (gl: GL) (voa: DebugPlane) (grid: GridPlane) (view: Matrix4x4) (projection: Matrix4x4) =        
        drawDebugPlane gl voa view projection
        gl.Disable(EnableCap.DepthTest)
        gl.DepthMask(false)
        drawGridPlane gl grid view projection
        gl.Enable(EnableCap.DepthTest)
        gl.DepthMask(true)


type IViewportHost =
    abstract member RequestScene: unit -> EditorSceneInstance option
    abstract member OnScreenRaycast: Ray * ScreenRaycastType -> unit

type Viewport3D(ctx: EditorContext, host: IViewportHost) as this =

    inherit OpenGlControlBase()
    let mutable gl = Operators.Unchecked.defaultof<GL>
    let mutable initalized = false
    let mutable moveForwards = false
    let mutable moveBackwards = false
    let mutable moveLeft = false
    let mutable moveRight = false
    let mutable target = Vector3.Zero
    let mutable shader = Operators.Unchecked.defaultof<Viewport3DShader>
    let mutable camera = Fiket.Engine.Core.CameraData()
    let mutable lastPos: Point option = None
    let mutable voa: DebugPlane = Operators.Unchecked.defaultof<DebugPlane>
    let mutable gridPlane: GridPlane = Operators.Unchecked.defaultof<GridPlane>

    let mutable scene: EditorSceneInstance option = Option.None

    do
        camera.Position <- Vector3(0f, 50f, 3f)
        camera.Up <- Vector3.UnitY
        camera.AspectRation <- (this.Bounds.Width |> float32) / (this.Bounds.Width |> float32)

    member _.GL = if initalized then Some gl else None

    override this.OnOpenGlInit(gli: GlInterface) =

        gl <- GL.GetApi(gli.GetProcAddress)

        //Console.WriteLine(gl.GetStringS(StringName.Version))
        //Console.WriteLine(gl.GetStringS(StringName.ShadingLanguageVersion))
        //Console.WriteLine(gl.GetStringS(StringName.Vendor))
        //Console.WriteLine(gl.GetStringS(StringName.Renderer))

        shader <- Viewport3DShader(gl)

        gl.Enable(EnableCap.Blend)
        gl.BlendFunc(BlendingFactor.SrcAlpha, BlendingFactor.OneMinusSrcAlpha)

        gl.ClearColor(Color.CornflowerBlue)

        voa <- DebugPlane(gl)
        gridPlane <- GridPlane(gl)

        initalized <- true

    override this.OnOpenGlRender(gli, fb) =
        gl.Clear(ClearBufferMask.ColorBufferBit ||| ClearBufferMask.DepthBufferBit)
        gl.Viewport(0, 0, this.Bounds.Width |> uint, this.Bounds.Height |> uint)

        camera.AspectRation <- (this.Bounds.Width |> float32) / (this.Bounds.Height |> float32)

        let dt = 1f / 60f

        this.HandleInput(dt)

        // Get view and projection
        let view = camera.ViewMatrix

        let projection = camera.ProjectMatrix

        match host.RequestScene() with
        | None -> ()
        | Some scene ->
            // Draw the models
            drawScene gl scene view projection
            
        drawDebug gl voa gridPlane view projection

        Dispatcher.UIThread.Post(this.InvalidateVisual, DispatcherPriority.Background)

    member private this.HandleInput(dt: float32) =
        let mutable moveVec = Vector3.Zero
        let speed = 20f

        if moveForwards then
            moveVec.Z <- moveVec.Z + 1f

        if moveBackwards then
            moveVec.Z <- moveVec.Z - 1f

        if moveLeft then
            moveVec.X <- moveVec.X - 1f

        if moveRight then
            moveVec.X <- moveVec.X + 1f

        camera.UpdateBasis()
        camera.Move(moveVec, speed, dt)

    member this.ForwardKeyUp(e: KeyEventArgs) =
        match e.Key with
        | Key.W -> moveForwards <- false
        | Key.S -> moveBackwards <- false
        | Key.A -> moveLeft <- false
        | Key.D -> moveRight <- false
        | _ -> ()

    member this.ForwardKeyDown(e: KeyEventArgs) =
        match e.Key with
        | Key.W -> moveForwards <- true
        | Key.S -> moveBackwards <- true
        | Key.A -> moveLeft <- true
        | Key.D -> moveRight <- true
        | _ -> ()

    member this.ForwardPointerMoved(e: PointerEventArgs) =
        let mods = e.KeyModifiers

        if mods.HasFlag(KeyModifiers.Shift) then
            let pos = e.GetPosition(this)

            match lastPos with
            | Some lp ->
                let dx = pos.X - lp.X
                let dy = pos.Y - lp.Y

                camera.Yaw <- camera.Yaw + (dx |> float32) * 0.02f
                camera.Pitch <- camera.Pitch - (dy |> float32) * 0.02f
                camera.Pitch <- Math.Clamp(camera.Pitch, -1.55f, 1.55f)
            | None -> ()

            lastPos <- Some pos

    member this.ForwardPointerPressed(e: PointerEventArgs) =
        if e.Properties.IsLeftButtonPressed then
            let p = e.GetPosition(this)

            // Get view and projection

            let ray =
                camera.Raycast(
                    Vector2((p.X |> float32), (p.Y |> float32)),
                    { X = 0f
                      Y = 0f
                      Height = this.Bounds.Height |> float32
                      Width = this.Bounds.Width |> float32 }
                )

            host.OnScreenRaycast(ray, ScreenRaycastType.MouseClick MouseClickType.LeftButton)