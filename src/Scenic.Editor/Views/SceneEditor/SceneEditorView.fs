namespace Scenic.Editor.Views.SceneEditor

open System
open System.Collections.Generic
open System.Numerics
open System.Windows.Input
open Avalonia.Controls
open Avalonia.Layout
open Avalonia.Media
open Avalonia.Threading
open CommonResourceFormats.AssetStore.Core.Domain
open CommonResourceFormats.AssetStore.Operations
open CommonResourceFormats.AssetStore.Store.Persistence
open FsToolbox.GLTF
open FsToolbox.GameDevelopment.Geometry.Types
open FsToolbox.OpenGL
open FsToolbox.OpenGL.Geometry
open FsToolbox.OpenGL.Materials
open FsToolbox.OpenGL.Types
open Scenic.Core.Workflows
open Scenic.Core.Workflows.Standard.V1
open Scenic.Component
open Scenic.Editor.Components.Debugging
open Scenic.Editor.Core
open Scenic.Editor.Core.Domain
open Scenic.Editor.Core.Input
open Scenic.Editor.Rendering
open Scenic.Editor.Rendering.Materials
open Silk.NET.OpenGL
open Avalonia.Interactivity
open FsToolbox.GameDevelopment.Core.Types

// # Material type naming
//
// [namespace]:[type]
//
// ## Examples
//
// opengl-material:basis
// opengl-material:unlit
// opengl-material:lit etc.
//
// # Shader type naming
//
// opengl-shader:vert
// opengl-shader:frag
//
// # Images/textures
//
// texture:colour
// texture:normal
// texture:
// img
//
// # Models
// gltf

(*
module RenderImportWorkflows =


    module Standard =

        module V1 =

            [<RequireQualifiedAccess>]
            module Keys =

                let ``component-model`` = EntityKey.Namespace(scenicNS, "component-model")

                let ``asset-importer`` = EntityKey.Namespace(scenicNS, "asset-importer")
                
                let ``material-slot`` = EntityKey.Namespace(scenicNS, "material-slot")
                
            let loadModel (comp: Component) =
                match
                    comp.Assets
                    |> Seq.tryFind (fun ca ->
                        ca.Metadata.TryGetBool(Keys.``component-model``)
                        |> Option.defaultValue false)
                with
                | None -> Error "No assets are marked as the component model for this component"
                | Some ca ->
                    match ca.Asset.Metadata.TryGet(Keys.``asset-importer``) with
                    | None -> Error "Missing asset importer value"
                    | Some "gltf" -> GLTFLoader.loadModel (ca.Asset.Path.Serialize()) |> Ok
                    | Some v -> Error $"Unknown asset importer: {v}"

            let tryLoadOpenGLMaterial (comp: Component)=
                //comp.Assets
                //|> Seq.filter (fun )
                
                
                match
                    comp.Assets
                    |> Seq.tryFind (fun ca ->
                        
                        ca.Metadata.TryGetBool(Keys.``component-model``)
                        |> Option.defaultValue false)
                with
                | None -> Error "No assets are marked as the component model for this component"
                | Some ca ->
                    match ca.Asset.AssetType with
                    | "opengl-material" ->
                        Ok ()
                    | at -> Error $"Incorrect asset type: {at}"
                    match ca.Asset.Metadata.TryGet(Keys.``asset-importer``) with
                    | None -> Error "Missing asset importer value"
                    | Some "gltf" -> GLTFLoader.loadModel (ca.Asset.Path.Serialize()) |> Ok
                    | Some v -> Error $"Unknown asset importer: {v}"
    
    [<RequireQualifiedAccess>]
    module Keys =

        let ``renderer-import-workflow`` = EntityKey.Namespace (scenicNS, "renderer-import-workflow")

    let tryLoadModel (comp: Component) =

        match comp.Metadata.TryGet(Keys.``renderer-import-workflow``) with
        | None -> Error "No renderer import workflow found"
        | Some "standard"
        | Some "standard-v1" ->
            // "scenic:asset-importer" "gltf"

            Standard.V1.loadModel comp
        | Some v ->
            // Unknown render handler
            Error "No renderer import workflow found"
*)

type SceneEditorView(ctx: EditorContext, parentWindow: Window, scene: Scene) as this =
    inherit DockPanel()
    
    // The transforms for all scene objects, stored in a map.
    // This is for easy access and so they can be resolved via an entity id easily.
    let transformMap = Dictionary<SceneObjectId, Transform>()
    
    let renderBatches = RenderBatches.Empty

    // An internal collection used to decide what will be rendered.
    // The data in this doesn't need to be saved.
    let objectInstances = Dictionary<EntityId, EditorSceneObjectInstance>()

    
    let primitivesToBuild = Queue<EntityId * Primitive>()

    let mutable viewportGL = Operators.Unchecked.defaultof<GL>

    let viewport = Viewport3D(ctx, this)

    let layout = StackPanel()

    let treeView = TreeView()

    let mutable debugPlane = Operators.Unchecked.defaultof<DebugPlane>

    let mutable editorGrid = Operators.Unchecked.defaultof<EditorGrid>

    //let mutable cm = Operators.Unchecked.defaultof<ContentManager>
    let mutable render = Operators.Unchecked.defaultof<Render>

    let objects = Dictionary<Guid, SceneObject>()

    let objectPanel = ObjectPanel(ctx, parentWindow)

    do
        objectPanel.ComponentAdded.Add(this.OnComponentAdded)

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

        sidePanelContextMenu.Items.Add(menuItem) |> ignore


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

        this.BuildTransformMap()
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

            // OpenGL in avalonia only guarantees the gl context will be active in the rendering loop.
            // If these are build else where then they will have no affect.
            while primitivesToBuild.Count > 0 do
                let (sceneObjectId, primitive) = primitivesToBuild.Dequeue()
                
                let em = ElementMesh(primitive.Layout)
                em.Build(gl, primitive.Vertices, primitive.Indices)
                              
                renderBatches.StandardOpaque.Add(RenderBatchItem(sceneObjectId, em))
                    
            gl.Enable(EnableCap.Blend)
            gl.BlendFunc(BlendingFactor.SrcAlpha, BlendingFactor.OneMinusSrcAlpha)
            
            
            gl.Enable(EnableCap.DepthTest)
            gl.DepthFunc(DepthFunction.Less)
            gl.DepthMask(true)

            // Get all object primitives and transforms.

            // Resolve any world transforms etc.

            // Basic render passes

            // Resolve transforms
            // Group all primitives by material

            //

            let vpCam = viewport.Camera
            
            // Sort the batches.
            for bi in renderBatches.StandardOpaque do
                    
                let mutable transform = Transform.Default // transformMap[bi.ObjectId]
                
                transform.Position <- Vector3(0f, 0f, -10f)
                
                let distanceToCamera =
                    Vector3.Dot(transform.Position - vpCam.Position, vpCam.Forward)
                
                bi.SetModelMatrix(transform.ViewMatrix)
                bi.SetDistanceToCamera(distanceToCamera)
            
            let material = viewport.GetMaterial(ScenicEditorMaterialType.Unlit)
            
            material.Use()
            
            material.BindViewProjection(view, projection)
                
            for bi in renderBatches.StandardOpaque |> Seq.sortBy _.DistanceToCamera do
                 
                material.BindModel(Transform.Default.ViewMatrix)
                
                bi.Mesh.Bind()
               
                render.DrawElements(PrimitiveType.Triangles, DrawElementsType.UnsignedInt, bi.Mesh.IndicesCount)
                
            
            // TODO handle transparent.
             
            debugPlane.Bind(view, projection)
            render.DrawElements(PrimitiveType.Triangles, DrawElementsType.UnsignedInt, 6u)
            
            //editorGrid.Draw(view, projection, viewport.CameraPosition)

        member this.ViewportLoaded(gl) =
            debugPlane <- DebugPlane(gl)
            render <- Render(gl)
            editorGrid <- EditorGrid(gl)
            viewportGL <- gl

    member this.BuildTransformMap() =
        let rec build (sceneObject: SceneObject) =
            transformMap.Add(sceneObject.Id, sceneObject.Transform)
            sceneObject.Children |> Seq.iter build
            
        for object in scene.Objects do
            build object
    
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

    member this.OnComponentAdded(e: ComponentAddedEventArgs) =
        match ctx.ScenicContext.AssetStore.GetComponentVersion e.ComponentVersionId with
        | Error errorValue -> printfn $"Error: {errorValue}"
        | Ok componentVersion ->

            // Check if the component has any renderable assets
            match ComponentWorkflows.tryLoadModel componentVersion.Component with
            | Error errorValue ->
                printfn $"Error: {errorValue}"
                failwith "todo"
            | Ok newModel ->
                //match objectInstances.TryGetValue e.SceneObjectId with
                //| false, _ -> ()
                //| true, so ->
                    for mesh in newModel.Meshes do
                          for primitive in mesh.Primitives do
                              // TODO clean up
                              primitivesToBuild.Enqueue(e.SceneObjectId, primitive)
                              //let em = ElementMesh(primitive.Layout)
                              //em.Build(viewportGL, primitive.Vertices, primitive.Indices)
                              
                              //renderBatches.StandardOpaque.Add(RenderBatchItem(e.SceneObjectId, em))
                    
                    (*
                    [ for mesh in newModel.Meshes do
                          for primitive in mesh.Primitives do
                              // TODO clean up
                              let em = ElementMesh(primitive.Layout)
                              em.Build(viewportGL, primitive.Vertices, primitive.Indices)
                              
                              renderBatches.StandardOpaque.Add(RenderBatchItem(e.SceneObjectId, em))
                              
                              yield { Mesh = em; MaterialId = ScenicEditorUnlitMaterial.EntityId } ]
                    |> so.Primitives.AddRange
                    *)

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
                           Metadata = EntityMetadata.Empty
                           Transform = Transform.Default }
                        : SceneObject)
                )
                
                transformMap.Add(eId, Transform.Default)

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
                           Metadata = EntityMetadata.Empty
                           Transform = Transform.Default }
                        : SceneObject)
                )
                
                transformMap.Add(eId, Transform.Default)
                
    member this.FocusNow() =
        // Ensure focus happens after layout
        Dispatcher.UIThread.Post(fun () -> this.Focus() |> ignore)
