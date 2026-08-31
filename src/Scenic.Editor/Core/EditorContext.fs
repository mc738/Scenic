namespace Scenic.Editor.Core

open CommonResourceFormats.AssetStore
open Microsoft.Extensions.DependencyInjection
open Microsoft.Extensions.Logging

type EditorContext =
    { ServiceProvider: ServiceProvider
      LoggerFactory: ILoggerFactory
      AssetStoreContext: AssetStoreContext }
