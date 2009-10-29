'
' DotNetNuke® - http://www.dotnetnuke.com
' Copyright (c) 2002-2009 by DotNetNuke Corp. 

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

Imports System
Imports System.Web
Imports System.Web.UI
Imports System.Web.UI.WebControls
Imports System.Web.HttpUtility
Imports System.Web.Security
Imports System.Text
Imports System.Configuration
Imports DotNetNuke
Imports DotNetNuke.Services.Localization
Imports DotNetNuke.Modules.Gallery.Config
Imports DotNetNuke.Modules.Gallery.Utils

Namespace DotNetNuke.Modules.Gallery.Views

    ''' <summary>
    ''' Base object used by various gallery view code
    ''' </summary>
    ''' <remarks></remarks>
    Public Class BaseView

        Private mGalleryControl As GalleryControl

        Public Sub New(ByVal GalleryControl As GalleryControl)
            mGalleryControl = GalleryControl
        End Sub 'New

        Public ReadOnly Property GalleryControl() As GalleryControl
            Get
                Return mGalleryControl
            End Get
        End Property

        Public ReadOnly Property Controls() As ControlCollection
            Get
                Return mGalleryControl.Controls
            End Get
        End Property

        Public Overridable Sub CreateChildControls()
        End Sub 'CreateChildControls

        Public Overridable Sub OnPreRender()
        End Sub 'OnPreRender

        Public Overridable Sub Render(ByVal wr As HtmlTextWriter)
        End Sub 'Render

        Protected Sub RenderTableBegin(ByVal wr As HtmlTextWriter, ByVal cellspacing As Integer, ByVal cellpadding As Integer, ByVal BorderWidth As Integer) ' Begin table in which we will render object
            Dim borderValue As String = String.Empty
            Dim cellpaddingValue As String = String.Empty
            Dim cellspacingValue As String = String.Empty

            'If (mGalleryControl.BorderStyle <> BorderStyle.None) Then
            wr.AddAttribute(HtmlTextWriterAttribute.Class, "Border")
            wr.AddAttribute(HtmlTextWriterAttribute.Align, "left")
            If BorderWidth > 0 Then
                borderValue = BorderWidth.ToString + "px"
                wr.AddAttribute(HtmlTextWriterAttribute.Border, borderValue)
            End If
            'wr.AddAttribute(HtmlTextWriterAttribute.Bordercolor, "white")
            cellspacingValue = cellspacing.ToString + "px"
            cellpaddingValue = cellpadding.ToString
            wr.AddAttribute(HtmlTextWriterAttribute.Cellspacing, cellspacingValue)
            wr.AddAttribute(HtmlTextWriterAttribute.Cellpadding, cellpaddingValue)
            wr.AddAttribute(HtmlTextWriterAttribute.Width, "100%")
            wr.AddAttribute(HtmlTextWriterAttribute.Id, "GalleryContent")
            wr.RenderBeginTag(HtmlTextWriterTag.Table)
            'End If
        End Sub 'RenderTableBegin

        Protected Sub RenderTableEnd(ByVal wr As HtmlTextWriter)
            wr.RenderEndTag()
        End Sub ' End table in which object was rendered

        Protected Sub RenderInfo(ByVal wr As HtmlTextWriter, ByVal Info As String)
            wr.RenderBeginTag(HtmlTextWriterTag.Tr)
            wr.AddAttribute(HtmlTextWriterAttribute.Align, "center")
            wr.AddAttribute(HtmlTextWriterAttribute.Valign, "middle")
            wr.AddAttribute(HtmlTextWriterAttribute.Width, "100%")
            wr.AddAttribute(HtmlTextWriterAttribute.Height, "50px")
            wr.AddAttribute(HtmlTextWriterAttribute.Class, "Gallery_Row")
            wr.RenderBeginTag(HtmlTextWriterTag.Td)

            wr.Write(Info)

            wr.RenderEndTag() ' td thumb
            wr.RenderEndTag() ' tr thumb
        End Sub

        Public Shared Sub RenderImage(ByVal wr As HtmlTextWriter, ByVal ImageURL As String, ByVal Tooltip As String, ByVal Css As String)
            wr.AddAttribute(HtmlTextWriterAttribute.Border, "0")
            wr.AddAttribute(HtmlTextWriterAttribute.Src, ImageURL)

            If Css.Length > 0 Then
                wr.AddAttribute(HtmlTextWriterAttribute.Class, Css)
            End If

            'xhtml requires an alt tag, even if it is empty - HWZassenhaus 9/28/2008
            If Tooltip.Length > 0 Then
                wr.AddAttribute(HtmlTextWriterAttribute.Alt, Tooltip)
            Else
                wr.AddAttribute(HtmlTextWriterAttribute.Alt, "")
            End If

            wr.RenderBeginTag(HtmlTextWriterTag.Img) ' img thumbs
            wr.RenderEndTag() ' img thumb

        End Sub

        Public Shared Sub RenderImageButton(ByVal wr As HtmlTextWriter, ByVal URL As String, ByVal ImageURL As String, ByVal Tooltip As String, ByVal Css As String)
            wr.AddAttribute(HtmlTextWriterAttribute.Href, URL.Replace("~/", ""))
            wr.RenderBeginTag(HtmlTextWriterTag.A)
            wr.AddAttribute(HtmlTextWriterAttribute.Border, "0")
            wr.AddAttribute(HtmlTextWriterAttribute.Src, ImageURL)

            If Css.Length > 0 Then
                wr.AddAttribute(HtmlTextWriterAttribute.Class, Css)
            End If

            If Tooltip.Length > 0 Then
                wr.AddAttribute(HtmlTextWriterAttribute.Alt, Tooltip)
            Else
                'xhtml requirement for alt tag, even if empty - GAL8522
                wr.AddAttribute(HtmlTextWriterAttribute.Alt, "")
            End If

            wr.RenderBeginTag(HtmlTextWriterTag.Img) ' img thumbs
            wr.RenderEndTag() ' img thumb
            wr.RenderEndTag() ' A
        End Sub

        Public Shared Sub RenderCommandButton(ByVal wr As HtmlTextWriter, ByVal URL As String, ByVal Text As String, ByVal Css As String)
            wr.AddAttribute(HtmlTextWriterAttribute.Href, URL.Replace("~/", ""))

            If Css.Length > 0 Then
                wr.AddAttribute(HtmlTextWriterAttribute.Class, Css)
            End If

            wr.RenderBeginTag(HtmlTextWriterTag.A)
            wr.Write(Text)
            wr.RenderEndTag() ' A
        End Sub

    End Class

End Namespace