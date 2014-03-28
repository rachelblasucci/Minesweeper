#nowarn "40"
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
        let s = new UISegmentedControl(new RectangleF((float32)50.f, (float32)Height*(ButtonSize+ButtonPadding)+50.f, (float32)200.f, (float32)50.f))
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

    let NewClearedMineButton mines frame = 
        let ub = new UncoveredButton(mines)
        ub.Frame <- frame
        ub.BackgroundColor <- UIColor.DarkGray
        if mines = 0 then
            ub.SetTitle("", UIControlState.Normal)
        else 
            ub.SetTitle(mines.ToString(), UIControlState.Normal)
        ub

    override this.ViewDidLoad () =
        this.View.AddSubview NewSliderControl

        let StartNewGame (board:MinesweeperButton[,]) = 
            let v = new UIView(new RectangleF(0.f, 0.f, this.View.Bounds.Width, this.View.Bounds.Height))
            board |> Array2D.iter (fun b -> v.AddSubview b) 
            this.View.AddSubview v
            this.View.BringSubviewToFront NewSliderControl

        let rec MinesweeperButtonClicked =
            let GameOver (view:UIView) heading text = 
                view.RemoveFromSuperview()
                view.Dispose()
                (new UIAlertView(heading, text, null, "Okay", null)).Show()
                StartNewGame <| GetNewGameBoard()

            let rec ClearCell (view:UIView) (ms:MinesweeperButton) = 
                let SwitchButton (msButton:MinesweeperButton) = 
                    view.WillRemoveSubview(msButton)
                    msButton.RemoveFromSuperview()
                    msButton.Dispose()
                    view.AddSubview <| NewClearedMineButton msButton.SurroundingMines msButton.Frame

                let allNeighbors = getAllNeighbors ms.i ms.j

                SwitchButton ms
                if ms.IsMine = false && ms.SurroundingMines = 0 then 
                    let IsCurrentNeighbor (msb:MinesweeperButton) = 
                        let listed = allNeighbors |> Array.tryFind (fun (i,j) -> i=msb.i && j=msb.j)
                        listed.IsSome

                    MinesweeperButtonsOnly view
                        |> Seq.filter IsCurrentNeighbor
                        |> Seq.iter (fun msb -> ClearCell view msb)

            new EventHandler(fun sender eventargs -> 
                let ms = sender :?> MinesweeperButton
                let v = ms.Superview
                match actionMode with 
                    | Flagging -> 
                        if (ms.CurrentImage = UIImage.FromBundle("Flag.png")) then
                            ms.SetImage(null, UIControlState.Normal)
                        else
                            ms.SetImage(UIImage.FromBundle("Flag.png"), UIControlState.Normal)
                    | Digging when ms.IsMine -> 
                        GameOver v ":(" "YOU LOSE!"
                    | Digging ->
                        ClearCell v ms
                        v.WillRemoveSubview(ms)
                        ms.RemoveFromSuperview()
                        ms.Dispose()
                        v.AddSubview <| NewClearedMineButton ms.SurroundingMines ms.Frame

                        let allNonMinesAreCleared = MinesweeperButtonsOnly v 
                                                        |> Seq.forall (fun o -> o.IsMine)

                        if allNonMinesAreCleared then
                            GameOver v ":)" "YOU WIN!"
                )

        and GetNewGameBoard() = 
                GetClearBoard()
                    |> Array2D.map (fun b -> b.Frame <- new RectangleF((float32)b.i*(ButtonSize+ButtonPadding)+25.f, (float32)b.j*(ButtonSize+ButtonPadding)+25.f, (float32)ButtonSize, (float32)ButtonSize); b)
                    |> Array2D.map (fun b -> b.TouchUpInside.AddHandler MinesweeperButtonClicked; b)
                    |> Array2D.map (fun b -> b.BackgroundColor <- UIColor.LightGray; b)
                    |> Array2D.map (fun b -> b.SetImage(null, UIControlState.Normal); b)
                    |> Array2D.map (fun b -> b.SetTitle("", UIControlState.Normal); b)

        StartNewGame <| GetNewGameBoard()
        base.ViewDidLoad ()
    