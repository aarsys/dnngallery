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
    ''' Class for using the viewer to view galleries
    ''' </summary>
    ''' <remarks></remarks>
    Public Class GalleryViewerRequest
        Inherits BaseRequest

        Private mRequest As HttpRequest

        ' Stored reference to ItemIndex of Browseable Items collection
        Private _currentItemIndex As Integer

        Private _currentItem As Integer
        Private _nextItem As Integer
        Private _previousItem As Integer

#Region "Public Properties"
        Public ReadOnly Property CurrentItem() As GalleryFile
            Get
                Return CType(MyBase.Folder.List.Item(_currentItem), GalleryFile)
            End Get
        End Property

        Public ReadOnly Property CurrentItemNumber() As Integer
            Get
                Return _currentItem + 1
            End Get
        End Property

        Public ReadOnly Property NextItem() As Integer
            Get
                Return _nextItem
            End Get
        End Property

        Public ReadOnly Property PreviousItem() As Integer
            Get
                Return _previousItem
            End Get
        End Property

#End Region

#Region "Public Methods"

        Public Sub New(ByVal ModuleID As Integer, Optional ByVal SortType As Config.GallerySort = Config.GallerySort.Name, Optional ByVal SortDescending As Boolean = False)
            MyBase.New(ModuleID, SortType, SortDescending)

            ' Don't want to continue processing if this is an invalid path
            If Not GalleryConfig.IsValidPath Then
                Exit Sub
            End If

            ' New sort feature since 2.0
            Dim myBrowsableItems As New ArrayList
            Dim intCounter As Integer

            'For Each intCounter In MyBase.Folder.BrowsableItems
            '    Dim item As GalleryFile = CType(MyBase.Folder.List.Item(intCounter), GalleryFile)
            '    If Not (item.Title.ToLower.IndexOf("icon") > -1) _
            '    AndAlso Not (item.Title.ToLower = "watermark") _
            '    AndAlso (item.ApprovedDate <= DateTime.Today OrElse MyBase.GalleryConfig.AutoApproval) Then
            '        myBrowsableItems.Add(item)
            '    Else
            '    End If
            'Next

            For Each intCounter In MyBase.Folder.BrowsableItems
                Dim item As GalleryFile = CType(MyBase.Folder.List.Item(intCounter), GalleryFile)
                myBrowsableItems.Add(item)
            Next

            myBrowsableItems.Sort(New Comparer(New String(0) {[Enum].GetName(GetType(Config.GallerySort), SortType)}, SortDescending))

            mRequest = HttpContext.Current.Request

            ' Determine initial item to be viewed.
            If Not mRequest.QueryString("currentitem") Is Nothing Then
                _currentItem = CInt(mRequest.QueryString("currentitem"))
            Else
                _currentItem = CType(myBrowsableItems.Item(0), GalleryFile).Index
            End If

            ' Grab the index of the item in the folder.list collection
            If MyBase.Folder.IsBrowsable Then
                For intCounter = 0 To myBrowsableItems.Count - 1
                    If CType(myBrowsableItems.Item(intCounter), GalleryFile).Index = _currentItem Then
                        _currentItemIndex = intCounter
                        Exit For
                    End If
                Next

                If _currentItemIndex = myBrowsableItems.Count - 1 Then
                    _nextItem = CType(myBrowsableItems.Item(0), GalleryFile).Index
                Else
                    _nextItem = CType(myBrowsableItems.Item(_currentItemIndex + 1), GalleryFile).Index
                End If

                If _currentItemIndex = 0 Then
                    _previousItem = CType(myBrowsableItems.Item(myBrowsableItems.Count - 1), GalleryFile).Index
                Else
                    _previousItem = CType(myBrowsableItems.Item(_currentItemIndex - 1), GalleryFile).Index
                End If

            End If

        End Sub

#End Region

    End Class

End Namespace