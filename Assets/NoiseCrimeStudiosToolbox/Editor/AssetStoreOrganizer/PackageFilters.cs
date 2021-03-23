using System;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;
using NoiseCrimeStudios.Core;
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
		
		private readonly string		invalidFilter = "!";

        private string      dynamicSearchText;

        // Filters
        private int         categoryFilterIndex     = 0;
        private int         publisherFilterIndex    = 0;
        private int         versionFilterIndex      = 0;
        
        private string[]    categoryFilterNames;
        private string[]    publisherFilterNames;
        private string[]    versionFilterNames;
        
        private GUIStyle    toolbarSeachTextField;
        private GUIStyle    toolbarSeachCancelButton;

        public  bool        OrderByAscending    { get; set; }
        public  Category    PrimaryCategory     { get; set; }


        public PackageFilters()
        {
            OrderByAscending    = true;
            PrimaryCategory     = Category.Title;
        }

        public void GuiSearchBar()
        {            
            if ( null == toolbarSeachTextField )    toolbarSeachTextField      = new GUIStyle( "ToolbarSeachTextField" );
            if ( null == toolbarSeachCancelButton ) toolbarSeachCancelButton   = new GUIStyle( "ToolbarSeachCancelButton" );

            // Dynamic search bar
            dynamicSearchText = GUILayout.TextField( dynamicSearchText, toolbarSeachTextField, GUILayout.Width( 256f - 20f ) ); // MinWidth

            if ( GUILayout.Button( "", toolbarSeachCancelButton ) )
            {
                dynamicSearchText = "";
                GUI.FocusControl( null );
            }

            // Filters  552 px
            GUILayout.Space(8f);
            categoryFilterIndex = EditorGUILayout.Popup( "Category", categoryFilterIndex, categoryFilterNames, EditorStyles.toolbarPopup, GUILayout.MinWidth( 192f ), GUILayout.MaxWidth( 192f ) );
            GUILayout.Space(16f);
            publisherFilterIndex = EditorGUILayout.Popup( "Publisher", publisherFilterIndex, publisherFilterNames, EditorStyles.toolbarPopup, GUILayout.MinWidth( 192f ), GUILayout.MaxWidth( 192f ) );
            GUILayout.Space(16f);
            versionFilterIndex = EditorGUILayout.Popup( "Unity", versionFilterIndex, versionFilterNames, EditorStyles.toolbarPopup, GUILayout.MinWidth( 128f ), GUILayout.MaxWidth( 128f ) );
        }        

        /// <summary>Returns a cleaned up Unity version string to just the major.minor version if applicable for filters.</summary>
        string GetUnityMajorMinorVersion( string version )
        {
            if ( string.IsNullOrEmpty( version ) || version == "NA" ) return "NA";
            int major = version.IndexOf('.');
            int minor = version.IndexOf('.', major + 1);
            return version.Substring( 0, minor );
        }
	
		private string GetPreviousFilter( string[] filterNames, int filterIndex )
		{
			if ( null != filterNames && filterIndex >= 0 && filterIndex < filterNames.Length)
				return filterNames[filterIndex];

			return invalidFilter;
		}

        /// <summary>Pre-compile string arrays of filter items for each type ( category, publisher, Unity version ).</summary>
        public void ConstructFilters( List<AssetPackage> packageLibrary )
        {
#if LOG_UNITY_METHODS
            Debug.Log( "PackageFilters: ConstructFilters" );
#endif
			// 2021: Cache Current Filters for re-instatement later.
			string previousCategoryFilter	= GetPreviousFilter( categoryFilterNames, categoryFilterIndex);
			string previousPublisherFilter	= GetPreviousFilter( publisherFilterNames, publisherFilterIndex);
			string previousVersionFilter	= GetPreviousFilter( versionFilterNames, versionFilterIndex);
			
            dynamicSearchText			= string.Empty;
  
            List<string> categoryNames  = new List<string>();
            List<string> publisherNames = new List<string>();
            List<string> versionNames   = new List<string>();

            foreach ( AssetPackage sp in packageLibrary )
            {
                string unity = GetUnityMajorMinorVersion( sp.unity_version );

                if ( !categoryNames.Contains( sp.category.label ) ) categoryNames.Add( sp.category.label );
                if ( !publisherNames.Contains( sp.publisher.label ) ) publisherNames.Add( sp.publisher.label );
                if ( !versionNames.Contains( unity ) ) versionNames.Add( unity );
            }

            categoryNames.Sort();
            publisherNames.Sort();
            versionNames.Sort();

            categoryNames.Insert( 0, "All" );
            publisherNames.Insert( 0, "All" );
            versionNames.Insert( 0, "All" );

            categoryFilterNames     = categoryNames.ToArray();
            publisherFilterNames    = publisherNames.ToArray();
            versionFilterNames      = versionNames.ToArray();

			// 2021: Reinstate filters - Could store these so if they aren't found they are still retained?
			categoryFilterIndex		= Mathf.Max(0, Array.FindIndex( categoryFilterNames, element => element == previousCategoryFilter ) );
			publisherFilterIndex	= Mathf.Max(0, Array.FindIndex( publisherFilterNames, element => element == previousPublisherFilter ) );
			versionFilterIndex		= Mathf.Max(0, Array.FindIndex( versionFilterNames, element => element == previousVersionFilter ) );
						
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
            if ( categoryFilterIndex > 0 ) packages = packages.Where( o => o.category.label.Contains( categoryFilterNames[ categoryFilterIndex ] ) );
            if ( publisherFilterIndex > 0 ) packages = packages.Where( o => o.publisher.label == publisherFilterNames[ publisherFilterIndex ] );
            if ( versionFilterIndex > 0 ) packages = packages.Where( o => o.unity_version.Contains( versionFilterNames[ versionFilterIndex ] ) );

            // Apply Dyanmic search string
            if ( !string.IsNullOrEmpty( dynamicSearchText ) )
                packages = packages.Where( o => o.title.IndexOf( dynamicSearchText, 0, StringComparison.CurrentCultureIgnoreCase ) != -1 );

            return MultiColumnSort( packages, activeColumn, secondColumn );
        }


        private List<AssetPackage> MultiColumnSort( IEnumerable<AssetPackage> packages, Category activeColumn, Category secondColumn )
        {
            IOrderedEnumerable<AssetPackage> orderedPackages = null;

            switch ( activeColumn )
            {
                case Category.Title:        orderedPackages = packages.OrderBy( o => o.title, OrderByAscending ); break;
                case Category.UnityVersion: orderedPackages = packages.OrderBy( o => o.unity_version, OrderByAscending ); break;
                case Category.Version:      orderedPackages = packages.OrderBy( o => o.version, OrderByAscending ); break;
                case Category.ModDate:      orderedPackages = packages.OrderBy( o => o.modifiedDate, OrderByAscending ); break;
                case Category.PubDate:      orderedPackages = packages.OrderBy( o => o.publishDate, OrderByAscending ); break;
                case Category.Size:         orderedPackages = packages.OrderBy( o => o.fileSize, OrderByAscending ); break;
                case Category.Publisher:    orderedPackages = packages.OrderBy( o => o.publisher.label, OrderByAscending ); break;
                case Category.Category:     orderedPackages = packages.OrderBy( o => o.category.label, OrderByAscending ); break;
                case Category.Archived:     orderedPackages = packages.OrderBy( o => o.isArchived, OrderByAscending ); break;
                default:
                    Assert.IsTrue( false, string.Format( "PackageFilters: Unhandled enum in switch case {0}", activeColumn ) );
                    break;
            }

            if ( activeColumn != secondColumn )
            {
                switch ( secondColumn )
                {
                    case Category.Title:        orderedPackages = orderedPackages.ThenBy( o => o.title, OrderByAscending ); break;
                    case Category.UnityVersion: orderedPackages = orderedPackages.ThenBy( o => o.unity_version, OrderByAscending ); break;
                    case Category.Version:      orderedPackages = orderedPackages.ThenBy( o => o.version, OrderByAscending ); break;
                    case Category.ModDate:      orderedPackages = orderedPackages.ThenBy( o => o.modifiedDate, OrderByAscending ); break;
                    case Category.PubDate:      orderedPackages = orderedPackages.ThenBy( o => o.publishDate, OrderByAscending ); break;
                    case Category.Size:         orderedPackages = orderedPackages.ThenBy( o => o.fileSize, OrderByAscending ); break;
                    case Category.Publisher:    orderedPackages = orderedPackages.ThenBy( o => o.publisher.label, OrderByAscending ); break;
                    case Category.Category:     orderedPackages = orderedPackages.ThenBy( o => o.category.label, OrderByAscending ); break;
                    case Category.Archived:     orderedPackages = orderedPackages.ThenBy( o => o.isArchived, OrderByAscending ); break;
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
                    case Category.Title:        packages = packages.OrderBy( o => o.title ); break;
                    case Category.UnityVersion: packages = packages.OrderBy( o => o.unity_version ); break;
                    case Category.Version:      packages = packages.OrderBy( o => o.version ); break;
                    case Category.ModDate:      packages = packages.OrderBy( o => o.modifiedDate ); break;
                    case Category.PubDate:      packages = packages.OrderBy( o => o.publishDate ); break;
                    case Category.Size:         packages = packages.OrderBy( o => o.fileSize ); break;
                    case Category.Publisher:    packages = packages.OrderBy( o => o.publisher.label ); break;
                    case Category.Category:     packages = packages.OrderBy( o => o.category.label ); break;
                    case Category.Archived:     packages = packages.OrderBy( o => o.isArchived ); break;
                    default:
                        Assert.IsTrue( false, string.Format( "PackageFilters: Unhandled enum in switch case {0}", activeColumn ) );
                        break;
                }
            }
            else
            {
                switch ( activeColumn )
                {
                    case Category.Title:        packages = packages.OrderByDescending( o => o.title ); break;
                    case Category.UnityVersion: packages = packages.OrderByDescending( o => o.unity_version ); break;
                    case Category.Version:      packages = packages.OrderByDescending( o => o.version ); break;
                    case Category.ModDate:      packages = packages.OrderByDescending( o => o.modifiedDate ); break;
                    case Category.PubDate:      packages = packages.OrderByDescending( o => o.publishDate ); break;
                    case Category.Size:         packages = packages.OrderByDescending( o => o.fileSize ); break;
                    case Category.Publisher:    packages = packages.OrderByDescending( o => o.publisher.label ); break;
                    case Category.Category:     packages = packages.OrderByDescending( o => o.category.label ); break;
                    case Category.Archived:     packages = packages.OrderByDescending( o => o.isArchived ); break;
                    default:
                        Assert.IsTrue( false, string.Format( "PackageFilters: Unhandled enum in switch case {0}", activeColumn ) );
                        break;
                }
            }

            return packages.ToList();        
        }
    }
}
