// #define LOG_UNITY_METHODS

using System;
using System.Collections.Generic;
using System.IO;
using NoiseCrimeStudios.Core.Formatting;
using NoiseCrimeStudios.Editor;
using NoiseCrimeStudios.Editor.IMGUI;
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
        private PackageLibrary          sourceLibrary           = new PackageLibrary();
        private PackageLibrary          archiveLibrary          = new PackageLibrary();
        private PackageFilters          packageFilters          = new PackageFilters();
        private List<AssetPackage>      sortedLibrary           = new List<AssetPackage>();

        private PackagesLocation        sourceLocation          = PackagesLocation.NativePackageList;
      
        private PackageFilters.Category activeColumn            = PackageFilters.Category.Title;
        private string                  packageInfoCount;		
        private readonly string[]		locationNameArray       = Enum.GetNames( typeof( PackagesLocation ) );

        // GUI
        private Vector2                 scrollPosition;        
        private Rect                    optionsButtonRect;
        private static float[]          columnWidths            = new float[]{280f, 72f, 64f, 64f, 88f, 88f, 256f, 224f, 64f};        
        private static float            lineHeight              = 24f;
        private readonly string         materialIconPath        = "Assets/NoiseCrimeStudios/Editor/Resources/GoogleMaterialIcons";       
        
        private readonly string         urlPublisherQuery       = @"https://assetstore.unity.com/?publisher=";
        private readonly string         urlCategoryQuery        = @"https://assetstore.unity.com/?category=";
        private readonly string         urlAssetTitleQuery      = @"https://assetstore.unity.com/packages/vfx/{0}/{1}-{2}";

        // e.g. https://assetstore.unity.com/packages/vfx/shaders/fullscreen-camera-effects/amplify-color-1894

        // Old Site URL's - No longer work
        // private readonly string      urlPublisherQuery       = @"https://www.assetstore.unity3d.com/en/#!/search/page=1/sortby=popularity/query=publisher:";
        // private readonly string      urlCategoryQuery        = @"https://www.assetstore.unity3d.com/en/#!/search/page=1/sortby=popularity/query=category:";
        // private readonly string      urlAssetTitleQuery      = @"https://www.assetstore.unity3d.com/en/#!/content/";
        
        private static Texture          iconImport;
        private static GUIContent       iconImportContent;
        private static Texture          iconCancel;
        private static GUIContent       iconCancelContent;
        private static Texture          iconCheck;
        private static GUIContent       iconCheckContent;
        private static GUIContent       reloadContent;

        private static GUIStyle         toolbarLabel;
        private static GUIStyle         iconLabelStyle;
        
        private bool requiresBrowserUpdate  = false;
        private bool requiresFilterUpdate   = false;

        [SerializeField]
        public  bool disableFileOperations  = false;

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

        void OnEnable()
        {
#if LOG_UNITY_METHODS
            Debug.Log( "StorePackageLibraryEditor: OnEnable" );
#endif
            activeColumn                    = PackageFilters.Category.Title;
            packageFilters.OrderByAscending = true;
            requiresBrowserUpdate           = false;
            requiresFilterUpdate            = false;
                
            UpdateLibraries();
        }

        void OnDisable()
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

        private void UpdateLibraries()
        {
#if LOG_UNITY_METHODS
            Debug.Log( "StorePackageLibraryEditor: InitialiseBrowser" );
#endif
            // Reset Flag
            requiresBrowserUpdate = false;

            // Populate Package Library Content
            sourceLibrary.PopulateLibraryContent( sourceLocation );
            // Populate Package Archive Content
            archiveLibrary.PopulateLibraryContent( PackagesLocation.Archive );            
            // Check Against Backups
            sourceLibrary.CompareAgainstArchive( archiveLibrary );

            // TODO
            // These filters only apply to sourceLibrary not archiveLibrary!!
            // How can we keep filters if reloading libraries?

            // Precompile filters
            packageFilters.ConstructFilters( sourceLibrary.packageLibrary );
            // Apply Sorting
            UpdateFilterSortResults();
        }

        private void UpdateFilterSortResults()
        {
#if LOG_UNITY_METHODS
            Debug.Log( "StorePackageLibraryEditor: UpdateFilterSortResults" );
#endif
            // Reset Flag
            requiresFilterUpdate    = false;

            // Get Sort & Filtered Result
            sortedLibrary     = packageFilters.SortAndFilterResults( sourceLibrary.packageLibrary, activeColumn );

            long sortedSizeOnDisk = 0;
            foreach( AssetPackage p in sortedLibrary )
                sortedSizeOnDisk += p.fileSize;

            packageInfoCount    = string.Format( "Size: {0} of {1}   Packages: {2} of {3}", Numerical.ByteCountToSuffixHumbads( sortedSizeOnDisk ), sourceLibrary.SizeOnDisk, sortedLibrary.Count, sourceLibrary.FileCount );            
        }

        /// <summary>Archives the source location packages based on curent sorting and filtering results</summary>
        public void StartArchiveSourceLibraryProcess()
        {
            Archiver.ArchiveAssetStorePackages( sortedLibrary, archiveLibrary, sourceLocation, !disableFileOperations );
            requiresBrowserUpdate = true;
        }

        #region GUI METHODS
        void SetUpStyles()
        {
            reloadContent = EditorGUIUtility.IconContent("d_Refresh"); // d_Refresh or d_RotateTool

            EditorGUIStyles.CreateIconContent( ref iconImport, ref iconImportContent,   materialIconPath, "{0}/ic_file_download_{1}_18dp_1x.png", "Import Package" );
            EditorGUIStyles.CreateIconContent( ref iconCancel, ref iconCancelContent,   materialIconPath, "{0}/baseline_cancel_{1}_18dp_1x.png", "Not In Archive" );
            EditorGUIStyles.CreateIconContent( ref iconCheck,  ref iconCheckContent,    materialIconPath, "{0}/baseline_check_circle_{1}_18dp_1x.png", "In Archive" );
            
            if ( null == toolbarLabel )
            {
                toolbarLabel            = new GUIStyle( EditorStyles.toolbar );
                toolbarLabel.alignment  = TextAnchor.MiddleLeft;
            }

            if ( null == iconLabelStyle )
            {
                iconLabelStyle              = new GUIStyle( "Label" );
                iconLabelStyle.padding      = new RectOffset( 2, 2, 2, 2);
                iconLabelStyle.alignment    = TextAnchor.MiddleCenter;
            }            
        }

        protected virtual void OnGUI()
        {
            SetUpStyles();
            
            if ( requiresBrowserUpdate ) UpdateLibraries();
            if ( requiresFilterUpdate ) UpdateFilterSortResults();

            float prevLabelWidth    = EditorGUIUtility.labelWidth;
            requiresBrowserUpdate   = false;
            requiresFilterUpdate    = false;

            GuiPackageTopBar();
            GUILayout.Space(8f);
            GuiBrowserBlock();

            EditorGUIUtility.labelWidth = prevLabelWidth;
        }


        void GuiPackageTopBar()
        {
            EditorGUIUtility.labelWidth = 64f;

            // Top Line
            using ( new EditorGUILayout.HorizontalScope( EditorStyles.toolbar ) )
            {
                if ( GUILayout.Button( reloadContent, EditorStyles.toolbarButton, GUILayout.Width( 32f )) )                
                    requiresBrowserUpdate = true;

                // Show Package Location Popup
                GUI.changed = false;
                sourceLocation = ( PackagesLocation )EditorGUILayout.Popup( ( int )sourceLocation, locationNameArray, EditorStyles.toolbarPopup, GUILayout.MinWidth( 256f - 40f) );
                requiresBrowserUpdate = GUI.changed;
                
                float locationWidth = 552f - 4f;
                
                GUILayout.Space(8f);

                switch ( sourceLocation )
                {
                    case PackagesLocation.NativePackageList:                      
                        GUILayout.Label( OrganizerPaths.StoreDirectoryRoot, toolbarLabel, GUILayout.Width( locationWidth ) );
                        break;
                    case PackagesLocation.AssetStore:
                        GUILayout.Label( OrganizerPaths.StoreDirectoryLegacy, toolbarLabel, GUILayout.Width( locationWidth ) ); 
                        break;
                    case PackagesLocation.AssetStore5x:
                        GUILayout.Label( OrganizerPaths.StoreDirectoryModern, toolbarLabel, GUILayout.Width( locationWidth ) ); 
                        break;
                    case PackagesLocation.Custom:
                        GuiPackageLocationButton( PackagesLocation.Custom, "Browse for custom asset store folder", locationWidth );
                        break;
                    case PackagesLocation.Archive:
                        GuiPackageLocationButton( PackagesLocation.Archive, "Browse for archive asset store folder", locationWidth );
                        break;
                }
                                
                GUILayout.FlexibleSpace();              

                if ( GUILayout.Button( "Archive Options", EditorGUIStyles.labelNormRight, GUILayout.MinWidth( 256f ), GUILayout.MaxWidth( 256f ) ) )
                    PopupWindow.Show( optionsButtonRect, new ArchiverOptionsPopup( this ) );

                if ( Event.current.type == EventType.Repaint ) optionsButtonRect = GUILayoutUtility.GetLastRect();        
            }

            // Second Line
            using ( new EditorGUILayout.HorizontalScope( EditorStyles.toolbar ) )
            {
                GUI.changed = false;
                packageFilters.GuiSearchBar();
                requiresFilterUpdate = GUI.changed;
                GUILayout.FlexibleSpace();
                EditorGUILayout.LabelField( packageInfoCount, EditorGUIStyles.labelNormRight, GUILayout.MinWidth( 320f ), GUILayout.MaxWidth( 320f ) );
            }
        }

        void GuiPackageLocationButton( PackagesLocation location, string title,  float width )
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
                    requiresBrowserUpdate = true;
                }
            }
        }

        void GuiBrowserBlock()
        {
            // Row - Column Names
            using ( new EditorGUILayout.HorizontalScope() )
            {
                GUILayout.Space( 48f );
                GuiColumnHeaderButton( "Package Name", PackageFilters.Category.Title, EditorGUIStyles.labelNormLeft, EditorGUIStyles.labelBoldLeft, columnWidths[ 0 ] );
                GuiColumnHeaderButton( "Archived", PackageFilters.Category.Archived, EditorGUIStyles.labelNormLeft, EditorGUIStyles.labelBoldLeft, columnWidths[ 8 ] );
                GuiColumnHeaderButton( "Version", PackageFilters.Category.Version, EditorGUIStyles.labelNormLeft, EditorGUIStyles.labelBoldLeft, columnWidths[ 3 ] );
                GuiColumnHeaderButton( "Unity", PackageFilters.Category.UnityVersion, EditorGUIStyles.labelNormLeft, EditorGUIStyles.labelBoldLeft, columnWidths[ 1 ] );
                GuiColumnHeaderButton( "Size", PackageFilters.Category.Size, EditorGUIStyles.labelNormRight, EditorGUIStyles.labelBoldRight, columnWidths[ 2 ] );
                GuiColumnHeaderButton( "Downloaded", PackageFilters.Category.ModDate, EditorGUIStyles.labelNormRight, EditorGUIStyles.labelBoldRight, columnWidths[ 4 ] );
                GuiColumnHeaderButton( "Published", PackageFilters.Category.PubDate, EditorGUIStyles.labelNormRight, EditorGUIStyles.labelBoldRight, columnWidths[ 5 ] );
                GuiColumnHeaderButton( "Category", PackageFilters.Category.Category, EditorGUIStyles.labelNormLeft, EditorGUIStyles.labelBoldLeft, columnWidths[ 6 ] );
                GuiColumnHeaderButton( "Publisher", PackageFilters.Category.Publisher, EditorGUIStyles.labelNormLeft, EditorGUIStyles.labelBoldLeft, columnWidths[ 7 ] );
            }

            // Results Scroll View
            scrollPosition  = EditorGUILayout.BeginScrollView( scrollPosition );
            bool lightStyle = true;

            foreach ( AssetPackage uasp in sortedLibrary )
            {
                GuiStorePackageItem( uasp, lightStyle ? EditorGUIStyles.scopeLight : EditorGUIStyles.scopeDark );
                lightStyle = !lightStyle;
            }

            EditorGUILayout.Space();
            EditorGUILayout.EndScrollView();
        }

        private void GuiStorePackageItem( AssetPackage ap, GUIStyle scope )
        {
            GUI.contentColor = ap.isUnityStandardAsset ? EditorGUIStyles.colorContentWarning : EditorGUIStyles.colorContentNormal;

            using ( new EditorGUILayout.HorizontalScope( scope, GUILayout.Height( lineHeight ) ) )
            {
                if ( GUILayout.Button( iconImportContent, EditorGUIStyles.fullButton, GUILayout.Height( lineHeight ), GUILayout.Width( 32f ) ) )
                    OrganizerMethods.ImportPackage( ap.fullFilePath );

                GUILayout.Space( 4f );

                // Column Entries
                GUI.contentColor = ap.isUnityStandardAsset ? EditorGUIStyles.colorContentWarning : EditorGUIStyles.colorTextNormal;
                GuiCustomMenu ( ap, EditorGUIStyles.labelNormLeft, columnWidths[ 0 ] );
                GUI.contentColor = ap.isArchived ? EditorGUIStyles.colorContentGreen : EditorGUIStyles.colorContentNormal;
                EditorGUILayout.LabelField( ap.isArchived ? iconCheckContent : iconCancelContent, iconLabelStyle, GUILayout.Height( lineHeight ), GUILayout.Width( columnWidths[ 8 ] ) );
                GUILayout.Space( 12f );
                GUI.contentColor = ap.isUnityStandardAsset ? EditorGUIStyles.colorContentWarning : EditorGUIStyles.colorTextNormal;
                GuiCustomLabel( ap.version, EditorGUIStyles.labelNormLeft, columnWidths[ 3 ] );
                GuiCustomLabel( ap.unity_version, EditorGUIStyles.labelNormLeft, columnWidths[ 1 ] );
                GuiCustomLabel( ap.displayFileSize, EditorGUIStyles.labelNormRight, columnWidths[ 2 ] );
                GuiCustomLabel( ap.displayModifiedDate, EditorGUIStyles.labelNormRight, columnWidths[ 4 ] );
                GuiCustomLabel( ap.pubdate, EditorGUIStyles.labelNormRight, columnWidths[ 5 ] );
                GuiCustomLabel( ap.category.label, EditorGUIStyles.labelNormLeft, columnWidths[ 6 ] );
                GuiCustomLabel( ap.publisher.label, EditorGUIStyles.labelNormLeft, columnWidths[ 7 ] );
                // GuiCustomLabel( uasp.category.label,  urlCategoryQuery,	uasp.category.id,		styleLeft,	columnWidths[6] ); // Obsolete - doesn't work with new website
                // GuiCustomLabel( uasp.publisher.label, urlPublisherQuery, uasp.publisher.label, styleLeft, columnWidths[ 7 ] );                
            }

            GUI.contentColor = EditorGUIStyles.colorContentNormal;
        }
#endregion


        private void GuiColumnHeaderButton( string name, PackageFilters.Category column, GUIStyle normalStyle, GUIStyle boldStyle, float width )
        {
            GUIStyle style  = activeColumn == column ? boldStyle : normalStyle;
            bool clicked    = GUILayout.Button( name, style, GUILayout.MinWidth( width ), GUILayout.MaxWidth( width ) );
            GUILayout.Space( 12f );
                        
            if ( clicked )
            {
                // Toggle sorting order direction if previous column is clicked again.
                if ( activeColumn == column ) packageFilters.OrderByAscending = !packageFilters.OrderByAscending;

                activeColumn            = column;
                requiresFilterUpdate    = true;
            }
        }

        private void GuiCustomLabel( string labelText, GUIStyle style, float width )
        {
            EditorGUILayout.LabelField( labelText, style, GUILayout.Height( lineHeight ), GUILayout.MinWidth( width ), GUILayout.MaxWidth( width ) );
            GUILayout.Space( 12f );
        }

        [Obsolete("No longer used, instead this is done through popup menu")]
        private void GuiCustomLabel( string labelText, string url, string id, GUIStyle style, float width )
        {
            if ( GUILayout.Button( labelText, style, GUILayout.Height( lineHeight ), GUILayout.MinWidth( width ), GUILayout.MaxWidth( width ) ) && id != "NA" )
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
            if ( GUILayout.Button( ap.title, style, GUILayout.Height( lineHeight ), GUILayout.MinWidth( width ), GUILayout.MaxWidth( width ) ) )
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
                    if ( hasLinkID ) menu.AddItem( new GUIContent( "Show Internet Browser/View Asset" ), false, OnMenuAssetPage, ap );
                    if ( hasPublisherID ) menu.AddItem( new GUIContent( "Show Internet Browser/View Publisher" ), false, OnMenuPublisherPage, ap );
                    if ( hasCategoryID ) menu.AddItem( new GUIContent( "Show Internet Browser/View Category" ), false, OnMenuCategoryPage, ap );

                    if ( hasLinkID ) menu.AddItem( new GUIContent( "Show AssetStore Tab/View Asset" ), false, OnMenuAssetWindow, ap );
                    if ( hasPublisherID ) menu.AddItem( new GUIContent( "Show AssetStore Tab/View Publisher" ), false, OnMenuPublisherWindow, ap );
                    if ( hasCategoryID ) menu.AddItem( new GUIContent( "Show AssetStore Tab/View Category" ), false, OnMenuCategoryWindow, ap );
                }

                menu.ShowAsContext();
            }

            GUILayout.Space( 12f );
        }

        /// <summary>Location dependant MenuItem for coping to/from either AssetStore or Archive based on location being shown.</summary>
        private void MenuItemCopyPackage( GenericMenu menu, AssetPackage sp )
        {  
            switch ( sourceLocation )
            {
                case PackagesLocation.NativePackageList:
                case PackagesLocation.AssetStore:
                case PackagesLocation.AssetStore5x:
                    menu.AddItem( new GUIContent( "Archive Package" ), false, OnMenuArchivePackage, sp);
                    break;            
                case PackagesLocation.Custom:
                case PackagesLocation.Archive:
                    menu.AddItem( new GUIContent( "Restore Package To Asset Store" ), false, OnMenuRestorePackage, sp);  
                    break;
                default:
                    menu.AddDisabledItem( new GUIContent("Unknown Package Location") );
                    break;
            }
        }

        /// <summary>Location dependant MenuItem for deleting the package from current location.</summary>
        private void MenuItemDeletePackage( GenericMenu menu, AssetPackage sp )
        {
            switch ( sourceLocation )
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
                    menu.AddDisabledItem( new GUIContent("Unknown Package Location") );
                    break;
            }
        }

        
        private static readonly string deletionMessageFormat   = "Are you sure you want to delete the package\n{0}\nfrom\n{1}";
        private static readonly string kArchiveMessageFormat   = "Are you sure you want to archive the package\n{0}\nto\n{1}";
        

        private string GetDeletionMessage( string title )
        {
            switch ( sourceLocation )
            {
                case PackagesLocation.NativePackageList:
                case PackagesLocation.AssetStore:
                case PackagesLocation.AssetStore5x:
                    return string.Format( deletionMessageFormat, title, "Asset Store Cache" );
                case PackagesLocation.Custom:
                    return string.Format( deletionMessageFormat, title, "Custom Cache Location" );
                case PackagesLocation.Archive:
                    return string.Format( deletionMessageFormat, title, "Archived Location" );
            }

            return "Unkown";
        }


        void OnMenuImportPackage( object sp ) 
        {
            AssetPackage pack = ( AssetPackage )sp;
            OrganizerMethods.ImportPackage( pack.fullFilePath );            
			Debug.LogFormat("StorePackageLibraryEditor: Imported Package - [{0}] {1}", pack.version, pack.fullFilePath);
        }

        void OnMenuEmbedPackage( object sp ) 
        {
            AssetPackage pack = ( AssetPackage )sp;
            OrganizerMethods.EmbedPackage( pack.fullFilePath );
            Debug.LogFormat("StorePackageLibraryEditor: Embed Package - [{0}] {1}", pack.version, pack.fullFilePath);
        }
                
        void OnMenuDeletePackage( object sp )
        {
            AssetPackage package = ( AssetPackage )sp;

            string message       = GetDeletionMessage( package.title );
                       
            if ( EditorUtility.DisplayDialog( "Delete Selected Package", message, "OK", "Cancel" ) )
            {
                File.Delete( package.fullFilePath );
                requiresBrowserUpdate = true;
            }
        }

        void OnMenuArchivePackage( object sp ) 
        {
            AssetPackage package = ( AssetPackage )sp;

            string message = string.Format( kArchiveMessageFormat, package.title, OrganizerPaths.StoreDirectoryBackup );

            if ( EditorUtility.DisplayDialog( "Archive Selected Package", message,  "OK", "Cancel" ) )
            {
                Archiver.ArchiveAssetStorePackages( new List<AssetPackage>() { package }, archiveLibrary, sourceLocation, !disableFileOperations );              
                requiresBrowserUpdate = true;
            }
        }

        void OnMenuRestorePackage( object sp ) 
        {
            AssetPackage package    = ( AssetPackage )sp;
            requiresBrowserUpdate   = Archiver.RestoreArchivePackageToAssetStore( package, sourceLocation, !disableFileOperations );
        }
                        
        void OnMenuShowInExplorer( object sp )      { OrganizerMethods.OpenPackageInExplorer( ( ( AssetPackage )sp ).fullFilePath ); }
        void OnMenuLogPackageInfo( object sp )      { Debug.Log( ( ( AssetPackage )sp ).ToString() ); }
        void OnMenuLogArchiveStatus( object sp )    { Archiver.LogPackageArchiveStatus( ( AssetPackage )sp, archiveLibrary ); }

        void OnMenuAssetPage( object sp )
        {
            AssetPackage ap =  ( AssetPackage )sp;
            Application.OpenURL( string.Format( urlAssetTitleQuery, ap.category.label, ap.title, ap.link.id ) );
        }
        void OnMenuPublisherPage( object sp )       { Application.OpenURL( urlPublisherQuery + ( ( AssetPackage )sp ).publisher.label ); } //.id); }
        void OnMenuCategoryPage( object sp )        { Application.OpenURL( urlCategoryQuery + ( ( AssetPackage )sp ).category.id ); }

        void OnMenuAssetWindow( object sp )         { AssetStore.Open( string.Format( "content/{0}", ( ( AssetPackage )sp ).link.id ) ); }
        void OnMenuPublisherWindow( object sp )     { AssetStore.Open( string.Format( "content/{0}", ( ( AssetPackage )sp ).publisher.id ) ); }
        void OnMenuCategoryWindow( object sp )      { AssetStore.Open( string.Format( "content/{0}", ( ( AssetPackage )sp ).category.id ) ); }
#endregion
	}
}
