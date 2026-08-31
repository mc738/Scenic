namespace Scenic.Editor

open Avalonia
open Avalonia.Controls
open Avalonia.Interactivity
open Avalonia.Layout
open Avalonia.Markup.Xaml
open Scenic.Editor.Core
open Scenic.Editor.Views
open Scenic.Editor.Windows

type MainWindow(ctx: EditorContext) as this =
    inherit Window()

    let mutable tabControl: TabControl option = None

    let newSceneWindow = NewSceneWindow()

    do this.InitializeComponent()

    member private this.InitializeComponent() =
        AvaloniaXamlLoader.Load(this)

        //let editorHold = this.FindControl<ContentControl>("SceneEditorHole")

        //let tabControl = this.FindControl<TabControl>("MainTabControl")

        //editorHold.Content <- SceneEditor(ctx)

    member _.AddNewScene(sender: obj, e: RoutedEventArgs) =
        async {
            // Create new scene.
            let tc =
                match tabControl with
                | Some tc -> tc
                | None ->
                    let tc = this.FindControl<TabControl>("MainTabControl")
                    tabControl <- Some tc
                    tc

            let! name = newSceneWindow.ShowDialog<string option>(this) |> Async.AwaitTask
            
            match name  with
            | None -> ()
            | Some value ->
                let newTab = new TabItem(Header = value)
                
                let g = Grid()
                
                g.VerticalAlignment <- VerticalAlignment.Stretch
                g.HorizontalAlignment <- HorizontalAlignment.Stretch
                
                let newId = ctx.AssetStoreContext.AddScene value
                
                let sceneEditor = SceneEditor(ctx, newId.VersionId)
                
                g.Children.Add(sceneEditor)
                
                newTab.Content <- g
                
                tc.Items.Add(newTab) |> ignore
        }
        |> Async.StartImmediate
