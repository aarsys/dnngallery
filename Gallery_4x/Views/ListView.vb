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

    Public Class ListView
        Inherits BaseView

        Private mUserRequest As GalleryUserRequest
        Private mCurrentItems As New ArrayList
        Private mGalleryConfig As DotNetNuke.Modules.Gallery.Config
        Private mGalleryAuthorize As DotNetNuke.Modules.Gallery.Authorization

        Public Sub New(ByVal GalleryCont As GalleryControl)
            MyBase.New(GalleryCont)

            mUserRequest = GalleryCont.UserRequest
            mCurrentItems = mUserRequest.CurrentItems
            mGalleryAuthorize = GalleryCont.GalleryAuthorize
            mGalleryConfig = GalleryCont.GalleryConfig
        End Sub

        Public Overrides Sub CreateChildControls()
        End Sub

        Public Overrides Sub OnPreRender()
        End Sub

        Private Sub RenderGallery(ByVal wr As HtmlTextWriter)
            Dim item As IGalleryObjectInfo
            Dim album As GalleryFolder
            Dim file As GalleryFile

            Dim newWidthA As String = String.Empty
            Dim newHeightA As String = String.Empty

            If mCurrentItems.Count = 0 Then
                RenderInfo(wr, Localization.GetString("AlbumEmpty", mGalleryConfig.SharedResourceFile))
                Return
            End If

            For Each item In mCurrentItems
                If TypeOf item Is GalleryFile Then
                    file = CType(item, GalleryFile)
                    ' Make compiler happy by assigning a value
                    album = Nothing
                Else
                    ' Make compiler happy by assigning a value
                    file = Nothing
                    album = CType(item, GalleryFolder)
                End If

                wr.RenderBeginTag(HtmlTextWriterTag.Tr)

                wr.AddAttribute(HtmlTextWriterAttribute.Width, "100%")
                wr.RenderBeginTag(HtmlTextWriterTag.Td)

                wr.AddAttribute(HtmlTextWriterAttribute.Width, "100%")
                wr.AddAttribute(HtmlTextWriterAttribute.Class, "Gallery_Container")
                wr.RenderBeginTag(HtmlTextWriterTag.Table) ' table container

                wr.RenderBeginTag(HtmlTextWriterTag.Tr)

                wr.AddAttribute(HtmlTextWriterAttribute.Align, "center")
                wr.AddAttribute(HtmlTextWriterAttribute.Valign, "middle")
                wr.AddAttribute(HtmlTextWriterAttribute.Colspan, "2")
                wr.AddAttribute(HtmlTextWriterAttribute.Class, "Gallery_RowHeader")
                wr.RenderBeginTag(HtmlTextWriterTag.Td)

                wr.AddAttribute(HtmlTextWriterAttribute.Width, "100%")
                'wr.AddAttribute(HtmlTextWriterAttribute.Border, "1")
                wr.RenderBeginTag(HtmlTextWriterTag.Table) ' table title & commands

                wr.RenderBeginTag(HtmlTextWriterTag.Tr)

                wr.AddAttribute(HtmlTextWriterAttribute.Align, "left")
                'wr.AddAttribute(HtmlTextWriterAttribute.Class, "Gallery_RowHeader")
                wr.AddAttribute(HtmlTextWriterAttribute.Width, "100%")
                wr.RenderBeginTag(HtmlTextWriterTag.Td) ' column for title   

                Dim strCurrentStrip As String = "currentstrip=" & mUserRequest.CurrentStrip.ToString

                'William Severance - Modified to append CurrentStrip parameter to URL as appropriate
                If Not mGalleryConfig.AllowPopup Then
                    RenderImageButton(wr, Utils.AppendURLParameter(item.BrowserURL, strCurrentStrip), item.IconURL, GalleryControl.LocalizedText("Open"), "")
                    wr.Write("&nbsp;")
                    RenderCommandButton(wr, Utils.AppendURLParameter(item.BrowserURL, strCurrentStrip), item.Title, "Gallery_AltHeaderText")
                Else
                    RenderImageButton(wr, item.BrowserURL, item.IconURL, GalleryControl.LocalizedText("Open"), "")
                    wr.Write("&nbsp;")
                    RenderCommandButton(wr, item.BrowserURL, item.Title, "Gallery_AltHeaderText")
                End If
                'RenderImageButton(wr, item.BrowserURL, item.IconURL, GalleryControl.LocalizedText("Open"), "")
                'wr.Write("&nbsp;")
                'RenderCommandButton(wr, item.BrowserURL, item.Title, "Gallery_AltHeaderText")

                wr.RenderEndTag() ' td

                wr.AddAttribute(HtmlTextWriterAttribute.Align, "right")
                wr.AddStyleAttribute("white-space", "nowrap")
                wr.RenderBeginTag(HtmlTextWriterTag.Td) ' column for commands

                RenderCommand(wr, item)

                If item.IsFolder Then
                    ' Is a folder
                    RenderAlbumCommand(wr, album)
                End If

                wr.RenderEndTag() ' </td> commands
                wr.RenderEndTag() ' </tr>
                wr.RenderEndTag() ' </table> title & commands

                wr.RenderEndTag() ' </td>
                wr.RenderEndTag() ' tr

                wr.RenderBeginTag(HtmlTextWriterTag.Tr) ' Gallery_Row for image & url

                wr.AddAttribute(HtmlTextWriterAttribute.Align, "left")
                wr.AddAttribute(HtmlTextWriterAttribute.Valign, "top")
                wr.AddAttribute(HtmlTextWriterAttribute.Class, "Gallery_RowPanel")
                newWidthA = Convert.ToString(mGalleryConfig.MaximumThumbWidth + 48) + "px"
                newHeightA = Convert.ToString(mGalleryConfig.MaximumThumbHeight + 24) + "px"
                'wr.AddAttribute(HtmlTextWriterAttribute.Width, (mGalleryConfig.MaximumThumbWidth + 48).ToString)
                'wr.AddAttribute(HtmlTextWriterAttribute.Height, (mGalleryConfig.MaximumThumbHeight + 24).ToString)
                'Modification to add unit of measure to width/height attributes for xhtml compliance GAL8522
                wr.AddAttribute(HtmlTextWriterAttribute.Width, newWidthA)
                wr.AddAttribute(HtmlTextWriterAttribute.Height, newHeightA)
                wr.RenderBeginTag(HtmlTextWriterTag.Td) ' td for image & url

                If TypeOf item Is GalleryFolder Then
                    RenderAlbum(wr, item)
                Else
                    wr.AddAttribute(HtmlTextWriterAttribute.Class, "Gallery_Image")
                    RenderFile(wr, item)
                    'RenderImageButton(wr, item.BrowserURL, item.ThumbnailURL, Gallery.LocalizedText("Open"), "Gallery_Image")
                End If

                wr.RenderEndTag() ' td

                wr.AddAttribute(HtmlTextWriterAttribute.Align, "right")
                wr.AddAttribute(HtmlTextWriterAttribute.Valign, "top")
                'wr.AddAttribute(HtmlTextWriterAttribute.Width, "100%")
                wr.RenderBeginTag(HtmlTextWriterTag.Td) ' td for info

                wr.AddAttribute(HtmlTextWriterAttribute.Width, "100%")
                'wr.AddAttribute(HtmlTextWriterAttribute.Border, "1")
                wr.AddAttribute(HtmlTextWriterAttribute.Cellpadding, "3px")
                wr.AddAttribute(HtmlTextWriterAttribute.Cellspacing, "1")
                wr.RenderBeginTag(HtmlTextWriterTag.Table) ' table for item info

                ' Get the Gallery Config
                Dim GalleryConfig As Config
                GalleryConfig = DotNetNuke.Modules.Gallery.Config.GetGalleryConfig(GalleryControl.ModuleId)

                If (GalleryConfig.TextDisplayOptions And Config.GalleryDisplayOption.Name) <> 0 Then
                    Dim sb As New StringBuilder(item.Name)
                    If (GalleryConfig.TextDisplayOptions And Config.GalleryDisplayOption.Size) <> 0 Then
                        sb.Append(" ")
                        Dim sizeInfo As String = Localization.GetString("FileSizeInfo", GalleryConfig.SharedResourceFile)
                        sizeInfo = sizeInfo.Replace("[FileSize]", Math.Ceiling(item.Size / 1024).ToString)
                        sb.Append(sizeInfo)
                    End If
                    RenderItemInfo(wr, GalleryControl.LocalizedText("Name"), sb.ToString)
                End If

                If (GalleryConfig.TextDisplayOptions And Config.GalleryDisplayOption.Author) <> 0 Then
                    RenderItemInfo(wr, GalleryControl.LocalizedText("Author"), item.Author)
                End If

                If (GalleryConfig.TextDisplayOptions And Config.GalleryDisplayOption.Client) <> 0 Then
                    RenderItemInfo(wr, GalleryControl.LocalizedText("Client"), item.Client)
                End If

                If (GalleryConfig.TextDisplayOptions And Config.GalleryDisplayOption.Location) <> 0 Then
                    RenderItemInfo(wr, GalleryControl.LocalizedText("Location"), item.Location)
                End If

                If (GalleryConfig.TextDisplayOptions And Config.GalleryDisplayOption.CreatedDate) <> 0 Then 'Then
                    RenderItemInfo(wr, GalleryControl.LocalizedText("CreatedDate"), item.CreatedDate.ToString)
                End If

                If (GalleryConfig.TextDisplayOptions And Config.GalleryDisplayOption.ApprovedDate) <> 0 Then 'Then
                    RenderItemInfo(wr, GalleryControl.LocalizedText("ApprovedDate"), DateToText(item.ApprovedDate)) 'WES - convert DateTime.MaxValue to empty string
                End If

                wr.RenderEndTag() ' table
                wr.RenderEndTag() ' td
                wr.RenderEndTag() ' tr

                wr.RenderBeginTag(HtmlTextWriterTag.Tr) ' Gallery_Row for voting, description

                wr.AddAttribute(HtmlTextWriterAttribute.Align, "center")
                wr.AddAttribute(HtmlTextWriterAttribute.Valign, "top")
                wr.AddAttribute(HtmlTextWriterAttribute.Class, "Gallery_RowHighLight")
                wr.RenderBeginTag(HtmlTextWriterTag.Td) ' td for voting    

                If Not item.IsFolder Then
                    If Me.mGalleryConfig.AllowVoting Then
                        Dim voteInfo As String = GalleryControl.LocalizedText("VoteInfo")
                        voteInfo = voteInfo.Replace("[VoteValue]", item.Score.ToString)
                        voteInfo = voteInfo.Replace("[VoteCount]", file.Votes.Count.ToString)

                        If mGalleryAuthorize.ItemCanVote(item) Then
                            'William Severance - added to append CurrentStrip parameter as needed
                            If Not mGalleryConfig.AllowPopup Then
                                RenderImageButton(wr, Utils.AppendURLParameter(item.VotingURL, strCurrentStrip), item.ScoreImageURL, voteInfo, "")
                            Else
                                RenderImageButton(wr, item.VotingURL, item.ScoreImageURL, voteInfo, "")
                            End If
                            'RenderImageButton(wr, item.VotingURL, item.ScoreImageURL, voteInfo, "")
                        Else
                            RenderImage(wr, item.ScoreImageURL, voteInfo, "")
                        End If
                    End If 'If Me.mGalleryConfig.AllowVoting Then

                Else ' if this is an album render item count, instead of rating image
                    Dim sizeInfo As String = GalleryControl.LocalizedText("AlbumSizeInfo")
                    sizeInfo = sizeInfo.Replace("[ItemCount]", (album.Size - (album.IconItems.Count + album.UnApprovedItems.Count)).ToString)
                    wr.Write(sizeInfo)
                End If

                wr.RenderEndTag() ' td voting

                wr.AddAttribute(HtmlTextWriterAttribute.Align, "left")
                wr.AddAttribute(HtmlTextWriterAttribute.Valign, "top")
                wr.AddAttribute(HtmlTextWriterAttribute.Class, "Gallery_Row")
                wr.RenderBeginTag(HtmlTextWriterTag.Td) ' td for description               
                If ((GalleryConfig.TextDisplayOptions And Config.GalleryDisplayOption.Description) <> 0) _
                      AndAlso item.Description.Length > 0 Then
                    wr.Write(item.Description)
                Else
                    wr.Write("&nbsp;")
                End If
                wr.RenderEndTag() ' td description
                wr.RenderEndTag() ' tr voting, description    
                wr.RenderEndTag() ' table container
                wr.RenderEndTag() ' td container   
                wr.RenderEndTag() ' tr 
            Next

        End Sub

        Private Sub RenderAlbum(ByVal wr As HtmlTextWriter, ByVal Album As IGalleryObjectInfo)

            Dim newHeightA As String = String.Empty
            Dim newWidthA As String = String.Empty

            ' table album
            'wr.AddAttribute(HtmlTextWriterAttribute.Width, (mGalleryConfig.MaximumThumbWidth + 20).ToString)
            'wr.AddAttribute(HtmlTextWriterAttribute.Height, "100%")
            wr.AddAttribute(HtmlTextWriterAttribute.Cellpadding, "0")
            wr.AddAttribute(HtmlTextWriterAttribute.Cellspacing, "0")
            wr.AddAttribute(HtmlTextWriterAttribute.Border, "0")
            'wr.AddAttribute(HtmlTextWriterAttribute.Alt, "''")
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

            'Required for xhtml compliance - GAL8522 - HWZassenhaus
            newWidthA = Convert.ToString(mGalleryConfig.MaximumThumbWidth) + "px"
            newHeightA = Convert.ToString(mGalleryConfig.MaximumThumbHeight) + "px"
            wr.AddAttribute(HtmlTextWriterAttribute.Height, newHeightA)
            wr.AddAttribute(HtmlTextWriterAttribute.Width, newWidthA)
            wr.RenderBeginTag(HtmlTextWriterTag.Td)
            'RenderImageButton(wr, Album.BrowserURL, Album.ThumbnailURL, Gallery.LocalizedText("Open"), "")
            RenderImageButton(wr, Album.BrowserURL, Album.ThumbnailURL, Album.Description, "")

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
            'alt tag is not permitted here - xhtml validation HWZassenhaus 9/24/2008
            'wr.AddAttribute(HtmlTextWriterAttribute.Alt, "''")
            wr.RenderBeginTag(HtmlTextWriterTag.Table) ' table image & details

            wr.RenderBeginTag(HtmlTextWriterTag.Tr)

            wr.AddAttribute(HtmlTextWriterAttribute.Class, "Gallery_FileTL")
            wr.RenderBeginTag(HtmlTextWriterTag.Td)
            RenderImage(wr, mGalleryConfig.GetImageURL("spacer_left.gif"), "", "")
            wr.RenderEndTag() ' td
            wr.AddAttribute(HtmlTextWriterAttribute.Class, "Gallery_FileTC")
            wr.RenderBeginTag(HtmlTextWriterTag.Td)
            wr.Write("&nbsp;")
            wr.RenderEndTag() ' td
            wr.AddAttribute(HtmlTextWriterAttribute.Class, "Gallery_FileTR")
            wr.RenderBeginTag(HtmlTextWriterTag.Td)
            RenderImage(wr, mGalleryConfig.GetImageURL("spacer_right.gif"), "", "")
            wr.RenderEndTag() ' td

            wr.RenderEndTag() ' tr

            wr.RenderBeginTag(HtmlTextWriterTag.Tr)

            wr.AddAttribute(HtmlTextWriterAttribute.Class, "Gallery_FileML")
            wr.RenderBeginTag(HtmlTextWriterTag.Td)
            wr.Write("&nbsp;")
            wr.RenderEndTag() ' td

            wr.AddAttribute(HtmlTextWriterAttribute.Class, "Gallery_File")
            wr.AddAttribute(HtmlTextWriterAttribute.Align, "center")
            wr.AddAttribute(HtmlTextWriterAttribute.Height, (mGalleryConfig.MaximumThumbHeight).ToString)
            wr.AddAttribute(HtmlTextWriterAttribute.Width, (mGalleryConfig.MaximumThumbWidth).ToString)
            wr.RenderBeginTag(HtmlTextWriterTag.Td)
            'William Severance - Modifed to append CurrentStrip parameter as appropriate
            If Not mGalleryConfig.AllowPopup Then
                RenderImageButton(wr, Utils.AppendURLParameter(File.BrowserURL, "currentstrip=" & mUserRequest.CurrentStrip.ToString), File.ThumbnailURL, File.Description, "")
            Else
                RenderImageButton(wr, File.BrowserURL, File.ThumbnailURL, File.Description, "")
            End If

            wr.RenderEndTag() ' td            

            wr.AddAttribute(HtmlTextWriterAttribute.Class, "Gallery_FileMR")
            wr.RenderBeginTag(HtmlTextWriterTag.Td)
            wr.Write("&nbsp;")
            wr.RenderEndTag() ' td

            wr.RenderEndTag() ' tr

            wr.RenderBeginTag(HtmlTextWriterTag.Tr)

            wr.AddAttribute(HtmlTextWriterAttribute.Class, "Gallery_FileBL")
            wr.RenderBeginTag(HtmlTextWriterTag.Td)
            wr.Write("&nbsp;")
            wr.RenderEndTag() ' td

            wr.AddAttribute(HtmlTextWriterAttribute.Class, "Gallery_FileBC")
            wr.RenderBeginTag(HtmlTextWriterTag.Td)
            wr.Write("&nbsp;")
            wr.RenderEndTag() ' td

            wr.AddAttribute(HtmlTextWriterAttribute.Class, "Gallery_FileBR")
            wr.RenderBeginTag(HtmlTextWriterTag.Td)
            wr.Write("&nbsp;")
            wr.RenderEndTag() ' td

            wr.RenderEndTag() ' tr
            wr.RenderEndTag() ' table            

        End Sub

        Private Sub RenderCommand(ByVal wr As HtmlTextWriter, ByVal Item As IGalleryObjectInfo)

            If mGalleryAuthorize.ItemCanSlideshow(Item) Then
                'William Severance - Modified to append CurrentStrip paramete to URL
                If Not mGalleryConfig.AllowPopup Then
                    RenderImageButton(wr, AppendURLParameter(Item.SlideshowURL, "currentstrip=" & mUserRequest.CurrentStrip.ToString), mGalleryConfig.GetImageURL("s_movie.gif"), Localization.GetString("Slideshow.Tooltip", mGalleryConfig.SharedResourceFile), "")
                Else
                    RenderImageButton(wr, Item.SlideshowURL, mGalleryConfig.GetImageURL("s_movie.gif"), Localization.GetString("Slideshow.Tooltip", mGalleryConfig.SharedResourceFile), "")
                End If
                ' RenderImageButton(wr, Item.SlideshowURL, mGalleryConfig.GetImageURL("s_movie.gif"), Localization.GetString("Slideshow.Tooltip", mGalleryConfig.SharedResourceFile), "")
                wr.Write("&nbsp;")
            End If

            If mGalleryAuthorize.ItemCanViewExif(Item) Then
                If Not mGalleryConfig.AllowPopup Then
                    RenderImageButton(wr, AppendURLParameter(Item.ExifURL, "currentstrip=" & mUserRequest.CurrentStrip.ToString), mGalleryConfig.GetImageURL("s_exif.gif"), Localization.GetString("EXIFData.Tooltip", mGalleryConfig.SharedResourceFile), "")
                Else
                    RenderImageButton(wr, Item.ExifURL, mGalleryConfig.GetImageURL("s_exif.gif"), Localization.GetString("EXIFData.Tooltip", mGalleryConfig.SharedResourceFile), "")
                End If
                'RenderImageButton(wr, Item.ExifURL, mGalleryConfig.GetImageURL("s_exif.gif"), Localization.GetString("EXIFData.Tooltip", mGalleryConfig.SharedResourceFile), "")
                wr.Write("&nbsp;")
            End If

            If mGalleryAuthorize.ItemCanDownload(Item) Then
                RenderImageButton(wr, Item.DownloadURL, mGalleryConfig.GetImageURL("s_download.gif"), Localization.GetString("Download.Tooltip", mGalleryConfig.SharedResourceFile), "")
                wr.Write("&nbsp;")
            End If

            If mGalleryAuthorize.ItemCanEdit(Item) Then
                Dim editTooltip As String
                If Item.IsFolder Then
                    editTooltip = Localization.GetString("MenuEdit.Tooltip", mGalleryConfig.SharedResourceFile)
                Else
                    editTooltip = Localization.GetString("MenuEditFile.Tooltip", mGalleryConfig.SharedResourceFile)
                End If
                If Not mGalleryConfig.AllowPopup Then
                    RenderImageButton(wr, AppendURLParameter(Item.EditURL, "currentstrip=" & mUserRequest.CurrentStrip.ToString), mGalleryConfig.GetImageURL("s_edit.gif"), editTooltip, "")
                Else
                    RenderImageButton(wr, Item.EditURL, mGalleryConfig.GetImageURL("s_edit.gif"), editTooltip, "")
                End If
                'RenderImageButton(wr, Item.EditURL, mGalleryConfig.GetImageURL("s_edit.gif"), editTooltip, "")
                wr.Write("&nbsp;")
            End If

        End Sub

        Private Sub RenderAlbumCommand(ByVal wr As HtmlTextWriter, ByVal Album As GalleryFolder)
            If mGalleryAuthorize.HasItemEditPermission(Album) Then
                RenderImageButton(wr, Album.AddSubAlbumURL, mGalleryConfig.GetImageURL("s_folder.gif"), Localization.GetString("MenuAddAlbum.Tooltip", mGalleryConfig.SharedResourceFile), "")
                wr.Write("&nbsp;")
            End If
            If mGalleryAuthorize.HasItemUploadPermission(Album) Then
                RenderImageButton(wr, Album.AddFileURL, mGalleryConfig.GetImageURL("s_new2.gif"), Localization.GetString("MenuAddFile.Tooltip", mGalleryConfig.SharedResourceFile), "")
                wr.Write("&nbsp;")
            End If
            If mGalleryAuthorize.HasItemEditPermission(Album) Then
                'William Severance - Modified to append CurrentStrip parameter
                RenderImageButton(wr, Utils.AppendURLParameter(Album.MaintenanceURL, "currentstrip=" & mUserRequest.CurrentStrip.ToString), mGalleryConfig.GetImageURL("s_bookopen.gif"), Localization.GetString("MenuMaintenance.Tooltip", mGalleryConfig.SharedResourceFile), "")
                'RenderImageButton(wr, Album.MaintenanceURL, mGalleryConfig.GetImageURL("s_bookopen.gif"), Localization.GetString("MenuMaintenance.Tooltip", mGalleryConfig.SharedResourceFile), "")
            End If
        End Sub

        Private Sub RenderItemInfo(ByVal wr As HtmlTextWriter, ByVal PropertyName As String, ByVal PropertyValue As String)
            wr.RenderBeginTag(HtmlTextWriterTag.Tr)

            wr.AddAttribute(HtmlTextWriterAttribute.Align, "right")
            wr.AddAttribute(HtmlTextWriterAttribute.Width, "120px")
            wr.AddAttribute(HtmlTextWriterAttribute.Height, "20px")
            wr.AddAttribute(HtmlTextWriterAttribute.Class, "Gallery_RowHighLight")
            wr.RenderBeginTag(HtmlTextWriterTag.Td)
            wr.Write(PropertyName)
            wr.RenderEndTag() ' td

            wr.AddAttribute(HtmlTextWriterAttribute.Align, "left")
            wr.AddAttribute(HtmlTextWriterAttribute.Class, "Gallery_Row")
            wr.RenderBeginTag(HtmlTextWriterTag.Td)
            wr.Write(PropertyValue)
            wr.RenderEndTag() ' td

            wr.RenderEndTag() ' tr

        End Sub

        Public Overrides Sub Render(ByVal wr As HtmlTextWriter)
            RenderTableBegin(wr, 1, 0, 1)
            RenderGallery(wr)
            RenderTableEnd(wr) '
        End Sub

    End Class

End Namespace