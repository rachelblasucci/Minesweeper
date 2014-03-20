namespace fsharptest

open System
open System.Drawing
open MonoTouch.Foundation
open MonoTouch.UIKit
open System.CodeDom.Compiler

[<Register ("FirstViewController")>]
type FirstViewController = 
    inherit UIViewController
    val mutable nameTextField : MonoTouch.UIKit.UITextField

    [<Outlet>]
    member x.nameTextButton 
        with get() = x.nameTextField
        and set(value) = x.nameTextField <- value
    member x.ReleaseDesignerOutlets = 
        x.nameTextButton.Dispose ()
        x.nameTextButton = null
    
    new () = {inherit UIViewController("FirstViewController", null); nameTextField = new MonoTouch.UIKit.UITextField()}
    new (handle:IntPtr) = {inherit UIViewController(handle); nameTextField = new MonoTouch.UIKit.UITextField()}

    override this.DidReceiveMemoryWarning () = 
        // Releases the view if it doesn't have a superview
        base.DidReceiveMemoryWarning ()

    override this.ViewWillAppear animated =
        base.ViewWillAppear animated

    override this.ViewDidDisappear animated =
        base.ViewDidDisappear animated

    override this.ViewDidLoad () =

        base.ViewDidLoad ()
