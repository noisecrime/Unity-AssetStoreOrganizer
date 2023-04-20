#define ENABLE_ARCHIVE_LOGS

using System.Collections.Generic;
using System.IO;
using NoiseCrimeStudios.Core;
using NoiseCrimeStudios.Core.IO;
using UnityEditor;
using UnityEngine;

namespace NoiseCrimeStudios.Toolbox.AssetStoreOrganizer
{
    /// <summary>Collection of Methods for archive, copy and restore packages between locations.</summary>
    /// <remarks>
    /// Due to file path limits in Windows its suggest to prepend only the version number to an asset.
    /// However some packages are updated but the version stays the same, so in some case we must add versionID.
    /// </remarks>
    public static class Archiver
    {
        private static readonly StringBuilderDebugLog s_debugLogOutput    = new StringBuilderDebugLog();

        private static readonly string s_overwriteFile           = "{0}\nAlready exists in the AssetStore Directory.\nDo you want to overwrite the original file with\n{1}";
        private static readonly string s_restoreMessageFormat    = "Are you sure you want to restore the package\n{0}\n({1})\nto\n{2}";

        /// <summary>
        /// Archive packages to supplied PackageLibrary.
        /// </summary>
        /// <param name="packages">List of packages to archive to archiveLib.</param>
        /// <param name="archiveLib">PackageLibrary representing the archive library used to compare against.</param>
        /// <param name="sourceLocation">Source Package location to archive from.</param>
        /// <param name="enableFileOperations">If FALSE will not perform file operations allowing non-destructive testing.</param>
        public static void ArchiveAssetStorePackages( List<AssetPackage> packages, PackageLibrary archiveLib, PackagesLocation sourceLocation, bool enableFileOperations = true )
        {
            if ( PackagesLocation.Archive == sourceLocation )
            {
                Debug.LogWarningFormat( "ArchiveMethods: Cannot archive packages from the archived directory." );
                return;
            }

            if ( !DirectoryUtils.IsValidPathToDirectory( OrganizerPaths.StoreDirectoryBackup ) )
            {
                Debug.LogWarningFormat( "ArchiveMethods: The operation could not be completed because ArchiveDirectory does not exist.\n{0}", OrganizerPaths.StoreDirectoryBackup );
                return;
            }
            
            if ( OrganizerPaths.GetPackageDirectory( sourceLocation ) == OrganizerPaths.StoreDirectoryBackup )
            {
                Debug.LogWarningFormat( "ArchiveMethods: The operation could not be completed because source location in the same as archive location.\n{0}", OrganizerPaths.StoreDirectoryBackup );
                return;
            }

            s_debugLogOutput.Clear();
            s_debugLogOutput.IndentReset(16);
            s_debugLogOutput.AppendLine( "-- ArchiveThePackagesInLibrary --" );

            for( int i=0; i < packages.Count; i++)
            {
                EditorUtility.DisplayProgressBar( "Copy Packages to Archive",  packages[i].title, i/(float)packages.Count );
                ArchiveAssetStorePackage( packages[i], archiveLib, sourceLocation, enableFileOperations, i);
                s_debugLogOutput.InsertSoftBreak( 1536 );
            }

            EditorUtility.ClearProgressBar();

            s_debugLogOutput.InsertHardBreak();
            s_debugLogOutput.AppendLine( "-- ArchiveThePackagesInLibrary --" );
            s_debugLogOutput.LogToConsole(true, true);
        }
        
        private static void ArchiveAssetStorePackage( AssetPackage package, PackageLibrary archiveLib, PackagesLocation sourceLocation, bool enableFileOperations, int counter )
        {
            if ( package.id == -1 ) // Ignore Legacy Builtin Unity StandardAssets.
            {
                s_debugLogOutput.AppendFormat( "[{0:D4}][{1:D8}] IGNORED\t\t{2} - Builtin StandardAsset [{3}]", counter, package.id, package.title, package.FullFilePath );
                return;  
            }

            AssetPackage matchedPackage;

            MatchState matchState = archiveLib.TryGetMatchedPackage( package, out matchedPackage );

#if ENABLE_ARCHIVE_LOGS
            if ( matchState == MatchState.Exact )
            {
                s_debugLogOutput.AppendFormat( "[{0:D4}][{1:D8}] EXISTS\t\t{2}", counter, package.id, package.title );
                s_debugLogOutput.IndentIncrement( 4 );
                s_debugLogOutput.AppendFormat( "{0} [package.fullFilePath]", package.FullFilePath );
                s_debugLogOutput.AppendFormat( "{0} [archivePack.fullFilePath]", matchedPackage.FullFilePath );
                s_debugLogOutput.IndentDecrement( 4 );
                s_debugLogOutput.AppendLine();
            }
#endif           
            // If partial or no match found then copy the package.
            if ( matchState != MatchState.Exact )
                CopyPackage( package, sourceLocation, PackagesLocation.Archive, matchState, enableFileOperations, counter );             
        }

        /// <summary>Copy File from storePackage location to archive location.</summary>
        /// <remarks>If we have a partial match then we need to copy the file but use versionID to distingush between them.</remarks>
        private static void CopyPackage( AssetPackage package, PackagesLocation srcLocation, PackagesLocation dstLocation, MatchState matchState, bool enableFileOperations, int counter )
        {
            // Assumption that source and destination does not contain Unity\Asset Store-5.x\ or Unity\Asset Store\   
            string srcPackageDirectory  = PathUtils.GetConsistentFilePath( OrganizerPaths.GetPackageDirectory( srcLocation ) );
            string dstPackageDirectory  = PathUtils.GetConsistentFilePath( OrganizerPaths.GetPackageDirectory( dstLocation ) );

            // Remove 'Asset Store-5.x' or 'AssetStore' if final part of source directory path.
            if ( srcLocation == PackagesLocation.AssetStore || srcLocation == PackagesLocation.AssetStore5x )
               srcPackageDirectory = Path.GetDirectoryName( srcPackageDirectory );

            // TODO - FIX: If dstPackageDirectory is not root of  'Asset Store-5.x' or 'AssetStore' we have a problem
            if ( !dstPackageDirectory.Contains( "Asset Store-5.x" ) && !dstPackageDirectory.Contains( "AssetStore" ) )
            {
                Debug.LogWarningFormat( "ArchiveMethods: The operation could not be completed because destination is not 'AssetStore'.\n{0}", dstPackageDirectory );
                return;
            }

            // Get the relative store package path
            string relativeSrcPackagePath = package.FullFilePath.Replace( srcPackageDirectory + Path.DirectorySeparatorChar, "" );
            // Remove filename from realtive path
            relativeSrcPackagePath        = Path.GetDirectoryName( relativeSrcPackagePath );
                       
            // Build archive filename for package
            string archiveFilename;
            string storePackFileName    = Path.GetFileName( package.FullFilePath );
            string storePackVersion     = string.Format("[{0}]", package.version );
            string storePackVersionID   = string.Format("[{0}]", package.version_id );
                       
            // Remove any existing tags - in case this comes from a custom previous archive/backup location!
            if ( matchState == MatchState.Partial )
            {
                storePackFileName   = storePackFileName.Replace( storePackVersion, "" );
                storePackFileName   = storePackFileName.Replace( storePackVersionID, "" );
                archiveFilename     = storePackVersion + storePackVersionID + storePackFileName;
            }
            else
            {
                storePackFileName   = storePackFileName.Replace( storePackVersion, "" );
                archiveFilename     = storePackVersion + storePackFileName;
            }

            string archiveDirectoryPath = dstPackageDirectory + Path.DirectorySeparatorChar + relativeSrcPackagePath;
            string archiveFilePath      = archiveDirectoryPath + Path.DirectorySeparatorChar + archiveFilename;

#if ENABLE_ARCHIVE_LOGS
            string copyText             = enableFileOperations ? "COPIED\t" : "COPY DISABLED";
            s_debugLogOutput.AppendFormat( "[{0:D4}][{1:D8}] {2}\t{3}  (PartialMatch = {4})", counter, package.id, copyText, package.title, matchState );
            s_debugLogOutput.IndentIncrement(4);
            s_debugLogOutput.AppendFormat( "{0} [package.fullFilePath]",     package.FullFilePath );
            s_debugLogOutput.AppendFormat( "{0} [archiveFilePath]",          archiveFilePath );
            s_debugLogOutput.AppendFormat( "{0} [archivePackageDirectory]",  dstPackageDirectory );
            s_debugLogOutput.AppendFormat( "{0} [storePackageDirectory]",    srcPackageDirectory );
            s_debugLogOutput.AppendFormat( "{0} [relativeStorePackagePath]", relativeSrcPackagePath );            
            s_debugLogOutput.AppendFormat( "{0} [archiveFilename]",          archiveFilename );
            s_debugLogOutput.IndentDecrement(4);
            s_debugLogOutput.AppendLine();
#endif

            if ( enableFileOperations )
                CopyFile( package.FullFilePath, archiveFilePath, archiveDirectoryPath, false );
        }


        private static void CopyFile( string sourceFilePath, string targetFilePath, string targetDirectoryPath, bool overwrite )
        {
            if ( !File.Exists( sourceFilePath ) )
            {
                Debug.LogWarningFormat( "ArchiveMethods: The operation could not be completed because the file doesn't exist.\n{0}", sourceFilePath );
                return;
            }

            if ( File.Exists( targetFilePath ) && !overwrite )
            {
                Debug.LogWarningFormat( "ArchiveMethods: The operation could not be completed because the file already exists.\n{0}\n{1}", sourceFilePath, targetFilePath );
                return;
            }

            if ( !Directory.Exists( targetDirectoryPath ) )
                Directory.CreateDirectory( targetDirectoryPath );

            FileInfo sourceFileInfo = new FileInfo( sourceFilePath );

            try
            {
                sourceFileInfo.CopyTo( targetFilePath, overwrite );
            }
            catch ( System.Exception ex )
            {
                Debug.LogErrorFormat( "Copy Failed: {0}", ex.Message );
            }
        }

        public static bool RestoreArchivePackageToAssetStore( AssetPackage package, PackagesLocation sourcelocation, bool enableFileOperations )
        {
            if ( sourcelocation != PackagesLocation.Custom && sourcelocation != PackagesLocation.Archive )
            {
                Debug.LogWarningFormat("ArchiveMethods: Invalid Source location, can only copy packages from Custom or Archive locations to AssetStore!");
                return false;
            }

            // Cache Tags
            string storePackVersion     = string.Format("[{0}]", package.version );
            string storePackVersionID   = string.Format("[{0}]", package.version_id );

            string locationDirectory    = PathUtils.GetConsistentFilePath( OrganizerPaths.GetPackageDirectory( sourcelocation ));
            string packageFilePath      = package.FullFilePath;
            // Get Relative path
            string relativeFilePath     = packageFilePath.Replace( locationDirectory, "" ); // this will leave initial directory seperator!
            string relativeFileName     = Path.GetFileNameWithoutExtension( packageFilePath );
            // Strip any tags we added            
            relativeFilePath            = relativeFilePath.Replace( storePackVersion, "" );
            relativeFilePath            = relativeFilePath.Replace( storePackVersionID, "" );

            string assetStoreRoot       = PathUtils.GetConsistentFilePath( OrganizerPaths.StoreDirectoryRoot );
            string destinationFilePath  = assetStoreRoot + relativeFilePath;
            string destinationDirectory = Path.GetDirectoryName( destinationFilePath );

            // Check for overwrite!
            bool overwrite  = true;
            bool comfirmed  = true;

            if ( File.Exists( destinationFilePath ) )
            {
                string message = string.Format( s_overwriteFile, package.title, relativeFileName );
                overwrite = ( EditorUtility.DisplayDialog( "Restore Package To Asset Store", message, "OK", "Cancel" ) );
            }
            else
            {
                string message = string.Format( s_restoreMessageFormat, package.title, relativeFileName, OrganizerPaths.StoreDirectoryBackup );
                comfirmed = ( EditorUtility.DisplayDialog( "Restore Package To Asset Store", message,  "OK", "Cancel" ) );           
            }

            bool restoreFile = ( enableFileOperations && comfirmed && overwrite );

#if ENABLE_ARCHIVE_LOGS
            StringBuilderDebugLog debugLogOutput = new StringBuilderDebugLog();
            debugLogOutput.AppendFormat( "[locationPath]\t {0}", locationDirectory );
            debugLogOutput.AppendFormat( "[packagePath]\t {0}", packageFilePath );
            debugLogOutput.AppendFormat( "[relativeFilePath]\t {0}", relativeFilePath );
            debugLogOutput.AppendFormat( "[assetStoreRoot]\t {0}", assetStoreRoot );
            debugLogOutput.AppendFormat( "[fullDestinationPath]\t {0}", destinationFilePath );
            debugLogOutput.AppendFormat( "[destinationDirectory]\t {0}", destinationDirectory );
            debugLogOutput.AppendFormat( "[File Restored]\t {0}  [Overwrite: {1}, Confirmed: {2}  enableFileOperations: {3}]", restoreFile, overwrite, comfirmed, enableFileOperations );
            debugLogOutput.LogToConsole();
#endif

            if ( restoreFile )
                CopyFile( packageFilePath, destinationFilePath, destinationDirectory, overwrite );

            return restoreFile;
        }

        /// <summary>Log to console the archived status of the storePackage.</summary>
        public static void LogPackageArchiveStatus( AssetPackage package, PackageLibrary library )
        {
            StringBuilderDebugLog output = new StringBuilderDebugLog();

            if ( library.PackagesIDTable.ContainsKey( package.id ) )
            {
                output.AppendFormat( "Store:\t{0}\t{1}\t{2}\t{3}\t{4}\t{5}",
                    package.unity_version, package.version, package.version_id, package.pubdate, package.title, package.FullFilePath );
                output.AppendLine();

                foreach ( AssetPackage archivePack in library.PackagesIDTable[ package.id ] )
                {
                    bool matchName          = package.title == archivePack.title;
                    bool matchUnityVersion  = package.unity_version == archivePack.unity_version;
                    bool matchVersion       = package.version == archivePack.version;
                    bool matchVersionID     = package.version_id == archivePack.version_id;
                    bool matchPubDate       = package.pubdate == archivePack.pubdate;
                    bool matched            = matchName && matchUnityVersion && matchVersion && matchVersionID && matchPubDate;

                    output.AppendFormat( "Archive:\t{0}\t{1}\t{2}\t{3}\t{4}\t{5}",
                        archivePack.unity_version, archivePack.version, archivePack.version_id, archivePack.pubdate, archivePack.title, archivePack.FullFilePath );
                    output.AppendFormat( "Archive:\t{0}\t{1}\t{2}\t{3}\t\t{4}",
                        matchUnityVersion, matchVersion, matchVersionID, matchPubDate, matchName );

                    if ( matched )
                        output.AppendLine( "Package Archive Matched!" );

                    output.AppendLine();
                }
            }
            else
            {
                output.AppendFormat( "Package Archive Status: Not Archived {0} [{1}]", package.title, package.id );
            }

            output.LogToConsole( true, true );
        }
    }
}
