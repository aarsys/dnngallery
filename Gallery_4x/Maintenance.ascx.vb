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

Imports System.IO
Imports DotNetNuke
Imports DotNetNuke.Security
Imports DotNetNuke.Entities.Portals
Imports DotNetNuke.Entities.Modules
Imports DotNetNuke.Entities.Modules.Actions
Imports DotNetNuke.Common.Globals
Imports DotNetNuke.Entities.Users
Imports DotNetNuke.Services.Localization
Imports DotNetNuke.Modules.Gallery.Utils
Imports DotNetNuke.Modules.Gallery.Config
Imports DotNetNuke.Modules.Gallery.WebControls

Namespace DotNetNuke.Modules.Gallery

    Public MustInherit Class Maintenance
        Inherits DotNetNuke.Entities.Modules.PortalModuleBase
        Implements DotNetNuke.Entities.Modules.IActionable
        Public lnkViewText As String = ""

#Region "Private Members"
        ' Obtain PortalSettings from Current Context   
        Dim _portalSettings As PortalSettings = PortalController.GetCurrentPortalSettings
        Private mRequest As GalleryMaintenanceRequest
        Private mGalleryConfig As DotNetNuke.Modules.Gallery.Config
        Private mFolder As GalleryFolder
        'Private mModuleID As Integer
        Private mGalleryAuthorization As Gallery.Authorization
        Private mSelectAll As Boolean
        Private mReturnCtl As String

        'For Localization of FileInfo text
        Private mLocalizedAlbumText As String
        Private mLocalizedSourceText As String
        Private mLocalizedThumbText As String
        Private mLocalizedMissingText As String

#End Region

#Region "Controls"
        'Protected WithEvents txtPath As System.Web.UI.WebControls.TextBox
        'Protected WithEvents txtName As System.Web.UI.WebControls.TextBox
        'Protected WithEvents lblAlbumInfo As System.Web.UI.WebControls.Label
        'Protected WithEvents btnSyncAll As System.Web.UI.WebControls.LinkButton
        'Protected WithEvents btnDeleteAll As System.Web.UI.WebControls.LinkButton
        'Protected WithEvents chkSelectAll As System.Web.UI.WebControls.CheckBox
        'Protected WithEvents grdContent As System.Web.UI.WebControls.DataGrid
        'Protected WithEvents UpdateButton As System.Web.UI.WebControls.ImageButton
        'Protected WithEvents ClearCache1 As System.Web.UI.WebControls.ImageButton
        'Protected WithEvents celGalleryMenu As System.Web.UI.HtmlControls.HtmlTableCell
        'Protected WithEvents celBreadcrumbs As System.Web.UI.HtmlControls.HtmlTableCell
        'Protected WithEvents cmdReturn As System.Web.UI.WebControls.LinkButton
#End Region

#Region " Web Form Designer Generated Code "

        'This call is required by the Web Form Designer.
        <System.Diagnostics.DebuggerStepThrough()> Private Sub InitializeComponent()

        End Sub

        Private Sub Page_Init(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles MyBase.Init
            'CODEGEN: This method call is required by the Web Form Designer
            'Do not modify it using the code editor.
            InitializeComponent()

            If mGalleryConfig Is Nothing Then
                mGalleryConfig = GetGalleryConfig(ModuleId)
            End If
            'mImageFolder = "../" & mGalleryConfig.ImageFolder.Substring(mGalleryConfig.ImageFolder.IndexOf("/DesktopModules"))

        End Sub

#End Region

#Region "Optional Interfaces"

        Public ReadOnly Property ModuleActions() As ModuleActionCollection Implements Entities.Modules.IActionable.ModuleActions
            Get
                Dim Actions As New ModuleActionCollection
                Actions.Add(GetNextActionID, Localization.GetString("Configuration.Action", LocalResourceFile), ModuleActionType.ModuleSettings, "", "edit.gif", EditUrl(ControlKey:="configuration"), "", False, SecurityAccessLevel.Admin, True, False)
                Actions.Add(GetNextActionID, Localization.GetString("GalleryHome.Action", LocalResourceFile), ModuleActionType.ContentOptions, "", "icon_moduledefinitions_16px.gif", NavigateURL(), False, SecurityAccessLevel.Edit, True, False)
                Return Actions
            End Get
        End Property

#End Region

        Public ReadOnly Property ReturnCtl() As String
            Get
                Return "Maintenance;"
            End Get
        End Property

        Private Sub Page_Load(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles MyBase.Load
            ' Ensure Gallery edit permissions (Andrew Galbraith Ryer)

            mRequest = New GalleryMaintenanceRequest(ModuleId)
            mGalleryConfig = mRequest.GalleryConfig
            mFolder = mRequest.Folder

            mGalleryAuthorization = New Authorization(ModuleId)
            If Not mGalleryAuthorization.HasItemEditPermission(mFolder) Then
                Response.Redirect(AccessDeniedURL(Localization.GetString("Insufficient_Maintenance_Permissions", mGalleryConfig.SharedResourceFile)))
            End If

            ' Load the styles
            DirectCast(Page, CDefault).AddStyleSheet(CreateValidID(GalleryConfig.Css()), GalleryConfig.Css())

            'modifications to make use of the resource file to conform with xhtml 1.0 Transitional HWZassenhaus 9/29/2008
            btnSyncAll.ToolTip = Localization.GetString("btnSyncAll.ToolTip", LocalResourceFile)
            btnSyncAll.Text = Localization.GetString("btnSyncAll.AlternateText", LocalResourceFile)
            btnDeleteAll.ToolTip = Localization.GetString("btnDeleteAll.ToolTip", LocalResourceFile)
            btnDeleteAll.Text = Localization.GetString("btnDeleteAll.AlternateText", LocalResourceFile)
            chkSelectAll.Text = Localization.GetString("checkAll.Text", LocalResourceFile)
            chkSelectAll.ToolTip = Localization.GetString("checkAll.ToolTip", LocalResourceFile)
            lnkViewText = Localization.GetString("lnkView.Text", LocalResourceFile)

            mLocalizedAlbumText = Localization.GetString("Album.Header", LocalResourceFile)
            mLocalizedSourceText = Localization.GetString("Source.Header", LocalResourceFile)
            mLocalizedThumbText = Localization.GetString("Thumb.Header", LocalResourceFile)
            mLocalizedMissingText = Localization.GetString("Missing", LocalResourceFile)

            'William Severance - below code not needed as PortalModuleBase provides ModuleId property
            'If Not Request.QueryString("mid") Is Nothing Then
            '    mModuleID = Int32.Parse(Request.QueryString("mid"))
            'End If

            If Not Request.QueryString("selectall") Is Nothing Then
                mSelectAll = Boolean.Parse(Request.QueryString("selectall"))
                chkSelectAll.Checked = mSelectAll
                If mSelectAll Then
                    chkSelectAll.Text = Localization.GetString("deselectAll.Text", LocalResourceFile)
                    chkSelectAll.ToolTip = Localization.GetString("deselectAll.ToolTip", LocalResourceFile)
                    'chkSelectAll.Text = "Deselect All"
                Else
                    chkSelectAll.Text = Localization.GetString("checkAll.Text", LocalResourceFile)
                    chkSelectAll.ToolTip = Localization.GetString("checkAll.ToolTip", LocalResourceFile)
                    'chkSelectAll.Text = "Select All"
                End If
            End If

            'William Severance - Added condition Not Page.IsPostBack to fix loss of
            'datagrid header localization following postback. LocalizeDataGrid must be called
            'before datagrid is data bound.

            If Not Page.IsPostBack Then Localization.LocalizeDataGrid(grdContent, LocalResourceFile)

            ' Load gallery menu, this method allow call request only once
            Dim galleryMenu As GalleryMenu = CType(LoadControl("Controls/ControlGalleryMenu.ascx"), GalleryMenu)
            With galleryMenu
                .ModuleID = ModuleId
                .GalleryRequest = CType(mRequest, BaseRequest)
            End With
            celGalleryMenu.Controls.Add(galleryMenu)

            ' Load gallery breadcrumbs, this method allow call request only once
            Dim galleryBreadCrumbs As BreadCrumbs = CType(LoadControl("Controls/ControlBreadCrumbs.ascx"), BreadCrumbs)
            With galleryBreadCrumbs
                .ModuleID = ModuleId
                .GalleryRequest = CType(mRequest, BaseRequest)
            End With
            celBreadcrumbs.Controls.Add(galleryBreadCrumbs)

            If Not Page.IsPostBack AndAlso mGalleryConfig.IsValidPath Then
                If Not mFolder.IsPopulated Then
                    'Response.Redirect("~/DesktopModules/Gallery/cache.aspx" & HttpContext.Current.Request.Url.Query & "&mid=" & mModuleID.ToString) '& "&tabid=" & TabId)
                    Response.Redirect(ApplicationURL)
                End If

                BindData()

                'lblHeader.Text = "Maintenance Album: " & mFolder.Name

                btnDeleteAll.Attributes.Add("onClick", "javascript: return confirm('Are you sure you wish to delete selected items?')")

                ' Store URL Referrer to return to portal for first request only
                'If Not Page.IsPostBack AndAlso Not ViewState("UrlReferrer") Is Nothing Then
                '    ViewState("UrlReferrer") = Request.UrlReferrer.ToString()
                'End If
                'End If

            End If

        End Sub

        'Private Sub BindData(ByVal Request As GalleryRequest)
        Private Sub BindData()

            With mFolder
                txtPath.Text = mFolder.URL
                txtName.Text = .Name
            End With

            lblAlbumInfo.Text = AlbumInfo()
            BindChildItems()

        End Sub

        Protected Function FileInfo(ByVal DataItem As Object) As String
            Dim file As GalleryMaintenanceFile = CType(DataItem, GalleryMaintenanceFile)
            Dim sb As New System.Text.StringBuilder

            If Not file.SourceExists Then sb.Append(mLocalizedSourceText)
            If Not file.ThumbExists Then
                If sb.Length > 0 Then sb.Append(", ")
                sb.Append(mLocalizedThumbText)
            End If

            If Not file.FileExists Then
                If sb.Length > 0 Then sb.Append(", ")
                sb.Append(mLocalizedAlbumText)
            End If

            If sb.Length > 0 Then
                sb.Append(" ")
                sb.Append(mLocalizedMissingText)
            End If

            Return sb.ToString
        End Function

        Protected Function AlbumInfo() As String
            Dim file As GalleryMaintenanceFile
            Dim itemCount As Integer = mRequest.ImageList.Count
            Dim sourceCount As Integer = 0
            Dim albumCount As Integer = 0
            Dim thumbCount As Integer = 0
            Dim sb As New System.Text.StringBuilder

            For Each file In mRequest.ImageList
                If file.SourceExists Then sourceCount += 1
                If file.FileExists Then albumCount += 1
                If file.ThumbExists Then thumbCount += 1
            Next

            sb.Append(Localization.GetString("AlbumInfo", LocalResourceFile))
            sb.Replace("[ItemCount]", itemCount.ToString)
            sb.Replace("[AlbumName]", mRequest.Folder.Name)
            sb.Replace("[ItemNoSource]", (itemCount - sourceCount).ToString)
            sb.Replace("[ItemNoAlbum]", (itemCount - albumCount).ToString)
            sb.Replace("[ItemNoThumb]", (itemCount - thumbCount).ToString)
            sb.Replace("[ImageSize]", mGalleryConfig.FixedHeight.ToString & " x " & mGalleryConfig.FixedWidth.ToString)
            sb.Replace("[ThumbSize]", mGalleryConfig.MaximumThumbHeight.ToString & " x " & mGalleryConfig.MaximumThumbWidth.ToString)
            'sb.Append("<b>")
            'sb.Append(itemCount)
            'sb.Append("</b> items were found in album ")
            'sb.Append(mRequest.Folder.Name)
            'sb.Append("<br/><b>")
            'sb.Append((itemCount - sourceCount))
            'sb.Append("</b> items without source.")
            'sb.Append("<br/><b>")
            'sb.Append((itemCount - albumCount))
            'sb.Append("</b> items without album file.")
            'sb.Append("<br/><b>")
            'sb.Append((itemCount - thumbCount))
            'sb.Append("</b> items without thumbnail.")
            'sb.Append("<br/>")
            'sb.Append("<br/>")
            'sb.Append("Select items in the list then choose an option below to rebuild files in your album")
            'sb.Append("<br/>")
            'sb.Append("<br/>")
            'sb.Append("Image will be created with max size: ")
            'sb.Append(mGalleryConfig.FixedHeight & "x" & mGalleryConfig.FixedWidth)
            'sb.Append("<br/>")
            'sb.Append("Thumbnail will be created with max size: ")
            'sb.Append(mGalleryConfig.MaximumThumbHeight & "x" & mGalleryConfig.MaximumThumbWidth)

            Return sb.ToString
        End Function

        Private Sub BindChildItems()

            grdContent.DataSource = mRequest.ImageList
            grdContent.PageSize = (mRequest.ImageList.Count + 1)
            grdContent.DataBind()

        End Sub

        Protected Function BrowserURL(ByVal DataItem As Object) As String
            Dim item As GalleryMaintenanceFile = CType(DataItem, GalleryMaintenanceFile)
            If mGalleryConfig.AllowPopup Then
                Return item.URL
            Else
                Dim params As New Generic.List(Of String)
                params.Add("returnctl=" & ReturnCtl)
                If Not Request.QueryString("currentstrip") Is Nothing Then params.Add("currentstrip=" & Request.QueryString("currentstrip"))
                Return Utils.AppendURLParameters(item.URL, params.ToArray())
            End If
        End Function

        

        Private Sub grdContent_ItemCommand(ByVal source As Object, ByVal e As System.Web.UI.WebControls.DataGridCommandEventArgs) Handles grdContent.ItemCommand

            Dim itemIndex As Integer = e.Item.ItemIndex 'Int16.Parse((CType(e.CommandSource, ImageButton).CommandArgument))

            Dim selItem As GalleryMaintenanceFile = CType(mRequest.ImageList.Item(itemIndex), GalleryMaintenanceFile)
            Select Case (CType(e.CommandSource, ImageButton).CommandName)
                Case "delete"

                Case "update"

            End Select

            mRequest.Folder.Populate(True)
            BindData()

        End Sub

        Private Sub grdContent_ItemCreated(ByVal sender As Object, ByVal e As System.Web.UI.WebControls.DataGridItemEventArgs) Handles grdContent.ItemCreated
            If Not Request.QueryString("selectall") Is Nothing Then
                Dim ctl As Control = e.Item.FindControl("chkSelect")
                If Not ctl Is Nothing Then
                    Dim chkSelect As CheckBox = CType(ctl, CheckBox)
                    If chkSelect.Enabled Then chkSelect.Checked = mSelectAll 'William Severance - do not check of no item edit permission
                End If
            End If
        End Sub

        'Private Sub dlStrip_ItemCreated(ByVal sender As Object, ByVal e As System.Web.UI.WebControls.DataListItemEventArgs)

        'End Sub

        Private Sub cmdReturn_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles cmdReturn.Click
            Config.ResetGalleryConfig(ModuleId)
            Goback()
        End Sub

        Private Sub Goback()
            Dim url As String
            'If Not ViewState("UrlReferrer") Is Nothing Then
            '    url = ViewState("UrlReferrer").ToString
            'Else
            'Dim params As String() = New String(0) {"path=" & mFolder.GalleryHierarchy}
            'url = NavigateURL(TabId, "", Utils.RemoveEmptyParams(params))
            url = GetURL(Page.Request.ServerVariables("URL"), Page, "", "ctl=&selectall=&mid=&currentitem=&media=")
            'End If
            Response.Redirect(url)
        End Sub

        Private Function SelectedFile() As ArrayList
            Dim i As Integer
            Dim selList As New ArrayList

            For i = 0 To grdContent.Items.Count - 1
                Dim myListItem As DataGridItem = grdContent.Items(i)
                Dim myCheck As CheckBox = DirectCast(myListItem.FindControl("chkSelect"), CheckBox)

                If myCheck.Checked Then
                    'Dim fileName As String = CType(grdContent.DataKeys(i), String)
                    Dim file As GalleryMaintenanceFile = mRequest.ImageList(i) 'CType(mRequest.ImageList.Item(CType(grdContent.DataKeys(i), String)), GalleryMaintenanceFile)
                    selList.Add(file)
                End If
            Next

            Return selList

        End Function

        Private Sub RefreshAlbum()

            mRequest.Folder.Clear() ' refresh gallery folder first ready for repopulate
            mRequest.Populate()
            BindData()

        End Sub

        Private Sub btnCopySource_Click(ByVal sender As Object, ByVal e As System.Web.UI.ImageClickEventArgs)
            Dim file As GalleryMaintenanceFile
            For Each file In SelectedFile()
                file.CreateFileFromSource()
            Next

            RefreshAlbum()

        End Sub

        Private Sub btnCopyFile_Click(ByVal sender As Object, ByVal e As System.Web.UI.ImageClickEventArgs)
            Dim file As GalleryMaintenanceFile
            For Each file In SelectedFile()
                file.CreateSourceFromFile()
            Next

            RefreshAlbum()

        End Sub

        Private Sub btnCreateThumb_Click(ByVal sender As Object, ByVal e As System.Web.UI.ImageClickEventArgs)
            Dim file As GalleryMaintenanceFile
            For Each file In SelectedFile()
                file.RebuildThumbnail()
            Next

            RefreshAlbum()

        End Sub

        Private Sub chkSelectAll_CheckedChanged(ByVal sender As Object, ByVal e As System.EventArgs) Handles chkSelectAll.CheckedChanged
            Dim url As String
            If chkSelectAll.Text = Localization.GetString("checkAll.Text", LocalResourceFile) Then
                url = GetURL(Page.Request.ServerVariables("URL"), Page, "selectall=true", "")
            Else
                url = GetURL(Page.Request.ServerVariables("URL"), Page, "selectall=false", "")
            End If
            Response.Redirect(url)

        End Sub

        Private Sub ClearCache1_Click(ByVal sender As Object, ByVal e As System.Web.UI.ImageClickEventArgs) Handles ClearCache1.Click
            Config.ResetGalleryConfig(ModuleId)
        End Sub

        Public ReadOnly Property GalleryConfig() As DotNetNuke.Modules.Gallery.Config
            Get
                Return mGalleryConfig
            End Get
        End Property

        Private Overloads Sub btnSyncAll_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles btnSyncAll.Click
            Dim file As GalleryMaintenanceFile
            For Each file In SelectedFile()
                file.Synchronize()
            Next

            RefreshAlbum()
        End Sub

        Private Overloads Sub btnDeleteAll_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles btnDeleteAll.Click
            Dim ps As PortalSettings = PortalController.GetCurrentPortalSettings()
            For Each gmFile As GalleryMaintenanceFile In SelectedFile()
                'file.DeleteAll()
                Try
                    'If IO.File.Exists(file.SourcePath) Then IO.File.Delete(file.SourcePath)
                    'If IO.File.Exists(file.ThumbPath) Then IO.File.Delete(file.ThumbPath)
                    'If IO.File.Exists(file.AlbumPath) Then IO.File.Delete(file.AlbumPath)

                    If File.Exists(gmFile.AlbumPath) Then mFolder.DeleteFile(gmFile.AlbumPath, ps, True)
                    If gmFile.Type = Config.ItemType.Image Then
                        If File.Exists(gmFile.ThumbPath) Then mFolder.DeleteFile(gmFile.ThumbPath, ps, True)
                        If File.Exists(gmFile.SourcePath) Then mFolder.DeleteFile(gmFile.SourcePath, ps, True)

                        'Reset folder thumbnail to "folder.gif" if its current
                        'thumbnail is being deleted. Will then get set to next image thumbnail during populate if there is one.
                        If mFolder.Thumbnail = gmFile.Name Then mFolder.ResetThumbnail()
                    End If

                    'Dim metaData As New GalleryXML(mFolder.Path)
                    ' JIMJ Access func through classname not instance
                    GalleryXML.DeleteMetaData(mFolder.Path, gmFile.Name)

                Catch exc As System.Exception

                End Try
            Next
            RefreshAlbum()
        End Sub


    End Class

End Namespace

