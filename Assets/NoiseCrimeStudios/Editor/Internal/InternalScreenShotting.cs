using UnityEditor;
using UnityEngine;

namespace NoiseCrimeStudios.Core.Editor.Internal
{ 
    /// <summary>
    /// Exposes the native Unity 'ScreenShotting' methods to Unity Menu & Shortcut keys.
    /// </summary>
    /// <remarks>
    /// Once imported you can find the menu in either Window/Analysis/Screenshot or Window/Internal/Screenshot.
    /// See UnityCsReference\Editor\Mono\GUI\ScreenShotting.cs
    /// Hotkey: % (ctrl on Windows, cmd on macOS), # (shift), & (alt).
    /// </remarks>
    public static class InternalScreenShotting
    {
        private static  System.Type s_screenShotsType = typeof(EditorApplication).Assembly.GetType("UnityEditor.ScreenShots", false);
        
#if UNITY_2018_2_OR_NEWER
        private const string MenuItemName = "Window/Analysis/Screenshot/";
#else
        private const string MenuItemName = "Window/Internal/Screenshot/";
#endif        
        
        /// <summary>Sets the entire Unity Window to 762x600.</summary>
        [MenuItem( MenuItemName + "SetMainWindowSize ( 762,600)", false, 115)]
        public static void SetMainWindowSizeSmall()
        {
            var method          = s_screenShotsType.GetMethod("SetMainWindowSizeSmall");
            if ( null != method )
                method.Invoke(null, null);
        }

        /// <summary>Sets the entire Unity Window to 1024x768.</summary>
        [MenuItem( MenuItemName + "SetMainWindowSize (1024,768)", false, 115)]
        public static void SetMainWindowSize()
        {            
            var method          = s_screenShotsType.GetMethod("SetMainWindowSize");
            if ( null != method )
                method.Invoke(null, null);
        }
        
        /// <summary>Screenshots the Game View Window.</summary>	
        [MenuItem( MenuItemName + "Snap Game View Content %&g", false, 115)]
        public static void ScreenGameViewContent()
        {
            Debug.Log("Snap Game View Content");
            var method          = s_screenShotsType.GetMethod("ScreenGameViewContent");
            if ( null != method )
                method.Invoke(null, null);
        }

        /// <summary>Screenshots the active Window.</summary>
        [MenuItem( MenuItemName + "Snap Active Window %&h", false, 115)]
        public static void Screenshot()
        {
            Debug.Log("Snap Active Window");
            var method          = s_screenShotsType.GetMethod("Screenshot");
            if ( null != method )
                method.Invoke(null, null);
        }
        
        /// <summary>Screenshots the active Window and the rest of the screen to the right. For example Sceneview & Inspector.</summary>
        [MenuItem( MenuItemName + "Snap Active Window Extended Right %&j", false, 115)]
        public static void ScreenshotExtendedRight()
        {
            Debug.Log("Snap Active Window Extended Right");
            var method          = s_screenShotsType.GetMethod("ScreenshotExtendedRight");
            if ( null != method )
                method.Invoke(null, null);
        }

        /// <summary>Screenshots the active window toolbar. Bit unreliable.</summary>
        [MenuItem( MenuItemName + "Snap Active Toolbar %&k", false, 115)]
        public static void ScreenshotToolbar()
        {
            Debug.Log("Screenshot Active Toolbar");
            var method          = s_screenShotsType.GetMethod("ScreenshotToolbar");
            if ( null != method )
                method.Invoke(null, null);
        }

        /// <summary>Screenshots the Component you rollover next. Bit unreliable.</summary>
        [MenuItem( MenuItemName + "Snap Component on Rollover %&l", false, 115)]
        public static void ScreenShotComponent()
        {
            Debug.Log("Snap Component - Waiting for rollover on taget component");
            var method          = s_screenShotsType.GetMethod("ScreenShotComponent", new System.Type[0]);
            if ( null != method )
                method.Invoke(null, new object[0]);
        }
    }
}

/*
* var assembly        = typeof(EditorApplication).Assembly;
* var screenShotsType = assembly.GetType("UnityEditor.ScreenShots", true);
* 
* static Type unityEditorScreenShotsClassType;
* static Type GetScreenShotsClassType() {	return typeof(EditorApplication).Assembly.GetType("UnityEditor.ScreenShots", true); }
*/
