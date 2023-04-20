// Was NoiseCrimeStudioToolEnum.cs

/// <remarks>
/// Setting a MenuItems priority prior to 2019.1 generally requires restarting Unity in most cases for it to be picked up.
/// Deleting, compiling, then restoring a priority may be sufficient to force Unity to reflect changes.
/// Unity Menu's and priorties link - https://blog.redbluegames.com/guide-to-extending-unity-editors-menus-b2de47a746db
/// PreviousPriorty + 11 == divider between entries.
/// As the menu path defines where item goes, the priorty can start at any value?
/// </remarks>

namespace NoiseCrimeStudios.Core.Editor
{
    public static class EditorMenuPaths
    {
        public const string NoiseCrimeStudios = "Window/NoiseCrimeStudios/";
    }

    public enum WindowMenuItemPriorty
    {
        None			= 0,

        // NoiseCrimeStudio Specific
        Inspectors		= 400,		// Window/NoiseCrimeStudios/Inspectors
        EditorSettings	= 411,		// Window/NoiseCrimeStudios/Settings 1105
        EditorTools		= 422,		// Window/NoiseCrimeStudios/Tools 1110
        Windows  		= 433,		// Window/NoiseCrimeStudios/Windows 1130
        Examples    	= 444,		// Window/NoiseCrimeStudios/Examples
        Validations     = 499       // Window/NoiseCrimeStudios/Validations
    }

    /// <summary>Enumeration for Asset Menu Items.</summary> 
    public enum AssetMenuItemPriority
    {
        Default			= 0,

        // Create
        CreateScripts   = 105,      // Assets/Create

        // Asset
        Packages		= 200,		// Assets/ImportAssetPackage
        References		= 201,		// Assets/Reference Checker

        AssetInspectors = 202,

        AssetTools		= 210,		// Assets/Tools
        AssetInfo		= 221,		// Assets/Tools - Info
        
        Legacy          = 1000
    }

    /// <summary>Enumeration for Project Menu Items. Where Project is the specific Unity Project.</summary>
    public enum ProjectMenuItemPriority
    {
        Default         = 0,        
        Build           = 10,       // Project/Build
        Tools           = 50,       // Project/Tools
        Validation      = 200       // Project/Validation
    }

    /* Needs to be in Core!
    /// <summary>Enumeration for CreateAsset Menu Items.</summary> 
    public enum CreateAssetMenuItemPriority
    {
        // TODO For ScriptableObjects
    }
    */

    /// <summary>Enumeration for GameObject Menu Items.</summary>    
    public enum GameObjectMenuPriority
    {
        Default			= -100,
        CreateBegin		= -10,
        CreateEmpty		= -1,
        Create3D		= 0,
        Create2D		= 1,
        CreateEnd		= 11,

        ChildrenTop		= 49,
        ChildrenEnd		= 50,

        ParentEnd		= 100,
        DefaultEnd		= 500
    }
}
