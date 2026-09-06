namespace Scenic.Editor.Core

open Avalonia
open Avalonia.Controls
open Avalonia.Controls.Primitives
open Avalonia.Input
open Avalonia.Interactivity
open Avalonia.Layout
open Avalonia.Media
open Scenic.Editor.Core.Domain


module Dsl =

    [<RequireQualifiedAccess>]
    type StretchType =
        | None
        | Vertical
        | Horizontal
        | Both

    type ControlStyle =
        { StretchType: StretchType
          Classes: string list }


        static member Fill =
            { StretchType = StretchType.Both
              Classes = [] }

        static member Default =
            { StretchType = StretchType.None
              Classes = [] }

    [<AutoOpen>]
    module Control =

        let withDataContext<'T, 'TControl when 'TControl :> StyledElement> (data: 'T) (c: 'TControl) =

            c.DataContext <- data
            c

        let tryGetDataContext<'T> (c: Control) =
            try
                c.DataContext :?> 'T |> Ok
            with ex ->
                Error ex.Message

    [<AutoOpen>]
    module SelectingItemsControl =

        let withItems<'T when 'T :> SelectingItemsControl> (clearCurrent: bool) (items: Control seq) (c: 'T) =
            if clearCurrent then
                c.Items.Clear()

            for item in items do
                c.Items.Add(item) |> ignore

            c

        let setItems<'T when 'T :> SelectingItemsControl> (clearCurrent: bool) (items: Control seq) (c: 'T) =
            if clearCurrent then
                c.Items.Clear()

            for item in items do
                c.Items.Add(item) |> ignore

    [<AutoOpen>]
    module Panel =

        let withChildren<'T when 'T :> Panel> (children: Control seq) (p: 'T) =
            p.Children.AddRange(children)
            p

        let setChildren<'T when 'T :> Panel> (children: Control seq) (p: 'T) = p.Children.AddRange(children)

    [<RequireQualifiedAccess>]
    module Grid =

        let create (style: ControlStyle) (columns: ColumnDefinitions option) (rows: RowDefinitions option) =
            let grid = Grid()

            match style.StretchType with
            | StretchType.None -> ()
            | StretchType.Vertical -> grid.VerticalAlignment <- VerticalAlignment.Stretch
            | StretchType.Horizontal -> grid.HorizontalAlignment <- HorizontalAlignment.Stretch
            | StretchType.Both ->
                grid.VerticalAlignment <- VerticalAlignment.Stretch
                grid.HorizontalAlignment <- HorizontalAlignment.Stretch

            grid

        let withChild (child: Control) (row: int option) (column: int option) (grid: Grid) =
            match column with
            | None -> ()
            | Some c -> Grid.SetColumn(child, c)

            match row with
            | None -> ()
            | Some r -> Grid.SetRow(child, r)

            grid.Children.Add(child)

    [<RequireQualifiedAccess>]
    module StackPanel =

        let create (style: ControlStyle) =
            let stackPanel = StackPanel()

            match style.StretchType with
            | StretchType.None -> ()
            | StretchType.Vertical -> stackPanel.VerticalAlignment <- VerticalAlignment.Stretch
            | StretchType.Horizontal -> stackPanel.HorizontalAlignment <- HorizontalAlignment.Stretch
            | StretchType.Both ->
                stackPanel.VerticalAlignment <- VerticalAlignment.Stretch
                stackPanel.HorizontalAlignment <- HorizontalAlignment.Stretch

            stackPanel

        let createDefault () = create ControlStyle.Default

        let withOrientation (o: Orientation) (sp: StackPanel) =
            sp.Orientation <- o
            sp

        let withChild (child: Control) (stackPanel: StackPanel) =
            stackPanel.Children.Add(child)
            stackPanel

    [<RequireQualifiedAccess>]
    module DockPanel =

        let create (style: ControlStyle) =
            let dockPanel = DockPanel()

            match style.StretchType with
            | StretchType.None -> ()
            | StretchType.Vertical -> dockPanel.VerticalAlignment <- VerticalAlignment.Stretch
            | StretchType.Horizontal -> dockPanel.HorizontalAlignment <- HorizontalAlignment.Stretch
            | StretchType.Both ->
                dockPanel.VerticalAlignment <- VerticalAlignment.Stretch
                dockPanel.HorizontalAlignment <- HorizontalAlignment.Stretch

            dockPanel

        let withChild (child: Control) (dock: Dock option) (stackPanel: StackPanel) =
            match dock with
            | None -> ()
            | Some d -> DockPanel.SetDock(child, d)

            stackPanel.Children.Add(child)

    [<RequireQualifiedAccess>]
    module Label =

        let create (style: ControlStyle) =
            let c = Label()

            match style.StretchType with
            | StretchType.None -> ()
            | StretchType.Vertical -> c.VerticalAlignment <- VerticalAlignment.Stretch
            | StretchType.Horizontal -> c.HorizontalAlignment <- HorizontalAlignment.Stretch
            | StretchType.Both ->
                c.VerticalAlignment <- VerticalAlignment.Stretch
                c.HorizontalAlignment <- HorizontalAlignment.Stretch

            for cl in style.Classes do
                c.Classes.Add(cl)

            c

        let createDefault () = create ControlStyle.Default

        let withContent (content: obj) (l: Label) =
            l.Content <- content
            l

        let withTarget (target: IInputElement) (label: Label) =
            label.Target <- target
            label

    [<RequireQualifiedAccess>]
    module TextBox =

        let create (style: ControlStyle) =
            let c = TextBox()

            match style.StretchType with
            | StretchType.None -> ()
            | StretchType.Vertical -> c.VerticalAlignment <- VerticalAlignment.Stretch
            | StretchType.Horizontal -> c.HorizontalAlignment <- HorizontalAlignment.Stretch
            | StretchType.Both ->
                c.VerticalAlignment <- VerticalAlignment.Stretch
                c.HorizontalAlignment <- HorizontalAlignment.Stretch

            for cl in style.Classes do
                c.Classes.Add(cl)

            c

        let withPlaceholderText (placeholderText: string) (c: TextBox) =
            c.PlaceholderText <- placeholderText
            c

    [<RequireQualifiedAccess>]
    module ListBox =

        let create (style: ControlStyle) =
            let c = ListBox()

            match style.StretchType with
            | StretchType.None -> ()
            | StretchType.Vertical -> c.VerticalAlignment <- VerticalAlignment.Stretch
            | StretchType.Horizontal -> c.HorizontalAlignment <- HorizontalAlignment.Stretch
            | StretchType.Both ->
                c.VerticalAlignment <- VerticalAlignment.Stretch
                c.HorizontalAlignment <- HorizontalAlignment.Stretch

            for cl in style.Classes do
                c.Classes.Add(cl)

            c

        let withItems (items: ListBoxItem list) (clearCurrentItems: bool) (lb: ListBox) =
            if clearCurrentItems then
                lb.Items.Clear()

            for item in items do
                lb.Items.Add(item) |> ignore

        let withContextMenu (cm: ContextMenu) (lb: ListBox) =
            lb.ContextMenu <- cm
            lb

        let onSelectionChanged (fn: RoutedEventArgs -> unit) (lb: ListBox) = lb.SelectionChanged.Add fn

    [<RequireQualifiedAccess>]
    module ListBoxItem =
        let create (style: ControlStyle) =
            let c = ListBoxItem()

            match style.StretchType with
            | StretchType.None -> ()
            | StretchType.Vertical -> c.VerticalAlignment <- VerticalAlignment.Stretch
            | StretchType.Horizontal -> c.HorizontalAlignment <- HorizontalAlignment.Stretch
            | StretchType.Both ->
                c.VerticalAlignment <- VerticalAlignment.Stretch
                c.HorizontalAlignment <- HorizontalAlignment.Stretch

            for cl in style.Classes do
                c.Classes.Add(cl)

            c

    [<RequireQualifiedAccess>]
    module ComboBox =

        let create (style: ControlStyle) =
            let c = ComboBox()

            match style.StretchType with
            | StretchType.None -> ()
            | StretchType.Vertical -> c.VerticalAlignment <- VerticalAlignment.Stretch
            | StretchType.Horizontal -> c.HorizontalAlignment <- HorizontalAlignment.Stretch
            | StretchType.Both ->
                c.VerticalAlignment <- VerticalAlignment.Stretch
                c.HorizontalAlignment <- HorizontalAlignment.Stretch

            for cl in style.Classes do
                c.Classes.Add(cl)

            c

        let createDefault () = create ControlStyle.Default

        let withItems (items: ComboBoxItem list) (clearCurrentItems: bool) (cb: ComboBox) =
            if clearCurrentItems then
                cb.Items.Clear()

            for item in items do
                cb.Items.Add(item) |> ignore

        let withSelectionChanged (fn: SelectionChangedEventArgs -> unit) (cb: ComboBox) =
            cb.SelectionChanged.Add fn
            cb

        let onSelectionChanged (fn: SelectionChangedEventArgs -> unit) (cb: ComboBox) = cb.SelectionChanged.Add fn

    [<RequireQualifiedAccess>]
    module ComboBoxItem =
        let create (style: ControlStyle) =
            let c = ComboBoxItem()

            match style.StretchType with
            | StretchType.None -> ()
            | StretchType.Vertical -> c.VerticalAlignment <- VerticalAlignment.Stretch
            | StretchType.Horizontal -> c.HorizontalAlignment <- HorizontalAlignment.Stretch
            | StretchType.Both ->
                c.VerticalAlignment <- VerticalAlignment.Stretch
                c.HorizontalAlignment <- HorizontalAlignment.Stretch

            for cl in style.Classes do
                c.Classes.Add(cl)

            c

        let createDefault () = create ControlStyle.Default

        let withContent (content: obj) (cbi: ComboBoxItem) =
            cbi.Content <- content
            cbi

    [<RequireQualifiedAccess>]
    module Button =
        let create (style: ControlStyle) =
            let c = Button()

            match style.StretchType with
            | StretchType.None -> ()
            | StretchType.Vertical -> c.VerticalAlignment <- VerticalAlignment.Stretch
            | StretchType.Horizontal -> c.HorizontalAlignment <- HorizontalAlignment.Stretch
            | StretchType.Both ->
                c.VerticalAlignment <- VerticalAlignment.Stretch
                c.HorizontalAlignment <- HorizontalAlignment.Stretch

            for cl in style.Classes do
                c.Classes.Add(cl)

            c

        let withContent (content: obj) (btn: Button) =
            btn.Content <- content
            btn

        let onClick (fn: RoutedEventArgs -> unit) (btn: Button) =
            btn.Click.Add fn
            btn

    [<RequireQualifiedAccess>]
    module ContextMenu =
        let create (style: ControlStyle) =
            let c = ContextMenu()

            match style.StretchType with
            | StretchType.None -> ()
            | StretchType.Vertical -> c.VerticalAlignment <- VerticalAlignment.Stretch
            | StretchType.Horizontal -> c.HorizontalAlignment <- HorizontalAlignment.Stretch
            | StretchType.Both ->
                c.VerticalAlignment <- VerticalAlignment.Stretch
                c.HorizontalAlignment <- HorizontalAlignment.Stretch

            for cl in style.Classes do
                c.Classes.Add(cl)

            c

        let withItems (items: MenuItem seq) (cm: ContextMenu) =
            for item in items do
                cm.Items.Add(item) |> ignore

            cm

    [<RequireQualifiedAccess>]
    module MenuItem =
        let create (style: ControlStyle) =
            let c = MenuItem()

            match style.StretchType with
            | StretchType.None -> ()
            | StretchType.Vertical -> c.VerticalAlignment <- VerticalAlignment.Stretch
            | StretchType.Horizontal -> c.HorizontalAlignment <- HorizontalAlignment.Stretch
            | StretchType.Both ->
                c.VerticalAlignment <- VerticalAlignment.Stretch
                c.HorizontalAlignment <- HorizontalAlignment.Stretch

            for cl in style.Classes do
                c.Classes.Add(cl)

            c

        let createDefault () =
            let style = ControlStyle.Default
            create style

        let withHeader (header: string) (mi: MenuItem) =
            mi.Header <- header
            mi

        let withCommand (fn: unit -> unit) (mi: MenuItem) =
            mi.Command <- RelayCommand(fn, (fun () -> true))
            mi


    module Presets =

        [<RequireQualifiedAccess>]
        module Titles =

            let dialog text =
                TextBlock(Text = text, FontWeight = FontWeight.Bold)


        module Labels =

            let input (content: obj) (target: IInputElement) =
                Label.createDefault () |> Label.withContent content |> Label.withTarget target

        module Buttons =


            let group (buttons: Button seq) =
                StackPanel.createDefault ()
                |> StackPanel.withOrientation Orientation.Horizontal
                |> withChildren (buttons |> Seq.cast<Control>)

            let general (content: obj) (fn: RoutedEventArgs -> unit) =
                Button.create ControlStyle.Default
                |> Button.withContent content
                |> Button.onClick fn
            
            let success (content: obj) (fn: RoutedEventArgs -> unit) =
                Button.create
                    { ControlStyle.Default with
                        Classes = [ "ok" ] }
                |> Button.withContent content
                |> Button.onClick fn

            let cancel (content: obj) (fn: RoutedEventArgs -> unit) =
                Button.create
                    { ControlStyle.Default with
                        Classes = [ "cancel" ] }
                |> Button.withContent content
                |> Button.onClick fn
