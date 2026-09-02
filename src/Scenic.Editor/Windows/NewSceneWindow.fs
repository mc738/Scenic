namespace Scenic.Editor.Windows

open Avalonia.Controls
open Avalonia.Layout


type NewSceneWindow() as this =
    inherit Window()

    let textBox = TextBox()
    
    let layout = StackPanel()
    
    let confirmButton = Button()
    let cancelButton = Button()
    
    do
        this.WindowDecorations <- WindowDecorations.None
        
        
        this.Height <- 200
        this.Width <- 400
        this.WindowStartupLocation <- WindowStartupLocation.CenterOwner
        this.Title <- "Create new scene"
        
        textBox.PlaceholderText <- "Scene name"
        
        layout.Children.Add(textBox)
        this.Content <- layout
        
        confirmButton.Content <- "Create"
        cancelButton.Content <- "Cancel"
        
        confirmButton.Classes.Add("ok")
        cancelButton.Classes.Add("cancel")
        
        confirmButton.Click.Add(fun e ->
            printfn "Ok!!"
            
            this.Close(Some textBox.Text)
            ())
        cancelButton.Click.Add(fun e ->
            printfn "Cancel"
            
            this.Close(None)
            ())
        
        let buttonsLayout = StackPanel()
        buttonsLayout.Orientation <- Orientation.Horizontal
        buttonsLayout.Children.Add(confirmButton)
        buttonsLayout.Children.Add(cancelButton)
        
        layout.Children.Add(buttonsLayout)
        
    
