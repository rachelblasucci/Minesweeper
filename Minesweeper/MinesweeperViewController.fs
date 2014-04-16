#nowarn "40"
namespace Minesweeper

open System
open System.Drawing
open MonoTouch.UIKit
open MonoTouch.Foundation
open utils

type MinesweeperButton(data) =
    inherit UIButton()
    member m.Data : MinesweeperData = data

type ClearedButton(data) =
    inherit UIButton()
    member m.Data : ClearedData = data

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
        let cb = new ClearedButton(ClearedData mines)
        cb.Frame <- frame
        cb.BackgroundColor <- UIColor.DarkGray
        if mines = 0 then
            cb.SetTitle("", UIControlState.Normal)
        else 
            cb.SetTitle(mines.ToString(), UIControlState.Normal)
        cb

    override this.ViewDidLoad () =
        base.ViewDidLoad ()
        this.View.AddSubview NewSliderControl

        let StartNewGame (board:MinesweeperButton[,]) = 
            let v = new UIView(new RectangleF(0.f, 0.f, this.View.Bounds.Width, this.View.Bounds.Height))
            board |> Array2D.iter (fun msb -> v.AddSubview msb) 
            this.View.AddSubview v
            this.View.BringSubviewToFront NewSliderControl

        let MinesweeperButtonsOnly (view:UIView) = view.Subviews
                                                    |> Array.filter (fun v -> v :? MinesweeperButton)
                                                    |> Seq.cast<MinesweeperButton>
        
        let rec MinesweeperButtonClicked =
            let GameOver (view:UIView) heading text = 
                view.RemoveFromSuperview()
                view.Dispose()
                (new UIAlertView(heading, text, null, "Okay", null)).Show()
                StartNewGame <| GetNewGameBoard()

            let rec ClearCell (view:UIView) (mb:MinesweeperButton) = 
                let allNeighbors = getAllNeighbors mb.Data.i mb.Data.j

                let SwitchButton (msButton:MinesweeperButton) = 
                    view.WillRemoveSubview(msButton)
                    msButton.RemoveFromSuperview()
                    msButton.Dispose()
                    view.AddSubview <| NewClearedMineButton msButton.Data.SurroundingMines msButton.Frame
                SwitchButton mb

                if mb.Data.IsMine = false && mb.Data.SurroundingMines = 0 then 
                    let IsCurrentNeighbor (md:MinesweeperButton) = 
                        let listed = allNeighbors |> Array.tryFind (fun (i,j) -> i=md.Data.i && j=md.Data.j)
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
                    | Digging when ms.Data.IsMine -> 
                        GameOver v ":(" "YOU LOSE!"
                    | Digging ->
                        ClearCell v ms
                        v.WillRemoveSubview(ms)
                        ms.RemoveFromSuperview()
                        ms.Dispose()
                        v.AddSubview <| NewClearedMineButton ms.Data.SurroundingMines ms.Frame

                        let allNonMinesAreCleared = MinesweeperButtonsOnly v 
                                                        |> Seq.forall (fun o -> o.Data.IsMine)

                        if allNonMinesAreCleared then
                            GameOver v ":)" "YOU WIN!"
                )

        and GetNewGameBoard() = 
                let CreateButtons (u:MinesweeperData) = 
                    let ub = new MinesweeperButton(u)
                    ub.Frame <- new RectangleF((float32)ub.Data.i*(ButtonSize+ButtonPadding)+25.f, (float32)ub.Data.j*(ButtonSize+ButtonPadding)+25.f, (float32)ButtonSize, (float32)ButtonSize)
                    ub.TouchUpInside.AddHandler MinesweeperButtonClicked
                    ub.BackgroundColor <- UIColor.LightGray
                    ub.SetImage(null, UIControlState.Normal)
                    ub.SetTitle("", UIControlState.Normal)
                    ub

                GetClearBoard()
                    |> Array2D.map (fun unknownData -> CreateButtons unknownData)

        StartNewGame <| GetNewGameBoard()
    