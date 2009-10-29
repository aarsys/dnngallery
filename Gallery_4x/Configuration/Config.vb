'
' DotNetNukeŽ - http://www.dotnetnuke.com
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
Imports System.Collections.Specialized
Imports System.IO
Imports DotNetNuke
Imports DotNetNuke.Security
Imports DotNetNuke.Security.Roles
Imports DotNetNuke.Entities.Portals
Imports DotNetNuke.Common.Globals
Imports DotNetNuke.Common.Utilities
Imports DotNetNuke.Services.Localization
Imports DotNetNuke.Modules.Gallery.Utils
Imports DotNetNuke.Entities.Users

Namespace DotNetNuke.Modules.Gallery

    Public Enum ObjectClass As Integer
        None = 0
        DNNUser = 101
        DNNRole = 102
    End Enum

    Public Class Config

#Region "Enumerators"
        Public Enum GalleryDisplayOption
            [Name]
            [Author]
            [Location]
            [Client]
            [CreatedDate]
            [ApprovedDate]
        End Enum

        Enum ItemType
            Folder
            Image
            Movie
            Flash
            Zip
        End Enum

        Enum GallerySort
            Name = 1
            Title = 3
            Size = 2
            Author = 4
            Location = 5
            Score = 6
            OwnerID = 7
            CreatedDate = 8
            ApprovedDate = 9
        End Enum

        Enum GalleryView
            CardView = 0
            ListView = 1
            Standard = 2
        End Enum

#End Region

#Region "Private Variables"
        Private Const GalleryConfigCacheKeyPrefix As String = "GalleryConfig"

        ' Holds a reference to the root gallery folder
        Private mSourceDirectory As String = DefaultSourceDirectory
        Private mSharedResourceFile As String = DefaultSharedResourceFile
        Private mRootFolder As GalleryFolder
        ' Root filesystem Path
        Private mRootPath As String
        Private mSourcePath As String
        Private mThumbPath As String
        Private mRootURL As String
        Private mTheme As String
        Private mThumbFolder As String = DefaultThumbFolder
        Private mSourceFolder As String = DefaultSourceFolder
        Private mTempFolder As String = DefaultTempFolder
        Private mGalleryTitle As String
        Private mGalleryDescription As String

        ' Number of pics per Gallery_Row on each page view & Number of rows per page view
        Private mStripWidth As Integer
        Private mStripHeight As Integer
        ' Acceptable file types
        '<daniel-- DefaultFileExtensions now gets the image file extensions from the core host extensions>
        Private mFileExtensions As String
        Private mMovieExtensions As String
        Private mFlashExtensions As String
        Private mCategoryValues As String
        Private mTextDisplayValues As String

        Private mFileTypes As New ArrayList
        Private mMovieTypes As New ArrayList
        Private mFlashTypes As New ArrayList
        Private mCategories As New ArrayList
        Private mTextDisplay As New ArrayList

        ' Cache Settings
        Private mBuildCacheonStart As Boolean
        Private mFixedSize As Boolean = True
        Private mFixedWidth As Integer
        Private mFixedHeight As Integer
        Private mKeepSource As Boolean = DefaultIsKeepSource
        Private mSlideshowSpeed As Integer
        Private mIsPrivate As Boolean
        Private mGalleryTabID As Integer
        Private mMaxFileSize As Integer
        Private mQuota As Integer
        Private mMaxPendingUploadsSize As Integer

        ' Thumbnail creation
        Private mMaxThumbWidth As Integer
        Private mMaxThumbHeight As Integer

        ' Created Date
        Private mCreatedDate As DateTime
        Private mAutoApproval As Boolean

        ' ModuleID for these settings
        Private mModuleID As Integer

        ' IsValidPath return whether the root gallery path is valid for this machine
        Private mIsValidPath As Boolean = True

        ' Owner - for User Gallery feature
        Private mOwnerID As Integer

        ' Gallery view feature settings
        Private mDefaultView As GalleryView
        Private mAllowChangeView As Boolean
        Private mDisplayWhatsNew As Boolean
        Private mUseWatermark As Boolean
        Private mMultiLevelMenu As Boolean
        Private mAllowSlideshow As Boolean
        Private mAllowPopup As Boolean
        Private mAllowVoting As Boolean
        Private mAllowExif As Boolean

        ' Sort feature settings
        Private mSortPropertyValues As String
        Private mSortProperties As New ArrayList
        Private mDefaultSort As GallerySort
        Private mDefaultSortDESC As Boolean

        ' AllowDownload feature
        Private mAllowDownload As Boolean = True
        Private mDownloadRoles As String = ""

        'Display Option
        Private mCellPadding As Integer = DefaultCellPadding
#End Region

#Region "Default Properties"
        Public Shared ReadOnly Property DefaultSourceDirectory() As String
            Get
                Return ApplicationPath & "/DesktopModules/Gallery"
            End Get
        End Property

        Public Shared ReadOnly Property DefaultSharedResourceFile() As String
            Get
                Return String.Join("/", New String() {DefaultSourceDirectory, Localization.LocalResourceDirectory, Localization.LocalSharedResourceFile})
            End Get
        End Property

        Public Shared ReadOnly Property DefaultRootURL() As String
            Get
                Dim _portalSettings As PortalSettings = PortalController.GetCurrentPortalSettings
                Return _portalSettings.HomeDirectory & "Gallery/"
            End Get
        End Property

        Public Shared ReadOnly Property DefaultRootPath() As String
            Get
                Dim _portalSettings As PortalSettings = PortalController.GetCurrentPortalSettings
                Return _portalSettings.HomeDirectoryMapPath & "Gallery\"
            End Get
        End Property

        Public Shared ReadOnly Property DefaultTheme() As String
            Get
                Return "DNNSimple"
            End Get
        End Property

        Public Shared ReadOnly Property DefaultThumbFolder() As String
            Get
                Return "_thumbs"
            End Get
        End Property

        Public Shared ReadOnly Property DefaultSourceFolder() As String
            Get
                Return "_source"
            End Get
        End Property

        Public Shared ReadOnly Property DefaultIsKeepSource() As Boolean
            Get
                Return True
            End Get
        End Property

        Public Shared ReadOnly Property DefaultTempFolder() As String
            Get
                Return "_temp"
            End Get
        End Property

        Public Shared ReadOnly Property DefaultGalleryTitle() As String
            Get
                Return "Gallery"
            End Get
        End Property

        Public Shared ReadOnly Property DefaultGalleryDescription() As String
            Get
                Return "Gallery"
            End Get
        End Property

        Public Shared ReadOnly Property DefaultBuildCacheOnStart() As Boolean
            Get
                Return True
            End Get
        End Property

        Public Shared ReadOnly Property DefaultStripWidth() As Integer
            Get
                Return 3
            End Get
        End Property

        Public Shared ReadOnly Property DefaultStripHeight() As Integer
            Get
                Return 2
            End Get
        End Property

        Public Shared ReadOnly Property DefaultMaxThumbWidth() As Integer
            Get
                Return 100
            End Get
        End Property

        Public Shared ReadOnly Property DefaultMaxThumbHeight() As Integer
            Get
                Return 100
            End Get
        End Property

        '<daniel-- removed to be replaced by the core host extensions11/22/05>
        Public Shared ReadOnly Property DefaultFileExtensions() As String
            Get
                Return permittedFileTypes(".jpg;.gif;.bmp;.png;.tif")
            End Get
        End Property

        Public Shared ReadOnly Property DefaultMovieExtensions() As String
            Get
                Return permittedFileTypes(".mov;.wmv;.wma;.mpg;.avi;.asf;.asx;.mpe;.mpeg;.mid;.midi;.wav;.aiff;.mp3")
            End Get
        End Property

        Public Shared ReadOnly Property DefaultFlashExtensions() As String
            Get
                Return permittedFileTypes(".swf")
            End Get
        End Property

        Public Shared ReadOnly Property DefaultCategoryValues() As String
            Get
                Return "Image;Movie;Music;Flash"
            End Get
        End Property

        Public Shared ReadOnly Property DefaultTextDisplayValues() As String
            Get
                Return "Name;Size;Description"
            End Get
        End Property

        Public Shared ReadOnly Property DefaultSortPropertyValues() As String
            Get
                Return "Name;Title;Size;Author;CreatedDate"
            End Get
        End Property

        Public Shared ReadOnly Property DefaultMaxFileSize() As Integer
            Get
                Return 1000
            End Get
        End Property

        Public Shared ReadOnly Property DefaultQuota() As Integer
            Get
                Return 0    '0 means no quota
            End Get
        End Property

        Public Shared ReadOnly Property DefaultMaxPendingUploadsSize() As Integer
            Get
                Return 0    '0 means no maximum.
            End Get
        End Property

        Public Shared ReadOnly Property DefaultFixedWidth() As Integer
            Get
                Return 500
            End Get
        End Property

        Public Shared ReadOnly Property DefaultFixedHeight() As Integer
            Get
                Return 500
            End Get
        End Property

        Public Shared ReadOnly Property DefaultSlideshowSpeed() As Integer
            Get
                Return 3000
            End Get
        End Property

        Public Shared ReadOnly Property DefaultIsPrivate() As Boolean
            Get
                Return False
            End Get
        End Property

        Public Shared ReadOnly Property DefaultOwnerId() As Integer
            Get
                Return -1 'Signifies not specified
                'Dim _portalSettings As PortalSettings = PortalController.GetCurrentPortalSettings
                'Return _portalSettings.AdministratorId
            End Get
        End Property

        Public Shared ReadOnly Property DefaultCellPadding() As Integer
            Get
                Return 15
            End Get
        End Property

        Public Shared ReadOnly Property DefaultMultiLevelMenu() As Boolean
            Get
                Return True
            End Get
        End Property

        Public Shared ReadOnly Property DefaultAllowSlideShow() As Boolean
            Get
                Return True
            End Get
        End Property

        Public Shared ReadOnly Property DefaultAllowPopup() As Boolean
            Get
                Return True
            End Get
        End Property

        Public Shared ReadOnly Property DefaultAllowVoting() As Boolean
            Get
                Return True
            End Get
        End Property

        Public Shared ReadOnly Property DefaultAllowExif() As Boolean
            Get
                Return True
            End Get
        End Property

        Public Shared ReadOnly Property DefaultAllowDownload() As Boolean
            Get
                Return True
            End Get
        End Property

        Public Shared ReadOnly Property DefaultDownloadRoles() As String
            Get
                Return String.Empty
            End Get
        End Property

        Public Shared ReadOnly Property DefaultAutoApproval() As Boolean
            Get
                Return True
            End Get
        End Property

        Public Shared ReadOnly Property DefaultDefaultView() As GalleryView
            Get
                Return GalleryView.Standard
            End Get
        End Property

        Public Shared ReadOnly Property DefaultAllowChangeView() As Boolean
            Get
                Return True
            End Get
        End Property

        Public Shared ReadOnly Property DefaultDefaultSortDESC() As Boolean
            Get
                Return False
            End Get
        End Property

        Public Shared ReadOnly Property DefaultDefaultSort() As GallerySort
            Get
                Return GallerySort.Title
            End Get
        End Property

        Public Shared ReadOnly Property DefaultDisplayWhatsNew() As Boolean
            Get
                Return True
            End Get
        End Property

        Public Shared ReadOnly Property DefaultUseWatermark() As Boolean
            Get
                Return True
            End Get
        End Property
#End Region

#Region "Shared Methods"
        Public Shared Function GetGalleryConfig(ByVal ModuleID As Integer) As DotNetNuke.Modules.Gallery.Config

            Dim config As DotNetNuke.Modules.Gallery.Config = Nothing
            Dim strKey As String = GalleryConfigCacheKeyPrefix & ModuleID.ToString

            If DataCache.GetCache(strKey) IsNot Nothing Then
                Try
                    config = CType(DataCache.GetCache(strKey), DotNetNuke.Modules.Gallery.Config)
                Catch
                    ' Cached version is bad. Re-create it.
                    DataCache.RemoveCache(strKey)
                    config = Nothing
                End Try
                Dim portalSettings As PortalSettings = PortalController.GetCurrentPortalSettings()
                If config IsNot Nothing AndAlso (config.GalleryTabID <> portalSettings.ActiveTab.TabID) Then
                    ' If the active tab ID does not match the GalleryTabId in the config, the module
                    ' has been moved to a different tab. The config is no longer valid.
                    DataCache.RemoveCache(strKey)
                    config = Nothing
                End If
            End If
            If config Is Nothing Then
                config = New DotNetNuke.Modules.Gallery.Config(ModuleID)
                'WES - Added sliding exception to avoid non-caching in DNN 4.9.1 when
                'overload without explicit sliding exception is used.
                DataCache.SetCache(strKey, config, TimeSpan.FromMinutes(5.0))
            End If

            Return config

        End Function

        'William Severance - changed sub to function to facilitate return of GalleryConfig when configuration
        'settings control has caused change of configuration requiring update of folders, files, etc.

        Public Shared Function ResetGalleryConfig(ByVal ModuleID As Integer) As DotNetNuke.Modules.Gallery.Config

            Dim strKey As String = GalleryConfigCacheKeyPrefix & ModuleID.ToString
            DataCache.RemoveCache(strKey)

            ' Call this to force storage of settings
            Dim config As DotNetNuke.Modules.Gallery.Config = New DotNetNuke.Modules.Gallery.Config(ModuleID, True)
            'WES - Added sliding exception to avoid non-caching in DNN 4.9.1 when
            'overload without explicit sliding exception is used.
            DataCache.SetCache(strKey, Config, TimeSpan.FromMinutes(5.0))
            Return config

        End Function

        Public Shared Sub ResetGalleryFolder(ByVal Album As GalleryFolder)
            Try
                Album.List.Clear()
                Album.IsPopulated = False
                ' Repopulate this album
                Utils.PopulateAllFolders(Album, True)
            Catch ex As Exception
                ResetGalleryConfig(Album.GalleryConfig.ModuleID)
            End Try

        End Sub
#End Region

#Region "Public Properties"
        Public ReadOnly Property RootFolder() As GalleryFolder
            Get
                Return mRootFolder
            End Get
        End Property

        Public ReadOnly Property SourceDirectory(Optional ByVal IncludeHost As Boolean = False) As String
            Get
                If IncludeHost Then
                    Return AddHTTP(HttpContext.Current.Request.ServerVariables("HTTP_HOST")) & mSourceDirectory
                Else
                    Return mSourceDirectory
                End If
            End Get
        End Property

        Public ReadOnly Property SharedResourceFile() As String
            Get
                Return mSharedResourceFile
            End Get
        End Property

        Public ReadOnly Property ItemsPerStrip() As Integer
            Get
                'Return CType((mStripWidth * mStripHeight), Integer)  'William Severance - no need for conversion!
                Return mStripWidth * mStripHeight
            End Get
        End Property

        Public ReadOnly Property RootPath() As String
            Get
                Return mRootPath
            End Get
        End Property

        Public ReadOnly Property RootURL() As String
            Get
                Return mRootURL
            End Get
        End Property

        Public ReadOnly Property ImageFolder(Optional ByVal IncludeHost As Boolean = False) As String
            Get
                Dim folder As String = SourceDirectory & "/Themes/" & Theme & "/Images/"
                If IncludeHost Then
                    Return AddHost(folder)
                Else
                    Return folder
                End If
            End Get
        End Property

        Public ReadOnly Property Theme() As String
            Get
                Return mTheme
            End Get
        End Property

        Public ReadOnly Property Css() As String
            Get
                Return SourceDirectory & "/Themes/" & Theme & "/" & Theme.ToLower & ".css"
            End Get
        End Property

        Public Function GetImageURL(ByVal Image As String, Optional ByVal IncludeHost As Boolean = False) As String
            If IncludeHost Then
                Return ImageFolder(True) & Image
            Else
                Return ImageFolder & Image
            End If

        End Function

        Public ReadOnly Property ThumbFolder() As String
            Get
                Return mThumbFolder
            End Get
        End Property

        Public ReadOnly Property SourceFolder() As String
            Get
                Return mSourceFolder
            End Get
        End Property

        Public ReadOnly Property TempFolder() As String
            Get
                Return mTempFolder
            End Get
        End Property

        Public ReadOnly Property GalleryTitle() As String
            Get
                Return mGalleryTitle
            End Get
        End Property

        Public ReadOnly Property GalleryDescription() As String
            Get
                Return mGalleryDescription
            End Get
        End Property


        Public ReadOnly Property BuildCacheonStart() As Boolean
            Get
                Return mBuildCacheonStart
            End Get
        End Property

        Public ReadOnly Property StripWidth() As Integer
            Get
                Return mStripWidth
            End Get
        End Property

        Public ReadOnly Property StripHeight() As Integer
            Get
                Return mStripHeight
            End Get
        End Property

        Public ReadOnly Property MaximumThumbWidth() As Integer
            Get
                Return mMaxThumbWidth
            End Get
        End Property

        Public ReadOnly Property MaximumThumbHeight() As Integer
            Get
                Return mMaxThumbHeight
            End Get
        End Property

        Public ReadOnly Property TypeFromExtension(ByVal FileExtension As String) As Config.ItemType
            Get
                If mFileTypes.Contains((FileExtension).ToLower) Then Return CType(Config.ItemType.Image, ItemType)
                If mMovieTypes.Contains((FileExtension).ToLower) Then Return CType(Config.ItemType.Movie, ItemType)
                If mFlashTypes.Contains((FileExtension).ToLower) Then Return CType(Config.ItemType.Flash, ItemType)
            End Get
        End Property

        Public ReadOnly Property IsValidImageType(ByVal FileExtension As String) As Boolean
            Get
                Return (mFileTypes.Contains((FileExtension).ToLower))
            End Get
        End Property

        Public ReadOnly Property IsValidMovieType(ByVal FileExtension As String) As Boolean
            Get
                Return mMovieTypes.Contains((FileExtension).ToLower)
            End Get
        End Property

        Public ReadOnly Property IsValidFlashType(ByVal FileExtension As String) As Boolean
            Get
                Return mFlashTypes.Contains((FileExtension).ToLower)
            End Get
        End Property

        Public ReadOnly Property IsValidMediaType(ByVal FileExtension As String) As Boolean
            Get
                Return (IsValidImageType(FileExtension) OrElse IsValidMovieType(FileExtension) OrElse IsValidFlashType(FileExtension))
            End Get
        End Property

        Public ReadOnly Property IsValidFileType(ByVal FileExtension As String) As Boolean
            Get
                Return (IsValidMediaType(FileExtension) OrElse (FileExtension.ToLower = ".zip"))
            End Get
        End Property

        Public ReadOnly Property FileExtensions() As String
            Get
                Return mFileExtensions
            End Get
        End Property

        Public ReadOnly Property MovieExtensions() As String
            Get
                Return mMovieExtensions
            End Get
        End Property

        Public ReadOnly Property MovieTypes() As ArrayList
            Get
                Return Me.mMovieTypes
            End Get
        End Property

        Public ReadOnly Property CategoryValues() As String
            Get
                Return mCategoryValues
            End Get
        End Property

        Public ReadOnly Property Categories() As ArrayList
            Get
                Return mCategories
            End Get
        End Property

        Public ReadOnly Property TextDisplay() As ArrayList
            Get
                Return mTextDisplay
            End Get
        End Property

        Public ReadOnly Property SortProperties() As ArrayList
            Get
                Return mSortProperties
            End Get
        End Property

        Public ReadOnly Property AutoApproval() As Boolean
            Get
                Return mAutoApproval
            End Get
        End Property

        Public ReadOnly Property IsValidPath() As Boolean
            Get
                Return mIsValidPath
            End Get
        End Property

        Public ReadOnly Property ModuleID() As Integer
            Get
                Return mModuleID
            End Get
        End Property

        Public ReadOnly Property IsFixedSize() As Boolean
            Get
                Return mFixedSize
            End Get
        End Property

        Public ReadOnly Property FixedWidth() As Integer
            Get
                Return mFixedWidth
            End Get
        End Property

        Public ReadOnly Property MaxFileSize() As Integer
            Get
                Return mMaxFileSize
            End Get
        End Property

        Public ReadOnly Property Quota() As Integer
            Get
                Return mQuota
            End Get
        End Property

        Public ReadOnly Property MaxPendingUploadsSize() As Integer
            Get
                Return mMaxPendingUploadsSize
            End Get
        End Property

        Public ReadOnly Property FixedHeight() As Integer
            Get
                Return mFixedHeight
            End Get
        End Property

        Public ReadOnly Property IsKeepSource() As Boolean
            Get
                Return mKeepSource
            End Get
        End Property

        Public ReadOnly Property SlideshowSpeed() As Integer
            Get
                Return mSlideshowSpeed
            End Get
        End Property

        Public ReadOnly Property IsPrivate() As Boolean
            Get
                Return mIsPrivate
            End Get
        End Property

        Public ReadOnly Property GalleryTabID() As Integer
            Get
                Return mGalleryTabID
            End Get
        End Property

        Public ReadOnly Property OwnerID() As Integer
            Get
                Return mOwnerID
            End Get
        End Property

        Public ReadOnly Property CreatedDate() As DateTime
            Get
                Return mCreatedDate
            End Get
        End Property

        Public ReadOnly Property MultiLevelMenu() As Boolean
            Get
                Return mMultiLevelMenu
            End Get
        End Property

        Public ReadOnly Property AllowSlideshow() As Boolean
            Get
                Return mAllowSlideshow
            End Get
        End Property

        Public ReadOnly Property AllowPopup() As Boolean
            Get
                Return mAllowPopup
            End Get
        End Property

        Public ReadOnly Property AllowVoting() As Boolean
            Get
                Return mAllowVoting
            End Get
        End Property

        Public ReadOnly Property AllowExif() As Boolean
            Get
                Return mAllowExif
            End Get
        End Property

        Public ReadOnly Property AllowDownload() As Boolean
            Get
                Return mAllowDownload
            End Get
        End Property

        Public ReadOnly Property DownloadRoles() As String
            Get
                Return mDownloadRoles
            End Get
        End Property

        Public ReadOnly Property HasDownloadPermission() As Boolean
            Get
                ' JIMJ add a loop to check the roles by name instead of by id
                For Each Role As String In DownloadRoles.Split(";"c)
                    If Not Role.Length = 0 Then

                        Dim RoleName As String
                        Select Case CType((Role), Integer)
                            Case CType((glbRoleAllUsers), Integer)      ' Global role
                                RoleName = glbRoleAllUsersName

                            Case Else   ' All other roles
                                Dim _portalSettings As PortalSettings = PortalController.GetCurrentPortalSettings
                                Dim ctlRole As New RoleController
                                Dim objRole As RoleInfo = ctlRole.GetRole(Int16.Parse(Role), _PortalSettings.PortalId)

                                RoleName = objRole.RoleName
                        End Select

                        If PortalSecurity.IsInRole(RoleName) Then
                            Return True
                        End If
                    End If
                Next

                ' Not found
                Return False

                ' Old code
                'Return PortalSecurity.IsInRoles(DownloadRoles)
            End Get
        End Property

        Public ReadOnly Property CellPadding() As Integer
            Get
                Return mCellPadding
            End Get
        End Property

        Public ReadOnly Property DisplayWhatsNew() As Boolean
            Get
                Return mDisplayWhatsNew
            End Get
        End Property

        Public ReadOnly Property DefaultView() As GalleryView
            Get
                Return mDefaultView
            End Get
        End Property

        Public ReadOnly Property AllowChangeView() As Boolean
            Get
                Return mAllowChangeView
            End Get
        End Property

        Public ReadOnly Property DefaultSort() As GallerySort
            Get
                Return mDefaultSort
            End Get
        End Property

        Public ReadOnly Property DefaultSortDESC() As Boolean
            Get
                Return mDefaultSortDESC
            End Get
        End Property

        Public ReadOnly Property UseWatermark() As Boolean
            Get
                Return mUseWatermark
            End Get
        End Property

#End Region

#Region "Public Methods, Functions"

        Shared Sub New()
            'Dim _portalSettings As PortalSettings = PortalController.GetCurrentPortalSettings
        End Sub

        Private Sub New(ByVal ModuleId As Integer)
            Me.New(ModuleId, False)
        End Sub

        Private Sub New(ByVal ModuleId As Integer, ByVal ReSync As Boolean)
            Dim _context As HttpContext = HttpContext.Current
            Dim fileExtension As String
            Dim catValue As String
            Dim textValue As String
            Dim sortProperty As String

            ' Grab the moduleID
            mModuleID = ModuleId

            ' Save the TabId
            Dim _portalSettings As PortalSettings = PortalController.GetCurrentPortalSettings
            mGalleryTabID = _portalSettings.ActiveTab.TabID

            ' Grab settings from the database
            Dim settings As Hashtable = PortalSettings.GetModuleSettings(ModuleId)

            If settings.Count > 0 Then
                ' Do Nothing
            Else
                ' PreConfigure the Module
                Dim mUserID As Integer
                If _context.Request.IsAuthenticated Then
                    mUserID = UserController.GetCurrentUserInfo.UserID

                    GalleryPreConfig.PreConfig(mModuleID, mUserID)

                    Dim newsettings As Hashtable = PortalSettings.GetModuleSettings(ModuleId)

                    If newsettings.Count > 0 Then
                        settings = newsettings
                    End If
                End If
            End If

            ' Now iterate through all the values and init local variables
            mRootURL = GetValue(settings("RootURL"), DefaultRootURL & ModuleId.ToString & "/")
            mGalleryTitle = GetValue(settings("GalleryTitle"), DefaultGalleryTitle)

            Try
                mRootPath = HttpContext.Current.Request.MapPath(mRootURL)
                mSourcePath = IO.Path.Combine(mRootPath, DefaultSourceFolder)
                mThumbPath = IO.Path.Combine(mRootPath, DefaultThumbFolder)

                mIsValidPath = GalleryFolder.CreateRootFolders(mRootURL) <> -1

            Catch ex As Exception

            End Try

            '<tamttt:note removed to support full skinning feature>
            'mImageURL = GetValue(settings("ImageURL"), mImageURL)
            '</tamttt:note>
            'Skins to themes change, make sure module settings are proper
            mTheme = GetValue(settings("Theme"), DefaultTheme)

            mGalleryDescription = GetValue(settings("GalleryDescription"), DefaultGalleryDescription)
            mBuildCacheonStart = GetValue(settings("BuildCacheOnStart"), DefaultBuildCacheOnStart)
            mStripWidth = GetValue(settings("StripWidth"), DefaultStripWidth)
            mStripHeight = GetValue(settings("StripHeight"), DefaultStripHeight)
            mMaxThumbWidth = GetValue(settings("MaxThumbWidth"), DefaultMaxThumbWidth)
            mMaxThumbHeight = GetValue(settings("MaxThumbHeight"), DefaultMaxThumbHeight)

            mQuota = GetValue(settings("Quota"), DefaultQuota)
            mMaxFileSize = GetValue(settings("MaxFileSize"), DefaultMaxFileSize)
            mMaxPendingUploadsSize = GetValue(settings("MaxPendingUploadsSize"), DefaultMaxPendingUploadsSize)
            '<tamttt:note FixedSize & KeepSource should be default>
            'mFixedSize = CBool(GetValue(settings("IsFixedSize"), CStr(mFixedSize)))
            'mKeepSource = CBool(GetValue(settings("IsKeepSource"), CStr(mKeepSource)))
            '</tamttt:note>
            mFixedWidth = GetValue(settings("FixedWidth"), DefaultFixedWidth)
            mFixedHeight = GetValue(settings("FixedHeight"), DefaultFixedHeight)
            mSlideshowSpeed = GetValue(settings("SlideshowSpeed"), DefaultSlideshowSpeed)
            mIsPrivate = GetValue(settings("IsPrivate"), DefaultIsPrivate)
            mMultiLevelMenu = GetValue(settings("MultiLevelMenu"), DefaultMultiLevelMenu)
            mAllowSlideshow = GetValue(settings("AllowSlideshow"), DefaultAllowSlideShow)
            mAllowPopup = GetValue(settings("AllowPopup"), DefaultAllowPopup)
            mAllowVoting = GetValue(settings("AllowVoting"), DefaultAllowVoting)
            mAllowExif = GetValue(settings("AllowExif"), DefaultAllowExif)

            mAllowDownload = GetValue(settings("AllowDownload"), DefaultAllowDownload)
            mDownloadRoles = GetValue(settings("DownloadRoles"), DefaultDownloadRoles)

            mAutoApproval = GetValue(settings("AutoApproval"), DefaultAutoApproval)
            'mIsAvatarsGallery = CBool(GetValue(settings("IsAvatarsGallery"), CStr(mIsAvatarsGallery)))
            'mCellPadding = GetValue(settings("CellPadding"), DefaultCellPadding)
            mDefaultSortDESC = GetValue(settings("DefaultSortDESC"), DefaultDefaultSortDESC)
            mDefaultSort = GetValue(settings("DefaultSort"), DefaultDefaultSort)

            ' Gallery view settings
            mDefaultView = GetValue(settings("DefaultView"), DefaultDefaultView)
            mAllowChangeView = GetValue(settings("AllowChangeView"), DefaultAllowChangeView)

            mDisplayWhatsNew = GetValue(settings("DisplayWhatsNew"), DefaultDisplayWhatsNew)
            mUseWatermark = GetValue(settings("UseWatermark"), DefaultUseWatermark)

            mOwnerID = GetValue(settings("OwnerID"), DefaultOwnerId)
            'mGalleryOwner = GalleryUser.GetGalleryUser(mOwnerID)
            mCreatedDate = GetValue(settings("CreatedDate"), DateTime.Today)

            mFileExtensions = DefaultFileExtensions

            '' Iterate through the file extensions and create the collection
            For Each fileExtension In Split(mFileExtensions, ";", , CompareMethod.Text)
                mFileTypes.Add((fileExtension).ToLower)
            Next

            mMovieExtensions = DefaultMovieExtensions

            ' Iterate through the movie extensions and create the collection
            For Each fileExtension In Split(mMovieExtensions, ";", , CompareMethod.Text)
                mMovieTypes.Add((fileExtension).ToLower)
            Next

            '<daniel s-- removed to be replaced by the core host extensions11/22/05>
            'William Severance - restored line as it need to be there for flash swf extensions
            mFlashExtensions = DefaultFlashExtensions

            For Each fileExtension In Split(mFlashExtensions, ";", , CompareMethod.Text)
                mFlashTypes.Add((fileExtension).ToLower)
            Next

            mCategoryValues = GetValue(settings("CategoryValues"), DefaultCategoryValues)
            For Each catValue In Split(mCategoryValues, ";", , CompareMethod.Text)
                'Added Localization for Checkboxes by M. Schlomann
                mCategories.Add(GetLocalization(catValue))
            Next

            mTextDisplayValues = GetValue(settings("TextDisplayValues"), DefaultTextDisplayValues)
            For Each textValue In Split(mTextDisplayValues, ";", , CompareMethod.Text)
                mTextDisplay.Add((textValue).ToLower)
            Next

            mSortPropertyValues = GetValue(settings("SortPropertyValues"), DefaultSortPropertyValues)
            For Each sortProperty In Split(mSortPropertyValues, ";", , CompareMethod.Text)
                'mSortProperties.Add(LCase(sortProperty))
                ' <tamttt:note sort properties to be display in GUI so we keep original case>
                mSortProperties.Add(sortProperty)
            Next

            Dim mSourceURL As String = BuildPath(New String(1) {mRootURL, mSourceFolder}, "/", False, False)
            Dim mThumbURL As String = BuildPath(New String(1) {mRootURL, mThumbFolder}, "/", False, False)

            ' Only run if we've got a valid filesystem path
            If mIsValidPath Then
                ' Initialize the root folder object
                'Dim rd As New Random
                Dim ID As Integer = -1
                'ID = rd.Next
                mRootFolder = New GalleryFolder(0, ID, Nothing, "", mGalleryTitle, mRootURL, mRootPath, "", "", mThumbPath, mThumbURL, "", "", mSourcePath, mSourceURL, mGalleryTitle, mGalleryDescription, mCategoryValues, "", "", "", mOwnerID, DateTime.MinValue, DateTime.MinValue, Me)

                ' Build the cache at once if required.
                If BuildCacheonStart Then
                    ' Populate all folders and sub-folders of the root
                    Utils.PopulateAllFolders(mRootFolder, ReSync)
                Else
                    ' Populate only the root folder and immediate sub-folders (if any) of the root.
                    Utils.PopulateAllFolders(mRootFolder, 2, ReSync)
                End If
            End If
        End Sub

#End Region

#Region "Private Functions"
        ' Localization for Checkboxes added by M. Schlomann
        Public Shared Function GetLocalization(ByVal catValue As String, Optional ByVal Setting As String = "") As String
            If catValue = String.Empty Then Return String.Empty
            'Dim LocalResourceFile As String = System.Web.HttpContext.Current.Request.ApplicationPath & "/DesktopModules/Gallery/" & Services.Localization.Localization.LocalResourceDirectory & "/SharedResources.resx"
            If (Setting <> String.Empty) Then catValue = String.Concat(catValue, "_", Setting)
            'Return Localization.GetString(catValue & "Categories", LocalResourceFile)
            'William Severance - modified to return catValue if no localization provided in SharedResources.resx
            Dim localizedCatValue As String = Localization.GetString(catValue & "Categories", DefaultSharedResourceFile)
            If String.IsNullOrEmpty(localizedCatValue) Then
                Return catValue
            Else
                Return localizedCatValue
            End If
        End Function

        '<daniel added to use core extensions>
        Private Shared Function permittedFileTypes(ByVal Extensions As String) As String

            Dim FileExtension As String

            ' break up the images into an array
            Dim coreImageTypesArray(Split(Extensions, ";", , CompareMethod.Text).Length) As String
            Dim Index As Long = 0

            ' get  dnn core file extensions
            Dim dnnPermittedFileExtensions As String = Common.Globals.HostSettings("FileExtensions").ToString.ToUpper

            ' step thru each and if they are permitted then keep it in the list
            ' otherwise remove it
            Dim newStandardExtensions As String = ""

            For Each FileExtension In Split(Extensions, ";", , CompareMethod.Text)
                If InStr(1, dnnPermittedFileExtensions, FileExtension.Remove(0, 1).ToUpper) <> 0 Then
                    ' build a new list
                    If newStandardExtensions.Length > 0 Then
                        newStandardExtensions += ";"
                    End If
                    newStandardExtensions += FileExtension
                End If
            Next

            Return newStandardExtensions
        End Function

        'William Severance - Replaced GetValue with .Net generics method

        Private Function GetValue(Of T)(ByVal Input As Object, ByVal DefaultValue As T) As T
            If Input Is Nothing OrElse ((TypeOf Input Is System.String) AndAlso (DirectCast(Input, String) = String.Empty)) Then
                Return DefaultValue
            Else
                If TypeOf DefaultValue Is System.Enum Then
                    Try
                        Return CType([Enum].Parse(GetType(T), CType(Input, String)), T)
                    Catch ex As ArgumentException
                        Return DefaultValue
                    End Try
                ElseIf TypeOf DefaultValue Is System.DateTime Then
                    Dim objDateTime As Object
                    Try
                        objDateTime = DateTime.Parse(CType(Input, String))
                    Catch ex As FormatException
                        Dim dt As DateTime
                        If Not DateTime.TryParse(CType(Input, String), Globalization.DateTimeFormatInfo.InvariantInfo, Globalization.DateTimeStyles.None, dt) Then
                            dt = DateTime.Now
                        End If
                        objDateTime = dt
                    End Try
                    Return CType(objDateTime, T)
                Else
                    Return CType(Input, T)
                End If
            End If
        End Function

#End Region

    End Class

End Namespace
