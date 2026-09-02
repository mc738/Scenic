namespace Scenic.Editor.Windows

open Avalonia.Controls
open Avalonia.Interactivity
open Avalonia.Layout
open Avalonia.Media
open Avalonia.Platform.Storage

type NewAssetWindow() as this =
    inherit Window()

    let mutable path = ""
    
    let nameBox = TextBox()
    
    let confirmButton = Button()
    let cancelButton = Button()
    
    
    do
        this.WindowDecorations <- WindowDecorations.None


        this.Height <- 200
        this.Width <- 400
        this.WindowStartupLocation <- WindowStartupLocation.CenterOwner
        this.Title <- "Create new asset"
        
        let layout = StackPanel()
        
        let title = TextBlock(Text = "Create new asset", FontWeight = FontWeight.Bold)
        
        
        let nameLabel = Label(Content = "Name", Target = nameBox)
        
        let assetTypeBox = ComboBox()
        
        let assetTypeLabel = Label(Content = "", Target = assetTypeBox)
        
        assetTypeBox.Items.Add(new ComboBoxItem(Content = "", DataContext = "")) |> ignore
        
        
        confirmButton.Content <- "Create"
        cancelButton.Content <- "Cancel"
        
        confirmButton.Classes.Add("ok")
        cancelButton.Classes.Add("cancel")
        
        confirmButton.Click.Add(fun e ->
            printfn "Ok!!"
            
            this.Close(None)
            ())
        cancelButton.Click.Add(fun e ->
            printfn "Cancel"
            
            this.Close(None)
            ())
        
        let buttonsLayout = StackPanel()
        buttonsLayout.Orientation <- Orientation.Horizontal
        buttonsLayout.Children.Add(confirmButton)
        buttonsLayout.Children.Add(cancelButton)
        
        
        
        layout.Children.Add(title)
        layout.Children.Add(nameLabel)
        layout.Children.Add(nameBox)
        layout.Children.Add(assetTypeLabel)
        layout.Children.Add(assetTypeBox)
       
        layout.Children.Add(buttonsLayout)
        
        this.Content <- layout



    member _.OpenFile(sender: obj, args: RoutedEventArgs) =
        async {
            let topLevel = TopLevel.GetTopLevel(this)
            
            let options = FilePickerOpenOptions()
            
            options.Title <- "Select asset path"
            options.AllowMultiple <- false
            
            let! files =
                topLevel.StorageProvider.OpenFilePickerAsync(options)
                |> Async.AwaitTask
            
            match files |> Seq.tryHead with
            | None -> ()
            | Some value ->
                
                path <- value.Path.AbsolutePath
                
        }
        |> Async.StartImmediate