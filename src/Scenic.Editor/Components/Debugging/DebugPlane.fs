namespace Scenic.Editor.Components.Debugging

open System.Numerics
open FsToolbox.OpenGL.Shaders
open FsToolbox.OpenGL.Types
open Silk.NET.OpenGL
open System

type DebugPlane(gl: GL) =

    let verts =
        [|
           // Vert 1
           -1000f
           -0.01f
           -1000f

           // Vert 2
           1000f
           -0.01f
           -1000f

           // Vert 3
           1000f
           -0.01f
           1000f

           // Vert 4
           -1000f
           -0.01f
           1000f |]

    let indices = [| 0u; 1u; 2u; 2u; 3u; 0u |]

    let mutable vertexBuffer =
        new VertexBufferObject(gl, verts.AsSpan(), BufferTargetARB.ArrayBuffer)

    let mutable indexBuffer =
        new IndexBufferObject(gl, indices.AsSpan(), BufferTargetARB.ElementArrayBuffer)

    let mutable voa =
        new VertexArrayObject(gl, vertexBuffer, indexBuffer)

    let shader =
        OpenGLShader.CreateFromFile(
            gl,
            @"C:\Users\mclif\Projects\dotnet\Scenic\src\Shaders\debug_plane.vert",
            @"C:\Users\mclif\Projects\dotnet\Scenic\src\Shaders\debug_plane.frag"
        )

    do
        voa.Bind()
        vertexBuffer.Bind()
        indexBuffer.Bind()

        gl.EnableVertexAttribArray(0u)
        gl.VertexAttribPointer(0u, 3, VertexAttribPointerType.Float, false, (3 * sizeof<float32>) |> uint, 0)

        gl.BindVertexArray(0u)

    member _.Bind(view: Matrix4x4, projection: Matrix4x4) =
        voa.Bind()
        indexBuffer.Bind()
        shader.Use()

        shader.SetUniform("uModel", Matrix4x4.Identity)
        shader.SetUniform("uView", view)
        shader.SetUniform("uProjection", projection)
    
    
    