using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Text.RegularExpressions;
using NoiseCrimeStudios.Core.Formatting;
using NoiseCrimeStudios.Core.IO;
using UnityEditor;
using UnityEngine;

namespace NoiseCrimeStudios.Toolbox.AssetStoreOrganizer
{
    /// <summary>
    /// AssetStorePackage Container
    /// </summary>
    /// <remarks>
    /// Editor\Mono\AssetStore\AssetStoreContext.cs
    /// </remarks>
    public class AssetPackage
    {
        /*   Sample Package Info JSON format
         *   Our class and field naming style must match these for Json parsing to work!
        {
            "link":
            {
                "id":"44869",
                "type":"content"
            },
            "unity_version":"4.6.6f2",
            "pubdate":"23 Sep 2015",
            "version":"0.50",
            "upload_id":"82586",
            "version_id":"138819",
            "category":
            {
                "id":"161",
                "label":"3D Models/Characters/UMA Avatars"
            },
            "id":"44869",
            "title":"AL Female Civilian Pack for UMA",
            "publisher":
            {
                "id":"7656",
                "label":"AlienLab"
            }
        }
        */

#pragma warning disable IDE1006 // Naming Styles

        [Serializable]
        public class StorePackageLink
        {
            public string id;
            public string type;
        }

        [Serializable]
        public class StorePackageLabelWithID
        {
            public string id;
            public string label;
        }        		

        // Unity Package Properties from Json
        public string					unity_version;
        public string					pubdate;
        public string					version;
        public string					description;			// Not in JSON?
        public int						upload_id;
        public int						version_id;
        public int						id;
        public string					title;
        public string					baseCategory;			// Cached base category for easy filter generation.

        public StorePackageLink			link;
        public StorePackageLabelWithID	category;
        public StorePackageLabelWithID	publisher;

#pragma warning restore IDE1006 // Naming Styles

        // Custom Properties
        public string					FullFilePath;
        public long						FileSize;
        public bool						IsUnityStandardAsset;
        public bool                     IsArchived;

        public DateTime					ModifiedDate;			// DateTime for correct sorting
        public DateTime					PublishDate;			// DateTime for correct sorting

        // Precalculate display values for optimal performance
        public string					DisplayModifiedDate;	// Display date
        public string					DisplayFileSize;		// Display FileSize

        // See Editor\Mono\AssetStore\AssetStoreContext.cs
        private static readonly Regex   s_standardPackage52RegExp   = new Regex("/Standard Packages/(Character\\ Controller|Glass\\ Refraction\\ \\(Pro\\ Only\\)|Image\\ Effects\\ \\(Pro\\ Only\\)|Light\\ Cookies|Light\\ Flares|Particles|Physic\\ Materials|Projectors|Scripts|Standard\\ Assets\\ \\(Mobile\\)|Skyboxes|Terrain\\ Assets|Toon\\ Shading|Tree\\ Creator|Water\\ \\(Basic\\)|Water\\ \\(Pro\\ Only\\))\\.unitypackage$", RegexOptions.IgnoreCase);
    //	private static readonly Regex   s_standardPackage54RegExp   = new Regex("/Standard Packages/(2D\\  Assets|Toon\\ Shading|Tree\\ Creator|Water\\ \\(Basic\\)|Water\\ \\(Pro\\ Only\\))\\.unitypackage$", RegexOptions.IgnoreCase);
    //	private static readonly Regex   s_generatedIDRegExp			= new Regex("^\\{(.*)\\}$");
    //	private static readonly Regex   s_invalidPathCharsRegExp	= new Regex("[^a-zA-Z0-9() _-]");
        private static readonly int     s_packageMagicNum = 67668767;

        public override string ToString()
        {
            StringBuilder sb = new StringBuilder();
            sb.AppendFormat( "Title: {0}    [Size: {1}  UnityStandardAsset: {2}  IsArchived: {3}]\n", title, FileSize, IsUnityStandardAsset, IsArchived );
            sb.AppendFormat( "PubDate: {0}   unity_version: {1}  version: {2}  version_id: {3}  upload_id: {4}  id: {5}\n", pubdate, unity_version, version, version_id, upload_id, id );
            sb.AppendFormat( "Link: id: {0}  Type: {1}\n", link.id, link.type );
            sb.AppendFormat( "Category id: {0}  label: {1}  Base: {2}\n", category.id, category.label, baseCategory );
            sb.AppendFormat( "Publisher id: {0}  label: {1}\n", publisher.id, publisher.label );
            sb.AppendFormat( "Package: {0}\n", FullFilePath);  
            sb.AppendFormat( "Description: {0}\n", description);
            return sb.ToString();
        }

        public static long CreatePackageInfoFromLocalStorage( string fullFilePath, List<AssetPackage> results )
        {
            long sizeOnDisk = 0;

            try
            {
                byte[] jsonBytes   = null;

                // Read first 16 bytes header - last 2 bytes are json length
                using ( BinaryReader reader = new BinaryReader( new FileStream( fullFilePath, FileMode.Open ) ) )
                {
                    FileInfo    fileInfo    = new FileInfo(fullFilePath);
                    long        fileLength  = fileInfo.Length;

                    int magicNum = reader.ReadInt32();  // 67668767

                    //	Debug.LogFormat( "magicNum: {0}  Length {1}  Name: {2}\nLastWriteTime: {3} | {4}  LastAccessTime: {5}  CreationTime: {6}",
                    //		magicNum, dataLength, fullFilePath, fileInfo.LastWriteTime,  fileInfo.LastWriteTimeUtc, fileInfo.LastAccessTime, fileInfo.CreationTime);

                    if ( magicNum == s_packageMagicNum ) // 67668767
                    {
                        reader.BaseStream.Seek( 14, 0 );
                        ushort dataLength = reader.ReadUInt16();

                        if ( dataLength > 0 && dataLength + 16 < fileLength )
                            jsonBytes = reader.ReadBytes( dataLength );
                        else
                            Debug.LogErrorFormat( "CreatePackageInfoFromLocalStorage Error: Invalid Package\n{0}\nBad dataLength: {1}", fullFilePath, dataLength );
                    }
                    else
                    {
                        Debug.LogErrorFormat( "CreatePackageInfoFromLocalStorage Error: Invalid Package\n{0}\nBad magicNum: {1} Expected: {2}", fullFilePath, magicNum, s_packageMagicNum );
                    }
                }

                if ( null != jsonBytes )
                {
                    PackageInfo packageInfo = new PackageInfo
                    {
                        jsonInfo	= Encoding.ASCII.GetString( jsonBytes, 0, jsonBytes.Length ),
                        packagePath = fullFilePath,
                        iconURL		= null
                    };

                    sizeOnDisk = PopulateFromPackageInfo( packageInfo, results );
                }
            }
            catch ( IOException ex )
            {
                Debug.LogErrorFormat( "CreatePackageInfoFromLocalStorage Error: Invalid Package\n{0}\nError: {1}", fullFilePath, ex.Message );
            }

            return sizeOnDisk;
        }

        /// <summary>Central method to populate custom StorePackage from packageInfo</summary>		
        /// <returns>Returns on disk size of package.</returns>
        public static long PopulateFromPackageInfo( PackageInfo packageInfo, List<AssetPackage> results )
        {
            string fullFilePath         = PathUtils.GetConsistentFilePath( packageInfo.packagePath );

            AssetPackage	pack;
            FileInfo		fileInfo	= new FileInfo(fullFilePath);
        //	bool isUnityStandardAsset	= IsBuiltinStandardAsset( fullFilePath );
            bool isUnityStandardAsset	= fullFilePath.Contains("Editor/Standard Assets") || fullFilePath.Contains( @"Editor\Standard Assets");

            if ( !string.IsNullOrEmpty( packageInfo.jsonInfo ) )			
                pack = JsonUtility.FromJson<AssetPackage>( packageInfo.jsonInfo );			
            else
            {
                // Catch Unity Standard Assets here
                string publisherLabel   = !isUnityStandardAsset ? "NA" : "Unity Technologies";
                string publisherID      = !isUnityStandardAsset ? "NA" : "1";
                string categoryLabel    = !isUnityStandardAsset ? "NA" : "Prefab Packages"; // "Builtin StandardAsset";
                string categoryID		= !isUnityStandardAsset ? "NA" : "4";
                string title            = !isUnityStandardAsset ? fileInfo.Name : "BuiltinStandardAsset." + Path.GetFileNameWithoutExtension(fileInfo.Name);
                string version			= !isUnityStandardAsset ? Application.unityVersion : "4.3.4";  // "3.5.0.0";

                pack = BuildDefaultStorePackage( title, version, version, categoryLabel, categoryID, publisherLabel, publisherID );
            }

            pack.FullFilePath			= fullFilePath;
            pack.FileSize				= fileInfo.Length;
            pack.IsUnityStandardAsset	= isUnityStandardAsset;
            pack.ModifiedDate			= fileInfo.LastWriteTime;
            pack.PublishDate			= pack.pubdate == "NA" ? new DateTime() : DateTime.Parse(pack.pubdate);

            pack.DisplayModifiedDate	= pack.ModifiedDate.ToString("dd MMM yyyy");		//.ToString(@"MM\/dd\/yyyy HH:mm");
            pack.DisplayFileSize		= Numerical.ByteCountToSuffixHumbads(pack.FileSize);
            

            // Fix potential issues
            if ( string.IsNullOrEmpty( pack.unity_version ) )
                pack.unity_version = "NA";  // Prior to Unity 5.x this was empty

            // Replace & symbol as for popup menu's it is used for something else - keyboard shortcut maybe?
            pack.category.label			= pack.category.label.Replace("&", "and");

            pack.baseCategory			= pack.category.label.Split('/')[0];
                        
            /*
            // Extract Icon: No Point every Icon is default Unity Icon!
            if ( null != packageInfo.iconURL )
            {
                byte[] iconBytes	= System.Convert.FromBase64String(packageInfo.iconURL.Substring(22));
                System.IO.File.WriteAllBytes( Application.dataPath + "/testpng.png", iconBytes);
                uasp.iconTexture		= new Texture2D( 2, 2, TextureFormat.RGB24, false );
                uasp.iconTexture.LoadImage(iconBytes);				
            }
            */

            results.Add( pack );

            return pack.FileSize;
        }

        private static AssetPackage BuildDefaultStorePackage( string title, string unityVersion, string version, string category, string categoryID, string publisher, string publisherID )
        {
            AssetPackage uasp = new AssetPackage
            {
                link            = new StorePackageLink(),
                category        = new StorePackageLabelWithID(),
                publisher       = new StorePackageLabelWithID(),

                unity_version   = unityVersion,
                pubdate         = "NA",
                version         = version,
                upload_id       = -1,
                version_id      = -1,
                id              = -1,
                title           = title,
                description		="NA"
            };

            uasp.link.id			= "NA";
            uasp.link.type			= "NA";

            uasp.category.id		= categoryID;
            uasp.category.label		= category;

            uasp.publisher.id		= publisherID;
            uasp.publisher.label	= publisher;

            uasp.baseCategory		= uasp.category.label.Split('/')[0];

            return uasp;
        }        

        #region HELPERS
        // Note:
        // see Editor\Mono\AssetStore\AssetStoreContext.cs
        // This no longer works correctly in Unity 5.4 as standard asset package names have changed!
        // Seems its stuck with Unity 3.5 version standard assets. 
        // Unsure if this is broken or by design.
        private static bool IsBuiltinStandardAsset(string path)
        {
            return s_standardPackage52RegExp.IsMatch(path);
        }
        
        // public static bool IsPackageInProject() { }
                        
        public static bool GetAllAssetBundleNames()
        {
            string[] assetBundles = AssetDatabase.GetAllAssetBundleNames();

            Debug.LogFormat("assetBundles Count: {0}\n", assetBundles.Length );

            foreach ( string s in assetBundles )			
                Debug.LogFormat("{0}\n", s);			
            
            return false;
        }
        #endregion
    }
}
