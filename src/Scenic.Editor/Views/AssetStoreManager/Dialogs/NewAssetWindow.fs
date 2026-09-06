namespace Scenic.Editor.Windows

open Avalonia.Controls
open Avalonia.Controls.Presenters
open Avalonia.Interactivity
open Avalonia.Layout
open Avalonia.Media
open Avalonia.Platform.Storage
open CommonResourceFormats.AssetStore.Core.Domain
open Scenic.Editor.Core.Workflows.Types
open Scenic.Editor.Core.Dsl

type NewAssetWindow(workflows: AssetWorkflowsHandler) as this =
    inherit Window()


    let mutable path = ""

    let nameBox = TextBox()

    let pathLabel = TextBlock()

    let isPrototypeBox = CheckBox()

    let confirmButton = Button()
    let cancelButton = Button()

    let assetTypeBox = ComboBox()

    let mutable workflowControl: NewAssetWorkflowControl option = None

    let workflowSpecificContent = ContentPresenter()

    do
        this.WindowDecorations <- WindowDecorations.None

        this.Height <- 300
        this.Width <- 400
        this.WindowStartupLocation <- WindowStartupLocation.CenterOwner
        this.Title <- "Create new asset"

        //let layout = StackPanel()

        //let title = TextBlock(Text = "Create new asset", FontWeight = FontWeight.Bold)

        //let nameLabel = Label(Content = "Name", Target = nameBox)

        //let assetTypeBox = ComboBox()

        //let protoTypeLabel = Label(Content = "Is prototype", Target = isPrototypeBox)

        //let assetTypeLabel = Label(Content = "Asset type", Target = assetTypeBox)

        //for listing in workflows.GetListings() do
        //    assetTypeBox.Items.Add(new ComboBoxItem(Content = "Model", DataContext = listing.Key))
        //    |> ignore

        //let assetPathLabel = Label(Content = "Path", Target = assetTypeBox)

        //let apLayout = StackPanel()
        //apLayout.Orientation <- Orientation.Horizontal

        //pathLabel.HorizontalAlignment <- HorizontalAlignment.Stretch

        //apLayout.Children.Add(pathLabel)

        //let b = new Button(Content = "Select")

        //b.Width <- 50
        //b.Click.Add(fun e -> this.OpenFile(this, e))

        //apLayout.Children.Add(b)

        (*
        assetTypeBox.SelectionChanged.Add(fun e ->
            if e.AddedItems.Count = 0 then
                ()
            else
                let item = e.AddedItems[0]

                let cb = item :?> ComboBoxItem

                let sat = cb.DataContext :?> EntityKey

                match workflows.Factories.TryFind sat with
                | None -> failwith "todo"
                | Some value ->
                    //  Clear the work flow specific stuff.
                    let newControl = value.CreateNewAssetWorkflowControl()

                    workflowControl <- Some newControl)
        *)

        //confirmButton.Content <- "Create"
        //cancelButton.Content <- "Cancel"
        //
        //confirmButton.Classes.Add("ok")
        //cancelButton.Classes.Add("cancel")
        //
        //confirmButton.Click.Add(fun e ->
        //    )
        //
        //cancelButton.Click.Add(fun e ->
        //    printfn "Cancel"
        //
        //    this.Close(None)
        //    ())

        //let buttonsLayout = StackPanel()
        //buttonsLayout.Orientation <- Orientation.Horizontal
        //buttonsLayout.Children.Add(confirmButton)
        //buttonsLayout.Children.Add(cancelButton)

        //layout.Children.Add(title)
        //layout.Children.Add(nameLabel)
        //layout.Children.Add(nameBox)
        //layout.Children.Add(protoTypeLabel)
        //layout.Children.Add(isPrototypeBox)
        //layout.Children.Add(Label(Content = "Path"))
        //layout.Children.Add(apLayout)
        //layout.Children.Add(assetTypeLabel)
        //layout.Children.Add(assetTypeBox)
        //
        //layout.Children.Add(assetTypeSpecificContent)
        //
        //layout.Children.Add(buttonsLayout)
        //
        //this.Content <- layout


        this.Content <-
            StackPanel.create ControlStyle.Fill
            |> withChildren
                [ TextBlock(Text = "Create new asset", FontWeight = FontWeight.Bold)
                  Label.createDefault () |> Label.withContent "name" |> Label.withTarget nameBox
                  nameBox

                  Label.createDefault ()
                  |> Label.withContent "Is prototype"
                  |> Label.withTarget isPrototypeBox
                  isPrototypeBox

                  Label.createDefault () |> Label.withContent "Path" //|> Label.withTarget apLayout
                  StackPanel.createDefault ()
                  |> StackPanel.withOrientation Orientation.Horizontal
                  |> withChildren
                      [ Button.create ControlStyle.Default
                        |> Button.withContent "Select"
                        |> Button.onClick this.OpenFile ]

                  Label.createDefault ()
                  |> Label.withContent "Asset type"
                  |> Label.withTarget assetTypeBox

                  assetTypeBox
                  |> withItems
                      false
                      [ for listing in workflows.GetListings() do
                            ComboBoxItem(Content = listing.Name, DataContext = listing.Key) ]
                  |> ComboBox.withSelectionChanged this.OnTypeSelectionChanged

                  workflowSpecificContent

                  StackPanel.createDefault ()
                  |> StackPanel.withOrientation Orientation.Horizontal
                  |> withChildren
                      [ Button.create
                            { ControlStyle.Default with
                                Classes = [ "ok" ] }
                        |> Button.withContent "Add"
                        |> Button.onClick this.OnOk
                        Button.create
                            { ControlStyle.Default with
                                Classes = [ "cancel" ] }
                        |> Button.withContent "Cancel"
                        |> Button.onClick this.OnCancel ] ]


    member _.OnOk(e: RoutedEventArgs) =
        printfn "Ok!!"

        match workflowControl with
        | None -> ()
        | Some wfc ->
            let asset =
                ({ Id = EntityId.Create()
                   VersionId = EntityId.Create()
                   Name = nameBox.Text
                   AssetType = wfc.GetAssetType()
                   IsPrototype = isPrototypeBox.IsChecked |> Option.ofNullable |> Option.defaultValue false
                   Metadata = EntityMetadata.Empty
                   VersionMetadata = EntityMetadata.Empty
                   Path = EntityPath.Absolute path }
                : NewAsset)

            this.Close(Some asset)

    member _.OnCancel(e: RoutedEventArgs) = this.Close(None)

    member _.OnTypeSelectionChanged(e: SelectionChangedEventArgs) =
        if e.AddedItems.Count = 0 then
            ()
        else
            let item = e.AddedItems[0]

            let cb = item :?> ComboBoxItem

            let sat = cb.DataContext :?> EntityKey

            match workflows.Factories.TryFind sat with
            | None -> failwith "todo"
            | Some value ->
                //  Clear the work flow specific stuff.
                let newControl = value.CreateNewWorkflowControl()

                workflowControl <- Some newControl
                workflowSpecificContent.Content <- newControl


    member _.OpenFile(args: RoutedEventArgs) =
        async {
            let topLevel = TopLevel.GetTopLevel(this)

            let options = FilePickerOpenOptions()

            options.Title <- "Select asset path"
            options.AllowMultiple <- false

            let! files = topLevel.StorageProvider.OpenFilePickerAsync(options) |> Async.AwaitTask

            match files |> Seq.tryHead with
            | None -> ()
            | Some value ->

                path <- value.Path.AbsolutePath

                printfn $"Path: {path}"

        }
        |> Async.StartImmediate
