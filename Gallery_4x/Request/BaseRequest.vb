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

Imports System.Web
Imports System.Text
Imports System.IO
Imports DotNetNuke
Imports DotNetNuke.Common.Globals
Imports DotNetNuke.services.Localization
Imports DotNetNuke.Modules.Gallery.Config
Imports DotNetNuke.Modules.Gallery.Utils

Namespace DotNetNuke.Modules.Gallery

    ''' <summary>
    ''' Base class for generic gallery browsing
    ''' </summary>
    ''' <remarks></remarks>
    Public Class BaseRequest
 
        Private mModuleId As Integer
        Private mGalleryConfig As DotNetNuke.Modules.Gallery.Config

        Private mRequest As HttpRequest

        Private mFolderPaths As New ArrayList
        Private mPath As String = ""
        Private mFolder As GalleryFolder
        Private mSortType As Config.GallerySort
        Private mSortDescending As Boolean

#Region "Public Properties"
        Public ReadOnly Property GalleryConfig() As DotNetNuke.Modules.Gallery.Config
            Get
                Return mGalleryConfig
            End Get
        End Property

        Public ReadOnly Property ModuleId() As Integer
            Get
                Return mModuleId
            End Get
        End Property

        Public ReadOnly Property Folder() As GalleryFolder
            ' Instance of current gallery folder being requested
            Get
                Return mFolder
            End Get
        End Property

        Public Property SortType() As Config.GallerySort
            Get
                Return mSortType
            End Get
            Set(ByVal Value As Config.GallerySort)
                mSortType = Value
            End Set
        End Property

        Public Property SortDescending() As Boolean
            Get
                Return mSortDescending
            End Get
            Set(ByVal Value As Boolean)
                mSortDescending = Value
            End Set
        End Property

        Public Property Path() As String
            Get
                Return mPath
            End Get
            Set(ByVal Value As String)
                mPath = Value
            End Set
        End Property
#End Region

#Region "Public Functions"

        Public Sub New(ByVal ModuleId As Integer, _
                Optional ByVal SortType As Config.GallerySort = Config.GallerySort.Name, _
                Optional ByVal SortDescending As Boolean = False, _
                Optional ByVal GalleryPath As String = "")
            'Dim some vars for this section only
            Dim paths() As String
            Dim pathCounter As Integer
            Dim newFolderDetail As FolderDetail

            mModuleId = ModuleId
            mSortType = SortType
            mSortDescending = SortDescending
            mPath = FriendlyURLDecode(GalleryPath)
            mGalleryConfig = Config.GetGalleryConfig(ModuleId)

            If Not mGalleryConfig.IsValidPath() Then
                ' Dont want to continue processing if the path is invalid.
                Exit Sub
            End If

            ' Grab the current request context
            mRequest = HttpContext.Current.Request

            ' Check if path has been assigned
            If mPath.Length = 0 Then
                ' Get the path
                If Not mRequest.QueryString("path") Is Nothing Then
                    mPath = HttpUtility.UrlDecode(mRequest.QueryString("path"))
                    mPath = FriendlyURLDecode(mPath)
                End If
            End If

            ' Init the root folder            
            mFolder = mGalleryConfig.RootFolder

            ' Create the root path information
            newFolderDetail = New FolderDetail
            newFolderDetail.Name = mGalleryConfig.GalleryTitle
            newFolderDetail.CurrentFolder = mFolder
            newFolderDetail.URL = NavigateURL()
            mFolderPaths.Add(newFolderDetail)

            ' Logic to determine path structure
            If Not mPath Is Nothing AndAlso mPath.Length > 0 Then
                Try
                    ' Split the input into distinct paths
                    paths = Split(mPath, "/")
                    'Dim intermediatePaths() As String
                    'intermediatePaths = New String(paths.GetUpperBound(0)) {}

                    ' Navigate the path structure
                    For pathCounter = 0 To paths.GetUpperBound(0)
                        mFolder = CType(mFolder.List.Item(paths(pathCounter)), GalleryFolder)

                        ' Gotta do it differently for the first path
                        'If pathCounter = 0 Then
                        '    intermediatePaths(pathCounter) = paths(pathCounter)
                        'Else
                        '    intermediatePaths(pathCounter) = intermediatePaths(pathCounter - 1) & "/" & paths(pathCounter)
                        'End If

                        ' Create the folder details for this folder
                        newFolderDetail = New FolderDetail
                        newFolderDetail.Name = mFolder.Title
                        newFolderDetail.CurrentFolder = mFolder
                        newFolderDetail.URL = mFolder.BrowserURL
                        'newFolderDetail.URL = intermediatePaths(pathCounter)
                        mFolderPaths.Add(newFolderDetail)

                        'William Severance modified to fix Gemini issue GAL-6168 to
                        'permit population of folders on demand when BuildCacheonStart is
                        'false.

                        If Not mFolder.IsPopulated Then
                            If mGalleryConfig.BuildCacheonStart Then
                                Utils.PopulateAllFolders(mFolder, False)
                            Else
                                Utils.PopulateAllFolders(mFolder, 2, False)
                            End If
                        End If
                    Next
                Catch ex As Exception ' an incorrect folder structure probably returned
                    ' Keep the last known good folder
                End Try

            Else
            End If

        End Sub

        Public Function FolderPaths() As ArrayList
            ' Returns hierarchy of folder paths to current path
            Return mFolderPaths

        End Function

#End Region

    End Class

End Namespace