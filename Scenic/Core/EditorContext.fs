namespace Scenic.Core

open Microsoft.Extensions.DependencyInjection

type EditorContext(serviceProvider: ServiceProvider) =

    member _.GetService<'T>() = serviceProvider.GetService<'T>()
