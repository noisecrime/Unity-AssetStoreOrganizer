using System;
using System.Diagnostics;
using System.IO;
using UnityEditor;
using UnityEngine;

namespace NoiseCrimeStudios.Toolbox.AssetStoreOrganizer
{
    // Alias
    using Debug = UnityEngine.Debug;

    public static class OrganizerMethods
    {
        static string embeddedPackageFolder   = Application.dataPath + Path.DirectorySeparatorChar + "EmbeddedPackages";

		public static void ImportPackage( string package)
		{
			try
			{
				AssetDatabase.ImportPackage( package, true );
			}
			catch ( Exception )
			{
				Debug.LogError( "Failed to import package: " + package );
				throw;
			}
		}

		public static void EmbedPackage( string package)
		{
            if ( !Directory.Exists( embeddedPackageFolder ) )
            {
                Directory.CreateDirectory( embeddedPackageFolder );

                if ( !Directory.Exists( embeddedPackageFolder ) )
                {
                    Debug.LogError( "Unable to create embeddedPackageFolder for: " + package );
                    return;
                }
            }
            
			try
			{
				File.Copy(package, embeddedPackageFolder + Path.DirectorySeparatorChar + Path.GetFileName(package) );
				AssetDatabase.Refresh();
			}
			catch ( Exception )
			{
				Debug.LogError( "Failed to Copy package: " + package );
				throw;
			}
		}

		public static void OpenPackageInExplorer( string package)
		{
			Process.Start(new System.Diagnostics.ProcessStartInfo()
			{
				FileName		= Path.GetDirectoryName(package),
				UseShellExecute = true,
				Verb			= "open"
			} );			
		}

       public static void OpenDirectoryInExplorer( string path)
		{
			Process.Start(new System.Diagnostics.ProcessStartInfo()
			{
				FileName		= path,
				UseShellExecute = true,
				Verb			= "open"
			} );			
		}
    }
}
