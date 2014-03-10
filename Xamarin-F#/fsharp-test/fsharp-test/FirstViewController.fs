namespace fsharptest

open System
open System.Drawing
open MonoTouch.Foundation
open MonoTouch.UIKit

type FirstViewController() = 
    inherit UIViewController()

    override this.DidReceiveMemoryWarning () = 
        // Releases the view if it doesn't have a superview.
        base.DidReceiveMemoryWarning ()

    override this.ViewWillAppear animated =
        base.ViewWillAppear animated

    override this.ViewDidDisappear animated =
        base.ViewDidDisappear animated

    override this.ViewDidLoad () =

        base.ViewDidLoad ()
