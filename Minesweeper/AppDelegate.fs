namespace Minesweeper

open System
open MonoTouch.UIKit
open MonoTouch.Foundation

[<Register ("AppDelegate")>]
type AppDelegate () =
    inherit UIApplicationDelegate ()

    let window = new UIWindow (UIScreen.MainScreen.Bounds)

    // This method is invoked when the application is ready to run.
    override this.FinishedLaunching (app, options) =
        let n = new UINavigationController (new MinesweeperViewController ())
        n.NavigationBar.Translucent <- false
        n.NavigationBar.BarTintColor <- UIColor.DarkGray // Will break on iOS6
        window.RootViewController <- n
        window.MakeKeyAndVisible ()
        true

module Main =
    [<EntryPoint>]
    let main args =
        UIApplication.Main (args, null, "AppDelegate")
        0

