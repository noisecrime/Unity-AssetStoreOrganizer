#if !UNITY_2019_4_OR_NEWER
using UnityEditor;

namespace NoiseCrimeStudios.Core.Editor.Internal
{
    /// <summary>Adds the Internal Unity IMGUI Debugger to the Window Menu.</summary>
    /// <remarks>
    /// Source:      UnityCsReference\Editor\Mono\GUIDebugger\GUIViewDebuggerWindow.cs
    /// MenuItem:    [MenuItem("Window/Analysis/IMGUI Debugger", false, 102, true)]
    /// </remarks>
    public static class InternalDebugIMGUI
    {
#if UNITY_2018_2_OR_NEWER
        [MenuItem( "Window/Analysis/IMGUI Debugger", false, 112 )]
#else
        [MenuItem( "Window/Internal/IMGUI Debugger", false, 112 )]
#endif        
        static void GUIViewDebuggerWindow()
        {
            EditorWindow.GetWindow ( typeof( EditorApplication ).Assembly.GetType ("UnityEditor.GUIViewDebuggerWindow") );
        }
    }
}
#endif

/*
// Get existing open window or if none, make a new one:
if ( s_ActiveInspector == null )
{
    EditorWindow window = EditorWindow.GetWindow ( GetType ("UnityEditor.UndoWindow"));
    s_ActiveInspector = window;
}
s_ActiveInspector.Show();
*/

