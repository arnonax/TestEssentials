# Test Automation Essentials

Test Automation Essentials is a set of tools that are handy for any test automation project. It contains all the code that I want to take with me from one project to another. I hope you'll find it useful for your projects too :-)

This project is composed from 3 assemblies:
1. **TestAutomationEssentials.Common** - this assembly contains useful utilities and extension methods that are not related directly to test automation. In fact, you can find many of these utilities useful for non test project too! This assembly includes:
   * Useful extension methods like `TryGet` (which provides functionality similiar to the [Null-Conditional Operator](https://msdn.microsoft.com/en-us/magazine/dn802602.aspx)) in C# 6, without C# 6!) and `IsEmpty` extension method of `IEnumerable`
   * Extension methods for enhancing the code readability. For example, instead of writing `TimeSpan.FromMinutes(3)` you can now write `3.Minutes()`
   * Some useful methods to work with file system paths, beyond those that exist in the `System.IO.Path` class. For example: `IsInFolder(@"C:\Root\Child\GrandChild", @"C:\Root")`
   * The `Wait` (static) class provides useful methods for polling until or while a condition is satisfied. For example: `Wait.Until(() => PageIsLoaded(), 30.Seconds(), "Page wasn't loaded!");`
   * And more...
2. **TestAutomationEssentials.MSTest** - this assembly contains useful classes and method to work with MS-Test based tests. This assembly includes:
   * The `TestBase` class which provides an improved cleanup mechanism for better test isolation. See [this blog post](http://blogs.microsoft.co.il/arnona/2014/09/02/right-way-test-cleanup/) for more details about the concept.
   * `TestUtils.ExpectException<T>(Action)`: a better (IMHO) way to assert for exceptions than the `[ExpectedException]` attribute.
   * A useful mechanism to work with configuration files in your tests.
3. **TestAutomationEssentials.CodedUI** - this assembly provide useful extension methods, specifically for Coded UI, that allows you to work without UIMaps but in an easy to read (and write) way. In fact, the API it provides is pretty similiar to Selenium's. For example, you can write: `var customerNameTextBox = mainWindow.Find<WinPanel>(By.Id("CustomerDetails")).Find<WinText>(By.Name("CustomerName"));`

    In addition, it provides few other extension methods that improves the readability of the code. For example: `myControl.RightClick();`, `myControl.DragTo(otherControl);` and the **very useful** method `myControl.IsVisible()`!


## How to use this project
There are few ways that you can take advantage of this project:
1. Download it from GitHub (coming soon!)
2. Clone the GitHub repository to your machine, and compile (or add it to your solution)
3. Just copy and paste specific methods that you find useful...

**The source code itself contains very detailed XML comments with remarks and some examples. I also strongly encourage you to look at the unit tests project as this is a very good source of documentation also!**  


###### Note: *This file was edited by http://jbt.github.io/markdown-editor/*