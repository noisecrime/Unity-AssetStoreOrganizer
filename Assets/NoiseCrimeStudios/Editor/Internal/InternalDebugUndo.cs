using UnityEditor;

namespace NoiseCrimeStudios.Editor.Internal
{
    /// <summary>Adds the Internal Unity Undo Debugger to the Widow Menu.</summary>
    /// <remarks>
    /// Source:      UnityCsReference\Editor\Mono\Undo\UndoWindow.cs
    /// MenuItem:    [MenuItem("Window/Internal/Undo", false, 1, true)]
    /// </remarks>
    public static class InternalDebugUndo
    {
#if UNITY_2018_2_OR_NEWER
        [MenuItem( "Window/Analysis/Undo Window", false, 113 )]
#else
        [MenuItem( "Window/Internal/Undo Window", false, 113 )]
#endif        
        static void GUIViewDebuggerWindow()
        {
            EditorWindow.GetWindow ( typeof( EditorApplication ).Assembly.GetType ("UnityEditor.UndoWindow") );
        }
    }
}
