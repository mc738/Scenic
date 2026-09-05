namespace Scenic.Editor.Views.SceneEditor

open System
open System.Collections.Generic
open System.Windows.Input
open Avalonia.Controls
open Avalonia.Layout
open Avalonia.Media
open Avalonia.Threading
open CommonResourceFormats.AssetStore.Core.Domain
open FsToolbox.OpenGL
open Scenic.Component
open Scenic.Editor.Components.Debugging
open Scenic.Editor.Core
open Scenic.Editor.Core.Domain
open Scenic.Editor.Core.Input
open Silk.NET.OpenGL
open Avalonia.Interactivity
open FsToolbox.GameDevelopment.Core.Types


type SceneEditorView(ctx: EditorContext, parentWindow: Window, scene: Scene) as this =
    inherit DockPanel()

    let viewport = Viewport3D(ctx, this)

    let layout = StackPanel()

    let treeView = TreeView()

    let mutable debugPlane = Operators.Unchecked.defaultof<DebugPlane>

    let mutable editorGrid = Operators.Unchecked.defaultof<EditorGrid>

    //let mutable cm = Operators.Unchecked.defaultof<ContentManager>
    let mutable render = Operators.Unchecked.defaultof<Render>

    let objects = Dictionary<Guid, SceneObject>()

    let objectPanel = ObjectPanel(ctx, parentWindow)

    // TEST
    //let tf = TransformControl()

    do
        let rec traverse (sceneObject: SceneObject) =
            objects.Add(
                match sceneObject.Id with
                | EntityId.Guid uid -> uid, sceneObject
            )

            sceneObject.Children |> Seq.iter traverse

        scene.Objects |> Seq.iter traverse


        layout.HorizontalAlignment <- HorizontalAlignment.Stretch
        layout.VerticalAlignment <- VerticalAlignment.Stretch
        viewport.HorizontalAlignment <- HorizontalAlignment.Stretch
        viewport.VerticalAlignment <- VerticalAlignment.Stretch
        layout.Children.Add(viewport)

        let sidePanel = Border()
        sidePanel.Width <- 250
        DockPanel.SetDock(sidePanel, Dock.Left)

        let objectPanelDock = Border()
        objectPanelDock.Width <- 250
        DockPanel.SetDock(objectPanelDock, Dock.Right)

        objectPanelDock.Child <- objectPanel

        treeView.VerticalAlignment <- VerticalAlignment.Stretch
        treeView.HorizontalAlignment <- HorizontalAlignment.Stretch

        let sidePanelLayout = Grid()

        sidePanelLayout.VerticalAlignment <- VerticalAlignment.Stretch
        sidePanelLayout.HorizontalAlignment <- HorizontalAlignment.Stretch

        sidePanel.Child <- sidePanelLayout

        //sidePanelLayout.Children.Add(tf)

        sidePanelLayout.Children.Add(treeView)


        let sidePanelContextMenu = ContextMenu()

        let menuItem = MenuItem()

        menuItem.Header <- "Add"
        menuItem.Name <- "Add"

        menuItem.Command <- RelayCommand((fun _ -> this.AddSceneObject(None)), (fun _ -> true))

        sidePanelContextMenu.Items.Add(menuItem)


        sidePanel.ContextMenu <- sidePanelContextMenu

        viewport.Height <- this.Bounds.Height
        viewport.Width <- this.Bounds.Width

        this.Children.Add(sidePanel)
        this.Children.Add(objectPanelDock)
        this.Children.Add(layout)
        this.Focusable <- true
        this.IsHitTestVisible <- true
        this.Focus() |> ignore

        // THIS IS IMPORTANT, or nothing renders but open gl clears.
        this.Background <- Brushes.Transparent
        this.KeyUp.Add(fun e -> viewport.ForwardKeyUp(e))
        this.KeyDown.Add(fun e -> viewport.ForwardKeyDown(e))

        this.PointerMoved.Add(fun e -> viewport.ForwardPointerMoved(e))
        this.PointerPressed.Add(fun e -> viewport.ForwardPointerPressed(e))

        this.SizeChanged.Add(fun e ->
            viewport.Height <- layout.Bounds.Size.Height
            viewport.Width <- layout.Bounds.Size.Width)

        treeView.SelectionChanged.Add(fun e ->
            if e.AddedItems.Count = 0 then
                ()
            else
                let item = e.AddedItems[0]

                let tvi = item :?> TreeViewItem

                let id =
                    match tvi.DataContext :?> EntityId with
                    | EntityId.Guid uid -> uid

                printfn $"Object selected: {id}"

                objectPanel.SetObject(objects[id])


                ())


        this.BuildTreeView()

    interface IViewportHost with
        member this.RequestScene() =
            (*
            match scene with
            | Some s -> Some s
            | None ->
                match viewport.GL with
                | None -> None
                | Some gl ->
                    //cm <- ContentManager(ctx.GetService<ILoggerFactory>().CreateLogger<ContentManager>(), gl)
                    //scene <- Some(Test.loadScene cm)
                    scene
            *)
            None

        member this.OnScreenRaycast(ray, t) =
            (*
            let mutable closest = Option<float32>.None
            let mutable closestObjID = Option<Guid>.None
            
            match t, scene with
            | _, None -> ()
            | MouseClick cmt, Some scene ->
                match cmt with
                | LeftButton ->
                    for obj in scene.GetSceneModels() do
                        match obj.BoundingBox with
                        | None -> ()
                        | Some bb ->
                            match bb with
                            | AABBs aabbs ->
                                for aabb in aabbs do
                                    match aabb.TestIntersect ray with
                                    | Some t ->
                                        match closest with
                                        | None ->
                                            closest <- Some t
                                            closestObjID <- Some obj.Id
                                        | Some c ->
                                            if (t < c) then
                                                closest <- t |> Some
                                                closestObjID <- Some obj.Id
                                            
                                    | None -> ()
                                    
                                    
                match closestObjID with
                | None -> ()
                | Some objId ->
                    let obj = scene.GetSceneObject(objId)
                    
                    printfn $"Clicked: {obj.Id}; Transform: {obj.Transform}"
                    
                    tf.SetObject(obj)
                
                ()
            *)
            ()

        member this.RenderScene(gl, view, projection) =

            gl.Enable(EnableCap.Blend)
            gl.BlendFunc(BlendingFactor.SrcAlpha, BlendingFactor.OneMinusSrcAlpha)

            editorGrid.Draw(view, projection, viewport.CameraPosition)


        //debugPlane.Bind(view, projection)
        //render.DrawElements(PrimitiveType.Triangles, DrawElementsType.UnsignedInt, 6u)

        member this.ViewportLoaded(gl) =
            debugPlane <- DebugPlane(gl)
            render <- Render(gl)
            editorGrid <- EditorGrid(gl)

    member this.BuildTreeView() =


        let rec build (parent: TreeViewItem) (object: SceneObject) =
            let item = TreeViewItem()

            item.DataContext <- object.Id
            item.Header <- object.Name

            let itemContextMenu = ContextMenu()

            let addItem = MenuItem()
            addItem.Header <- "Add child"
            addItem.Command <- RelayCommand((fun _ -> this.AddSceneObject(Some item)), (fun _ -> true))

            itemContextMenu.Items.Add(addItem) |> ignore
            item.ContextMenu <- itemContextMenu


            for child in object.Children do
                build item child


            parent.Items.Add(item) |> ignore


        for object in scene.Objects do
            let item = TreeViewItem()

            item.DataContext <- object.Id
            item.Header <- object.Name

            let itemContextMenu = ContextMenu()

            let addItem = MenuItem()
            addItem.Header <- "Add child"
            addItem.Command <- RelayCommand((fun _ -> this.AddSceneObject(Some item)), (fun _ -> true))

            itemContextMenu.Items.Add(addItem) |> ignore
            item.ContextMenu <- itemContextMenu

            for child in object.Children do
                build item child

            treeView.Items.Add(item) |> ignore

        ()





    member this.AddSceneObject(parent: TreeViewItem option) =
        let eId = EntityId.Create()
        let name = "New object"

        match parent with
        | None ->
            match ctx.ScenicContext.AssetStore.AddSceneObject(scene.VersionId, None, name, Transform.Default) with
            | Error errorValue -> printfn $"Error adding scene object: {errorValue}"
            | Ok eId ->
                let mutable item = TreeViewItem()

                item.DataContext <- eId

                item.Header <- name

                let itemContextMenu = ContextMenu()

                let addItem = MenuItem()
                addItem.Header <- "Add child"
                addItem.Command <- RelayCommand((fun _ -> this.AddSceneObject(Some item)), (fun _ -> true))

                itemContextMenu.Items.Add(addItem) |> ignore
                item.ContextMenu <- itemContextMenu

                treeView.Items.Add(item) |> ignore

                objects.Add(
                    match eId with
                    | EntityId.Guid uid ->
                        uid,
                        ({ Id = eId
                           Name = name
                           Children = ResizeArray<SceneObject>()
                           Components = ResizeArray<SceneObjectComponent>()
                           Metadata = Map.empty
                           Transform = Transform.Default }
                        : SceneObject)
                )

        | Some(value: TreeViewItem) ->
            let pId = value.DataContext :?> EntityId

            match ctx.ScenicContext.AssetStore.AddSceneObject(scene.VersionId, (Some pId), name, Transform.Default) with
            | Error errorValue -> printfn $"Error adding scene object: {errorValue}"
            | Ok eId ->
                let item = TreeViewItem()

                item.Header <- name

                let itemContextMenu = ContextMenu()

                item.DataContext <- eId

                let addItem = MenuItem()
                addItem.Header <- "Add child"
                addItem.Command <- RelayCommand((fun _ -> this.AddSceneObject(Some item)), (fun _ -> true))

                itemContextMenu.Items.Add(addItem) |> ignore
                item.ContextMenu <- itemContextMenu

                value.Items.Add(item) |> ignore
                
                objects.Add(
                    match eId with
                    | EntityId.Guid uid ->
                        uid,
                        ({ Id = eId
                           Name = name
                           Children = ResizeArray<SceneObject>()
                           Components = ResizeArray<SceneObjectComponent>()
                           Metadata = Map.empty
                           Transform = Transform.Default }
                        : SceneObject)
                )


    member this.FocusNow() =
        // Ensure focus happens after layout
        Dispatcher.UIThread.Post(fun () -> this.Focus() |> ignore)
