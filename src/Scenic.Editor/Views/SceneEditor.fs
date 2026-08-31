namespace Scenic.Editor.Views

open System
open System.Windows.Input
open Avalonia.Controls
open Avalonia.Layout
open Avalonia.Media
open Avalonia.Threading
open FsToolbox.OpenGL
open Scenic.Component
open Scenic.Editor.Components.Debugging
open Scenic.Editor.Core
open Scenic.Editor.Core.Domain
open Scenic.Editor.Core.Input
open Silk.NET.OpenGL
open Avalonia.Interactivity


type RelayCommand(action: unit -> unit, canExecute: unit -> bool) =
    interface ICommand with
        member this.CanExecute(parameter) =
            canExecute()

        member this.add_CanExecuteChanged(value: EventHandler) =
            ()


        member this.remove_CanExecuteChanged(value: EventHandler) =
            ()

        member this.Execute(parameter) = action()


    member _.Test = ()

type SceneEditor(ctx: EditorContext) as this =
    inherit DockPanel()

    let viewport = Viewport3D(ctx, this)

    let layout = StackPanel()


    let treeView = TreeView()

    let mutable debugPlane = Operators.Unchecked.defaultof<DebugPlane>
    
    let mutable editorGrid = Operators.Unchecked.defaultof<EditorGrid>

    //let mutable cm = Operators.Unchecked.defaultof<ContentManager>
    let mutable scene = Option<EditorSceneInstance>.None
    let mutable render = Operators.Unchecked.defaultof<Render>

    // TEST
    //let tf = TransformControl()

    do
        layout.HorizontalAlignment <- HorizontalAlignment.Stretch
        layout.VerticalAlignment <- VerticalAlignment.Stretch
        viewport.HorizontalAlignment <- HorizontalAlignment.Stretch
        viewport.VerticalAlignment <- VerticalAlignment.Stretch
        layout.Children.Add(viewport)

        let sidePanel = Border()
        sidePanel.Width <- 250
        DockPanel.SetDock(sidePanel, Dock.Left)


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

        
        let rec addTreeViewItem (parent: TreeViewItem option) (id: Guid) =
            let item = TreeViewItem()
            
            
            item.Header <- "New Item"
            
            let itemContextMenu = ContextMenu()
            
            
            let addItem = MenuItem()
            addItem.Header <- "Add child"
            addItem.Command <- RelayCommand((fun _ -> printfn "Add child to %s" (id.ToString("n")); addTreeViewItem (Some item) (Guid.NewGuid())), (fun _ -> true))

            itemContextMenu.Items.Add(addItem) |> ignore
            item.ContextMenu <- itemContextMenu
           
            match parent with
            | None -> treeView.Items.Add(item) |> ignore
            | Some p -> p.Items.Add(item) |> ignore
            
        menuItem.Command <- RelayCommand((fun _ -> addTreeViewItem None (Guid.NewGuid())), (fun _ -> true))

        sidePanelContextMenu.Items.Add(menuItem)


        sidePanel.ContextMenu <- sidePanelContextMenu

        viewport.Height <- this.Bounds.Height
        viewport.Width <- this.Bounds.Width

        this.Children.Add(sidePanel)
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



    member this.FocusNow() =
        // Ensure focus happens after layout
        Dispatcher.UIThread.Post(fun () -> this.Focus() |> ignore)
