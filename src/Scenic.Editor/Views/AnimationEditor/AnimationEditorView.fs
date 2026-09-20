namespace Scenic.Editor.Views.AnimationEditor

open System.Numerics
open Avalonia.Controls
open Avalonia.Layout
open Avalonia.Media
open Avalonia.Threading
open FsToolbox.GLTF
open FsToolbox.GameDevelopment.Core
open FsToolbox.OpenGL
open FsToolbox.OpenGL.Geometry
open FsToolbox.OpenGL.Materials
open FsToolbox.OpenGL.Shaders
open Scenic.Component
open Scenic.Editor.Components.Debugging
open Scenic.Editor.Core
open Scenic.Editor.Core.Dsl
open SharpGLTF.Schema2
open Silk.NET.OpenGL

type AnimationMaterial(shader) =
    inherit OpenGLMaterial(shader)

    override this.OnModelBind() = ()
        

type AnimationEditorView(ctx: EditorContext) as this =
    inherit DockPanel()
    
    let viewport = Viewport3D(ctx, this)
    
    let model = GLTFLoader.loadModel "/home/maxc/Projects/blender/low_poly_male_rigged.gltf"
    
    let root = GLTFLoader.loadRoot "/home/maxc/Projects/blender/low_poly_male_rigged.gltf"
    
    let armature = Domain.Operations.loadArmature root |> List.head
    let animation = (Domain.Operations.loadAnimations root).[1]
   
    let mutable render = Operators.Unchecked.defaultof<OpenGLRenderer>
    let mutable debugPlane = Operators.Unchecked.defaultof<DebugPlane>
    
    let mutable material = Unchecked.defaultof<OpenGLMaterial>
    
    let mutable shader = Unchecked.defaultof<OpenGLShader>
    
    let renderable = ResizeArray<ElementMesh>()
    
    let mutable totalTime = 0f
    
    
    do
        this.HorizontalAlignment <- HorizontalAlignment.Stretch
        this.VerticalAlignment <- VerticalAlignment.Stretch
        viewport.Height <- this.Bounds.Height
        viewport.Width <- this.Bounds.Width
        
        // THIS IS IMPORTANT, or nothing renders but open gl clears.
        this.Background <- Brushes.Transparent
        this.KeyUp.Add(fun e -> viewport.ForwardKeyUp(e))
        this.KeyDown.Add(fun e -> viewport.ForwardKeyDown(e))

        this.PointerMoved.Add(fun e -> viewport.ForwardPointerMoved(e))
        this.PointerPressed.Add(fun e -> viewport.ForwardPointerPressed(e))

        this.SizeChanged.Add(fun e ->
            viewport.Height <- this.Bounds.Size.Height
            viewport.Width <- this.Bounds.Size.Width)
        
        this.Focusable <- true
        this.Focus() |> ignore
        
        this
        |> setChildren [
           viewport   
           
           //Grid.create ControlStyle.Fill None None
           //|> withChildren [
           // 
           //]
        ]
   
    interface IViewportHost with
    
        member this.OnScreenRaycast(ray, rayType) = ()
        member this.RenderScene(gl, dt,  view, projection) =
            totalTime <- totalTime + dt
            gl.Enable(EnableCap.Blend)
            gl.BlendFunc(BlendingFactor.SrcAlpha, BlendingFactor.OneMinusSrcAlpha)


            gl.Enable(EnableCap.DepthTest)
            gl.DepthFunc(DepthFunction.Less)
            gl.DepthMask(true)
            
            
            material.Use()
            
            material.BindViewProjection(view, projection)
          
            for renderable in renderable do
                let modelMatrix = Transform.Default
                // Draw model
                material.BindModel(modelMatrix.ViewMatrix)
                
                renderable.Bind()

                let time = totalTime % animation.Duration
                
                let anim = Domain.Operations.generateShaderPalette armature animation.Channels time armature.Joints
                
                
                for i, m in anim |> Array.indexed do
                    shader.SetUniform($"uFinalBonesMatrices[{i}]", m)
                    
                    ()
                
                render.DrawElements(PrimitiveType.Triangles, DrawElementsType.UnsignedInt, renderable.IndicesCount)
            
            // Draw debug plane
            debugPlane.Bind(view, projection)
            render.DrawElements(PrimitiveType.Triangles, DrawElementsType.UnsignedInt, 6u)
            
            ()
        member this.RequestScene() = None
        member this.ViewportLoaded(gl) =
            // Upload the model.
            debugPlane <- DebugPlane(gl)

            shader <- OpenGLShader.CreateFromFile(gl, "/home/maxc/Projects/dotnet/Scenic/src/Shaders/test_character.vert", "/home/maxc/Projects/dotnet/Scenic/src/Shaders/test_character.frag")
            
            render <- OpenGLRenderer(gl)
            material <- AnimationMaterial(shader)
            viewport.Camera.SetPosition(Vector3(0f, 1f, 5f))
            
            for mesh in model.Meshes do
                for primitive in mesh.Primitives do
                    let em = ElementMesh(primitive.Layout)
                    em.Build(gl, primitive.Vertices, primitive.Indices)
                    renderable.Add(em)
        
    
    member this.FocusNow() =
        // Ensure focus happens after layout
        Dispatcher.UIThread.Post(fun () -> this.Focus() |> ignore)   


