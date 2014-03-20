namespace Minesweeper

open System
open System.Drawing

open MonoTouch.UIKit
open MonoTouch.Foundation
open utils

[<Register ("MinesweeperViewController")>]
type MinesweeperViewController () =
    inherit UIViewController ()

    //create empty buttons and set up board tiles

    override this.ViewDidLoad () =
        let mines, neighbors = setMinesAndGetNeighbors

        let getButton i j = 
            let MinesweeperButtonClicked = 
                new EventHandler(fun sender eventargs -> 
                    let ms = sender :?> MinesweeperButton
                    ms.BackgroundColor <- UIColor.Green)
            
            let b = new MinesweeperButton(mines.[i,j], neighbors.[i,j])
            b.BackgroundColor <- UIColor.Blue
            b.Frame <- new RectangleF((float32)i*35.f+25.f, (float32)j*35.f+25.f, (float32)32.f, (float32)32.f)
            if (mines.[i,j]) then
                b.SetImage(UIImage.FromBundle("Bomb.png"), UIControlState.Normal)
            elif (b.SurroundingMines > 0) then
                b.SetTitle(b.SurroundingMines.ToString(), UIControlState.Normal)
            b.TouchUpInside.AddHandler MinesweeperButtonClicked
            b

        let CreateButtonView i j = 
            this.View.Add (getButton i j)

        let CreateSliderView = 
            let l = new UILabel(new RectangleF((float32)50.f, (float32)Height*35.f+100.f, (float32)200.f, (float32)50.f))
            l.Text <- "hi"
            l.TextColor <- UIColor.White
            let s = new UISegmentedControl(new RectangleF((float32)50.f, (float32)Height*35.f+50.f, (float32)200.f, (float32)50.f))

            let HandleSegmentChanged = 
                new EventHandler(fun sender eventargs -> l.Text <- s.ValueForKey.ToString() )

            s.InsertSegment(UIImage.FromBundle("Flag.png"), 0, false)
            s.InsertSegment(UIImage.FromBundle("Bomb.png"), 1, false)
            s.SelectedSegment <- 1
            s.ValueChanged.AddHandler HandleSegmentChanged
            this.View.Add l
            this.View.Add s

        let boardTiles = Array2D.init Width Height CreateButtonView
        CreateSliderView

        base.ViewDidLoad ()

    override this.ShouldAutorotateToInterfaceOrientation (orientation) =
        orientation <> UIInterfaceOrientation.PortraitUpsideDown

