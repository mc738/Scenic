namespace Scenic.Views


type SceneEditor(ctx: EditorContext) as this =
    inherit DockPanel()

    let viewport = Viewport3D(ctx, this)

    let layout = StackPanel()

    let mutable cm = Operators.Unchecked.defaultof<ContentManager>
    let mutable scene = Option<EditorSceneInstance>.None

    // TEST
    let tf = TransformControl()
    
    do
        layout.HorizontalAlignment <- HorizontalAlignment.Stretch
        layout.VerticalAlignment <- VerticalAlignment.Stretch
        viewport.HorizontalAlignment <- HorizontalAlignment.Stretch
        viewport.VerticalAlignment <- VerticalAlignment.Stretch
        layout.Children.Add(viewport)

        let sidePanel = Border()
        sidePanel.Width <- 250
        DockPanel.SetDock(sidePanel, Dock.Left)

        
        let sidePanelLayout = StackPanel()
        sidePanel.Child <- sidePanelLayout
        
        sidePanelLayout.Children.Add(tf)

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
            match scene with
            | Some s -> Some s
            | None ->
                match viewport.GL with
                | None -> None
                | Some gl ->
                    cm <- ContentManager(ctx.GetService<ILoggerFactory>().CreateLogger<ContentManager>(), gl)
                    scene <- Some(Test.loadScene cm)
                    scene

        member this.OnScreenRaycast(ray, t) =
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
    member this.FocusNow() =
        // Ensure focus happens after layout
        Dispatcher.UIThread.Post(fun () -> this.Focus() |> ignore)
