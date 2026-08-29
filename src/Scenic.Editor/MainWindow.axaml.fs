namespace Scenic.Editor

open Avalonia
open Avalonia.Controls
open Avalonia.Markup.Xaml
open Scenic.Editor.Core
open Scenic.Editor.Views

type MainWindow (ctx: EditorContext) as this = 
    inherit Window ()

    do this.InitializeComponent()

    member private this.InitializeComponent() =
        AvaloniaXamlLoader.Load(this)
        
        let editorHold = this.FindControl<ContentControl>("SceneEditorHole")
        
        editorHold.Content <- SceneEditor(ctx)