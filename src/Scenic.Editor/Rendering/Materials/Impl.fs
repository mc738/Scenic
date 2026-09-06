namespace Scenic.Editor.Rendering.Materials

open FsToolbox.OpenGL.Materials
open Silk.NET.OpenGL

type ScenicEditorMaterialType =
    | Unlit

type ScenicEditorMaterials =
    { Unlit: ScenicEditorUnlitMaterial }

    static member Create(gl: GL) =
        { Unlit = ScenicEditorUnlitMaterial.Create(gl) }

    member sem.GetMaterialType(material: ScenicEditorMaterialType) =
        match material with
        | Unlit -> sem.Unlit :> OpenGLMaterial 