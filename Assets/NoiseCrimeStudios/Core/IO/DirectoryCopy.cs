using System.IO;

namespace NoiseCrimeStudios.Core.IO
{
    public static partial class DirectoryUtils
    {
        /// <summary>Provides various settings for use when Copying Directories.</summary>
        public class CopyDirectorySettings
        {
            public bool         enablePackageSizeLimit;
            public int          packageSizeByteLimit;
            public string[]     excludeSubDirectories;
        }


        // Copy Directory Contents
        // https://stackoverflow.com/questions/58744/how-to-copy-the-entire-contents-of-directory-in-c#3822913
        // TODO: Improve this - write class for stringBuilder, enable debug output or not etc.
        // See CustomAssetStoreBackUp.cs - originally used for copying Unity assetStore folder to a back up location.
        // Note - we could try Unity's own FileUtil.CopyFileOrDirectory()
        public static void CopyDirectory( string sourceDirectory, string targetDirectory, CopyDirectorySettings copyDirectorySettings, bool enableLogging )
        {
            DirectoryInfo       diSource    = new DirectoryInfo(sourceDirectory);
            DirectoryInfo       diTarget    = new DirectoryInfo(targetDirectory);

            StringBuilderDebugLog  output      = new StringBuilderDebugLog( enableLogging );

            if ( output.ExternalEnableLogging ) output.AppendLine( "-- FileCopying:CopyDirectory -- " );

            CopyDirectoryAll( diSource, diTarget, copyDirectorySettings, output );

            if ( output.ExternalEnableLogging )
            {
                output.InsertHardBreak();
                output.AppendLine( "-- FileCopying:CopyDirectory -- " );
                output.LogToConsole( true );
            }
        }


        static void CopyDirectoryAll( DirectoryInfo source, DirectoryInfo target, CopyDirectorySettings copyDirectorySettings, StringBuilderDebugLog output )
        {
            // Will create all directories up to target.
            Directory.CreateDirectory( target.FullName );

            if ( output.ExternalEnableLogging )
            {
                output.AppendFormat( "Copying Directory:\n\tSrc: {0}\n\tDst: {1}\n", source.FullName, target.FullName );
                output.IndentIncrement();
            }

            // Copy each file into the new directory.
            foreach ( FileInfo fi in source.GetFiles() )
            {
                if ( output.ExternalEnableLogging ) output.AppendFormat( "{0}", fi.Name );
                fi.CopyTo( Path.Combine( target.FullName, fi.Name ), true );
            }

            // Copy each subdirectory using recursion.
            foreach ( DirectoryInfo diSourceSubDir in source.GetDirectories() )
            {
                string  subDirectoryName    = diSourceSubDir.Name;
                bool    isExcluded          = false;

                foreach ( string exclusion in copyDirectorySettings.excludeSubDirectories )
                {
                    if ( subDirectoryName == exclusion )
                    {
                        isExcluded = true;
                        if ( output.ExternalEnableLogging ) output.AppendFormat( "{0} is Excluded", subDirectoryName );
                        break;
                    }
                }

                if ( !isExcluded )
                {
                    DirectoryInfo nextTargetSubDir = target.CreateSubdirectory(subDirectoryName);
                    CopyDirectoryAll( diSourceSubDir, nextTargetSubDir, copyDirectorySettings, output );
                }
            }

            if ( output.ExternalEnableLogging ) output.IndentDecrement();
        }
    }
}
