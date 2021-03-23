using UnityEditor;
using UnityEngine;

namespace NoiseCrimeStudios.Editor.IMGUI
{
    /// <summary>Collection of static methods to help with Unity's Immediate Mode GUI layout.</summary>
    public static class EditorGUIMethods
    {
        #region Texture Generation
        public static Texture2D CreateTexture( int width, int height, Color32 col )
        {
            // UnityEngine.Debug.LogFormat("Color: {0} HasProLicense: {1}  colorSpace: {2}", col, Application.HasProLicense(), PlayerSettings.colorSpace);
            bool linear = false; // ( PlayerSettings.colorSpace == ColorSpace.Linear );

            Color32[] pix = new Color32[width*height];

            for ( int i = 0; i < pix.Length; i++ )
                pix[ i ] = col;

            Texture2D result = new Texture2D(width, height, TextureFormat.ARGB32, false, linear );
            result.SetPixels32( pix );
            result.Apply();

            return result;
        }
        #endregion

        public static void PresentOpenFolderDialog( ref string folderPath, string title, string defaultName )
        {
            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.PrefixLabel( "Location", EditorStyles.popup );

            if ( GUILayout.Button( string.IsNullOrEmpty( folderPath ) ? "Browse..." : folderPath, EditorStyles.popup, GUILayout.Width( 320f ) ) )
            {
                var path = EditorUtility.OpenFolderPanel( title, folderPath, defaultName);
                if ( path.Length > 0 ) folderPath = path;
            }
            EditorGUILayout.EndHorizontal();
        }

        #region CUSTOM SINGLE LINE GUI
        public static void SingleLineVector2Field( string title, ref Vector2 input )
        {
            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.PrefixLabel( title );
            input = EditorGUILayout.Vector2Field( "", input );
            EditorGUILayout.EndHorizontal();
        }

        public static void SingleLineRectField( string title, ref Rect input )
        {
            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.PrefixLabel( title );
            input = EditorGUILayout.RectField( "", input );
            EditorGUILayout.EndHorizontal();
        }
        #endregion


        #region Boxes
        public static float BeginBoxOut()
        {
            Rect r = EditorGUILayout.BeginHorizontal ();
            GUI.Box( r, GUIContent.none );
            EditorGUILayout.BeginVertical();
            return r.y;
        }

        public static void EndBoxOut()
        {
            EditorGUILayout.EndVertical();
            EditorGUILayout.EndHorizontal();
        }

        public static float BeginBoxOutBordered( int border )
        {
            Rect r = EditorGUILayout.BeginHorizontal ();
            GUI.Box( r, GUIContent.none );
            EditorGUILayout.BeginVertical();

            GUILayout.Space( border );
            EditorGUILayout.BeginHorizontal();
            GUILayout.Space( border );
            EditorGUILayout.BeginVertical();

            return r.y;
        }

        public static void EndBoxOutBordered( int border )
        {
            GUILayout.Space( border );
            EditorGUILayout.EndVertical();
            GUILayout.Space( border );
            EditorGUILayout.EndHorizontal();

            EditorGUILayout.EndVertical();
            EditorGUILayout.EndHorizontal();
        }
        #endregion
    }
}
