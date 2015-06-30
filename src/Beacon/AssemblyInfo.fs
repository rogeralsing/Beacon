namespace System
open System.Reflection

[<assembly: AssemblyTitleAttribute("Beacon")>]
[<assembly: AssemblyProductAttribute("Beacon")>]
[<assembly: AssemblyDescriptionAttribute("HTTP Service Discovery")>]
[<assembly: AssemblyVersionAttribute("1.0")>]
[<assembly: AssemblyFileVersionAttribute("1.0")>]
do ()

module internal AssemblyVersionInformation =
    let [<Literal>] Version = "1.0"
