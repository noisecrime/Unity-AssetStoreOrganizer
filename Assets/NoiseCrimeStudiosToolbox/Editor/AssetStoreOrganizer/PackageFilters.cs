using System;
using System.Collections.Generic;
using System.Linq;
using NoiseCrimeStudios.Core;
using UnityEditor;
using UnityEngine;
using UnityEngine.Assertions;

namespace NoiseCrimeStudios.Toolbox.AssetStoreOrganizer
{

    /// <summary>Creates string arrays to be used in dropdowns for filtering by category, publisher, Unity version.</summary>
    public class PackageFilters
    {
        public enum Category
        {
            None,
            Title,
            UnityVersion,
            Version,
            ModDate,
            PubDate,
            Size,
            Publisher,
            Category,
            Archived
        }
        
        private readonly string		m_invalidFilter = "!";

        private string      m_dynamicSearchText;

        // Filters
        private int         m_categoryFilterIndex     = 0;
        private int         m_publisherFilterIndex    = 0;
        private int         m_versionFilterIndex      = 0;
        
        private string[]    m_categoryFilterNames;
        private string[]    m_publisherFilterNames;
        private string[]    m_versionFilterNames;
        
        private GUIStyle    m_toolbarSeachTextField;
        private GUIStyle    m_toolbarSeachCancelButton;

        public  bool        OrderByAscending    { get; set; }
        public  Category    PrimaryCategory     { get; set; }


        public PackageFilters()
        {
            OrderByAscending    = true;
            PrimaryCategory     = Category.Title;
        }

        public void GuiSearchBar()
        {            
            if ( null == m_toolbarSeachTextField )
                m_toolbarSeachTextField      = new GUIStyle( "ToolbarSeachTextField" );
            if ( null == m_toolbarSeachCancelButton )
                m_toolbarSeachCancelButton   = new GUIStyle( "ToolbarSeachCancelButton" );

            // Dynamic search bar
            m_dynamicSearchText = GUILayout.TextField( m_dynamicSearchText, m_toolbarSeachTextField, GUILayout.Width( 256f - 20f ) ); // MinWidth

            if ( GUILayout.Button( "", m_toolbarSeachCancelButton ) )
            {
                m_dynamicSearchText = "";
                GUI.FocusControl( null );
            }

            // Filters  552 px
            GUILayout.Space(8f);
            m_categoryFilterIndex = EditorGUILayout.Popup( "Category", m_categoryFilterIndex, m_categoryFilterNames, EditorStyles.toolbarPopup, GUILayout.MinWidth( 192f ), GUILayout.MaxWidth( 192f ) );
            GUILayout.Space(16f);
            m_publisherFilterIndex = EditorGUILayout.Popup( "Publisher", m_publisherFilterIndex, m_publisherFilterNames, EditorStyles.toolbarPopup, GUILayout.MinWidth( 192f ), GUILayout.MaxWidth( 192f ) );
            GUILayout.Space(16f);
            m_versionFilterIndex = EditorGUILayout.Popup( "Unity", m_versionFilterIndex, m_versionFilterNames, EditorStyles.toolbarPopup, GUILayout.MinWidth( 128f ), GUILayout.MaxWidth( 128f ) );
        }        

        /// <summary>Returns a cleaned up Unity version string to just the major.minor version if applicable for filters.</summary>
        private string GetUnityMajorMinorVersion( string version )
        {
            if ( string.IsNullOrEmpty( version ) || version == "NA" )
                return "NA";
            int major = version.IndexOf('.');
            int minor = version.IndexOf('.', major + 1);
            return version.Substring( 0, minor );
        }
    
        private string GetPreviousFilter( string[] filterNames, int filterIndex )
        {
            if ( null != filterNames && filterIndex >= 0 && filterIndex < filterNames.Length)
                return filterNames[filterIndex];

            return m_invalidFilter;
        }

        /// <summary>Pre-compile string arrays of filter items for each type ( category, publisher, Unity version ).</summary>
        public void ConstructFilters( List<AssetPackage> packageLibrary )
        {
#if LOG_UNITY_METHODS
            Debug.Log( "PackageFilters: ConstructFilters" );
#endif
            // 2021: Cache Current Filters for re-instatement later.
            string previousCategoryFilter	= GetPreviousFilter( m_categoryFilterNames, m_categoryFilterIndex);
            string previousPublisherFilter	= GetPreviousFilter( m_publisherFilterNames, m_publisherFilterIndex);
            string previousVersionFilter	= GetPreviousFilter( m_versionFilterNames, m_versionFilterIndex);
            
            m_dynamicSearchText			= string.Empty;
  
            List<string> categoryNames  = new List<string>();
            List<string> publisherNames = new List<string>();
            List<string> versionNames   = new List<string>();

            foreach ( AssetPackage sp in packageLibrary )
            {
                string unity = GetUnityMajorMinorVersion( sp.unity_version );

                if ( !categoryNames.Contains( sp.category.label ) )
                    categoryNames.Add( sp.category.label );
                if ( !publisherNames.Contains( sp.publisher.label ) )
                    publisherNames.Add( sp.publisher.label );
                if ( !versionNames.Contains( unity ) )
                    versionNames.Add( unity );
            }

            categoryNames.Sort();
            publisherNames.Sort();
            versionNames.Sort();

            categoryNames.Insert( 0, "All" );
            publisherNames.Insert( 0, "All" );
            versionNames.Insert( 0, "All" );

            m_categoryFilterNames     = categoryNames.ToArray();
            m_publisherFilterNames    = publisherNames.ToArray();
            m_versionFilterNames      = versionNames.ToArray();

            // 2021: Reinstate filters - Could store these so if they aren't found they are still retained?
            m_categoryFilterIndex		= Mathf.Max(0, Array.FindIndex( m_categoryFilterNames, element => element == previousCategoryFilter ) );
            m_publisherFilterIndex	= Mathf.Max(0, Array.FindIndex( m_publisherFilterNames, element => element == previousPublisherFilter ) );
            m_versionFilterIndex		= Mathf.Max(0, Array.FindIndex( m_versionFilterNames, element => element == previousVersionFilter ) );
                        
            // Debug.LogFormat( "Cat:{0} {1} Pub: {2} {3}  Version: {4} {5}",
            //    previousCategoryFilter, categoryFilterIndex, previousPublisherFilter, publisherFilterIndex, previousVersionFilter, versionFilterIndex );
        }

        public List<AssetPackage> SortAndFilterResults( List<AssetPackage> packageLibrary )
        {
            return SortAndFilterResults( packageLibrary, PrimaryCategory, Category.Title );
        }

        public List<AssetPackage> SortAndFilterResults( List<AssetPackage> packageLibrary, Category activeColumn, Category secondColumn = Category.Title )
        {
            IEnumerable<AssetPackage> packages = packageLibrary;

            // Apply Filters
            if ( m_categoryFilterIndex > 0 )
                packages = packages.Where( o => o.category.label.Contains( m_categoryFilterNames[ m_categoryFilterIndex ] ) );
            if ( m_publisherFilterIndex > 0 )
                packages = packages.Where( o => o.publisher.label == m_publisherFilterNames[ m_publisherFilterIndex ] );
            if ( m_versionFilterIndex > 0 )
                packages = packages.Where( o => o.unity_version.Contains( m_versionFilterNames[ m_versionFilterIndex ] ) );

            // Apply Dyanmic search string
            if ( !string.IsNullOrEmpty( m_dynamicSearchText ) )
                packages = packages.Where( o => o.title.IndexOf( m_dynamicSearchText, 0, StringComparison.CurrentCultureIgnoreCase ) != -1 );

            return MultiColumnSort( packages, activeColumn, secondColumn );
        }


        private List<AssetPackage> MultiColumnSort( IEnumerable<AssetPackage> packages, Category activeColumn, Category secondColumn )
        {
            IOrderedEnumerable<AssetPackage> orderedPackages = null;

            switch ( activeColumn )
            {
                case Category.Title:
                    orderedPackages = packages.OrderBy( o => o.title, OrderByAscending );
                    break;
                case Category.UnityVersion:
                    orderedPackages = packages.OrderBy( o => o.unity_version, OrderByAscending );
                    break;
                case Category.Version:
                    orderedPackages = packages.OrderBy( o => o.version, OrderByAscending );
                    break;
                case Category.ModDate:
                    orderedPackages = packages.OrderBy( o => o.ModifiedDate, OrderByAscending );
                    break;
                case Category.PubDate:
                    orderedPackages = packages.OrderBy( o => o.PublishDate, OrderByAscending );
                    break;
                case Category.Size:
                    orderedPackages = packages.OrderBy( o => o.FileSize, OrderByAscending );
                    break;
                case Category.Publisher:
                    orderedPackages = packages.OrderBy( o => o.publisher.label, OrderByAscending );
                    break;
                case Category.Category:
                    orderedPackages = packages.OrderBy( o => o.category.label, OrderByAscending );
                    break;
                case Category.Archived:
                    orderedPackages = packages.OrderBy( o => o.IsArchived, OrderByAscending );
                    break;
                default:
                    Assert.IsTrue( false, string.Format( "PackageFilters: Unhandled enum in switch case {0}", activeColumn ) );
                    break;
            }

            if ( activeColumn != secondColumn )
            {
                switch ( secondColumn )
                {
                    case Category.Title:
                        orderedPackages = orderedPackages.ThenBy( o => o.title, OrderByAscending );
                        break;
                    case Category.UnityVersion:
                        orderedPackages = orderedPackages.ThenBy( o => o.unity_version, OrderByAscending );
                        break;
                    case Category.Version:
                        orderedPackages = orderedPackages.ThenBy( o => o.version, OrderByAscending );
                        break;
                    case Category.ModDate:
                        orderedPackages = orderedPackages.ThenBy( o => o.ModifiedDate, OrderByAscending );
                        break;
                    case Category.PubDate:
                        orderedPackages = orderedPackages.ThenBy( o => o.PublishDate, OrderByAscending );
                        break;
                    case Category.Size:
                        orderedPackages = orderedPackages.ThenBy( o => o.FileSize, OrderByAscending );
                        break;
                    case Category.Publisher:
                        orderedPackages = orderedPackages.ThenBy( o => o.publisher.label, OrderByAscending );
                        break;
                    case Category.Category:
                        orderedPackages = orderedPackages.ThenBy( o => o.category.label, OrderByAscending );
                        break;
                    case Category.Archived:
                        orderedPackages = orderedPackages.ThenBy( o => o.IsArchived, OrderByAscending );
                        break;
                    default:
                        Assert.IsTrue( false, string.Format( "PackageFilters: Unhandled enum in switch case {0}", secondColumn ) );
                        break;
                }
            }

            return orderedPackages.ToList();
        }


        private List<AssetPackage> SingleColumnSort( IEnumerable<AssetPackage> packages, Category activeColumn )
        {
            if ( OrderByAscending )
            {
                switch ( activeColumn )
                {
                    case Category.Title:
                        packages = packages.OrderBy( o => o.title );
                        break;
                    case Category.UnityVersion:
                        packages = packages.OrderBy( o => o.unity_version );
                        break;
                    case Category.Version:
                        packages = packages.OrderBy( o => o.version );
                        break;
                    case Category.ModDate:
                        packages = packages.OrderBy( o => o.ModifiedDate );
                        break;
                    case Category.PubDate:
                        packages = packages.OrderBy( o => o.PublishDate );
                        break;
                    case Category.Size:
                        packages = packages.OrderBy( o => o.FileSize );
                        break;
                    case Category.Publisher:
                        packages = packages.OrderBy( o => o.publisher.label );
                        break;
                    case Category.Category:
                        packages = packages.OrderBy( o => o.category.label );
                        break;
                    case Category.Archived:
                        packages = packages.OrderBy( o => o.IsArchived );
                        break;
                    default:
                        Assert.IsTrue( false, string.Format( "PackageFilters: Unhandled enum in switch case {0}", activeColumn ) );
                        break;
                }
            }
            else
            {
                switch ( activeColumn )
                {
                    case Category.Title:
                        packages = packages.OrderByDescending( o => o.title );
                        break;
                    case Category.UnityVersion:
                        packages = packages.OrderByDescending( o => o.unity_version );
                        break;
                    case Category.Version:
                        packages = packages.OrderByDescending( o => o.version );
                        break;
                    case Category.ModDate:
                        packages = packages.OrderByDescending( o => o.ModifiedDate );
                        break;
                    case Category.PubDate:
                        packages = packages.OrderByDescending( o => o.PublishDate );
                        break;
                    case Category.Size:
                        packages = packages.OrderByDescending( o => o.FileSize );
                        break;
                    case Category.Publisher:
                        packages = packages.OrderByDescending( o => o.publisher.label );
                        break;
                    case Category.Category:
                        packages = packages.OrderByDescending( o => o.category.label );
                        break;
                    case Category.Archived:
                        packages = packages.OrderByDescending( o => o.IsArchived );
                        break;
                    default:
                        Assert.IsTrue( false, string.Format( "PackageFilters: Unhandled enum in switch case {0}", activeColumn ) );
                        break;
                }
            }

            return packages.ToList();        
        }
    }
}
