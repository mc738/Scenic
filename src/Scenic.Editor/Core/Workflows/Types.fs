namespace Scenic.Editor.Core.Workflows

open Avalonia.Controls
open CommonResourceFormats.AssetStore.Core.Domain
open Scenic.Core

module Types =

    type WorkflowListingItem =
        {
            Key: EntityKey
            Name: string
        }
    
    [<AbstractClass>]
    type NewAssetWorkflowControl() =
        inherit StackPanel()

        abstract member CreateMetadata: unit -> EntityMetadata

        abstract member GetAssetType: unit -> EntityKey

    [<AbstractClass>]
    type PreviewAssetWorkflowControl() =
        inherit Control()

        abstract member CreateMetadata: unit -> EntityMetadata
    
    [<AbstractClass>]
    type EditAssetWorkflowControl(ctx: ScenicContext, parentWindow: Window, asset: Asset) =
        inherit StackPanel()
        
        abstract member Save: unit -> unit
        

    type AssetWorkflowFactory =
        { Key: EntityKey
          Name: string
          CreateNewWorkflowControl: unit -> NewAssetWorkflowControl
          CreatePreviewWorkflowControl: ScenicContext -> Asset -> PreviewAssetWorkflowControl
          CreateEditWorkflowControl: ScenicContext -> Window -> Asset -> EditAssetWorkflowControl }

    type AssetWorkflowsHandler =
        { Factories: Map<EntityKey, AssetWorkflowFactory> }
        
        member this.GetListings() =
            this.Factories.Values |> Seq.map (fun f -> { Key = f.Key; Name = f.Name })


    [<AbstractClass>]
    type NewComponentWorkflowControl() =
        inherit StackPanel()

        abstract member CreateMetadata: unit -> EntityMetadata

        abstract member GetComponentType: unit -> EntityKey

    [<AbstractClass>]
    type PreviewComponentWorkflowControl() =
        inherit Control()

        abstract member CreateMetadata: unit -> EntityMetadata

    
    [<AbstractClass>]
    type EditComponentWorkflowControl(ctx: ScenicContext, parentWindow: Window, comp: Component) =
        inherit StackPanel()
        
        abstract member Save: unit -> unit
        
    
    type ComponentWorkflowFactory =
        { Key: EntityKey
          Name: string
          CreateNewWorkflowControl: unit -> NewComponentWorkflowControl
          CreatePreviewWorkflowControl: ScenicContext -> Component -> PreviewComponentWorkflowControl
          CreateEditWorkflowControl: ScenicContext -> Window -> Component -> EditComponentWorkflowControl }

    type ComponentWorkflowsHandler =
        { Factories: Map<EntityKey, ComponentWorkflowFactory> }
        
        member this.GetListings() =
            this.Factories.Values |> Seq.map (fun f -> { Key = f.Key; Name = f.Name })

    type WorkflowHandlers =
        { Assets: AssetWorkflowsHandler
          Components: ComponentWorkflowsHandler
        // Resources
        // Scenes
        }

    type WorkflowsBuilder() as this =
        let mutable assetHandlers = ResizeArray<EntityKey * AssetWorkflowFactory>()
        let mutable componentHandlers = ResizeArray<EntityKey * ComponentWorkflowFactory>()

        member _.WithAssetHandler(key: EntityKey, factory: AssetWorkflowFactory) =
            assetHandlers.Add(key, factory)
            this

        member _.WithAssetHandlers(handlers: (EntityKey * AssetWorkflowFactory) seq) =
            assetHandlers.AddRange(handlers)
            this

        member _.WithComponentHandler(key: EntityKey, factory: ComponentWorkflowFactory) =
            componentHandlers.Add(key, factory)
            this

        member _.WithComponentHandlers(handlers: (EntityKey * ComponentWorkflowFactory) seq) =
            componentHandlers.AddRange(handlers)
            this

        member _.Build() =
            ({
            Assets = { Factories = assetHandlers |> Map.ofSeq } 
            Components = { Factories = componentHandlers |> Map.ofSeq }
        }: WorkflowHandlers)