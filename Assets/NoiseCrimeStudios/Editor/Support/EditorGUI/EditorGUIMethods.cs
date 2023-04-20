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
    }
}
