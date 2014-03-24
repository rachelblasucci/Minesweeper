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
        let NewSliderControl = 
            let s = new UISegmentedControl(new RectangleF((float32)50.f, (float32)Height*35.f+50.f, (float32)200.f, (float32)50.f))
            s.InsertSegment(UIImage.FromBundle("Flag.png"), 0, false)
            s.InsertSegment(UIImage.FromBundle("Bomb.png"), 1, false)
            s.SelectedSegment <- 1
            actionMode <- Digging

            let HandleSegmentChanged = 
                new EventHandler(fun sender eventargs -> 
                    let s = sender :?> UISegmentedControl
                    actionMode <- match s.SelectedSegment with 
                                    | 0 -> Flagging
                                    | 1 -> Digging
                    )
            s.ValueChanged.AddHandler HandleSegmentChanged
            s

        this.View.AddSubview NewSliderControl

        let rec playGame() =
            let MinesweeperButtonClicked = 
                new EventHandler(fun sender eventargs -> 
                    let ms = sender :?> MinesweeperButton
                    let v = ms.Superview
                    if (actionMode = Flagging) then //flag or unflag cell
                        if (ms.CurrentImage = UIImage.FromBundle("Flag.png")) then
                            ms.SetImage(null, UIControlState.Normal)
                        else
                            ms.SetImage(UIImage.FromBundle("Flag.png"), UIControlState.Normal)
                    elif (actionMode = Digging && ms.IsMine) then //if you're digging, and you found a mine: death! :( 
                        v.BackgroundColor <- UIColor.Red
                        (new UIAlertView(":(", "YOU LOSE!", null, "Okay", null)).Show()
                        playGame()
                    else // you're digging, clear the cell
                        v.WillRemoveSubview(ms)
                        let ub = new UncoveredButton(ms.SurroundingMines)
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
            
            if (this.View.Subviews.Length > 3) then this.View.Subviews.[3].RemoveFromSuperview()

            let v = new UIView(new RectangleF(0.f, 0.f, this.View.Bounds.Width, this.View.Bounds.Height))
            v.BackgroundColor <- UIColor.Black
            getEmptyBoard()
                |> Array2D.map (fun b -> b.Frame <- new RectangleF(b.X, b.Y, b.Width, b.Height); b)
                |> Array2D.map (fun b -> b.BackgroundColor <- UIColor.LightGray; b)
                |> Array2D.map (fun b -> b.TouchUpInside.AddHandler MinesweeperButtonClicked; b)
                |> Array2D.map (fun b -> v.AddSubview b; b)
            this.View.AddSubview v
            this.View.BringSubviewToFront NewSliderControl
        
        playGame()
        base.ViewDidLoad ()
    