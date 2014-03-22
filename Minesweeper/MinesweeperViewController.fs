namespace Minesweeper

open System
open System.Drawing

open MonoTouch.UIKit
open MonoTouch.Foundation
open utils

[<Register ("MinesweeperViewController")>]
type MinesweeperViewController () =
    inherit UIViewController ()

    let mutable actionMode = Digging

    override this.ViewDidLoad () =
        let rec playGame() =
            let mines, neighbors = setMinesAndGetNeighbors
 
            let CreateButton i j = 
                let b = new MinesweeperButton(mines.[i,j], neighbors.[i,j])
                b.Frame <- new RectangleF((float32)i*35.f+25.f, (float32)j*35.f+25.f, (float32)32.f, (float32)32.f)
                b

            let boardTiles = Array2D.init Width Height CreateButton

//            let rec ClearTiles i j = 
//                if (boardTiles.[i,j].SurroundingMines = 0) then
//                    
                    //clear more

            let MinesweeperButtonClicked = 
                new EventHandler(fun sender eventargs -> 
                    let ms = sender :?> MinesweeperButton
                    if (actionMode = Flagging) then //flag or unflag cell
                        if (ms.CurrentImage = UIImage.FromBundle("Flag.png")) then
                            ms.SetImage(null, UIControlState.Normal)
                        else
                            ms.SetImage(UIImage.FromBundle("Flag.png"), UIControlState.Normal)
                    elif (actionMode = Digging && ms.IsMine) then //if you're digging, and you found a mine: death! :( 
                        ms.BackgroundColor <- UIColor.Red
                        (new UIAlertView(":(", "YOU LOSE!", null, "Okay", "Cancel")).Show()
                        //todo: vibrate phone?
                        playGame()
                    else // you're digging, clear the cell
                        let v = ms.Superview
                        v.WillRemoveSubview(ms)
                        let ub = new UncoveredButton(ms.IsMine, ms.SurroundingMines)
                        ub.Frame <- ms.Frame
                        ub.BackgroundColor <- UIColor.DarkGray
                        if (ub.SurroundingMines = 0) then
                            ub.SetTitle("", UIControlState.Normal)
                            //todo: keep clearing all 0 cells
                        else 
                            ub.SetTitle(ms.SurroundingMines.ToString(), UIControlState.Normal)
                        //todo: if all non-mine cells are cleared, you win.
                        v.AddSubview ub
                    )

            let CreateSliderView = 
                let s = new UISegmentedControl(new RectangleF((float32)50.f, (float32)Height*35.f+50.f, (float32)200.f, (float32)50.f))

                let HandleSegmentChanged = 
                    new EventHandler(fun sender eventargs -> 
                        let s = sender :?> UISegmentedControl
                        actionMode <- match s.SelectedSegment with 
                                        | 0 -> Flagging
                                        | 1 -> Digging
                        )

                s.InsertSegment(UIImage.FromBundle("Flag.png"), 0, false)
                s.InsertSegment(UIImage.FromBundle("Bomb.png"), 1, false)
                s.SelectedSegment <- 1
                actionMode <- Digging
                s.ValueChanged.AddHandler HandleSegmentChanged
                s
            
            if (this.View.Subviews.Length > 0) then this.View.Subviews.[0].RemoveFromSuperview()

            let v = new UIView(new RectangleF(0.f, 0.f, this.View.Bounds.Width, this.View.Bounds.Height))
            boardTiles 
                |> Array2D.map (fun b -> b.BackgroundColor <- UIColor.LightGray; b)
                |> Array2D.map (fun b -> b.TouchUpInside.AddHandler MinesweeperButtonClicked; b)
                |> Array2D.map (fun b -> v.AddSubview b; b)

            v.AddSubview CreateSliderView
            this.View.AddSubview v

        playGame()
        base.ViewDidLoad ()

    override this.ShouldAutorotateToInterfaceOrientation (orientation) =
        orientation <> UIInterfaceOrientation.PortraitUpsideDown
    