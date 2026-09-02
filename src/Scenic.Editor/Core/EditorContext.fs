namespace Scenic.Editor.Core

open CommonResourceFormats.AssetStore
open Microsoft.Extensions.DependencyInjection
open Microsoft.Extensions.Logging
open Scene.Core

type EditorContext =
    { ServiceProvider: ServiceProvider
      LoggerFactory: ILoggerFactory
      ScenicContext: ScenicContext }
