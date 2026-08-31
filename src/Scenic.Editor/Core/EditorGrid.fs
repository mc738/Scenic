namespace Scenic.Editor.Core

open System.Numerics
open System.Runtime.InteropServices
open FsToolbox.OpenGL.Shaders
open Silk.NET.OpenGL


type EditorGrid(gl: GL) =

    let shader =
        OpenGLShader.CreateFromFile(
            gl,
            @"C:\Users\mclif\Projects\dotnet\Scenic\src\Shaders\editor_grid.vert",
            @"C:\Users\mclif\Projects\dotnet\Scenic\src\Shaders\editor_grid.frag"
        )

    let voa = gl.GenVertexArray()

    do gl.BindVertexArray(voa)

    member _.Draw(view: Matrix4x4, projection: Matrix4x4, cameraPosition: Vector3) =

        gl.BindVertexArray(voa)
        shader.Use()

        shader.SetUniform("uView", view)
        shader.SetUniform("uProjection", projection)
        shader.SetUniform("uCameraPosition", cameraPosition)

        gl.DrawArrays(PrimitiveType.Triangles, 0, 6u)
