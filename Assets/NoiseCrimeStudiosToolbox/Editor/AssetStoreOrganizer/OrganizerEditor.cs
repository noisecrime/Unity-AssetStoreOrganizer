// #define LOG_UNITY_METHODS

using System;
using System.Collections.Generic;
using System.IO;
using NoiseCrimeStudios.Core.Editor;
using NoiseCrimeStudios.Core.Editor.IMGUI;
using NoiseCrimeStudios.Core.Formatting;
using UnityEditor;
using UnityEditorInternal;
using UnityEngine;

namespace NoiseCrimeStudios.Toolbox.AssetStoreOrganizer
{
    // Add option to 'Assets' menu
    public class AssetsMenu_OpenStoreOrganizerEditor
    {
        [MenuItem( "Assets/Open Asset Store Organizer", false, ( int )AssetMenuItemPriority.Packages )]
        public static void OpenStoreOrganizerEditor()
        {
            EditorWindow.GetWindow<OrganizerEditor>( "Store Organizer" );
        }
    }

    // Main Editor Window for Organizer
    public class OrganizerEditor : EditorWindow
    {
        private readonly PackageLibrary m_sourceLibrary           = new PackageLibrary();
        private readonly PackageLibrary m_archiveLibrary          = new PackageLibrary();
        private readonly PackageFilters m_packageFilters          = new PackageFilters();
        private List<AssetPackage>      m_sortedLibrary           = new List<AssetPackage>();

        private PackagesLocation        m_sourceLocation          = PackagesLocation.NativePackageList;
      
        private PackageFilters.Category m_activeColumn            = PackageFilters.Category.Title;
        private string                  m_packageInfoCount;		
        private readonly string[]		m_locationNameArray       = Enum.GetNames( typeof( PackagesLocation ) );

        // GUI
        private static readonly float[] s_columnWidths            = new float[]{280f, 72f, 64f, 64f, 88f, 88f, 256f, 224f, 64f};        
        private static readonly float   s_lineHeight              = 24f;
        private Vector2                 m_scrollPosition;        
        private Rect                    m_optionsButtonRect;

        private readonly string         m_materialIconPath        = "Assets/NoiseCrimeStudios/Editor/Resources/GoogleMaterialIcons";       
        
        private readonly string         m_urlPublisherQuery       = @"https://assetstore.unity.com/?publisher=";
        private readonly string         m_urlCategoryQuery        = @"https://assetstore.unity.com/?category=";
        private readonly string         m_urlAssetTitleQuery      = @"https://assetstore.unity.com/packages/vfx/{0}/{1}-{2}";

        // e.g. https://assetstore.unity.com/packages/vfx/shaders/fullscreen-camera-effects/amplify-color-1894

        // Old Site URL's - No longer work
        // private readonly string      urlPublisherQuery       = @"https://www.assetstore.unity3d.com/en/#!/search/page=1/sortby=popularity/query=publisher:";
        // private readonly string      urlCategoryQuery        = @"https://www.assetstore.unity3d.com/en/#!/search/page=1/sortby=popularity/query=category:";
        // private readonly string      urlAssetTitleQuery      = @"https://www.assetstore.unity3d.com/en/#!/content/";
        
        private static Texture          s_iconImport;
        private static GUIContent       s_iconImportContent;
        private static Texture          s_iconCancel;
        private static GUIContent       s_iconCancelContent;
        private static Texture          s_iconCheck;
        private static GUIContent       s_iconCheckContent;
        private static GUIContent       s_reloadContent;

        private static GUIStyle         s_toolbarLabel;
        private static GUIStyle         s_iconLabelStyle;
        
        private bool m_requiresBrowserUpdate  = false;
        private bool m_requiresFilterUpdate   = false;

        // [SerializeField]
        public  bool DisableFileOperations  = false;

        // Shortcut = [Alt + F1]
        [MenuItem( "Window/NoiseCrimeStudios/Asset Store Organizer &1", false, ( int )WindowMenuItemPriorty.Windows )]
        private static void InitWindow()
        {
            GetWindow<OrganizerEditor>( "Store Organizer" );
        }

        // Constructor
        public OrganizerEditor()
        {
#if LOG_UNITY_METHODS
            Debug.Log( "StorePackageLibraryEditor: Constructor" );
#endif
        }

        private void OnEnable()
        {
#if LOG_UNITY_METHODS
            Debug.Log( "StorePackageLibraryEditor: OnEnable" );
#endif
            m_activeColumn                    = PackageFilters.Category.Title;
            m_packageFilters.OrderByAscending = true;
            m_requiresBrowserUpdate           = false;
            m_requiresFilterUpdate            = false;
                
            UpdateLibraries();
        }
        
#pragma warning disable UNT0001 // Empty Unity message

        private void OnDisable()
        {
#if LOG_UNITY_METHODS
            Debug.Log( "StorePackageLibraryEditor: OnDisable" );
#endif
        }

        private void OnDestroy()
        {
#if LOG_UNITY_METHODS
            Debug.Log( "StorePackageLibraryEditor: OnDestroy" );
#endif
        }

#pragma warning restore UNT0001 // Empty Unity message

        private void UpdateLibraries()
        {
#if LOG_UNITY_METHODS
            Debug.Log( $"StorePackageLibraryEditor: InitialiseBrowser: {m_sourceLocation}" );
#endif
            // Reset Flag
            m_requiresBrowserUpdate = false;

            // Populate Package Library Content
            m_sourceLibrary.PopulateLibraryContent( m_sourceLocation );
            // Populate Package Archive Content
            m_archiveLibrary.PopulateLibraryContent( PackagesLocation.Archive );            
            // Check Against Backups
            m_sourceLibrary.CompareAgainstArchive( m_archiveLibrary );

            // TODO
            // These filters only apply to sourceLibrary not archiveLibrary!!
            // How can we keep filters if reloading libraries?

            // Precompile filters
            m_packageFilters.ConstructFilters( m_sourceLibrary.Packages );
            // Apply Sorting
            UpdateFilterSortResults();
        }

        private void UpdateFilterSortResults()
        {
#if LOG_UNITY_METHODS
            Debug.Log( "StorePackageLibraryEditor: UpdateFilterSortResults" );
#endif
            // Reset Flag
            m_requiresFilterUpdate    = false;

            // Get Sort & Filtered Result
            m_sortedLibrary     = m_packageFilters.SortAndFilterResults( m_sourceLibrary.Packages, m_activeColumn );

            long sortedSizeOnDisk = 0;
            foreach( AssetPackage p in m_sortedLibrary )
                sortedSizeOnDisk += p.FileSize;

            m_packageInfoCount    = string.Format( "Size: {0} of {1}   Packages: {2} of {3}", Numerical.ByteCountToSuffixHumbads( sortedSizeOnDisk ), m_sourceLibrary.SizeOnDisk, m_sortedLibrary.Count, m_sourceLibrary.FileCount );            
        }

        /// <summary>Archives the source location packages based on curent sorting and filtering results</summary>
        public void StartArchiveSourceLibraryProcess()
        {
            Archiver.ArchiveAssetStorePackages( m_sortedLibrary, m_archiveLibrary, m_sourceLocation, !DisableFileOperations );
            m_requiresBrowserUpdate = true;
        }

        #region GUI METHODS
        private void SetUpStyles()
        {
            s_reloadContent = EditorGUIUtility.IconContent("d_Refresh"); // d_Refresh or d_RotateTool

            EditorGUIStyles.CreateIconContent( ref s_iconImport, ref s_iconImportContent,   m_materialIconPath, "{0}/ic_file_download_{1}_18dp_1x.png", "Import Package" );
            EditorGUIStyles.CreateIconContent( ref s_iconCancel, ref s_iconCancelContent,   m_materialIconPath, "{0}/baseline_cancel_{1}_18dp_1x.png", "Not In Archive" );
            EditorGUIStyles.CreateIconContent( ref s_iconCheck,  ref s_iconCheckContent,    m_materialIconPath, "{0}/baseline_check_circle_{1}_18dp_1x.png", "In Archive" );
            
            if ( null == s_toolbarLabel )
            {
                s_toolbarLabel = new GUIStyle( EditorStyles.toolbar )
                {
                    alignment = TextAnchor.MiddleLeft
                };
            }

            if ( null == s_iconLabelStyle )
            {
                s_iconLabelStyle = new GUIStyle( "Label" )
                {
                    padding = new RectOffset( 2, 2, 2, 2 ),
                    alignment = TextAnchor.MiddleCenter
                };
            }            
        }

        protected virtual void OnGUI()
        {
            SetUpStyles();
            
            if ( m_requiresBrowserUpdate )
                UpdateLibraries();
            if ( m_requiresFilterUpdate )
                UpdateFilterSortResults();

            float prevLabelWidth    = EditorGUIUtility.labelWidth;
            m_requiresBrowserUpdate   = false;
            m_requiresFilterUpdate    = false;

            GuiPackageTopBar();
            GUILayout.Space(8f);
            GuiBrowserBlock();

            EditorGUIUtility.labelWidth = prevLabelWidth;
        }


        private void GuiPackageTopBar()
        {
            EditorGUIUtility.labelWidth = 64f;

            // Top Line
            using ( new EditorGUILayout.HorizontalScope( EditorStyles.toolbar ) )
            {
                if ( GUILayout.Button( s_reloadContent, EditorStyles.toolbarButton, GUILayout.Width( 32f )) )                
                    m_requiresBrowserUpdate = true;

                // Show Package Location Popup
                GUI.changed = false;
                m_sourceLocation = ( PackagesLocation )EditorGUILayout.Popup( ( int )m_sourceLocation, m_locationNameArray, EditorStyles.toolbarPopup, GUILayout.MinWidth( 256f - 40f) );
                m_requiresBrowserUpdate = GUI.changed;
                
                float locationWidth = 552f - 4f;
                
                GUILayout.Space(8f);

                switch ( m_sourceLocation )
                {
                    case PackagesLocation.NativePackageList:                      
                        GUILayout.Label( OrganizerPaths.StoreDirectoryRoot, s_toolbarLabel, GUILayout.Width( locationWidth ) );
                        break;
                    case PackagesLocation.AssetStore:
                        GUILayout.Label( OrganizerPaths.StoreDirectoryLegacy, s_toolbarLabel, GUILayout.Width( locationWidth ) ); 
                        break;
                    case PackagesLocation.AssetStore5x:
                        GUILayout.Label( OrganizerPaths.StoreDirectoryModern, s_toolbarLabel, GUILayout.Width( locationWidth ) ); 
                        break;
                    case PackagesLocation.Custom:
                        GuiPackageLocationButton( PackagesLocation.Custom, "Browse for custom asset store folder", locationWidth );
                        break;
                    case PackagesLocation.Archive:
                        GuiPackageLocationButton( PackagesLocation.Archive, "Browse for archive asset store folder", locationWidth );
                        break;
                }
                                
                GUILayout.FlexibleSpace();              

                if ( GUILayout.Button( "Archive Options", EditorGUIStyles.LabelNormRight, GUILayout.MinWidth( 256f ), GUILayout.MaxWidth( 256f ) ) )
                    PopupWindow.Show( m_optionsButtonRect, new ArchiverOptionsPopup( this ) );

                if ( Event.current.type == EventType.Repaint )
                    m_optionsButtonRect = GUILayoutUtility.GetLastRect();        
            }

            // Second Line
            using ( new EditorGUILayout.HorizontalScope( EditorStyles.toolbar ) )
            {
                GUI.changed = false;
                m_packageFilters.GuiSearchBar();
                m_requiresFilterUpdate = GUI.changed;
                GUILayout.FlexibleSpace();
                EditorGUILayout.LabelField( m_packageInfoCount, EditorGUIStyles.LabelNormRight, GUILayout.MinWidth( 320f ), GUILayout.MaxWidth( 320f ) );
            }
        }

        private void GuiPackageLocationButton( PackagesLocation location, string title,  float width )
        {
            string directory = OrganizerPaths.GetPackageDirectory(location);

            if ( !string.IsNullOrEmpty( directory ) && !Directory.Exists( directory ) )
                directory = string.Empty;

            if ( GUILayout.Button( string.IsNullOrEmpty( directory ) ? "Browse..." : directory, EditorStyles.toolbarPopup, GUILayout.Width( width ) ) )
            {
                string path = EditorUtility.OpenFolderPanel( title, directory, "Package Folder");

                if ( path.Length > 0 && Directory.Exists( path ) )
                {
                    OrganizerPaths.SetPackageDirectory( location, path );               
                    m_requiresBrowserUpdate = true;
                }
            }
        }

        private void GuiBrowserBlock()
        {
            // Row - Column Names
            using ( new EditorGUILayout.HorizontalScope() )
            {
                GUILayout.Space( 48f );
                GuiColumnHeaderButton( "Package Name", PackageFilters.Category.Title, EditorGUIStyles.LabelNormLeft, EditorGUIStyles.LabelBoldLeft, s_columnWidths[ 0 ] );
                GuiColumnHeaderButton( "Archived", PackageFilters.Category.Archived, EditorGUIStyles.LabelNormLeft, EditorGUIStyles.LabelBoldLeft, s_columnWidths[ 8 ] );
                GuiColumnHeaderButton( "Version", PackageFilters.Category.Version, EditorGUIStyles.LabelNormLeft, EditorGUIStyles.LabelBoldLeft, s_columnWidths[ 3 ] );
                GuiColumnHeaderButton( "Unity", PackageFilters.Category.UnityVersion, EditorGUIStyles.LabelNormLeft, EditorGUIStyles.LabelBoldLeft, s_columnWidths[ 1 ] );
                GuiColumnHeaderButton( "Size", PackageFilters.Category.Size, EditorGUIStyles.LabelNormRight, EditorGUIStyles.LabelBoldRight, s_columnWidths[ 2 ] );
                GuiColumnHeaderButton( "Downloaded", PackageFilters.Category.ModDate, EditorGUIStyles.LabelNormRight, EditorGUIStyles.LabelBoldRight, s_columnWidths[ 4 ] );
                GuiColumnHeaderButton( "Published", PackageFilters.Category.PubDate, EditorGUIStyles.LabelNormRight, EditorGUIStyles.LabelBoldRight, s_columnWidths[ 5 ] );
                GuiColumnHeaderButton( "Category", PackageFilters.Category.Category, EditorGUIStyles.LabelNormLeft, EditorGUIStyles.LabelBoldLeft, s_columnWidths[ 6 ] );
                GuiColumnHeaderButton( "Publisher", PackageFilters.Category.Publisher, EditorGUIStyles.LabelNormLeft, EditorGUIStyles.LabelBoldLeft, s_columnWidths[ 7 ] );
            }

            // Results Scroll View
            m_scrollPosition  = EditorGUILayout.BeginScrollView( m_scrollPosition );
            bool lightStyle = true;

            foreach ( AssetPackage uasp in m_sortedLibrary )
            {
                GuiStorePackageItem( uasp, lightStyle ? EditorGUIStyles.ScopeLight : EditorGUIStyles.ScopeDark );
                lightStyle = !lightStyle;
            }

            EditorGUILayout.Space();
            EditorGUILayout.EndScrollView();
        }

        private void GuiStorePackageItem( AssetPackage ap, GUIStyle scope )
        {
            GUI.contentColor = ap.IsUnityStandardAsset ? EditorGUIStyles.ColorContentWarning : EditorGUIStyles.ColorContentNormal;

            using ( new EditorGUILayout.HorizontalScope( scope, GUILayout.Height( s_lineHeight ) ) )
            {
                if ( GUILayout.Button( s_iconImportContent, EditorGUIStyles.FullButton, GUILayout.Height( s_lineHeight ), GUILayout.Width( 32f ) ) )
                    OrganizerMethods.ImportPackage( ap.FullFilePath );

                GUILayout.Space( 4f );

                // Column Entries
                GUI.contentColor = ap.IsUnityStandardAsset ? EditorGUIStyles.ColorContentWarning : EditorGUIStyles.ColorTextNormal;
                GuiCustomMenu ( ap, EditorGUIStyles.LabelNormLeft, s_columnWidths[ 0 ] );
                GUI.contentColor = ap.IsArchived ? EditorGUIStyles.ColorContentGreen : EditorGUIStyles.ColorContentNormal;
                EditorGUILayout.LabelField( ap.IsArchived ? s_iconCheckContent : s_iconCancelContent, s_iconLabelStyle, GUILayout.Height( s_lineHeight ), GUILayout.Width( s_columnWidths[ 8 ] ) );
                GUILayout.Space( 12f );
                GUI.contentColor = ap.IsUnityStandardAsset ? EditorGUIStyles.ColorContentWarning : EditorGUIStyles.ColorTextNormal;
                GuiCustomLabel( ap.version, EditorGUIStyles.LabelNormLeft, s_columnWidths[ 3 ] );
                GuiCustomLabel( ap.unity_version, EditorGUIStyles.LabelNormLeft, s_columnWidths[ 1 ] );
                GuiCustomLabel( ap.DisplayFileSize, EditorGUIStyles.LabelNormRight, s_columnWidths[ 2 ] );
                GuiCustomLabel( ap.DisplayModifiedDate, EditorGUIStyles.LabelNormRight, s_columnWidths[ 4 ] );
                GuiCustomLabel( ap.pubdate, EditorGUIStyles.LabelNormRight, s_columnWidths[ 5 ] );
                GuiCustomLabel( ap.category.label, EditorGUIStyles.LabelNormLeft, s_columnWidths[ 6 ] );
                GuiCustomLabel( ap.publisher.label, EditorGUIStyles.LabelNormLeft, s_columnWidths[ 7 ] );
                // GuiCustomLabel( uasp.category.label,  urlCategoryQuery,	uasp.category.id,		styleLeft,	columnWidths[6] ); // Obsolete - doesn't work with new website
                // GuiCustomLabel( uasp.publisher.label, urlPublisherQuery, uasp.publisher.label, styleLeft, columnWidths[ 7 ] );                
            }

            GUI.contentColor = EditorGUIStyles.ColorContentNormal;
        }
#endregion


        private void GuiColumnHeaderButton( string name, PackageFilters.Category column, GUIStyle normalStyle, GUIStyle boldStyle, float width )
        {
            GUIStyle style  = m_activeColumn == column ? boldStyle : normalStyle;
            bool clicked    = GUILayout.Button( name, style, GUILayout.MinWidth( width ), GUILayout.MaxWidth( width ) );
            GUILayout.Space( 12f );
                        
            if ( clicked )
            {
                // Toggle sorting order direction if previous column is clicked again.
                if ( m_activeColumn == column )
                    m_packageFilters.OrderByAscending = !m_packageFilters.OrderByAscending;

                m_activeColumn            = column;
                m_requiresFilterUpdate    = true;
            }
        }

        private void GuiCustomLabel( string labelText, GUIStyle style, float width )
        {
            EditorGUILayout.LabelField( labelText, style, GUILayout.Height( s_lineHeight ), GUILayout.MinWidth( width ), GUILayout.MaxWidth( width ) );
            GUILayout.Space( 12f );
        }

        [Obsolete("No longer used, instead this is done through popup menu")]
        private void GuiCustomLabel( string labelText, string url, string id, GUIStyle style, float width )
        {
            if ( GUILayout.Button( labelText, style, GUILayout.Height( s_lineHeight ), GUILayout.MinWidth( width ), GUILayout.MaxWidth( width ) ) && id != "NA" )
            {
                if ( Event.current.button == 0 )
                    Application.OpenURL( url + id );
                else
                    AssetStore.Open( string.Format( "content/{0}", id ) );
            }
            GUILayout.Space( 12f );
        }


#region CUSTOM POPUP MENU
        private void GuiCustomMenu( AssetPackage ap, GUIStyle style, float width)
        {
            if ( GUILayout.Button( ap.title, style, GUILayout.Height( s_lineHeight ), GUILayout.MinWidth( width ), GUILayout.MaxWidth( width ) ) )
            {                
                bool hasLinkID      = (ap.link.id      != "NA");
                bool hasPublisherID = (ap.publisher.id != "NA");
                bool hasCategoryID  = (ap.category.id  != "NA");

                GenericMenu menu = new GenericMenu();
                menu.AddItem( new GUIContent( "Import Package" ), false, OnMenuImportPackage, ap );
                menu.AddItem( new GUIContent( "Embed Package" ), false, OnMenuEmbedPackage, ap );
                MenuItemDeletePackage( menu, ap );
                MenuItemCopyPackage( menu, ap);
                menu.AddSeparator( "" );
                menu.AddItem( new GUIContent( "Show In Explorer" ), false, OnMenuShowInExplorer, ap );
                menu.AddItem( new GUIContent( "Log Package Information" ), false, OnMenuLogPackageInfo, ap );
                menu.AddItem( new GUIContent( "Log Archive Status" ), false, OnMenuLogArchiveStatus, ap );
                menu.AddSeparator( "" );

                if ( hasLinkID || hasPublisherID || hasCategoryID )
                {
                    if ( hasLinkID )
                        menu.AddItem( new GUIContent( "Show Internet Browser/View Asset" ), false, OnMenuAssetPage, ap );
                    if ( hasPublisherID )
                        menu.AddItem( new GUIContent( "Show Internet Browser/View Publisher" ), false, OnMenuPublisherPage, ap );
                    if ( hasCategoryID )
                        menu.AddItem( new GUIContent( "Show Internet Browser/View Category" ), false, OnMenuCategoryPage, ap );

                    if ( hasLinkID )
                        menu.AddItem( new GUIContent( "Show AssetStore Tab/View Asset" ), false, OnMenuAssetWindow, ap );
                    if ( hasPublisherID )
                        menu.AddItem( new GUIContent( "Show AssetStore Tab/View Publisher" ), false, OnMenuPublisherWindow, ap );
                    if ( hasCategoryID )
                        menu.AddItem( new GUIContent( "Show AssetStore Tab/View Category" ), false, OnMenuCategoryWindow, ap );
                }

                menu.ShowAsContext();
            }

            GUILayout.Space( 12f );
        }

        /// <summary>Location dependant MenuItem for coping to/from either AssetStore or Archive based on location being shown.</summary>
        private void MenuItemCopyPackage( GenericMenu menu, AssetPackage sp )
        {
            switch ( m_sourceLocation )
            {
                case PackagesLocation.NativePackageList:
                case PackagesLocation.AssetStore:
                case PackagesLocation.AssetStore5x:
                    menu.AddItem( new GUIContent( "Archive Package" ), false, OnMenuArchivePackage, sp );
                    break;
                case PackagesLocation.Custom:
                case PackagesLocation.Archive:
                    menu.AddItem( new GUIContent( "Restore Package To Asset Store" ), false, OnMenuRestorePackage, sp );
                    break;
                default:
                    menu.AddDisabledItem( new GUIContent( "Unknown Package Location" ) );
                    break;
            }
        }

        /// <summary>Location dependant MenuItem for deleting the package from current location.</summary>
        private void MenuItemDeletePackage( GenericMenu menu, AssetPackage sp )
        {
            switch ( m_sourceLocation )
            {
                case PackagesLocation.NativePackageList:
                case PackagesLocation.AssetStore:
                case PackagesLocation.AssetStore5x:
                    menu.AddItem( new GUIContent( "Delete from Asset Store Cache" ), false, OnMenuDeletePackage, sp );
                    break;
                case PackagesLocation.Custom:
                    menu.AddItem( new GUIContent( "Delete from Custom Cache Location" ), false, OnMenuDeletePackage, sp );
                    break;
                case PackagesLocation.Archive:
                    menu.AddItem( new GUIContent( "Delete from Archived Location" ), false, OnMenuDeletePackage, sp );
                    break;
                default:
                    menu.AddDisabledItem( new GUIContent( "Unknown Package Location" ) );
                    break;
            }
        }

        
        private static readonly string s_deletionMessageFormat  = "Are you sure you want to delete the package\n{0}\nfrom\n{1}";
        private static readonly string s_archiveMessageFormat   = "Are you sure you want to archive the package\n{0}\nto\n{1}";


        private string GetDeletionMessage( string title )
        {
            switch ( m_sourceLocation )
            {
                case PackagesLocation.NativePackageList:
                case PackagesLocation.AssetStore:
                case PackagesLocation.AssetStore5x:
                    return string.Format( s_deletionMessageFormat, title, "Asset Store Cache" );
                case PackagesLocation.Custom:
                    return string.Format( s_deletionMessageFormat, title, "Custom Cache Location" );
                case PackagesLocation.Archive:
                    return string.Format( s_deletionMessageFormat, title, "Archived Location" );
            }

            return "Unkown";
        }


        private void OnMenuImportPackage( object sp ) 
        {
            AssetPackage pack = ( AssetPackage )sp;
            OrganizerMethods.ImportPackage( pack.FullFilePath );            
            Debug.LogFormat("StorePackageLibraryEditor: Imported Package - [{0}] {1}", pack.version, pack.FullFilePath);
        }

        private void OnMenuEmbedPackage( object sp ) 
        {
            AssetPackage pack = ( AssetPackage )sp;
            OrganizerMethods.EmbedPackage( pack.FullFilePath );
            Debug.LogFormat("StorePackageLibraryEditor: Embed Package - [{0}] {1}", pack.version, pack.FullFilePath);
        }
                
        private void OnMenuDeletePackage( object sp )
        {
            AssetPackage package = ( AssetPackage )sp;

            string message       = GetDeletionMessage( package.title );
                       
            if ( EditorUtility.DisplayDialog( "Delete Selected Package", message, "OK", "Cancel" ) )
            {
                File.Delete( package.FullFilePath );
                m_requiresBrowserUpdate = true;
            }
        }

        private void OnMenuArchivePackage( object sp ) 
        {
            AssetPackage package = ( AssetPackage )sp;

            string message = string.Format( s_archiveMessageFormat, package.title, OrganizerPaths.StoreDirectoryBackup );

            if ( EditorUtility.DisplayDialog( "Archive Selected Package", message,  "OK", "Cancel" ) )
            {
                Archiver.ArchiveAssetStorePackages( new List<AssetPackage>() { package }, m_archiveLibrary, m_sourceLocation, !DisableFileOperations );              
                m_requiresBrowserUpdate = true;
            }
        }

        private void OnMenuRestorePackage( object sp ) 
        {
            AssetPackage package    = ( AssetPackage )sp;
            m_requiresBrowserUpdate   = Archiver.RestoreArchivePackageToAssetStore( package, m_sourceLocation, !DisableFileOperations );
        }
                        
        private void OnMenuShowInExplorer( object sp )      { OrganizerMethods.OpenPackageInExplorer( ( ( AssetPackage )sp ).FullFilePath ); }
        private void OnMenuLogPackageInfo( object sp )      { Debug.Log( ( ( AssetPackage )sp ).ToString() ); }
        private void OnMenuLogArchiveStatus( object sp )    { Archiver.LogPackageArchiveStatus( ( AssetPackage )sp, m_archiveLibrary ); }

        private void OnMenuAssetPage( object sp )
        {
            AssetPackage ap =  ( AssetPackage )sp;
            Application.OpenURL( string.Format( m_urlAssetTitleQuery, ap.category.label, ap.title, ap.link.id ) );
        }

        private void OnMenuPublisherPage( object sp )       { Application.OpenURL( m_urlPublisherQuery + ( ( AssetPackage )sp ).publisher.label ); } //.id); }
        private void OnMenuCategoryPage( object sp )        { Application.OpenURL( m_urlCategoryQuery + ( ( AssetPackage )sp ).category.id ); }
        private void OnMenuAssetWindow( object sp )         { AssetStore.Open( string.Format( "content/{0}", ( ( AssetPackage )sp ).link.id ) ); }
        private void OnMenuPublisherWindow( object sp )     { AssetStore.Open( string.Format( "content/{0}", ( ( AssetPackage )sp ).publisher.id ) ); }
        private void OnMenuCategoryWindow( object sp )      { AssetStore.Open( string.Format( "content/{0}", ( ( AssetPackage )sp ).category.id ) ); }
#endregion
    }
}
