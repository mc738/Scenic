module Scenic.Editor.Extensions.TurnBasedTactics.Views.LevelEditor

open System.Numerics
open Avalonia.Controls
open Avalonia.Layout
open Avalonia.Media
open Avalonia.Threading
open FsToolbox.GameDevelopment.Geometry.Shapes
open FsToolbox.GameDevelopment.Geometry.Types
open FsToolbox.OpenGL
open FsToolbox.OpenGL.Geometry
open FsToolbox.OpenGL.Materials
open FsToolbox.OpenGL.Shaders
open Scenic.Component
open Scenic.Editor.Core
open Scenic.Editor.Core.Dsl
open Silk.NET.OpenGL

type TBTLevelEditorMaterial(shader: OpenGLShader) =
    inherit OpenGLMaterial(shader)

    override this.OnModelBind() = ()

[<AutoOpen>]
module private Internal =

    let vertexLayout =
        ({ Items =
            [ { Name = "Position"
                ShaderName = "uPos"
                Type = VertexAttributeEncodingType.Float
                Size = 3 }
              { Name = "Normal"
                ShaderName = "uNormal"
                Type = VertexAttributeEncodingType.Float
                Size = 3 }
              { Name = "UV"
                ShaderName = "uUv"
                Type = VertexAttributeEncodingType.Float
                Size = 2 } ] }
        : VertexLayout)

type TBTLevelEditorView(ctx: EditorContext, parentWindow: Window) as this =
    inherit DockPanel()

    let viewport = Viewport3D(ctx, this)

    let mutable renderer = Unchecked.defaultof<OpenGLRenderer>

    // A grid od

    let gridCellSize = 2f

    let gridWidth = 40
    let gridHeight = 40
    let gridLevels = 1

    let levelHeight = 3f

    let instanceMeshCfg =
        let xOffset = (float32 gridWidth * gridCellSize) / 2f
        let zOffset = (float32 gridHeight * gridCellSize) / 2f

        let rng = System.Random()

        ({ Properties = [| InstancedMeshPropertyType.Float4("Color", 0) |]
           Items =
             [| for x in 0 .. gridWidth - 1 do
                    for z in 0 .. gridHeight - 1 do
                        //for y in 0 .. gridLevels - 1 do
                            let mutable t = FsToolbox.GameDevelopment.Core.Types.Transform.Default

                            t.Position <- Vector3((float32 x * gridCellSize) - xOffset, 0f, (float32 z * gridCellSize) - zOffset)

                            ({ Transform = t
                               Properties =
                                 [| { Value = [| rng.NextSingle(); rng.NextSingle(); rng.NextSingle(); 1f |] } |] }
                            : InstancedMeshItem) |] }
        : InstancedMeshConfiguration)

    let layout = StackPanel.create ControlStyle.Fill

    do
        viewport.HorizontalAlignment <- HorizontalAlignment.Stretch
        viewport.VerticalAlignment <- VerticalAlignment.Stretch

        layout |> setChildren [ viewport ]

        // THIS IS IMPORTANT, or nothing renders but open gl clears.
        this.Background <- Brushes.Transparent
        this.KeyUp.Add(fun e -> viewport.ForwardKeyUp(e))
        this.KeyDown.Add(fun e -> viewport.ForwardKeyDown(e))

        this.PointerMoved.Add(fun e -> viewport.ForwardPointerMoved(e))
        this.PointerPressed.Add(fun e -> viewport.ForwardPointerPressed(e))

        this.SizeChanged.Add(fun e ->
            viewport.Height <- layout.Bounds.Size.Height
            viewport.Width <- layout.Bounds.Size.Width)

        this.Children.Add(layout)

    let gridMesh = InstancedElementMesh(Internal.vertexLayout)

    let mutable material = Unchecked.defaultof<TBTLevelEditorMaterial>

    interface IViewportHost with

        member this.OnScreenRaycast(ray, raycastType) =
            
            
            
            ()

        member this.RenderScene(gl, dt, view, projection) =
            gl.Enable(EnableCap.Blend)
            gl.BlendFunc(BlendingFactor.SrcAlpha, BlendingFactor.OneMinusSrcAlpha)


            gl.Enable(EnableCap.DepthTest)
            gl.DepthFunc(DepthFunction.Less)
            gl.DepthMask(true)


            material.Use()
            material.BindViewProjection(view, projection)

            gridMesh.Bind()

            renderer.DrawElementsInstanced(
                PrimitiveType.Triangles,
                DrawElementsType.UnsignedInt,
                uint Quad.indices.Length,
                (gridWidth * gridHeight * gridLevels) |> uint
            )

        member this.RequestScene() = None

        member this.ViewportLoaded(gl) =
            // Load anything requiring loading.
            let verts =
                Quad.buildVerticesData Quad.QuadVertexLayout.Standard gridCellSize gridCellSize

            renderer <- OpenGLRenderer(gl)

            material <-
                TBTLevelEditorMaterial(
                    OpenGLShader.CreateFromFile(
                        gl,
                        @"C:\Users\mclif\Projects\dotnet\Scenic\src\Shaders\tbt_instance.vert",
                        @"C:\Users\mclif\Projects\dotnet\Scenic\src\Shaders\tbt_instance.frag"
                    )
                )

            gridMesh.Build(gl, verts, Quad.indices, instanceMeshCfg)


    
    member this.FocusNow() =
        // Ensure focus happens after layout
        Dispatcher.UIThread.Post(fun () -> this.Focus() |> ignore)