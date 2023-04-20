using System;
using System.Diagnostics;
using System.IO;
using UnityEditor;
using UnityEngine;
using Debug = UnityEngine.Debug;

namespace NoiseCrimeStudios.Toolbox.AssetStoreOrganizer
{
    public static class OrganizerMethods
    {
        private static readonly string s_embeddedPackageFolder   = Application.dataPath + Path.DirectorySeparatorChar + "EmbeddedPackages";

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
            if ( !Directory.Exists( s_embeddedPackageFolder ) )
            {
                Directory.CreateDirectory( s_embeddedPackageFolder );

                if ( !Directory.Exists( s_embeddedPackageFolder ) )
                {
                    Debug.LogError( "Unable to create embeddedPackageFolder for: " + package );
                    return;
                }
            }
            
            try
            {
                File.Copy(package, s_embeddedPackageFolder + Path.DirectorySeparatorChar + Path.GetFileName(package) );
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
