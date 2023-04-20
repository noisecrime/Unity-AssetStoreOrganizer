using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using UnityEngine;

namespace NoiseCrimeStudios.Core.IO
{
    /// <summary>Collection of System.IO Utility Methods.</summary>
    /// 
    /// <remarks>
    /// FilePathValidation
    /// <see cref="Assets\NoiseCrimeStudios\Core\Scripts\FileSupport\FilePathValidation.cs"/>
    /// UnityEditor.FileUtil is Unity's own collection of useful methods
    /// <see cref="UnityCsReference\Editor\Mono\FileUtil.cs"/>
    /// </remarks>
    public static partial class DirectoryUtils
    {
#if LEGACY_ENUMERATION
        public enum SearchPattern
        {
            ANY,
            IMAGE,
            XML,
            QEP,
            UNITYPACKAGE,
            CUSTOM
        }
#else
        public enum SearchPattern
        {
            Any             = 1,
            ImageImportable = 10,
            ImageFormats    = 11,
            Xml             = 20,
            QEP             = 50,
            UnityPackage    = 100,
            Custom          = 1000
        };
#endif

        /// <summary>Converts an index to the enum, where index is the position in the array of enums and not the value!</summary>
        /// <remarks>This is quite fragile so should only be used via UI selections e.g DevPanel_FileSystem</remarks>
        public static SearchPattern GetSearchPatternEnumFromIndex( int index )
        {
            return ( SearchPattern )Enum.GetValues( typeof( SearchPattern ) ).GetValue( index );
        }

        public static string GetSearchPattern( SearchPattern searchPattern )
        {
            switch ( searchPattern )
            {
                case SearchPattern.Any:
                    return "*.*";
                case SearchPattern.ImageImportable:
                    return "*.png|*.jpg|*.jpeg";
                case SearchPattern.ImageFormats:
                    return "*.png|*.jpg|*.jpeg|*.tiff|*.tif";
                case SearchPattern.Xml:
                    return "*.xml";
                case SearchPattern.QEP:
                    return "*.qet";
                case SearchPattern.UnityPackage:
                    return "*.unitypackage";
            }
            throw new NotImplementedException( "Not Implemented SearchPattern: " + searchPattern.ToString() );
        }

        public static bool IsValidPathToDirectory( string path )
        {
            return ( !string.IsNullOrEmpty( path ) && Directory.Exists( path ) );
        }

        public static bool IsValidPathInProject( string path )
        {
            return ( !string.IsNullOrEmpty( path ) && path.StartsWith( Application.dataPath ) && Directory.Exists( path ) );
        }

        /// <summary>Determine if directory has no files or subfolders and if it has subfolders they do not have files.</summary>
        /// <returns>bool - true if no files found</returns>
        public static bool IsDirectoryHeirarchyEmptyOfFiles( string sourcePath )
        {
            bool hasContents = true;

            try
            {
                hasContents = Directory.GetDirectories( sourcePath ).Length == 0 && Directory.GetFiles( sourcePath, "*", SearchOption.AllDirectories ).Length == 0;
            }
            catch ( Exception ex )
            {
                Debug.LogWarning( "IsDirectoryHeirarchyEmptyOfFiles " + ex.Message );
            }

            return hasContents;
        }


        public static List<string> GetFoldersInDirectory( string sourcePath )
        {
            List<string> directoryList = new List<string>();

            directoryList.AddRange( Directory.GetDirectories( sourcePath ) );
            directoryList.Sort();

            return directoryList;
        }

        // TODO: Add versions that take a List<string> so we can reuse memory.
        #region GET FILE PATHS FROM DIRECTORY
        /// <summary>Returns list of all files that match search patterns the top directory only, no children.</summary>
        /// <param name="sourcePath">Absolute file path to top directory.</param>
        /// <param name="multiSearchPattern">Single string of | delimited search patterns e.g. "*.png|*.jpg"</param>
        /// <param name="sortResults">(Optional) Sorts the results by name. Default false.</param>
        /// <param name="filenameOnly">(Optional) Returns only the filename without extension. Default false.</param>
        /// <returns>List of strings of absolute path names or filenames.</returns>
        public static List<string> GetFileListFromTopDirectory( string sourcePath, string multiSearchPattern, bool sortResults = false, bool filenameOnly = false )
        {
            return GetFileListFromDirectory( sourcePath, multiSearchPattern, SearchOption.TopDirectoryOnly, sortResults, filenameOnly );
        }

        /// <summary>Returns list of all files that match search patterns the top directory and its sub-directories.</summary>
        /// <param name="sourcePath">Absolute file path to top directory.</param>
        /// <param name="multiSearchPattern">Single string of | delimited search patterns e.g. "*.png|*.jpg"</param>
        /// <param name="sortResults">(Optional) Sorts the results by name. Default false.</param>
        /// <param name="filenameOnly">(Optional) Returns only the filename without extension. Default false.</param>
        /// <returns>List of strings of absolute path names or filenames.</returns>
        public static List<string> GetFileListFromAllDirectories( string sourcePath, string multiSearchPattern, bool sortResults = false, bool filenameOnly = false )
        {
            return GetFileListFromDirectory( sourcePath, multiSearchPattern, SearchOption.AllDirectories, sortResults, filenameOnly );
        }

        /// <summary>Returns list of all files that match search patterns in top directory and optionally subdirectories.</summary>
        /// <remarks>Previouly known as GetFilesFromDirectoriesAsFullPaths, GetFilesFromDirectoryAsFileNameWithoutExtension, GetFileNameNoExtListFromDirectory, GetFileNamesViaMultiplePatterns</remarks>
        /// <param name="sourcePath">Absolute file path to top directory.</param>
        /// <param name="multiSearchPattern">Single string of | delimited search patterns e.g. "*.png|*.jpg"</param>
        /// <param name="searchOption">SearchOptions - TopDirectory or AllDirectories</param>
        /// <param name="sortResults">(Optional) Sorts the results by name. Default false.</param>
        /// <param name="filenameOnly">(Optional) Returns only the filename without extension. Default false.</param>
        /// <returns>List of strings of absolute path names or filenames.</returns>
        private static List<string> GetFileListFromDirectory( string sourcePath, string multiSearchPattern, SearchOption searchOption, bool sortResults = false, bool filenameOnly = false )
        {
            if ( !Directory.Exists( sourcePath ) )
            {
                Debug.LogWarningFormat( "SupportFileIO:GetFileListFromDirectory: Directory does not exist.\n{0}", sourcePath );
                return new List<string>();
            }

            string[]        searchPatterns  = multiSearchPattern.Split('|');
            List<string>    results         = new List<string>();

            // Could probably optimise this if searchPattern is '*.*'
            if ( filenameOnly )
            {
                // Stripping extension per searchPattern will be more efficient overal, in performance and memory usage.
                foreach ( string searchPattern in searchPatterns )
                {
                    string[] tmpFiles = Directory.GetFiles( sourcePath, searchPattern, searchOption );

                    foreach ( string file in tmpFiles )
                        results.Add( Path.GetFileNameWithoutExtension( file ) );
                }
            }
            else
            {
                foreach ( string searchPattern in searchPatterns )
                    results.AddRange( Directory.GetFiles( sourcePath, searchPattern, searchOption ) );
            }

            if ( sortResults )
                results.Sort();

            return results;
        }
        #endregion


        #region DUMP DIRECTORY CONTENTS
        public static string DumpDirectoryContents( string sourcePath, bool subFolders )
        {
            if ( !Directory.Exists( sourcePath ) )
            {
                Debug.LogWarningFormat( "SupportFileIO:DumpDirectoryContents: Directory does not exist.\n{0}", sourcePath );
                return "ERROR";
            }

            StringBuilder info      = new StringBuilder();

            DumpDirectory( sourcePath, subFolders, ref info );

            return info.ToString();
        }

        public static void DumpDirectory( string sourcePath, bool subFolders, ref StringBuilder info )
        {
            string[] directories    = Directory.GetDirectories(sourcePath);

            info.AppendFormat( "Path: {0}\n", sourcePath );

            string[] filelist = Directory.GetFiles(sourcePath);

            foreach ( string file in filelist )
                info.AppendFormat( "   File: {0}\n", file );

            if ( subFolders )
            {
                foreach ( string directory in directories )
                    DumpDirectory( directory, subFolders, ref info );
            }
        }
        #endregion

        #region VALIDATE FILEIO SUPPORT METHODS
       
        // TODO - Replace PrintToConsole with StringOutputLogger
#if UNITY_EDITOR
        [UnityEditor.MenuItem( "Window/NoiseCrimeStudios/Validations/FileIO RelativeToAbsolutePaths", false, 1300 )]        
#endif
        private static void ValidateRelativeToAbsolutePathExamples()
        {
            string realtivePath0 = @"MyFolder\MySubFolder\FinalFolder";
            string realtivePath1 = @"MyFolder/MySubFolder/FinalFolder";
            string realtivePath2 = @"..\MyFolder\MySubFolder\FinalFolder";
            string realtivePath3 = @"..\..\MyFolder\MySubFolder\FinalFolder";

            string log = "Get Absolute Path Examples:\n";
            log += string.Format( "[{0}]\tResult: '{1}'\n", realtivePath0, PathUtils.GetAbsolutePath( realtivePath0 ) );
            log += string.Format( "[{0}]\tResult: '{1}'\n", realtivePath1, PathUtils.GetAbsolutePath( realtivePath1 ) );
            log += string.Format( "[{0}]\tResult: '{1}'\n", realtivePath2, PathUtils.GetAbsolutePath( realtivePath2 ) );
            log += string.Format( "[{0}]\tResult: '{1}'\n", realtivePath3, PathUtils.GetAbsolutePath( realtivePath3 ) );
            log += "\nGet Absolute Full Path Examples:\n";
            log += string.Format( "[{0}]\tResult: '{1}'\n", realtivePath0, PathUtils.GetAbsoluteFullPath( realtivePath0 ) );
            log += string.Format( "[{0}]\tResult: '{1}'\n", realtivePath1, PathUtils.GetAbsoluteFullPath( realtivePath1 ) );
            log += string.Format( "[{0}]\tResult: '{1}'\n", realtivePath2, PathUtils.GetAbsoluteFullPath( realtivePath2 ) );
            log += string.Format( "[{0}]\tResult: '{1}'\n", realtivePath3, PathUtils.GetAbsoluteFullPath( realtivePath3 ) );
            Debug.Log( log );
        }

#if UNITY_EDITOR
        [UnityEditor.MenuItem( "Window/NoiseCrimeStudios/Validations/FileIO GetListFromDirectory", false, 1300 )]        
#endif
        private static void ValidateGetListFromDirectory()
        {
            bool            truncate            = true;
            string          sourcePath1         = Application.dataPath + Path.AltDirectorySeparatorChar + "NoiseCrimeStudios";
            string          sourcePath2         = Application.dataPath + Path.AltDirectorySeparatorChar + "NoiseCrimeStudios" + Path.AltDirectorySeparatorChar;
            string          sourcePath3         = @"C:\Windows\System32\"; // Application.dataPath + Path.AltDirectorySeparatorChar;
            string          searchPattern1      = "*.*";
            string          searchPattern2      = "*.meta";
            string          resultsDump;

            // NOTE:
            // When parsing Unity folders you'll end up with both the file and the meta file.
            // When you return filename only without extension that will result in file having striped extension, but the meta file stripped will be filename.ext!
            //
            // sourcePath3 Cannot use AllDirectories as some system folders are restricted and throw an exception.

            FileListTest( sourcePath1, searchPattern1, SearchOption.TopDirectoryOnly, true, false, truncate );
            FileListTest( sourcePath1, searchPattern1, SearchOption.TopDirectoryOnly, true, true, truncate );

            FileListTest( sourcePath2, searchPattern1, SearchOption.TopDirectoryOnly, true, false, truncate );
            FileListTest( sourcePath2, searchPattern1, SearchOption.TopDirectoryOnly, true, true, truncate );

            FileListTest( sourcePath1, searchPattern1, SearchOption.AllDirectories, true, false, truncate );
            FileListTest( sourcePath1, searchPattern1, SearchOption.AllDirectories, true, true, truncate );
            FileListTest( sourcePath1, searchPattern2, SearchOption.AllDirectories, true, false, truncate );
            FileListTest( sourcePath1, searchPattern2, SearchOption.AllDirectories, true, true, truncate );

            FileListTest( sourcePath2, searchPattern1, SearchOption.AllDirectories, true, false, truncate );
            FileListTest( sourcePath2, searchPattern1, SearchOption.AllDirectories, true, true, truncate );
            FileListTest( sourcePath2, searchPattern2, SearchOption.AllDirectories, true, false, truncate );
            FileListTest( sourcePath2, searchPattern2, SearchOption.AllDirectories, true, true, truncate );

            FileListTest( sourcePath3, searchPattern1, SearchOption.TopDirectoryOnly, true, false, truncate );
            FileListTest( sourcePath3, searchPattern1, SearchOption.TopDirectoryOnly, true, true, truncate );

            resultsDump = DumpDirectoryContents( sourcePath1, false );
            Debug.Log( "DumpDirectoryContents.TopDirectoryOnly\n" + resultsDump );

            resultsDump = DumpDirectoryContents( sourcePath1, true );
            Debug.Log( "DumpDirectoryContents.AllDirectories\n" + resultsDump );
        }

        private static void FileListTest( string path, string multiSearchPattern, SearchOption searchOption, bool sorted, bool filenameOnly, bool truncate )
        {
            string header = string.Format("GetFileListFromDirectory - '{0}' SearchOption.{1}{2}{3}\n",
                multiSearchPattern, searchOption, sorted ? " - Sorted" : "", filenameOnly ? " - filename" : "" );

            List<string> results = GetFileListFromDirectory( path, multiSearchPattern, searchOption, sorted, filenameOnly);
            PrintToConsole( header + path, results, truncate );
        }

        /// <summary>Print list of strings to console avoiding truncation.</summary>	
        private static void PrintToConsole( string header, List<string> stringList, bool truncate = false )
        {
            StackTraceLogType traceType = Application.GetStackTraceLogType( LogType.Log );
            Application.SetStackTraceLogType( LogType.Log, StackTraceLogType.None );

            StringBuilder   stringBuilder    = new StringBuilder();
            int             lineCount       = 0;

            stringBuilder.AppendLine( string.Format( "{0} Count: {1}\n", header, stringList.Count ) );

            foreach ( string s in stringList )
            {
                stringBuilder.AppendLine( s );
                lineCount++;

                // Unity console will truncate a long string, so we limit and log it here if necessarry.                
                if ( stringBuilder.Length > 16000 )
                {
                    if ( truncate )
                        break;
                    else
                    {
                        stringBuilder.AppendLine( "" );
                        Debug.Log( stringBuilder.ToString() );

                        stringBuilder.Length = 0;
                        stringBuilder.AppendLine( string.Format( "{0} continued ... {1}", header, lineCount ) );
                    }
                }
            }

            if ( stringBuilder.Length > 0 )
                Debug.Log( stringBuilder.ToString() );

            Application.SetStackTraceLogType( LogType.Log, traceType );
        }
        #endregion
    }
}
