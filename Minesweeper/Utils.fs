namespace Minesweeper

open System
open System.Drawing
open MonoTouch.UIKit
open MonoTouch.Foundation

module utils = 
    let Width = 8 
    let Height = 8 
    let NumberOfMines = 12

    type ActionMode =
        | Flagging
        | Digging

    type MinesweeperButton(isMine, countSurrounding, width, height, x, y) = 
        inherit UIButton() 
        member m.SurroundingMines : int = countSurrounding
        member m.IsMine : bool = isMine
        member m.Width : float32 = width
        member m.Height : float32 = height
        member m.X : float32 = x
        member m.Y : float32 = y

    type UncoveredButton(isMine, countSurrounding, width, height, x, y) = 
        inherit UIButton() 
        member u.SurroundingMines : int = countSurrounding
        member u.IsMine : bool = isMine
        member u.Width : float32 = width
        member u.Height : float32 = height
        member u.X : float32 = x
        member u.Y : float32 = y

    let filterIndices i j = 
        let indices = [|(i-1,j-1);(i-1,j);(i-1,j+1);(i,j-1);(i,j+1);(i+1,j-1);(i+1,j);(i+1,j+1)|]

        let filterOutsideBounds = function 
                                    | x, y when x < 0 || y < 0 || x > Width-1 || y > Height-1 -> false
                                    | _,_ -> true

        Array.filter filterOutsideBounds indices

    let rand = new Random()
    let mutable countMines = 0

    let setMinesAndGetNeighbors() =  
        countMines <- 0
        let mines = 
            let SetIsMine() = 
                if (countMines >= NumberOfMines) then
                    false
                elif (rand.NextDouble() > 0.75) then
                    countMines <- countMines + 1
                    true
                else
                    false
            Array2D.init Width Height (fun i j -> SetIsMine())

        let countNeighbors = 
            let addNeighbors i j = 
                filterIndices i j 
                    |> Array.map (fun (x,y) -> mines.[x,y])
                    |> Array.map (fun x -> match x with | true -> 1 | false -> 0) 
                    |> Array.sum
            Array2D.init Width Height addNeighbors

        mines, countNeighbors

    let getNewBoard() = 
        let mines, neighbors = setMinesAndGetNeighbors()

        let CreateButton i j = 
            MinesweeperButton(mines.[i,j], neighbors.[i,j], (float32)32.f, (float32)32.f, (float32)i*35.f+25.f, (float32)j*35.f+25.f)

        let boardTiles = Array2D.init Width Height CreateButton
        boardTiles
