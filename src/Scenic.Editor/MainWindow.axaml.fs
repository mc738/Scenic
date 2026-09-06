namespace Scenic.Editor

open Avalonia
open Avalonia.Controls
open Avalonia.Interactivity
open Avalonia.Layout
open Avalonia.Markup.Xaml
open CommonResourceFormats.AssetStore.Core.Domain
open Scenic.Editor.Core
open Scenic.Editor.Views
open Scenic.Editor.Views.SceneEditor
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
                
                let newId = ctx.ScenicContext.AssetStore.AddScene value
                
                let sceneEditor = SceneEditorView(ctx, this, newId)
                
                g.Children.Add(sceneEditor)
                
                newTab.Content <- g
                
                tc.Items.Add(newTab) |> ignore
        }
        |> Async.StartImmediate

    member _.OpenScene(sender: obj, e: RoutedEventArgs) =
        async {
            let tc =
                match tabControl with
                | Some tc -> tc
                | None ->
                    let tc = this.FindControl<TabControl>("MainTabControl")
                    tabControl <- Some tc
                    tc
             
            let loadSceneWindow = LoadSceneWindow(ctx.ScenicContext.AssetStore.GetSceneListings())
                    
            let! evId = loadSceneWindow.ShowDialog<EntityId option>(this) |> Async.AwaitTask
            
            match evId with
            | None -> ()
            | Some sceneVersionId ->
                match ctx.ScenicContext.AssetStore.GetSceneVersion(sceneVersionId) with
                | Error e ->
                    printfn $"Failed to load scene: {e}"
                | Ok scene ->
                    
                    let newTab = TabItem(Header = scene.Name)
                    
                    let g = Grid()
                    
                    g.VerticalAlignment <- VerticalAlignment.Stretch
                    g.HorizontalAlignment <- HorizontalAlignment.Stretch
                  
                    let sceneEditor = SceneEditorView(ctx, this, scene)
                    
                    g.Children.Add(sceneEditor)
                    
                    newTab.Content <- g
                    
                    tc.Items.Add(newTab) |> ignore
                
                ()
            
        }
        |> Async.StartImmediate
        
    member _.OpenAssetStoreManager(sender: obj, e: RoutedEventArgs) =
        async {
            
            let assetStoreManagerWindow = AssetStoreManagerWindow(ctx)
            
            let! refreshRequired = assetStoreManagerWindow.ShowDialog<bool>(this) |> Async.AwaitTask
            
            if refreshRequired then
                // TODO handle refresh
                
                ()
            
            ()
        }
        |> Async.StartImmediate
        