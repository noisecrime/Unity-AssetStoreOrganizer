using System;
using System.IO;
using NoiseCrimeStudios.Core;
using UnityEditor;
using UnityEngine.Assertions;

namespace NoiseCrimeStudios.Toolbox.AssetStoreOrganizer
{
    // Aliases
    using Debug = UnityEngine.Debug;

    public enum PackagesLocation		
    {
        NativePackageList, 
        AssetStore,
        AssetStore5x,
        Custom,
        Archive
    }

    public static class OrganizerPaths
    {
        // Gets the root directory for Unity application data.
        private static readonly string s_storeDirectoryRoot    = Path.Combine( EnvironmentPath,  @"Unity");
        // Gets the Unity AssetStore legacy directory - pre Unity 5.x.
        private static readonly string s_storeDirectoryLegacy  = Path.Combine( EnvironmentPath,  @"Unity\Asset Store");
        // Gets the Unity AssetStore modern directory from Unity 5.x and up.
        private static readonly string s_storeDirectoryModern  = Path.Combine( EnvironmentPath,  @"Unity\Asset Store-5.x");
        
        // Static strings for Editor Preference Keys
        private static readonly string  s_customDirectoryEditorKey = @"com.noisecrimestudios.assetStoreManager.customDirectory";
        private static readonly string  s_backupDirectoryEditorKey = @"com.noisecrimestudios.assetStoreManager.backupDirectory";


        /// <summary>Get/Set the directory used for custom assetstore location.</summary>
        public static string StoreDirectoryCustom
        {
            get { return EditorPrefs.GetString( s_customDirectoryEditorKey, string.Empty); }
            set { EditorPrefs.SetString( s_customDirectoryEditorKey, value); }
        }

        /// <summary>Get/Set the directory used for assetstore backup location.</summary>
        public static string StoreDirectoryBackup
        {
            get { return EditorPrefs.GetString( s_backupDirectoryEditorKey, string.Empty); }
            set { EditorPrefs.SetString( s_backupDirectoryEditorKey, value); }
        }
        
        /// <summary>Gets the parent Directory for the Unity Application from system environment.</summary>
        public static string EnvironmentPath
        {
            get { return Environment.GetFolderPath( Environment.SpecialFolder.ApplicationData ); }
        }

        /// <summary>Gets the root directory for Unity application data.</summary>
        public static string StoreDirectoryRoot     { get { return s_storeDirectoryRoot; } }

        /// <summary>Gets the Unity AssetStore legacy directory - pre Unity 5.x.</summary>
        public static string StoreDirectoryLegacy   { get { return s_storeDirectoryLegacy; } }

        /// <summary>Gets the Unity AssetStore modern directory from Unity 5.x and up.</summary>
        public static string StoreDirectoryModern   { get { return s_storeDirectoryModern; } }

        /// <summary>Resturns the directory path to the desired Packages Location.</summary>
        public static string GetPackageDirectory( PackagesLocation packagesLocation )
        {
            switch ( packagesLocation )
            {
                case PackagesLocation.NativePackageList:
                    return StoreDirectoryRoot;
                case PackagesLocation.AssetStore:
                    return StoreDirectoryLegacy;
                case PackagesLocation.AssetStore5x:
                    return StoreDirectoryModern;
                case PackagesLocation.Custom:
                    return StoreDirectoryCustom;
                case PackagesLocation.Archive:
                    return StoreDirectoryBackup;
                default:
                    Assert.IsTrue( false, string.Format( "StorePackageLibrary:GetPackageDirectory: Unhandled enum in switch case {0}", packagesLocation ) );
                    return "";
            }
        }

        /// <summary>Sets the directory path for the desired packages location if applicable.</summary>
        public static void SetPackageDirectory( PackagesLocation packagesLocation, string path )
        {
            switch ( packagesLocation )
            {
                case PackagesLocation.NativePackageList:
                case PackagesLocation.AssetStore:
                case PackagesLocation.AssetStore5x:
                    Debug.LogErrorFormat("StorePackageLibrary:SetPackageDirectory: Setting {0} is not permitted.", packagesLocation );
                    break;
                case PackagesLocation.Custom:
                    StoreDirectoryCustom = path;
                    break;
                case PackagesLocation.Archive:
                    StoreDirectoryBackup = path;    
                    break;
                default:   
                    Assert.IsTrue( false, string.Format( "StorePackageLibrary:SetPackageDirectory: Unhandled enum in switch case {0}", packagesLocation ) );                    
                    break;
            }
        }

        public static void LogToConsole()
        {
            StringBuilderDebugLog debugLogOutput = new StringBuilderDebugLog();
            debugLogOutput.AppendFormat( "[NativePackageList]\t {0}", StoreDirectoryRoot );
            debugLogOutput.AppendFormat( "[AssetStore Legacy]\t {0}", StoreDirectoryLegacy );
            debugLogOutput.AppendFormat( "[AssetStore Modern]\t {0}", StoreDirectoryModern );
            debugLogOutput.AppendFormat( "[Custom Path]\t {0}", StoreDirectoryCustom );
            debugLogOutput.AppendFormat( "[Archive Path]\t {0}", StoreDirectoryBackup );
            debugLogOutput.LogToConsole();
        }
    }
}


/// <summary>Returns the location of the AssetStore default Root location.</summary>
// public string AssetStoreDirectoryRoot    { get; private set; }
/// <summary>Returns the location of the old legacy AssetStore location.</summary>
//    public string AssetStoreDirectoryLegacy  { get; private set; }
/// <summary>Returns the location of the old legacy AssetStore location.</summary>
//    public string AssetStoreDirectoryModern  { get; private set; }

/*
        public StorePackageLibrary()
        {
            string appdata              = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);

            AssetStoreDirectoryRoot     = Path.Combine( appdata,  @"Unity");
            AssetStoreDirectoryLegacy   = Path.Combine( appdata,  @"Unity\Asset Store");
            AssetStoreDirectoryModern   = Path.Combine( appdata,  @"Unity\Asset Store-5.x");
        }
*/
    //    if ( null == storeDirectoryRoot) storeDirectoryRoot = Path.Combine( EnvironmentPath,  @"Unity");
    //    if ( null == storeDirectoryLegacy) storeDirectoryLegacy = Path.Combine( EnvironmentPath,  @"Unity\Asset Store");
    //    if ( null == storeDirectoryModern) storeDirectoryModern = Path.Combine( EnvironmentPath,  @"Unity\Asset Store-5.x");
