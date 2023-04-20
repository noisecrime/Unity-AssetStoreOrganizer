using System.IO;
using UnityEngine;

namespace NoiseCrimeStudios.Core.IO
{
    public enum PathType
    {
        Unknown,
        File,
        Directory
    }

    /// <summary>Collection of System.IO Utility Methods.</summary>
    /// <remarks>
    /// FilePathValidation
    /// <see cref="Assets\NoiseCrimeStudios\Core\Scripts\FileSupport\FilePathValidation.cs"/>
    /// UnityEditor.FileUtil is Unity's own collection of useful methods
    /// <see cref="UnityCsReference\Editor\Mono\FileUtil.cs"/>
    /// </remarks>
    public static class PathUtils
    {
        // Note:
        // Windows Path.DirectorySeparatorChar     = '\' = BackSlash
        // Windows Path.AltDirectorySeparatorChar  = '/' = ForwardSlash
        // Could use Application.platform to avoid some processing here.
        // Might want to add UnityEditor.FileUtil path methods for dealing with Windows Network!
        // UnityEditor.FileUtil.NiceWinPath Replaces forward with Back Slashes!

        /// <summary>Returns the path with FORWARD slash replaced with BACK slash to match Unity style.</summary>
        public static string ReplaceForwardWithBackSlash( string path )
        {
            return path.Replace( "/", @"\" );
        }

        /// <summary>Returns the path with BACK slash replaced with FORWARD slash.</summary>
        public static string ReplaceBackWithForwardSlash( string path )
        {
            return path.Replace( @"\", "/" );
        }

        /// <summary>Splits a file path by seperator into string array.</summary>
        public static string[] SplitFilePath( string path )
        {
            return path.Split( new char[] { Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar } );
        }

        /// <summary>Force path to use DirectorySeparatorChar instead of AltDirectorySeparatorChar to match Unity Style. Win: '/' = '\' - FORWARD replaced with BACK Slash.</summary>
        public static string GetConsistentFilePath( string path )
        {
            return path.Replace( Path.AltDirectorySeparatorChar, Path.DirectorySeparatorChar );
        }

        /// <summary>Force path to use AltDirectorySeparatorChar instead of DirectorySeparatorChar. Win: '\' = '/' - BACK replaced with FORWARD Slash.</summary>
        public static string GetAltConsistentFilePath( string path )
        {
            return path.Replace( Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar );
        }

        /// <summary>Returns the name of the final directory in the path. Works for files or folders</summary>
        public static string GetNameOfDirectory( string directoryPath )
        {
            return new DirectoryInfo( directoryPath ).Name;
        }

        /// <summary>Returns the absolute path based on the Application.dataPath.</summary>
        /// <remarks>e.g. F:\Folder\SubFolder\Project\Assets + / + relativePath</remarks>
        public static string GetAbsolutePath( string relativePath )
        {
            return Path.GetFullPath( Path.Combine( Application.dataPath, relativePath ) );
        }

        /// <summary>Returns the absolute path based on the Application Path (will not include Assets folder).</summary>
        /// <remarks>e.g. F:\Folder\SubFolder\Project\ + / + relativePath</remarks>
        public static string GetAbsoluteFullPath( string relativePath )
        {
            return Path.GetFullPath( relativePath );
        }

        /// <summary>Returns the relative path ( to Unity Assets folder ) of an absolute path.</summary>
        public static string GetRelativeAssetPath( string absolutePath )
        {
            return absolutePath.Replace( Application.dataPath, "Assets" );
        }

        /// <summary>Checks if a path is to a Directory, File or Unknown</summary>
        public static bool? IsPathToDirectory( string path )
        {
            if ( string.IsNullOrEmpty( path ) )
                return null;
            if ( !Directory.Exists( path ) && !File.Exists( path ) )
                return null;
            FileAttributes attribute = File.GetAttributes(path);
            return FileAttributes.Directory == ( attribute & FileAttributes.Directory );
            //	return attribute.HasFlag(FileAttributes.Directory);
        }

        /// <summary>Checks if a path is to a Directory, File or Unknown</summary>
        public static PathType GetPathType( string path )
        {
            if ( string.IsNullOrEmpty( path ) )
                return PathType.Unknown;
            if ( !Directory.Exists( path ) && !File.Exists( path ) )
                return PathType.Unknown;
            FileAttributes attribute = File.GetAttributes( path );
            return FileAttributes.Directory == ( attribute & FileAttributes.Directory ) ? PathType.Directory : PathType.File;
            //	return attribute.HasFlag(FileAttributes.Directory) ? PathType.Directory : PathType.File;
        }

        /*
        using System.Linq;
        /// <summary>Combine Multiple paths into a single one using Path.Combine.</summary>
        /// <remarks>
        /// https://stackoverflow.com/questions/1996139/in-c-how-do-i-combine-more-than-two-parts-of-a-file-path-at-once
        /// In 2017.1 exp and 2018.1 .Net4 Path.Combine does this natively!
        /// </remarks>   
        public static string CombinePaths( string path, params string[] paths )
        {
            if ( path == null )            
                throw new System.ArgumentNullException( "path" );
            
            if ( paths == null )            
                throw new System.ArgumentNullException( "paths" );
            
            return paths.Aggregate( path, ( acc, p ) => Path.Combine( acc, p ) );
        }*/
    }
}
