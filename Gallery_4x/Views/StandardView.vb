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

Imports System.Diagnostics
Imports System
Imports System.IO
Imports System.Web
Imports System.Web.UI
Imports System.Web.UI.WebControls
Imports DotNetNuke
Imports DotNetNuke.Services.Localization
Imports DotNetNuke.Modules.Gallery.Config
Imports DotNetNuke.Modules.Gallery.Utils

Namespace DotNetNuke.Modules.Gallery.Views

    Public Class StandardView
        Inherits BaseView

        Private mUserRequest As GalleryUserRequest
        Private mCurrentItems As New ArrayList
        Private mGalleryConfig As DotNetNuke.Modules.Gallery.Config
        Private mGalleryAuthorize As DotNetNuke.Modules.Gallery.Authorization
        Private ctlMenu As MediaMenu

        Public Sub New(ByVal GalleryCont As GalleryControl)
            MyBase.New(GalleryCont)

            mUserRequest = GalleryCont.UserRequest 'New GalleryUserRequest(ModuleID, Gallery.Sort, Gallery.SortDESC)
            mCurrentItems = mUserRequest.CurrentItems
            mGalleryAuthorize = GalleryCont.GalleryAuthorize
            mGalleryConfig = GalleryCont.GalleryConfig

        End Sub

        Public Overrides Sub CreateChildControls()
            Dim item As IGalleryObjectInfo
            For Each item In mCurrentItems
                'If Not (item.Type = ItemType.Flash OrElse item.Type = ItemType.Movie) Then
                ctlMenu = New MediaMenu(GalleryControl.ModuleId, item)
                ctlMenu.ID = item.ID.ToString
                Controls.Add(ctlMenu)
                'End If
            Next
        End Sub

        Public Overrides Sub OnPreRender()
        End Sub

        Private Sub RenderGallery(ByVal wr As HtmlTextWriter)
            Dim item As IGalleryObjectInfo
            Dim album As GalleryFolder
            Dim file As GalleryFile
            Dim rowCount As Integer = 0
            Dim i As Integer
            Dim ii As Integer
            Dim w As Integer

            If mCurrentItems.Count = 0 Then
                RenderInfo(wr, Localization.GetString("AlbumEmpty", mGalleryConfig.SharedResourceFile))
                Return
            End If

            rowCount = CType(Math.Ceiling(mCurrentItems.Count / mGalleryConfig.StripWidth), Integer)
            If Not rowCount = 0 Then
                For i = 0 To rowCount - 1
                    wr.RenderBeginTag(HtmlTextWriterTag.Tr)

                    For ii = ((i * mGalleryConfig.StripWidth) + 1) To ((i + 1) * mGalleryConfig.StripWidth)
                        ' image column
                        'wr.AddAttribute(HtmlTextWriterAttribute.Class, "Body")
                        wr.AddAttribute(HtmlTextWriterAttribute.Align, "center")

                        wr.AddAttribute(HtmlTextWriterAttribute.Valign, "top") 'WES-changed bottom to top vertical alignment
                        'Percentage width must be an integer xhtml 1.0 - HWZassenhaus 9/24/2008
                        w = Convert.ToInt32(100 / mGalleryConfig.StripWidth)
                        wr.AddAttribute(HtmlTextWriterAttribute.Width, Unit.Percentage(w).ToString)
                        'wr.AddAttribute(HtmlTextWriterAttribute.Width, Unit.Percentage(100 / mGalleryConfig.StripWidth).ToString)
                        wr.RenderBeginTag(HtmlTextWriterTag.Td)

                        If ii <= mCurrentItems.Count Then
                            item = CType(mCurrentItems.Item(ii - 1), IGalleryObjectInfo)

                            If TypeOf item Is GalleryFile Then
                                file = CType(mCurrentItems.Item(ii - 1), GalleryFile)
                            Else
                                ' JIMJ Assign a value to make compiler happy
                                file = Nothing
                                album = CType(mCurrentItems.Item(ii - 1), GalleryFolder)
                            End If

                            wr.AddAttribute(HtmlTextWriterAttribute.Width, "100%")
                            'wr.AddAttribute(HtmlTextWriterAttribute.Height, "100%")
                            'wr.AddAttribute(HtmlTextWriterAttribute.Border, "1")
                            wr.AddAttribute(HtmlTextWriterAttribute.Class, "Body")
                            wr.RenderBeginTag(HtmlTextWriterTag.Table) ' table image & details

                            ' Gallery_Row for thumbnail image
                            wr.RenderBeginTag(HtmlTextWriterTag.Tr)

                            wr.AddAttribute(HtmlTextWriterAttribute.Valign, "middle")
                            wr.AddAttribute(HtmlTextWriterAttribute.Align, "center")
                            'wr.AddAttribute(HtmlTextWriterAttribute.Height, (mGalleryConfig.MaximumThumbHeight + 20).ToString)
                            'wr.AddAttribute(HtmlTextWriterAttribute.Class, "Gallery_Image")
                            wr.RenderBeginTag(HtmlTextWriterTag.Td)

                            If TypeOf mCurrentItems.Item(ii - 1) Is GalleryFolder Then
                                'wr.RenderBeginTag(HtmlTextWriterTag.Td)
                                RenderAlbum(wr, item)
                            Else
                                wr.AddAttribute(HtmlTextWriterAttribute.Class, "Gallery_Image")
                                RenderFile(wr, item)
                            End If

                            wr.RenderEndTag() ' td thumb
                            wr.RenderEndTag() ' tr thumb

                            'voting
                            If Me.mGalleryConfig.AllowVoting AndAlso Not item.IsFolder Then
                                wr.RenderBeginTag(HtmlTextWriterTag.Tr) ' voting
                                wr.AddAttribute(HtmlTextWriterAttribute.Valign, "middle")
                                wr.AddAttribute(HtmlTextWriterAttribute.Align, "center")
                                wr.AddAttribute(HtmlTextWriterAttribute.Height, "24px")
                                wr.RenderBeginTag(HtmlTextWriterTag.Td)

                                'If Not item.IsFolder Then
                                Dim voteInfo As String = GalleryControl.LocalizedText("VoteInfo")
                                voteInfo = voteInfo.Replace("[VoteValue]", item.Score.ToString)
                                voteInfo = voteInfo.Replace("[VoteCount]", file.Votes.Count.ToString)

                                If mGalleryAuthorize.ItemCanVote(item) Then
                                    'William Severance - Modified to append CurrentStrip parameter to URL
                                    RenderImageButton(wr, AppendURLParameter(item.VotingURL, "currentstrip=" & mUserRequest.CurrentStrip.ToString), item.ScoreImageURL, voteInfo, "")
                                    'RenderImageButton(wr, item.VotingURL, item.ScoreImageURL, voteInfo, "")
                                Else
                                    RenderImage(wr, item.ScoreImageURL, voteInfo, "")
                                End If
                                'Else
                                '    wr.Write("&nbsp;")
                                'End If

                                wr.RenderEndTag() ' td voting
                                wr.RenderEndTag() ' tr voting
                            End If 'If mGalleryAuthorize.ItemCanVote(item) Then

                            If (mGalleryConfig.TextDisplayOptions And GalleryDisplayOption.Title) <> 0 Then
                                wr.RenderBeginTag(HtmlTextWriterTag.Tr) ' title
                                wr.AddAttribute(HtmlTextWriterAttribute.Valign, "middle")
                                wr.AddAttribute(HtmlTextWriterAttribute.Align, "center")
                                wr.AddAttribute(HtmlTextWriterAttribute.Class, "Gallery_AltHeaderText")
                                wr.RenderBeginTag(HtmlTextWriterTag.Td)
                                'William Severance - Modified to append CurrentStrip parameter to querystring

                                If Not mGalleryConfig.AllowPopup Then
                                    RenderCommandButton(wr, Utils.AppendURLParameter(item.BrowserURL, "currentstrip=" & mUserRequest.CurrentStrip.ToString), item.Title, "Gallery_AltHeaderText")
                                Else
                                    RenderCommandButton(wr, item.BrowserURL, item.Title, "Gallery_AltHeaderText")
                                End If
                                'RenderCommandButton(wr, item.BrowserURL, item.Title, "Gallery_AltHeaderText")
                                wr.RenderEndTag() ' td title
                                wr.RenderEndTag() ' tr title
                            End If
                            wr.RenderBeginTag(HtmlTextWriterTag.Tr) ' info
                            wr.AddAttribute(HtmlTextWriterAttribute.Valign, "middle")
                            wr.AddAttribute(HtmlTextWriterAttribute.Align, "center")
                            wr.AddAttribute(HtmlTextWriterAttribute.Class, "Gallery_NormalGrey")
                            wr.RenderBeginTag(HtmlTextWriterTag.Td)
                            wr.Write(item.ItemInfo)
                            wr.RenderEndTag() ' td info
                            wr.RenderEndTag() ' tr info

                            wr.RenderBeginTag(HtmlTextWriterTag.Tr) ' description
                            wr.AddAttribute(HtmlTextWriterAttribute.Valign, "top")
                            wr.AddAttribute(HtmlTextWriterAttribute.Align, "center")
                            wr.AddAttribute(HtmlTextWriterAttribute.Class, "Gallery_NormalGrey")
                            wr.AddAttribute(HtmlTextWriterAttribute.Height, "100%")
                            wr.RenderBeginTag(HtmlTextWriterTag.Td)

                            If ((mGalleryConfig.TextDisplayOptions And Config.GalleryDisplayOption.Description) <> 0) _
                                    AndAlso Not item.Description.Length = 0 Then
                                wr.Write(item.Description)
                            Else
                                wr.Write("&nbsp;")
                            End If

                            wr.RenderEndTag() ' td description
                            wr.RenderEndTag() ' tr description

                            'RenderCommand(wr, item)

                            'If mGalleryAuthorize.HasItemEditPermission(item) AndAlso item.IsFolder Then
                            '    RenderAlbumCommand(wr, CType(item, GalleryFolder))
                            'End If

                            wr.RenderEndTag() ' table image & details
                        End If
                        wr.RenderEndTag() ' td
                    Next
                    wr.RenderEndTag() ' tr
                Next
            End If

        End Sub

        Private Sub RenderAlbum(ByVal wr As HtmlTextWriter, ByVal Album As IGalleryObjectInfo)

            ' table album
            'wr.AddAttribute(HtmlTextWriterAttribute.Width, (mGalleryConfig.MaximumThumbWidth + 20).ToString)
            'wr.AddAttribute(HtmlTextWriterAttribute.Height, "100%")
            wr.AddAttribute(HtmlTextWriterAttribute.Cellpadding, "0")
            wr.AddAttribute(HtmlTextWriterAttribute.Cellspacing, "0")
            wr.AddAttribute(HtmlTextWriterAttribute.Border, "0")
            wr.AddAttribute(HtmlTextWriterAttribute.Alt, "''")
            wr.RenderBeginTag(HtmlTextWriterTag.Table) ' table image & details

            wr.RenderBeginTag(HtmlTextWriterTag.Tr)

            wr.AddAttribute(HtmlTextWriterAttribute.Class, "Gallery_AlbumTL")
            wr.RenderBeginTag(HtmlTextWriterTag.Td)
            RenderImage(wr, mGalleryConfig.GetImageURL("spacer_left.gif"), "", "")
            wr.RenderEndTag() ' td
            wr.AddAttribute(HtmlTextWriterAttribute.Class, "Gallery_AlbumTC")
            wr.RenderBeginTag(HtmlTextWriterTag.Td)
            wr.Write("&nbsp;")
            wr.RenderEndTag() ' td
            wr.AddAttribute(HtmlTextWriterAttribute.Class, "Gallery_AlbumTR")
            wr.RenderBeginTag(HtmlTextWriterTag.Td)
            RenderImage(wr, mGalleryConfig.GetImageURL("spacer_right.gif"), "", "")
            wr.RenderEndTag() ' td

            wr.RenderEndTag() ' tr

            wr.RenderBeginTag(HtmlTextWriterTag.Tr)

            wr.AddAttribute(HtmlTextWriterAttribute.Class, "Gallery_AlbumML")
            wr.RenderBeginTag(HtmlTextWriterTag.Td)
            wr.Write("&nbsp;")
            wr.RenderEndTag() ' td

            wr.AddAttribute(HtmlTextWriterAttribute.Class, "Gallery_Album")
            wr.AddAttribute(HtmlTextWriterAttribute.Align, "center")
            wr.AddAttribute(HtmlTextWriterAttribute.Height, (mGalleryConfig.MaximumThumbHeight).ToString)
            wr.AddAttribute(HtmlTextWriterAttribute.Width, (mGalleryConfig.MaximumThumbWidth).ToString)
            wr.RenderBeginTag(HtmlTextWriterTag.Td)

            'RenderImageButton(wr, Album.BrowserURL, Album.ThumbnailURL, Album.Description, "")
            Dim objControl As Control
            For Each objControl In Controls
                If TypeOf objControl Is MediaMenu Then
                    'William Severance - Modified to append CurrentStrip parameters as needed
                    Dim objMediaMenu As MediaMenu = CType(objControl, MediaMenu)
                    If objMediaMenu.ID = Album.ID.ToString Then
                        objMediaMenu.AppendCurrentStripParameters(mUserRequest.CurrentStrip)
                        objMediaMenu.RenderControl(wr)
                    End If
                End If
            Next

            wr.RenderEndTag() ' td            

            wr.AddAttribute(HtmlTextWriterAttribute.Class, "Gallery_AlbumMR")
            wr.RenderBeginTag(HtmlTextWriterTag.Td)
            wr.Write("&nbsp;")
            wr.RenderEndTag() ' td

            wr.RenderEndTag() ' tr

            wr.RenderBeginTag(HtmlTextWriterTag.Tr)

            wr.AddAttribute(HtmlTextWriterAttribute.Class, "Gallery_AlbumBL")
            wr.RenderBeginTag(HtmlTextWriterTag.Td)
            wr.Write("&nbsp;")
            wr.RenderEndTag() ' td

            wr.AddAttribute(HtmlTextWriterAttribute.Class, "Gallery_AlbumBC")
            wr.RenderBeginTag(HtmlTextWriterTag.Td)
            wr.Write("&nbsp;")
            wr.RenderEndTag() ' td

            wr.AddAttribute(HtmlTextWriterAttribute.Class, "Gallery_AlbumBR")
            wr.RenderBeginTag(HtmlTextWriterTag.Td)
            wr.Write("&nbsp;")
            wr.RenderEndTag() ' td

            wr.RenderEndTag() ' tr
            wr.RenderEndTag() ' table            

        End Sub

        Private Sub RenderFile(ByVal wr As HtmlTextWriter, ByVal File As IGalleryObjectInfo)
            ' table File            
            wr.AddAttribute(HtmlTextWriterAttribute.Cellpadding, "0")
            wr.AddAttribute(HtmlTextWriterAttribute.Cellspacing, "0")
            wr.AddAttribute(HtmlTextWriterAttribute.Border, "0")
            'alt tag is not allowed here - xhtml validation HWZassenhaus 9/24/2008
            'wr.AddAttribute(HtmlTextWriterAttribute.Alt, "''")
            wr.RenderBeginTag(HtmlTextWriterTag.Table) ' table image & details
            wr.RenderBeginTag(HtmlTextWriterTag.Tr)

            wr.AddAttribute(HtmlTextWriterAttribute.Class, "Gallery_FileTL")
            wr.RenderBeginTag(HtmlTextWriterTag.Td)
            'RenderImage(wr, mGalleryConfig.GetImageURL("spacer_left.gif"), "", "")
            wr.RenderEndTag() ' td
            wr.AddAttribute(HtmlTextWriterAttribute.Class, "Gallery_FileTC")
            wr.RenderBeginTag(HtmlTextWriterTag.Td)
            'wr.Write("&nbsp;")
            'RenderImage(wr, mGalleryConfig.GetImageURL("spacer.gif"), "", "")
            wr.RenderEndTag() ' td
            wr.AddAttribute(HtmlTextWriterAttribute.Class, "Gallery_FileTR")
            wr.RenderBeginTag(HtmlTextWriterTag.Td)
            'RenderImage(wr, mGalleryConfig.GetImageURL("spacer_right.gif"), "", "")
            wr.RenderEndTag() ' td

            wr.RenderEndTag() ' tr

            wr.RenderBeginTag(HtmlTextWriterTag.Tr)

            wr.AddAttribute(HtmlTextWriterAttribute.Class, "Gallery_FileML")
            wr.RenderBeginTag(HtmlTextWriterTag.Td)
            'wr.Write("&nbsp;")
            wr.RenderEndTag() ' td

            wr.AddAttribute(HtmlTextWriterAttribute.Class, "Gallery_File")
            wr.AddAttribute(HtmlTextWriterAttribute.Align, "center")
            wr.AddAttribute(HtmlTextWriterAttribute.Height, (mGalleryConfig.MaximumThumbHeight).ToString)
            wr.AddAttribute(HtmlTextWriterAttribute.Width, (mGalleryConfig.MaximumThumbWidth).ToString)
            wr.RenderBeginTag(HtmlTextWriterTag.Td)
            'RenderImageButton(wr, File.BrowserURL, File.ThumbnailURL, File.Description, "")

            'If Not (File.Type = ItemType.Flash OrElse File.Type = ItemType.Movie) Then
            Dim objControl As Control
            For Each objControl In Controls
                'William Severance - Modified to append CurrentStrip parameters as needed
                Dim objMediaMenu As MediaMenu = CType(objControl, MediaMenu)
                If objMediaMenu.ID = File.ID.ToString Then
                    objMediaMenu.AppendCurrentStripParameters(mUserRequest.CurrentStrip)
                    objMediaMenu.RenderControl(wr)
                End If
            Next
            'Else
            'William Severance - Modified to append CurrentStrip parameter as needed
            'If Not mGalleryConfig.AllowPopup Then
            '    RenderImageButton(wr, Utils.AppendURLParameter(File.BrowserURL, "currentstrip=" & mUserRequest.CurrentStrip.ToString), File.ThumbnailURL, File.Description, "")
            'Else
            '    RenderImageButton(wr, File.BrowserURL, File.ThumbnailURL, File.Description, "")
            'End If

            'End If

            wr.RenderEndTag() ' td            

            wr.AddAttribute(HtmlTextWriterAttribute.Class, "Gallery_FileMR")
            wr.RenderBeginTag(HtmlTextWriterTag.Td)
            'wr.Write("&nbsp;")
            wr.RenderEndTag() ' td

            wr.RenderEndTag() ' tr

            wr.RenderBeginTag(HtmlTextWriterTag.Tr)

            wr.AddAttribute(HtmlTextWriterAttribute.Class, "Gallery_FileBL")
            wr.RenderBeginTag(HtmlTextWriterTag.Td)
            'wr.Write("&nbsp;")
            wr.RenderEndTag() ' td

            wr.AddAttribute(HtmlTextWriterAttribute.Class, "Gallery_FileBC")
            wr.RenderBeginTag(HtmlTextWriterTag.Td)
            'wr.Write("&nbsp;")
            'RenderImage(wr, mGalleryConfig.GetImageURL("spacer.gif"), "", "")
            wr.RenderEndTag() ' td

            wr.AddAttribute(HtmlTextWriterAttribute.Class, "Gallery_FileBR")
            wr.RenderBeginTag(HtmlTextWriterTag.Td)
            'wr.Write("&nbsp;")
            wr.RenderEndTag() ' td

            wr.RenderEndTag() ' tr
            wr.RenderEndTag() ' table            

        End Sub

        Private Sub RenderCommand(ByVal wr As HtmlTextWriter, ByVal Item As IGalleryObjectInfo)
            wr.RenderBeginTag(HtmlTextWriterTag.Tr) ' command
            wr.AddAttribute(HtmlTextWriterAttribute.Valign, "top")
            wr.AddAttribute(HtmlTextWriterAttribute.Align, "center")
            wr.AddAttribute(HtmlTextWriterAttribute.Class, "Gallery_RowHighLight")
            wr.RenderBeginTag(HtmlTextWriterTag.Td)

            If mGalleryAuthorize.ItemCanSlideshow(Item) Then
                'William Severance - Modified to append CurrentStrip paramete to URL
                If Not mGalleryConfig.AllowPopup Then
                    RenderImageButton(wr, AppendURLParameter(Item.SlideshowURL, "currentstrip=" & mUserRequest.CurrentStrip.ToString), mGalleryConfig.GetImageURL("s_movie.gif"), "View slideshow...", "")
                Else
                    RenderImageButton(wr, Item.SlideshowURL, mGalleryConfig.GetImageURL("s_movie.gif"), "View slideshow...", "")
                End If
                ' RenderImageButton(wr, Item.SlideshowURL, mGalleryConfig.GetImageURL("s_movie.gif"), "View slideshow...", "")
                wr.Write("&nbsp;")
            End If

            If mGalleryAuthorize.ItemCanViewExif(Item) Then
                If Not mGalleryConfig.AllowPopup Then
                    RenderImageButton(wr, AppendURLParameter(Item.ExifURL, "currentstrip=" & mUserRequest.CurrentStrip.ToString), mGalleryConfig.GetImageURL("s_exif.gif"), "View Exif Metadata...", "")
                Else
                    RenderImageButton(wr, Item.ExifURL, mGalleryConfig.GetImageURL("s_exif.gif"), "View Exif Metadata...", "")
                End If
                'RenderImageButton(wr, Item.ExifURL, mGalleryConfig.GetImageURL("s_exif.gif"), "View Exif Metadata...", "")
                wr.Write("&nbsp;")
            End If

            If mGalleryAuthorize.ItemCanDownload(Item) Then
                RenderImageButton(wr, Item.DownloadURL, mGalleryConfig.GetImageURL("s_download.gif"), "Download...", "")
                wr.Write("&nbsp;")
            End If

            If mGalleryAuthorize.ItemCanEdit(Item) Then
                If Not mGalleryConfig.AllowPopup Then
                    RenderImageButton(wr, AppendURLParameter(Item.EditURL, "currentstrip=" & mUserRequest.CurrentStrip.ToString), mGalleryConfig.GetImageURL("s_edit.gif"), "Edit...", "")
                Else
                    RenderImageButton(wr, Item.EditURL, mGalleryConfig.GetImageURL("s_edit.gif"), "Edit...", "")
                End If
                'RenderImageButton(wr, Item.EditURL, mGalleryConfig.GetImageURL("s_edit.gif"), "Edit...", "")
                wr.Write("&nbsp;")
            End If

            wr.RenderEndTag() ' td command
            wr.RenderEndTag() ' tr command

        End Sub

        Private Sub RenderAlbumCommand(ByVal wr As HtmlTextWriter, ByVal Album As GalleryFolder)

            RenderImageButton(wr, Album.AddSubAlbumURL, mGalleryConfig.GetImageURL("s_folder.gif"), "Add sub album...", "")
            wr.Write("&nbsp;")
            RenderImageButton(wr, Album.AddFileURL, mGalleryConfig.GetImageURL("s_new2.gif"), "Add files...", "")
            wr.Write("&nbsp;")
            'William Severance - Modified to append CurrentStrip paramete to URL
            RenderImageButton(wr, Utils.AppendURLParameter(Album.MaintenanceURL, "currentstrip=" & mUserRequest.CurrentStrip.ToString), mGalleryConfig.GetImageURL("s_bookopen.gif"), "Maintenance...", "")
            'RenderImageButton(wr, Album.MaintenanceURL, mGalleryConfig.GetImageURL("s_bookopen.gif"), "Maintenance...", "")
        End Sub

        Public Overrides Sub Render(ByVal wr As HtmlTextWriter)
            RenderTableBegin(wr, 0, 3, 0)
            RenderGallery(wr)
            RenderTableEnd(wr) '
        End Sub

    End Class

End Namespace