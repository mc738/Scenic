namespace Scenic.Editor.Core

open CommonResourceFormats.AssetStore
open Microsoft.Extensions.DependencyInjection
open Microsoft.Extensions.Logging
open Scenic.Core
open Scenic.Editor.Core.Workflows.Types

type EditorContext =
    { ServiceProvider: ServiceProvider
      LoggerFactory: ILoggerFactory
      ScenicContext: ScenicContext
      WorkflowHandlers: WorkflowHandlers }
