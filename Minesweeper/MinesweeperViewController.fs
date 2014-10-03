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

type MinesweeperViewController () =
    inherit UIViewController ()
 
    let mutable actionMode = Digging

    /// Creates Slider Control
    let NewSliderControl =
        let s = new UISegmentedControl(new RectangleF((float32)50.f, (float32)Height*(ButtonSize+ButtonPadding)+50.f, (float32)200.f, (float32)50.f))
        s.InsertSegment(UIImage.FromBundle("Flag.png"), 0, false)
        s.InsertSegment(UIImage.FromBundle("Axe.png"), 1, false)
        s.SelectedSegment <- 1
        actionMode <- Digging

        let HandleSegmentChanged =
            new EventHandler(fun sender _ ->
                let s = sender :?> UISegmentedControl
                actionMode <- match s.SelectedSegment with
                                | 0 -> Flagging
                                | 1 -> Digging
                                | _ -> failwith "No such state!"
                )
        s.ValueChanged.AddHandler HandleSegmentChanged
        s

    /// Creates a new cleared mine button using a specifc minesweeper data button
    let NewClearedButton (msButton:MinesweeperButton) =
        let cb = new ClearedButton(ClearedData msButton.Data.SurroundingMines, Frame = msButton.Frame, BackgroundColor = UIColor.DarkGray)
        if msButton.Data.SurroundingMines = 0 then
            cb.SetTitle("", UIControlState.Normal)
        else
            cb.SetTitle(msButton.Data.SurroundingMines.ToString(), UIControlState.Normal)
        cb

    override this.ViewDidLoad () =
        base.ViewDidLoad ()
        this.View.AddSubview NewSliderControl

        /// Takes a 2D array of MinesweeperButtons. Creates a subview, adds each button to it, then ensures the slider is on top. 
        let StartNewGame (board:MinesweeperButton[,]) =
            let v = new UIView(new RectangleF(0.f, 0.f, this.View.Bounds.Width, this.View.Bounds.Height))
            board |> Array2D.iter (fun msb -> v.AddSubview msb)
            this.View.AddSubview v
            this.View.BringSubviewToFront NewSliderControl

        /// Filters all subviews for only MinesweeperButtons. 
        let MinesweeperButtonsOnly (view:UIView) = view.Subviews
                                                    |> Array.filter (fun v -> v :? MinesweeperButton)
                                                    |> Seq.cast<MinesweeperButton>
        
        /// Click event for MinesweeperButtons
        let rec MinesweeperButtonClicked =

            /// Called when game is over, shows final alert, then restarts game.
            let GameOver (view:UIView) heading text = 
                view.RemoveFromSuperview()
                view.Dispose()
                (new UIAlertView(heading, text, null, "Okay", null)).Show()
                StartNewGame <| GetNewGameBoard()

            /// Switches MinesweeperButton for a ClearedButton 
            let SwitchButton (view:UIView) (msButton:MinesweeperButton) = 
                view.WillRemoveSubview(msButton)
                msButton.RemoveFromSuperview()
                msButton.Dispose()
                view.AddSubview <| NewClearedButton msButton

            /// Recursively clears empty cells and displays numbered cells after clicking 
            let rec ClearCell (view:UIView) (mb:MinesweeperButton) = 
                let allNeighbors = GetAllNeighbors mb.Data.i mb.Data.j

                SwitchButton view mb

                if mb.Data.IsMine = false && mb.Data.SurroundingMines = 0 then 
                    let IsCurrentNeighbor (md:MinesweeperButton) = 
                        let listed = allNeighbors |> Array.tryFind (fun (i,j) -> i=md.Data.i && j=md.Data.j)
                        listed.IsSome

                    MinesweeperButtonsOnly view
                        |> Seq.filter IsCurrentNeighbor
                        |> Seq.iter (fun msb -> ClearCell view msb)

            /// Actual MinesweeperButton Click event handler. 
            new EventHandler(fun sender _ -> 
                let ms = sender :?> MinesweeperButton
                let v = ms.Superview
                match actionMode with 
                    | Flagging -> 
                        if (ms.CurrentImage <> null) then
                            ms.SetImage(null, UIControlState.Normal)
                        else
                            ms.SetImage(UIImage.FromBundle("Flag.png"), UIControlState.Normal)
                    | Digging when ms.Data.IsMine -> 
                        GameOver v ":(" "YOU LOSE!"
                    | Digging ->
                        ClearCell v ms
                        SwitchButton v ms

                        let allNonMinesAreCleared = MinesweeperButtonsOnly v 
                                                        |> Seq.forall (fun o -> o.Data.IsMine)

                        if allNonMinesAreCleared then
                            GameOver v ":)" "YOU WIN!"
                )

        /// Creates actual MinesweeperButtons from MinesweeperData array
        and GetNewGameBoard() = 
                let CreateButtons (u:MinesweeperData) = 
                    let ub = new MinesweeperButton(
                                u, 
                                Frame = new RectangleF((float32)u.i*(ButtonSize+ButtonPadding)+25.f, (float32)u.j*(ButtonSize+ButtonPadding)+25.f, (float32)ButtonSize, (float32)ButtonSize),
                                BackgroundColor = UIColor.LightGray)
                    ub.SetImage(null, UIControlState.Normal)
                    ub.SetTitle("", UIControlState.Normal)
                    ub.TouchUpInside.AddHandler MinesweeperButtonClicked
                    ub

                GetClearBoard()
                    |> Array2D.map (fun unknownData -> CreateButtons unknownData)

        StartNewGame <| GetNewGameBoard()
    