Imports System.IO

Namespace Ui

    ''' <summary>Where Pantheon's quick start, privacy policy, support and source live.</summary>
    Friend Module ProductInfo

        Friend Const PrivacyPolicy As String = "https://ayers.repair/legal/pantheon/privacy.html"
        Friend Const SupportAddress As String = "support@ayers.repair"
        Friend Const SourceCode As String = "https://github.com/ZachariahAyers/Pantheon"
        Private Const ProjectPage As String = "https://ayers.repair/opensource/pantheon/"

        ''' <summary>
        ''' The quick start in the App Store bundle, which keeps it in Contents/Help beside the folder
        ''' that holds the add-in. A build loaded with NETLOAD has none, so the project page stands in.
        ''' </summary>
        Friend Function QuickStart() As String
            Dim modules = If(Path.GetDirectoryName(GetType(ProductInfo).Assembly.Location), "")
            Dim bundled = Path.GetFullPath(Path.Combine(modules, "..", "Help", "Pantheon.html"))
            Return If(File.Exists(bundled), bundled, ProjectPage)
        End Function

    End Module

End Namespace
