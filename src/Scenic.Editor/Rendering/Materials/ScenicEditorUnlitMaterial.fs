namespace Scenic.Editor.Rendering.Materials

open System
open CommonResourceFormats.AssetStore.Core.Domain
open FsToolbox.OpenGL.Materials
open FsToolbox.OpenGL.Shaders
open Silk.NET.OpenGL

type ScenicEditorUnlitMaterial(shader: OpenGLShader) =
    inherit OpenGLMaterial(shader)
    
    /// A fixed entity ID that will always be used for this.
    static member EntityId = Guid.Parse("7CC0B76D-0552-4C95-9AFB-5E83488AB92A") |> EntityId.Guid 
    
    static member Create(gl: GL) =
        let shader = OpenGLShader.CreateFromFile(gl, "", "")
        
        ScenicEditorUnlitMaterial(shader)
    
    member _.Test() = ()
    
    override this.OnModelBind() =
        
        
        ()