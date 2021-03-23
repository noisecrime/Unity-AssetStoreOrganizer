using System.IO;
using System.Text;
using UnityEngine;

/// <remarks WINDOWS>
/// System.IO
/// Path.DirectorySeparatorChar					= '\' = BackSlash
/// Path.AltDirectorySeparatorChar				= '/' = ForwardSlash
/// 
/// Path.Combine								- Inserts BACKSLASH seperator between paths items, but retains each strings own seperators.
/// Path.GetDirectoryName						- Converts all seperators to BACKSLASH.
/// Path.GetFullPath							- Converts all seperators to BACKSLASH.
/// 
/// UnityEngine.Application.Paths				- All seperators are FORWARDSLASH.
/// 
/// 
/// UnityEditor.EditorUtility.OpenFolderPanel	- All seperators are FORWARDSLASH 	
/// </remarks>

/// <remarks MacOS>
/// System.IO 	
/// Path.DirectorySeparatorChar		= '/' ?	 
/// Path.AltDirectorySeparatorChar	= '/' ?
/// </remarks>

/// <remarks Linux>
/// System.IO 
/// Path.DirectorySeparatorChar		= '/' = BackSlash	 
/// Path.AltDirectorySeparatorChar	= '/' = ForwardSlash
/// </remarks>

namespace NoiseCrimeStudios.Core.IO
{
    /// <summary>Explores descrepencies in use of directory seperators in Windows and Unity.</summary>
    /// <remarks>
    /// UnityEditor.FileUtil is Unity's own collection of useful methods
    /// <see cref="UnityCsReference\Editor\Mono\FileUtil.cs"/>
    /// </remarks>
    public static class ValidationOfFilePaths
    {
#if UNITY_EDITOR
        [UnityEditor.MenuItem( "Window/NoiseCrimeStudios/Validations/FilePaths", false, 1300 )] // WindowMenuItemPriorty.Validations
#endif
        public static void LogFilePathValidations()
        {
            string examplePathFwd   = @"SubAssets/Fwd/Textures";
            string examplePathBck   = @"SubAssets\Bck\Textures";

            string pathCombineFwd   = Path.Combine( Application.dataPath, examplePathFwd ) ;
            string pathCombineBck   = Path.Combine( Application.dataPath, examplePathBck ) ;
#if NET_4_6
           	string pathCombineFwd46	= Path.Combine( new string[] { Application.dataPath, examplePathFwd } ) ;
            string pathCombineBck46	= Path.Combine( new string[] { Application.dataPath, examplePathBck } ) ;
#endif
            string getFullPathFwd   = Path.GetFullPath( examplePathFwd );
            string getFullPathBck   = Path.GetFullPath( examplePathBck );
            string directoryNameFwd = Path.GetDirectoryName( pathCombineFwd );
            string directoryNameBck = Path.GetDirectoryName( pathCombineBck );
            string getPathRootFwd   = Path.GetPathRoot( pathCombineFwd );
            string getPathRootBck   = Path.GetPathRoot( pathCombineBck );


            StringBuilder sb = new StringBuilder();

            sb.AppendFormat( "Platform: {0}  Date: {1}\n",                  Application.platform, System.DateTime.UtcNow );
#if UNITY_2018_4_OR_NEWER
            sb.AppendFormat( "OS Family: {0}  OS: {1}\n",                   SystemInfo.operatingSystemFamily, SystemInfo.operatingSystem );
#else
            sb.AppendFormat( "OS: {0}\n",                                   SystemInfo.operatingSystem );
#endif
            sb.AppendFormat( "Device: Model: {0}  Type: {1}  Name: {2}\n",  SystemInfo.deviceModel, SystemInfo.deviceType, SystemInfo.deviceName );
            sb.AppendLine( "----------------------------------------" );
            sb.AppendFormat( "DirectorySeparatorChar\t'{0}'\n",             Path.DirectorySeparatorChar );
            sb.AppendFormat( "AltDirectorySeparatorChar\t'{0}'\n",          Path.AltDirectorySeparatorChar );
            sb.AppendLine( "----------------------------------------" );
            sb.AppendFormat( "[{0}]\t[Application.dataPath]\n",             Application.dataPath );
            sb.AppendFormat( "[{0}]\t[Application.streamingAssetsPath]\n",  Application.streamingAssetsPath );
            sb.AppendFormat( "[{0}]\t[Application.persistentDataPath]\n",   Application.persistentDataPath );
            sb.AppendFormat( "[{0}]\t[Application.temporaryCachePath]\n",   Application.temporaryCachePath );
            sb.AppendLine( "----------------------------------------" );
            sb.AppendFormat( "[{0}]\t[examplePathFwd]\n",                           examplePathFwd );
            sb.AppendFormat( "[{0}]\t[examplePathBck)]\n",                          examplePathBck );
            sb.AppendFormat( "[{0}]\t[Path.Combine(dataPath, pathCombineFwd]\n",    pathCombineFwd );
            sb.AppendFormat( "[{0}]\t[Path.Combine(dataPath, pathCombineBck)]\n",   pathCombineBck );
#if NET_4_6
            sb.AppendFormat( "[{0}]\t[Path.Combine(dataPath, pathCombineFwd46]\n",	pathCombineFwd46 );
            sb.AppendFormat( "[{0}]\t[Path.Combine(dataPath, pathCombineBck46)]\n",	pathCombineBck46 );
#endif

            sb.AppendFormat( "[{0}]\t[Path.GetFullPath(examplePathFwd)]\n",         getFullPathFwd );
            sb.AppendFormat( "[{0}]\t[Path.GetFullPath(examplePathBck)]\n",         getFullPathBck );
            sb.AppendFormat( "[{0}]\t[Path.GetDirectoryName(pathCombineFwd)]\n",    directoryNameFwd );
            sb.AppendFormat( "[{0}]\t[Path.GetDirectoryName(pathCombineBck)]\n",    directoryNameBck );
            sb.AppendFormat( "[{0}]\t[Path.GetPathRoot(pathCombineFwd)]\n",         getPathRootFwd );
            sb.AppendFormat( "[{0}]\t[Path.GetPathRoot(pathCombineBck)]\n",         getPathRootBck );
            sb.AppendLine( "----------------------------------------" );

            Debug.Log( sb.ToString() );
        }
    }
}



/*
Platform: WindowsEditor  Date: 29/06/2020 22:54:35
OS Family: Windows  OS: Windows 10  (10.0.0) 64bit
Device: Model: MS-7885 (MSI)  Type: Desktop  Name: TYPHOON
----------------------------------------
DirectorySeparatorChar	    '\'
AltDirectorySeparatorChar	'/'
----------------------------------------
[F:/Clients_2016/QEP/Research/VoxelBusters/CPNP 1.5.7p4 (U18.4.23f)/Assets]	[Application.dataPath]
[F:/Clients_2016/QEP/Research/VoxelBusters/CPNP 1.5.7p4 (U18.4.23f)/Assets/StreamingAssets]	[Application.streamingAssetsPath]
[C:/Users/Noise/AppData/LocalLow/NoiseCrimeStudios/CPNP 1_5_7p4 (U18_4_23f)]	[Application.persistentDataPath]
[C:/Users/Noise/AppData/Local/Temp/NoiseCrimeStudios/CPNP 1_5_7p4 (U18_4_23f)]	[Application.temporaryCachePath]
----------------------------------------
[F:/Clients_2016/QEP/Research/VoxelBusters/CPNP 1.5.7p4 (U18.4.23f)/Assets\SubAssets/Fwd/Textures]	[Path.Combine(dataPath, pathCombineFwd]
[F:/Clients_2016/QEP/Research/VoxelBusters/CPNP 1.5.7p4 (U18.4.23f)/Assets\SubAssets\Bck\Textures]	[Path.Combine(dataPath, pathCombineBck)]
[F:\Clients_2016\QEP\Research\VoxelBusters\CPNP 1.5.7p4 (U18.4.23f)\SubAssets\Fwd\Textures]	[Path.GetFullPath(examplePathFwd)]
[F:\Clients_2016\QEP\Research\VoxelBusters\CPNP 1.5.7p4 (U18.4.23f)\SubAssets\Bck\Textures]	[Path.GetFullPath(examplePathBck)]
[F:\Clients_2016\QEP\Research\VoxelBusters\CPNP 1.5.7p4 (U18.4.23f)\Assets\SubAssets\Fwd]	[Path.GetDirectoryName(pathCombineFwd)]
[F:\Clients_2016\QEP\Research\VoxelBusters\CPNP 1.5.7p4 (U18.4.23f)\Assets\SubAssets\Bck]	[Path.GetDirectoryName(pathCombineBck)]
[F:/]	[Path.GetPathRoot(pathCombineFwd)]
[F:/]	[Path.GetPathRoot(pathCombineBck)]
----------------------------------------
*/
