using System.Collections.Generic;
using System.IO;
using System.Reflection;
using NoiseCrimeStudios.Core.Formatting;
using NoiseCrimeStudios.Core.IO;
using UnityEditor;
using UnityEngine.Assertions;
using Debug = UnityEngine.Debug;

namespace NoiseCrimeStudios.Toolbox.AssetStoreOrganizer
{
    public enum MatchState { None, Partial, Exact }

    /// <summary>Library holds all packages found within the specificed directory location.</summary>
    public class PackageLibrary
    {        
        private static readonly string[]            s_searchPatterns = new string[]{ "*.unitypackage" };

        public List<AssetPackage>                   Packages        = new List<AssetPackage>();
        public Dictionary<int, List<AssetPackage>>  PackagesIDTable = new Dictionary<int, List<AssetPackage>>();


        /// <summary>Returns number of files or packages checked.</summary>
        public int              FileCount       { get; private set; }
        /// <summary>Returns number of packages in Library.</summary>
        public int              PackageCount    { get { return Packages.Count; } }
        /// <summary>Returns a display string formatting of the file size of all packages on disk.</summary>
        public string           SizeOnDisk      { get; private set; }

        public PackagesLocation Location        {  get; private set; }


        public PackageLibrary() { }

        public PackageLibrary( PackagesLocation packagesLocation )
        {
            PopulateLibraryContent( packagesLocation );
        }

        public virtual void PopulateLibraryContent( PackagesLocation packagesLocation )
        {
            Location = packagesLocation;

            Packages.Clear();            
            PackagesIDTable.Clear();

            // Build packages list
            switch ( packagesLocation )
            {
                case PackagesLocation.NativePackageList:
                    PopulateFromPackageList();
                    break;
                case PackagesLocation.AssetStore:
                    PopulateFileSearchBegin( OrganizerPaths.StoreDirectoryLegacy );
                    break;
                case PackagesLocation.AssetStore5x:
                    PopulateFileSearchBegin( OrganizerPaths.StoreDirectoryModern );
                    break;
                case PackagesLocation.Custom:
                    PopulateFileSearchBegin( OrganizerPaths.StoreDirectoryCustom );
                    break;
                case PackagesLocation.Archive:
                    PopulateFileSearchBegin( OrganizerPaths.StoreDirectoryBackup );
                    break;
                default:
                    Assert.IsTrue( false, string.Format( "StorePackageLibrary: Unhandled enum in switch case {0}", packagesLocation ) );
                    break;
            }

            // Build lookuptable for existing archive packages by id, with multiple packages added to value list.       
            foreach ( AssetPackage sp in Packages )
            {
               List<AssetPackage> list;

               if ( PackagesIDTable.TryGetValue( sp.id, out list ) )
                    list.Add( sp );
               else
                    PackagesIDTable.Add( sp.id, new List<AssetPackage>(){ sp } );            
            }
        }

        /// <summary>Populates packages list from Unity provided native PackageInfo Method.</summary>
        private void PopulateFromPackageList()
        {
            long libraryDiskSize = 0;

            var flags		= BindingFlags.Static | BindingFlags.NonPublic;
            var methodInfo	= typeof(PackageInfo).GetMethod("GetPackageList", flags);
            object result	= methodInfo.Invoke(null, null ); 

            PackageInfo[] packageList = (PackageInfo[])result;

            foreach ( PackageInfo p in packageList )			
                libraryDiskSize += AssetPackage.PopulateFromPackageInfo( p, Packages );
            
            SizeOnDisk  = Numerical.ByteCountToSuffixHumbads( libraryDiskSize );
            FileCount   = packageList.Length;
        }

        /// <summary>Populates packages list from iterating through the provided directory.</summary>
        private void PopulateFileSearchBegin( string packagesPath )
        {
            if ( !DirectoryUtils.IsValidPathToDirectory( packagesPath ) )
            {
                Debug.LogWarningFormat( "PackageLibrary: Directory does not exist\n{0}", packagesPath );
                return;
            }

             int  fileCounter       = 0;
             long libraryDiskSize   = 0;

             GetFilesRecursiveViaMultiplePatterns( packagesPath, s_searchPatterns, true, ref fileCounter, ref libraryDiskSize);

            SizeOnDisk  = Numerical.ByteCountToSuffixHumbads( libraryDiskSize ); 
            FileCount   = fileCounter;
        }
        
        /// <summary>Given a path and search pattern will populate Library from package files found in directory.</summary>
        private void GetFilesRecursiveViaMultiplePatterns( string packagesPath, string[] searchPatterns, bool subFolders, ref int fileCounter, ref long libraryDiskSize )
        {
            if ( !DirectoryUtils.IsValidPathToDirectory( packagesPath ) )
            { 
                Debug.LogWarningFormat("PackageLibrary: Directory does not exist\n{0}", packagesPath);
                return;
            }

            string[] directories = Directory.GetDirectories( packagesPath );

            foreach ( string directory in directories )			
                GetFilesRecursiveViaMultiplePatterns( directory, searchPatterns, subFolders, ref fileCounter, ref libraryDiskSize );				
                    
            foreach ( string sp in searchPatterns )
            {
                string[] tmpFiles = Directory.GetFiles( packagesPath, sp );
                fileCounter += tmpFiles.Length;

                foreach ( string fullFilePath in tmpFiles )
                    libraryDiskSize += AssetPackage.CreatePackageInfoFromLocalStorage( fullFilePath, Packages );			
            }			
        }

        /*
        public void CompareAgainstArchive( PackageArchive storePackageArchives )
        {
            foreach( AssetPackage storePack in packageLibrary )            
               storePack.isArchived = storePackageArchives.IsPackageArchived( storePack );
        }*/

        public void CompareAgainstArchive( PackageLibrary archiveLibrary )
        {
            foreach( AssetPackage pack in Packages )            
               pack.IsArchived = ( archiveLibrary.ContainsPackage( pack ) == MatchState.Exact );
        }

        /*
        public void CompareAgainstLibrary( PackageLibrary otherLibrary )
        {
             foreach( AssetPackage pack in packageLibrary )
        }
        */

        /// <summary>Checks if package is found within this library - not object compare!</summary>
        public MatchState ContainsPackage( AssetPackage package )
        {
            List<AssetPackage> packages;

            if ( PackagesIDTable.TryGetValue( package.id, out packages ) )
            {
                bool partialMatch = true;

                foreach ( AssetPackage archivePack in packages )
                {
                    bool matchName          = package.title == archivePack.title;
                    bool matchUnityVersion  = package.unity_version == archivePack.unity_version;
                    bool matchVersion       = package.version == archivePack.version;
                    bool matchVersionID     = package.version_id == archivePack.version_id;
                    bool matchPubDate       = package.pubdate == archivePack.pubdate;
                                        
                    bool exactMatched       = matchName && matchVersion && matchUnityVersion && matchVersionID && matchPubDate;

                    // Maintain Partial Match State in case we don't find exact match.
                    // Have found cases where we get a name and version match but other data is mismatched.
                    partialMatch |= ( matchName && matchVersion && !exactMatched );

                    if ( exactMatched )
                        return MatchState.Exact;                  
                }

                if ( partialMatch )
                    return MatchState.Partial;  
            }

            return MatchState.None;
        }


        /// <summary>
        /// Checks if package is found within this library and if Matched puts the AssetPackage into result.
        /// Partial Matches are not returned.
        /// </summary>
        /// <returns>Matched state and if matched the AssetPackage in result.</returns>
        public MatchState TryGetMatchedPackage( AssetPackage package, out AssetPackage result )
        {
            result = null;
            List<AssetPackage> packages;

            if ( PackagesIDTable.TryGetValue( package.id, out packages ) )
            {
                bool partialMatch = true;

                foreach ( AssetPackage archivePack in packages )
                {
                    bool matchName          = package.title == archivePack.title;
                    bool matchUnityVersion  = package.unity_version == archivePack.unity_version;
                    bool matchVersion       = package.version == archivePack.version;
                    bool matchVersionID     = package.version_id == archivePack.version_id;
                    bool matchPubDate       = package.pubdate == archivePack.pubdate;

                    bool exactMatched       = matchName && matchVersion && matchUnityVersion && matchVersionID && matchPubDate;

                    // Maintain Partial Match State in case we don't find exact match.
                    // Have found cases where we get a name and version match but other data is mismatched.
                    partialMatch |= ( matchName && matchVersion && !exactMatched );

                    if ( exactMatched )
                    {
                        result = archivePack;
                        return MatchState.Exact;
                    }
                }

                if ( partialMatch )
                    return MatchState.Partial;                
            }

            return MatchState.None;
        }
    }
}
