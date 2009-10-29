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
Imports DotNetNuke
Imports DotNetNuke.Services.Search

Namespace DotNetNuke.Modules.Gallery

    Public Class Controller
        Implements Entities.Modules.ISearchable

        Private Const MAX_DESCRIPTION_LENGTH As Integer = 100

        Public Function GetSearchItems(ByVal ModInfo As Entities.Modules.ModuleInfo) As Services.Search.SearchItemInfoCollection Implements Entities.Modules.ISearchable.GetSearchItems

            Dim SearchItemCollection As New SearchItemInfoCollection
            'Dim config As DotNetNuke.Modules.Gallery.Config = config.GetGalleryConfig(ModInfo.ModuleID)
            'PopulateSearch(config.RootFolder, SearchItemCollection)
            Return SearchItemCollection

        End Function

        ' When BuildCacheOnStart is set to true, this recursively populates the folder objects
        Public Shared Sub PopulateSearch(ByVal rootFolder As GalleryFolder, ByVal SearchCollection As SearchItemInfoCollection)
            Dim folder As Object

            If Not rootFolder.IsPopulated Then
                rootFolder.Populate(False)
                For Each item As IGalleryObjectInfo In rootFolder.List
                    Dim strDescription As String = HtmlUtils.Shorten(HtmlUtils.Clean(item.Description, False), MAX_DESCRIPTION_LENGTH, "...")
                    Dim SearchItem As SearchItemInfo = New SearchItemInfo(item.Title, item.ItemInfo, item.OwnerID, item.CreatedDate, rootFolder.GalleryConfig.ModuleID, item.Name, strDescription, item.ID)
                    SearchCollection.Add(SearchItem)
                Next
            End If

            For Each folder In rootFolder.List
                If TypeOf folder Is GalleryFolder AndAlso Not CType(folder, GalleryFolder).IsPopulated Then
                    CType(folder, GalleryFolder).Populate(False)
                    PopulateSearch(CType(folder, GalleryFolder), SearchCollection)
                End If
            Next

        End Sub

    End Class
End Namespace
