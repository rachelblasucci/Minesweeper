namespace Minesweeper

open System
open System.Drawing
open MonoTouch.UIKit
open MonoTouch.Foundation

module utils = 
    let Width = 8 
    let Height = 8 
    let NumberOfMines = 10

    type ActionMode =
        | Flagging
        | Digging

    type MinesweeperButton(isMine, countSurrounding, i, j, width, height) = 
        inherit UIButton() 
        member m.SurroundingMines : int = countSurrounding
        member m.IsMine : bool = isMine
        member m.Width : float32 = width
        member m.Height : float32 = height
        member m.i : int = i
        member m.j : int = j

    type UncoveredButton(countSurrounding) = 
        inherit UIButton() 
        member u.SurroundingMines : int = countSurrounding

    let filterIndices neighbors i j = 
        let filterOutsideBounds = function 
                                    | x, y when x < 0 || y < 0 || x > Width-1 || y > Height-1 -> false
                                    | _,_ -> true

        Array.filter filterOutsideBounds neighbors

    let getAllNeighbors i j = 
        filterIndices [|(i-1,j-1);(i-1,j);(i-1,j+1);(i,j-1);(i,j+1);(i+1,j-1);(i+1,j);(i+1,j+1)|] i j 

    let MinesweeperButtonsOnly (view:UIView) = view.Subviews
                                                |> Array.filter (fun v -> v :? MinesweeperButton)
                                                |> Seq.cast<MinesweeperButton> 

    let rand = new Random()
    let mutable countMines = 0

    let setMinesAndCountNeighbors() =  
        countMines <- 0
        let mines = 
            let SetIsMine() = 
                if countMines >= NumberOfMines then
                    false
                elif rand.NextDouble() > 0.85 then
                    countMines <- countMines + 1
                    true
                else
                    false
            Array2D.init Width Height (fun i j -> SetIsMine())

        let countNeighbors = 
            let addNeighbors i j = 
                getAllNeighbors i j 
                    |> Array.map (fun (x,y) -> match mines.[x,y] with | true -> 1 | false -> 0)
                    |> Array.sum
            Array2D.init Width Height addNeighbors

        mines, countNeighbors

    let GetClearBoard() = 
        let mines, neighbors = setMinesAndCountNeighbors()

        let CreateButton i j = 
            new MinesweeperButton(mines.[i,j], neighbors.[i,j], i, j, (float32)32.f, (float32)32.f)

        let boardTiles = Array2D.init Width Height CreateButton
        boardTiles
