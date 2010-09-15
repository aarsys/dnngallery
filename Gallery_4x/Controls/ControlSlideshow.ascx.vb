'
' DotNetNuke® - http://www.dotnetnuke.com
' Copyright (c) 2002-2010 by DotNetNuke Corp. 

'
' Permission is hereby granted, free of charge, to any person obtaining a copy of this software and associated 
' documentation files (the "Software"), to deal in the Software without restriction, including without limitation 
' the rights to use, copy, modify, merge, publish, distribute, sublicense, and/or sell copies of the Software, and 
' to permit persons to whom the Software is furnished to do so, subject to the following conditions:
'
' The above copyright notice and this permission notice shall be included in all copies or substantial portions 
' of the Software.
'
' THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED 
' TO THE WARRANTIES OF MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL 
' THE AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION OF 
' CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER 
' DEALINGS IN THE SOFTWARE.
'
Option Strict On
Option Explicit On

Imports System.Text
Imports System.IO
Imports system.xml
Imports DotNetNuke.Common.Globals
Imports DotNetNuke.Modules.Gallery.Utils

Namespace DotNetNuke.Modules.Gallery.WebControls

    Public Class Slideshow
        Inherits GalleryWebControlBase

        Private _CurrentRequest As GalleryUserRequest

#Region "Public Properties"
        Public Property CurrentRequest() As GalleryUserRequest
            Get
                Return _CurrentRequest
            End Get
            Set(ByVal value As GalleryUserRequest)
                _CurrentRequest = value
            End Set
        End Property
#End Region

#Region " Web Form Designer Generated Code "

        'This call is required by the Web Form Designer.
        <System.Diagnostics.DebuggerStepThrough()> Private Sub InitializeComponent()

        End Sub

        Private Sub Page_Init(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles MyBase.Init
            'CODEGEN: This method call is required by the Web Form Designer
            'Do not modify it using the code editor.
            InitializeComponent()
        End Sub

#End Region

        Private Sub Page_Load(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles MyBase.Load
            ErrorMessage.Visible = False

            _CurrentRequest = New GalleryUserRequest(ModuleId)

            If CurrentRequest Is Nothing OrElse Not CurrentRequest.Folder.IsPopulated Then
                Response.Redirect(ApplicationURL)
            End If
            If Not IsPostBack Then
                If CurrentRequest.Folder.IsBrowsable Then
                    celPicture.Width = GalleryConfig.FixedWidth.ToString
                    celPicture.Height = GalleryConfig.FixedHeight.ToString
                    'WES - localized loading message
                    lblTitleBox.Text = Localization.GetString("Loading", GalleryConfig.SharedResourceFile)

                    Dim albumPath As String = CurrentRequest.Folder.Path
                    Dim slideSpeed As String = GalleryConfig.SlideshowSpeed.ToString
                    Dim sb As New StringBuilder

                    'Generate Clientside Javascript for Slideshow

                    Dim Count As Integer

                    sb.Append("<script type='text/javascript' language='javascript'>")
                    sb.Append(vbCrLf)
                    sb.Append("var slideShowSpeed = ")
                    sb.Append(slideSpeed)
                    sb.Append(vbCrLf)
                    sb.Append("var Pic = new Array()")
                    sb.Append(vbCrLf)
                    sb.Append("var Title = new Array()")
                    sb.Append(vbCrLf)
                    sb.Append("var Description = new Array()")
                    sb.Append(vbCrLf)

                    ' Write all of the images out
                    Dim image As GalleryFile
                    For Each image In CurrentRequest.ValidImages
                        sb.Append("Pic[")
                        sb.Append(Count)
                        sb.Append("] = """)
                        sb.Append(image.URL)
                        sb.Append("""")
                        sb.Append(vbCrLf)
                        sb.Append("Title[")
                        sb.Append(Count)
                        sb.Append("] = """)
                        sb.Append(JSEncode(image.Title.Replace(vbCrLf, "<br />")))
                        sb.Append("""")
                        sb.Append(vbCrLf)
                        sb.Append("Description[")
                        sb.Append(Count)
                        sb.Append("] = """)
                        sb.Append(JSEncode(image.Description.Replace(vbCrLf, "<br />")))
                        sb.Append("""")
                        sb.Append(vbCrLf)
                        Count = Count + 1
                    Next

                    sb.Append("var t")
                    sb.Append(vbCrLf)
                    sb.Append("var j = 0")
                    sb.Append(vbCrLf)
                    sb.Append("var p = Pic.length")
                    sb.Append(vbCrLf)

                    sb.Append("var preLoad = new Array()")
                    sb.Append(vbCrLf)
                    sb.Append("for (i = 0; i < p; i++){")
                    sb.Append(vbCrLf)
                    sb.Append("preLoad[i] = new Image()")
                    sb.Append(vbCrLf)
                    sb.Append("preLoad[i].src = Pic[i]")
                    sb.Append(vbCrLf)
                    sb.Append("}")
                    sb.Append(vbCrLf)
                    sb.Append("runSlideShow();")
                    sb.Append(vbCrLf)
                    sb.Append("</script>")

                    ' JIMJ Set the start image as the first image
                    Dim StartImage As String
                    StartImage = CType(CurrentRequest.ValidImages(0), GalleryFile).URL
                    ' JIMJ wrap src in extra "'s
                    ImageSrc.Text = "<img src=""" & StartImage & """ name='SlideShow' alt='' class='Gallery_Image'/>" 'style='border-color:#D1D7DC;border-width:2px;border-style:Outset;'>"

                    'WES - Use ClientScript.RegisterStartupScript rather than injecting script via label
                    'as use of <body onload = "runSlideShow();"> in markup results in second body tag when slide show
                    'is configured to run in page rather than popup. Fix for Gemini issue GAL-8110
                    'NOTE: This javascript and others do not inject clientID so that more than one module may be
                    'placed on the same page.

                    Page.ClientScript.RegisterStartupScript(Me.GetType, "SlideShow", sb.ToString)
                Else
                    ErrorMessage.Visible = True
                    'WES - localized album empty message
                    ErrorMessage.Text = Localization.GetString("AlbumEmpty", GalleryConfig.SharedResourceFile) 'Album contains no images!
                End If
            End If
        End Sub
    End Class

End Namespace