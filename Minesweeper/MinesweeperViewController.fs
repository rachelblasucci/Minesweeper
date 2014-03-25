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

    let NewSliderControl = 
        let s = new UISegmentedControl(new RectangleF((float32)50.f, (float32)Height*35.f+50.f, (float32)200.f, (float32)50.f))
        s.InsertSegment(UIImage.FromBundle("Flag.png"), 0, false)
        s.InsertSegment(UIImage.FromBundle("Axe.png"), 1, false)
        s.SelectedSegment <- 1
        actionMode <- Digging

        let HandleSegmentChanged = 
            new EventHandler(fun sender eventargs -> 
                let s = sender :?> UISegmentedControl
                actionMode <- match s.SelectedSegment with 
                                | 0 -> Flagging
                                | 1 -> Digging
                                | _ -> failwith "No such state!" 
                )
        s.ValueChanged.AddHandler HandleSegmentChanged
        s

    let NewUncoveredButton mines frame = 
        let ub = new UncoveredButton(mines)
        ub.Frame <- frame
        ub.BackgroundColor <- UIColor.DarkGray
        if (mines = 0) then
            ub.SetTitle("", UIControlState.Normal)
        else 
            ub.SetTitle(mines.ToString(), UIControlState.Normal)
        ub

    override this.ViewDidLoad () =
        this.View.AddSubview NewSliderControl

        let playNewGame (board:MinesweeperButton[,]) = 
            let v = new UIView(new RectangleF(0.f, 0.f, this.View.Bounds.Width, this.View.Bounds.Height))
            board |> Array2D.iter (fun b -> v.AddSubview b) 
            this.View.AddSubview v
            this.View.BringSubviewToFront NewSliderControl

        let rec MinesweeperButtonClicked =
            let GameOver (view:UIView) heading text = 
                view.RemoveFromSuperview()
                (new UIAlertView(heading, text, null, "Okay", null)).Show()
                playNewGame <| gameBoard()

            new EventHandler(fun sender eventargs -> 
                let ms = sender :?> MinesweeperButton
                let v = ms.Superview
                match actionMode with 
                    | Flagging -> //flag or unflag cell
                        if (ms.CurrentImage = UIImage.FromBundle("Flag.png")) then
                            ms.SetImage(null, UIControlState.Normal)
                        else
                            ms.SetImage(UIImage.FromBundle("Flag.png"), UIControlState.Normal)
                    | Digging when ms.IsMine -> //if you're digging, and you found a mine: death! :( 
                        GameOver v ":(" "YOU LOSE!"
                    | Digging -> // clear the cell
                        v.WillRemoveSubview(ms)
                        v.AddSubview <| NewUncoveredButton ms.SurroundingMines ms.Frame
                        let msButtons = v.Subviews
                                            |> Array.filter (fun o -> o.GetType().ToString().Contains("MinesweeperButton"))
                                            |> Array.map (fun o -> o :?> MinesweeperButton)
                                            |> Array.tryFind (fun o -> o.IsMine <> true)

                        if (msButtons.IsNone) then
                            GameOver v ":)" "YOU WIN!"
             )

        and gameBoard() = 
                getEmptyBoard()
                    |> Array2D.map (fun b -> b.Frame <- new RectangleF(b.X, b.Y, b.Width, b.Height); b)
                    |> Array2D.map (fun b -> b.TouchUpInside.AddHandler MinesweeperButtonClicked; b)
                    |> Array2D.map (fun b -> b.BackgroundColor <- UIColor.LightGray; b)
                    |> Array2D.map (fun b -> b.SetImage(null, UIControlState.Normal); b)
                    |> Array2D.map (fun b -> b.SetTitle("", UIControlState.Normal); b)

        playNewGame <| gameBoard()
        base.ViewDidLoad ()
    