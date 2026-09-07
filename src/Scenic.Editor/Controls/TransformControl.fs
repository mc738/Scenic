namespace Scenic.Editor.Controls

open Avalonia.Controls
open Avalonia.Layout
open CommonResourceFormats.AssetStore.Core.Domain
open FsToolbox.GameDevelopment.Core

type TransformControl() as this =
    inherit StackPanel()

    let transformUpdated = Event<Transform>()
    
    let mutable transform = Transform.Default

    let posX = NumericUpDown()
    let posY = NumericUpDown()
    let posZ = NumericUpDown()

    let rotX = NumericUpDown()
    let rotY = NumericUpDown()
    let rotZ = NumericUpDown()


    let scaX = NumericUpDown()
    let scaY = NumericUpDown()
    let scaZ = NumericUpDown()

    let mutable objet: SceneObject option = None


    do
        // Add position.
        let posSP = Grid()

        posSP.ColumnDefinitions.Add(new ColumnDefinition(GridLength.Star))
        posSP.ColumnDefinitions.Add(new ColumnDefinition(GridLength.Star))
        posSP.ColumnDefinitions.Add(new ColumnDefinition(GridLength.Star))

        posSP.HorizontalAlignment <- HorizontalAlignment.Stretch


        Grid.SetColumn(posX, 0)
        Grid.SetColumn(posY, 1)
        Grid.SetColumn(posZ, 2)
        //posSP.Background <- Brushes.Aqua

        posX.ShowButtonSpinner <- false
        posY.ShowButtonSpinner <- false
        posZ.ShowButtonSpinner <- false
        //posX.HorizontalAlignment <- HorizontalAlignment.Stretch
        //posY.HorizontalAlignment <- HorizontalAlignment.Stretch
        //posZ.HorizontalAlignment <- HorizontalAlignment.Stretch

        posX.Increment <- 0.01 |> decimal
        posY.Increment <- 0.01 |> decimal
        posZ.Increment <- 0.01 |> decimal

        posSP.Children.Add(posX)
        posSP.Children.Add(posY)
        posSP.Children.Add(posZ)

        let posLabel = Label(Content = "Position")


        this.Children.Add(posLabel)
        this.Children.Add(posSP)



        // Add position.
        let rotSP = Grid()

        rotSP.ColumnDefinitions.Add(new ColumnDefinition(GridLength.Star))
        rotSP.ColumnDefinitions.Add(new ColumnDefinition(GridLength.Star))
        rotSP.ColumnDefinitions.Add(new ColumnDefinition(GridLength.Star))

        rotSP.HorizontalAlignment <- HorizontalAlignment.Stretch



        Grid.SetColumn(rotX, 0)
        Grid.SetColumn(rotY, 1)
        Grid.SetColumn(rotZ, 2)
        //posSP.Background <- Brushes.Aqua

        rotX.ShowButtonSpinner <- false
        rotY.ShowButtonSpinner <- false
        rotZ.ShowButtonSpinner <- false
        //posX.HorizontalAlignment <- HorizontalAlignment.Stretch
        //posY.HorizontalAlignment <- HorizontalAlignment.Stretch
        //posZ.HorizontalAlignment <- HorizontalAlignment.Stretch

        rotX.Increment <- 0.01 |> decimal
        rotY.Increment <- 0.01 |> decimal
        rotZ.Increment <- 0.01 |> decimal

        rotSP.Children.Add(rotX)
        rotSP.Children.Add(rotY)
        rotSP.Children.Add(rotZ)

        let rotLabel = Label(Content = "Rotation")

        this.Children.Add(rotLabel)
        this.Children.Add(rotSP)

        // Add position.
        let scaSP = Grid()

        scaSP.ColumnDefinitions.Add(new ColumnDefinition(GridLength.Star))
        scaSP.ColumnDefinitions.Add(new ColumnDefinition(GridLength.Star))
        scaSP.ColumnDefinitions.Add(new ColumnDefinition(GridLength.Star))

        scaSP.HorizontalAlignment <- HorizontalAlignment.Stretch

        Grid.SetColumn(scaX, 0)
        Grid.SetColumn(scaY, 1)
        Grid.SetColumn(scaZ, 2)
        //posSP.Background <- Brushes.Aqua

        scaX.ShowButtonSpinner <- false
        scaY.ShowButtonSpinner <- false
        scaZ.ShowButtonSpinner <- false
        //posX.HorizontalAlignment <- HorizontalAlignment.Stretch
        //posY.HorizontalAlignment <- HorizontalAlignment.Stretch
        //posZ.HorizontalAlignment <- HorizontalAlignment.Stretch

        scaX.Increment <- 0.01 |> decimal
        scaY.Increment <- 0.01 |> decimal
        scaZ.Increment <- 0.01 |> decimal

        scaSP.Children.Add(scaX)
        scaSP.Children.Add(scaY)
        scaSP.Children.Add(scaZ)

        let scaLabel = Label(Content = "Scale")

        this.Children.Add(scaLabel)
        this.Children.Add(scaSP)

        
        posX.ValueChanged.Add(fun e ->
            match objet with
            | None -> ()
            | Some obj ->
                transform.Position.X <- e.NewValue |> Option.ofNullable |> Option.map float32 |> Option.defaultValue 0f
                this.TriggerUpdate())
                

        posY.ValueChanged.Add(fun e ->
            match objet with
            | None -> ()
            | Some obj ->
                transform.Position.Y <- e.NewValue |> Option.ofNullable |> Option.map float32 |> Option.defaultValue 0f
                this.TriggerUpdate())

        posZ.ValueChanged.Add(fun e ->
            match objet with
            | None -> ()
            | Some obj ->
                transform.Position.Z <- e.NewValue |> Option.ofNullable |> Option.map float32 |> Option.defaultValue 0f
                this.TriggerUpdate())

    [<CLIEvent>]
    member _.TransformUpdated = transformUpdated.Publish
    
    member _.SetObject(obj: SceneObject) =
        objet <- Some obj
        transform <- obj.Transform
        this.SetValuesToTransform()

    member _.SetValue(newTransform: Transform) =
        transform <- newTransform
        this.SetValuesToTransform()

    member _.SetValuesToTransform() =
        posX.Value <- transform.Position.X |> decimal
        posY.Value <- transform.Position.Y |> decimal
        posZ.Value <- transform.Position.Z |> decimal
        rotX.Value <- transform.Rotation.X |> decimal
        rotY.Value <- transform.Rotation.Y |> decimal
        rotZ.Value <- transform.Rotation.Z |> decimal
        scaX.Value <- transform.Scale.X |> decimal
        scaY.Value <- transform.Scale.Y |> decimal
        scaZ.Value <- transform.Scale.Z |> decimal

    member _.GetTransform() = transform
    
    member _.TriggerUpdate() =
        transformUpdated.Trigger(transform)
        