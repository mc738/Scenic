namespace Scene.Core

open System.IO
open CommonResourceFormats.AssetStore
open CommonResourceFormats.AssetStore.Core.Domain

type ScenicInternalDirectories =
    { Main: string
      Tools: string
      Scripts: string
      Plugins: string
      Tmp: string }

type ScenicContext =
    { RootPath: string
      AssetStore: AssetStoreContext
      InternalDirectories: ScenicInternalDirectories }

    static member Initialize(root: string) =

        // Create the main .scenic folder.
        let scenicDirectoryPath = Path.Combine(root, ".scenic")
        Directory.CreateDirectory(scenicDirectoryPath) |> ignore

        // Create various directories.
        let toolsDirectoryPath = Path.Combine(scenicDirectoryPath, "tools")
        Directory.CreateDirectory(toolsDirectoryPath) |> ignore

        let scriptsDirectoryPath = Path.Combine(scenicDirectoryPath, "scripts")
        Directory.CreateDirectory(scriptsDirectoryPath) |> ignore

        let pluginsDirectoryPath = Path.Combine(scenicDirectoryPath, "plugins")
        Directory.CreateDirectory(pluginsDirectoryPath) |> ignore

        let tmpDirectoryPath = Path.Combine(scenicDirectoryPath, ".tmp")
        Directory.CreateDirectory(tmpDirectoryPath) |> ignore

        // Create an asset store if it doesn't currently exist.
        let assetStorePath = Path.Combine(scenicDirectoryPath, "asset_store.db")

        let assetStoreContext = AssetStoreContext(assetStorePath)

        { RootPath = root
          AssetStore = assetStoreContext
          InternalDirectories =
            { Main = scenicDirectoryPath
              Tools = toolsDirectoryPath
              Scripts = scriptsDirectoryPath
              Plugins = pluginsDirectoryPath
              Tmp = tmpDirectoryPath } }

    member this.TryResolvePath(entityPath: EntityPath) =
        match entityPath with
        | EntityPath.Relative(relativePathType, s) ->
            match relativePathType with
            | RelativePathType.Root -> Ok(Path.Combine(this.RootPath, s))
            | RelativePathType.Named name ->
                match name.ToUpper() with
                | "TOOLS" -> Ok this.InternalDirectories.Tools
                | "SCRIPTS" -> Ok this.InternalDirectories.Scripts
                | "PLUGINS" -> Ok this.InternalDirectories.Plugins
                | "TMP" -> Ok this.InternalDirectories.Tmp
                | _ -> Error $"Unknown named path: {name}"
                |> Result.map (fun fp -> Path.Combine(fp, s))
        | EntityPath.Absolute s -> Ok s
