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

Imports System.IO
Imports System.Text
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

Namespace DotNetNuke.Modules.Gallery

    Public MustInherit Class Viewer
        Inherits DotNetNuke.Entities.Modules.PortalModuleBase
        Implements DotNetNuke.Entities.Modules.IActionable

        'Private mModuleID As Integer
        Private mGalleryConfig As DotNetNuke.Modules.Gallery.Config
        Private mGalleryAuthorization As DotNetNuke.Modules.Gallery.Authorization

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

#Region "Optional Interfaces"

        Public ReadOnly Property ModuleActions() As ModuleActionCollection Implements Entities.Modules.IActionable.ModuleActions
            Get
                Dim Actions As New ModuleActionCollection
                Actions.Add(GetNextActionID, Localization.GetString("Configuration.Action", LocalResourceFile), ModuleActionType.ModuleSettings, "", "edit.gif", EditUrl(controlkey:="configuration"), "", False, SecurityAccessLevel.Admin, True, False)
                Actions.Add(GetNextActionID, Localization.GetString("GalleryHome.Action", LocalResourceFile), ModuleActionType.ContentOptions, "", "icon_moduledefinitions_16px.gif", NavigateURL(), False, SecurityAccessLevel.Edit, True, False)
                Return Actions
            End Get
        End Property

#End Region

        Private Sub Page_Load(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles MyBase.Load

            mGalleryConfig = Config.GetGalleryConfig(ModuleId)
            mGalleryAuthorization = New Authorization(ModuleId)

            ' Load the styles
            DirectCast(Page, CDefault).AddStyleSheet(CreateValidID(GalleryConfig.Css()), GalleryConfig.Css())

            'If Not Request.QueryString("mid") Is Nothing Then
            '    mModuleID = Int16.Parse(Request.QueryString("mid"))
            'End If

            'If Not Page.IsPostBack _
            'AndAlso Request.QueryString("color") Is Nothing _
            'AndAlso Request.QueryString("flipx") Is Nothing _
            'AndAlso Request.QueryString("flipy") Is Nothing _
            'AndAlso Request.QueryString("rotate") Is Nothing _
            'AndAlso Request.QueryString("zoomindex") Is Nothing _
            'AndAlso (Not Request.UrlReferrer Is Nothing) Then
            '    ViewState("UrlReferrer") = Request.UrlReferrer.ToString()
            'End If

            ReturnCtl = Request.QueryString("returnctl")

            'WES - Added to change control title if user had edit permission
            If mGalleryAuthorization.HasEditPermission Then
                Dim ctl As Control = DotNetNuke.Common.FindControlRecursiveDown(Me.ContainerControl, "lblTitle")
                If Not ctl Is Nothing Then
                    CType(ctl, Label).Text = Localization.GetString("ControlTitle_editor", LocalResourceFile)
                End If
            End If
        End Sub

        Private Sub btnBack_Click(ByVal sender As Object, ByVal e As EventArgs) Handles btnBack.Click
            'Dim url As String
            'If Request.QueryString("mode") Is Nothing Then ' This view called by gallery container
            '    url = GetURL(Page.Request.ServerVariables("URL"), Page, "", "currentitem=&media=&ctl=&mid=") '"currentstrip=&currentitem=&media=&ctl=&mid=")
            'Else
            '    url = GetURL(Page.Request.ServerVariables("URL"), Page, "ctl=FileEdit", "mode=&color=&flipx=&flipy=&rotate=&zoomindex=")
            'End If
            'Response.Redirect(url) '(CType(Session("UrlReferrer"), String))
            Response.Redirect(ReturnURL(TabId, ModuleId, Request))
        End Sub

        Public ReadOnly Property GalleryConfig() As DotNetNuke.Modules.Gallery.Config
            Get
                Return mGalleryConfig
            End Get
        End Property

        Public Property ReturnCtl() As String
            Get
                If Not ViewState("ReturnCtl") Is Nothing Then
                    Return CType(ViewState("ReturnCtl"), String)
                Else
                    Return "Viewer;"
                End If
            End Get
            Set(ByVal value As String)
                ViewState("ReturnCtl") = value
            End Set
        End Property

        ''Removed by Quinn 2/23/2009
        ''Moved to a backend function, This was a duplicate function, and had security issues
        ''Easier to maintain in a single location
        ''Utilities.vb :: ReturnURL
        'Public ReadOnly Property ReturnURL() As String
        '    Get
        '        Dim params As New Generic.List(Of String)
        '        Dim ctl As String = ""
        '        If Not Request.QueryString("returnctl") Is Nothing Then
        '            Dim returnCtls As String() = Request.QueryString("returnctl").Split(";"c)
        '            If returnCtls.Length > 1 Then
        '                ctl = returnCtls(returnCtls.Length - 2)
        '                Dim rtnctl As String = Request.QueryString("returnctl").Replace(ctl & ";", "")
        '                If Not String.IsNullOrEmpty(rtnctl) Then params.Add("returnctl=" & rtnctl)
        '                params.Add("mid=" & ModuleId.ToString)
        '            End If
        '        End If
        '        If Not Request.QueryString("path") Is Nothing Then params.Add("path=" & Request.QueryString("path"))
        '        If Not Request.QueryString("currentstrip") Is Nothing Then params.Add("currentstrip=" & Request.QueryString("currentstrip"))
        '        If Not Request.QueryString("currentitem") Is Nothing Then params.Add("currentitem=" & Request.QueryString("currentitem"))
        '        Return NavigateURL(ctl, params.ToArray())
        '    End Get

        'End Property

    End Class
End Namespace
