'
' DotNetNukeŽ - http://www.dotnetnuke.com
' Copyright (c) 2002-2011 by DotNetNuke Corp. 

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
        Actions.Add(GetNextActionID, Localization.GetString("Configuration.Action", LocalResourceFile), ModuleActionType.ModuleSettings, "", "edit.gif", EditUrl(ControlKey:="configuration"), "", False, SecurityAccessLevel.Admin, True, False)
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

      ReturnCtl = Request.QueryString("returnctl")

      'WES - Added to change control title if user had edit permission
      If mGalleryAuthorization.HasEditPermission Then
        Dim ctl As Control = DotNetNuke.Common.FindControlRecursiveDown(Me.ContainerControl, "lblTitle")
        If Not ctl Is Nothing Then
          CType(ctl, Label).Text = Localization.GetString("ControlTitle_editor", LocalResourceFile)
        End If
      End If

    End Sub

    Private Sub Page_PreRender(sender As Object, e As System.EventArgs) Handles Me.PreRender
      Dim gutterWidth As Integer = 60
      If GalleryConfig.Theme <> "DNN Simple" Then gutterWidth += 20
      Dim width As String = (GalleryConfig.FixedWidth + gutterWidth).ToString & "px"
      tblViewControl.Style.Add("width", width)
    End Sub

    Private Sub btnBack_Click(ByVal sender As Object, ByVal e As EventArgs) Handles btnBack.Click
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

    Public Property Title As String
      Get
        Return lblTitle.Text
      End Get
      Set(value As String)
        lblTitle.Text = value
      End Set
    End Property
  End Class
End Namespace
