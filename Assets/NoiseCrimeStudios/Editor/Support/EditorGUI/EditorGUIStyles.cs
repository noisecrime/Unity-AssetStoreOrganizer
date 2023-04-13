using UnityEditor;
using UnityEngine;

namespace NoiseCrimeStudios.Editor.IMGUI
{
    /// <summary>Collection of custom styles for Unity's Immediate Mode GUI layout.</summary>
    /// <remarks>
    /// Loading Unity Editor Icons as GUIContent
    /// EditorGUIUtility.IconContent("d_Refresh")  <see cref="https://unitylist.com/p/5c3/Unity-editor-icons"/>
    /// </remarks>
    public static class EditorGUIStyles
    {
        #region Unity Styles Reference
        // EditorStyles.centeredGreyMiniLabel
        // EditorStyles.miniLabel
        // EditorStyles.miniButton
        // EditorStyles.toolbar
        // EditorStyles.toolbarButton
        #endregion

        #region Label Styles		
        public static GUIStyle  labelBgLightLeft;
        public static GUIStyle  labelBgLightRight;
        public static GUIStyle  labelBgDarkLeft;
        public static GUIStyle  labelBgDarkRight;
        public static GUIStyle  labelBgStdLeft;
        public static GUIStyle  labelBgStdRight;

        public static GUIStyle  labelNormLeft;
        public static GUIStyle  labelNormRight;
        public static GUIStyle  labelBoldLeft;
        public static GUIStyle  labelBoldRight;

        public static GUIStyle  labelNormIcon;
        #endregion

        public static GUIStyle  fullButton;

        public static GUIStyle  miniButtonDark;
        public static GUIStyle  miniButtonLight;

        public static GUIStyle  textAreaImage;

        public static GUIStyle  scopeDark;
        public static GUIStyle  scopeLight;
        public static GUIStyle  scopeStd;             

        public static Color     colorContentNormal  = Color.white;
        public static Color     colorContentError   = new Color( 0.5f, 0f, 0f );
        public static Color     colorContentWarning = IsUsingDarkSkinMode ? Color.yellow : new Color( 0.6f, 0.4f, 0.0f );
        public static Color     colorContentGreen   = IsUsingDarkSkinMode ? Color.green  : new Color( 0.0f, 0.6f, 0.0f );
        public static Color     colorTextNormal     = IsUsingDarkSkinMode ? Color.white  : Color.black;

        // How to destroy these?
        public static Texture2D backgroundTxDark;
        public static Texture2D backgroundTxStd;
        public static Texture2D backgroundTxLight;

        // GUISkin skin = EditorGUIUtility.GetBuiltinSkin(EditorSkin.Inspector);

        /// <summary>
        /// Single access point to check for use of 'DarkSkin/ProSkin' use in Unity Editor.
        /// </summary>
        /// <remarks>
        /// Switched from Application.HasProLicense() to EditorGUIUtility.isProSkin to fix issues with GUI Styles.
        /// Enables easier testing.
        /// Force this to return true if you want dark skin mode regardless.
        /// </remarks>
        public static bool IsUsingDarkSkinMode { get { return EditorGUIUtility.isProSkin; } } 


        static EditorGUIStyles()
        {
            // Textures
            backgroundTxDark    = EditorGUIMethods.CreateTexture( 8, 8, IsUsingDarkSkinMode ? new Color32( 48, 48, 48, 255 ) : new Color32( 154, 154, 154, 255 ) );
            backgroundTxLight   = EditorGUIMethods.CreateTexture( 8, 8, IsUsingDarkSkinMode ? new Color32( 64, 64, 64, 255 ) : new Color32( 193, 193, 193, 255 ) );
            backgroundTxStd     = EditorGUIMethods.CreateTexture( 8, 8, IsUsingDarkSkinMode ? new Color32( 56, 56, 56, 255 ) : new Color32( 162, 162, 162, 255 ) );
            //	backgroundTextureSelect	= IMGUIMethods.CreateTexture(8,8, new Color32(56,56,156,255));

            RectOffset margin  = new RectOffset(0,0,0,0);
            RectOffset padding = new RectOffset(2,2,2,2);

            DefineStyle( ref fullButton, GUI.skin.button, FontStyle.Normal, TextAnchor.MiddleCenter, null, null, margin, new RectOffset( 2, 2, 2, 2 ) );

            DefineStyle( ref scopeLight,        EditorStyles.label, FontStyle.Normal,   TextAnchor.MiddleCenter, backgroundTxLight, null, margin, padding );
            DefineStyle( ref scopeStd,          EditorStyles.label, FontStyle.Normal,   TextAnchor.MiddleCenter, backgroundTxStd, null, margin, padding );
            DefineStyle( ref scopeDark,         EditorStyles.label, FontStyle.Normal,   TextAnchor.MiddleCenter, backgroundTxDark, null, margin, padding );

            DefineStyle( ref miniButtonLight,   EditorStyles.miniButton, EditorStyles.miniButton.fontStyle, EditorStyles.miniButton.alignment, backgroundTxLight );
            DefineStyle( ref miniButtonDark,    EditorStyles.miniButton, EditorStyles.miniButton.fontStyle, EditorStyles.miniButton.alignment, backgroundTxDark );

            DefineStyle( ref labelBgLightLeft,  EditorStyles.label, FontStyle.Normal,   TextAnchor.MiddleLeft, backgroundTxLight );
            DefineStyle( ref labelBgLightRight, EditorStyles.label, FontStyle.Normal,   TextAnchor.MiddleRight, backgroundTxLight );
            DefineStyle( ref labelBgStdLeft,    EditorStyles.label, FontStyle.Normal,   TextAnchor.MiddleLeft, backgroundTxStd );
            DefineStyle( ref labelBgStdRight,   EditorStyles.label, FontStyle.Normal,   TextAnchor.MiddleRight, backgroundTxStd );
            DefineStyle( ref labelBgDarkLeft,   EditorStyles.label, FontStyle.Normal,   TextAnchor.MiddleLeft, backgroundTxDark );
            DefineStyle( ref labelBgDarkRight,  EditorStyles.label, FontStyle.Normal,   TextAnchor.MiddleRight, backgroundTxDark );

            DefineStyle( ref labelNormLeft,     EditorStyles.label, FontStyle.Normal,   TextAnchor.MiddleLeft );
            DefineStyle( ref labelNormRight,    EditorStyles.label, FontStyle.Normal,   TextAnchor.MiddleRight );
            DefineStyle( ref labelBoldLeft,     EditorStyles.label, FontStyle.Bold,     TextAnchor.MiddleLeft );
            DefineStyle( ref labelBoldRight,    EditorStyles.label, FontStyle.Bold,     TextAnchor.MiddleRight );
            DefineStyle( ref labelNormIcon,     EditorStyles.label, FontStyle.Normal,   TextAnchor.MiddleCenter, null, null, null, new RectOffset( 2, 2, 2, 2 ) );

            textAreaImage = new GUIStyle( EditorStyles.textArea )
            {
                imagePosition = ImagePosition.ImageOnly,
                alignment = TextAnchor.MiddleCenter
            };
        }

        static void DefineStyle( ref GUIStyle style, GUIStyle from, FontStyle font, TextAnchor anchor, Texture2D normalBG = null, RectOffset border = null, RectOffset margin = null, RectOffset padding = null )
        {
            if ( null == border ) border = from.border;
            if ( null == margin ) margin = from.margin;
            if ( null == padding ) padding = from.padding;

            style = new GUIStyle( from )
            {
                fontStyle = font,
                alignment = anchor,
                border = border,
                margin = margin,
                padding = padding
            };

            if ( null != normalBG ) style.normal.background = normalBG;
        }

        /// <summary>Create Google Material Icons & Content Helper Method.</summary>		
        /// <param name="iconNameFormat">'{0}ic_file_download_{1}_18dp_1x.png'</param>
        public static void CreateIconContent( ref Texture iconTexture, ref GUIContent iconContent, string materialIconsPath, string iconNameFormat, string contentText )
        {
            string  iconColor   = "white"; // IsUsingDarkSkinMode ? "white" : "black";

            if ( null == iconTexture )
                iconTexture = ( Texture )AssetDatabase.LoadAssetAtPath( string.Format( iconNameFormat, materialIconsPath, iconColor ), typeof( Texture ) );

            if ( null == iconContent )
                iconContent = new GUIContent( iconTexture, contentText );
        }
    }
}


/*
// Examples - just copy the OnGUI into an editorWindow class to check out the styles.
protected virtual void OnGUI()
{
    EditorGUILayout.LabelField("Test Label Dark Left",  IMGUICustomStyles.leftAlignLabelDark, GUILayout.MinWidth(256f),	GUILayout.MaxWidth(256f));
    EditorGUILayout.LabelField("Test Label Light Left", IMGUICustomStyles.leftAlignLabelLight, GUILayout.MinWidth(256f),	GUILayout.MaxWidth(256f));
                
    EditorGUILayout.Space();
    EditorGUILayout.LabelField("Test Label Dark Right",  IMGUICustomStyles.rightAlignLabelDark, GUILayout.MinWidth(256f),	GUILayout.MaxWidth(256f));
    EditorGUILayout.LabelField("Test Label Light Right", IMGUICustomStyles.rightAlignLabelLight, GUILayout.MinWidth(256f),	GUILayout.MaxWidth(256f));
                
    EditorGUILayout.Space();
    EditorGUILayout.LabelField("Test Label Left Bold",  IMGUICustomStyles.leftBoldLabel, GUILayout.MinWidth(256f),	GUILayout.MaxWidth(256f));
    EditorGUILayout.LabelField("Test Label Light Norm", IMGUICustomStyles.leftNormalLabel, GUILayout.MinWidth(256f),	GUILayout.MaxWidth(256f));

    EditorGUILayout.Space();
    EditorGUILayout.LabelField("Test Label Right Bold",  IMGUICustomStyles.rightBoldLabel, GUILayout.MinWidth(256f),	GUILayout.MaxWidth(256f));
    EditorGUILayout.LabelField("Test Label Right Norm",  IMGUICustomStyles.rightNormalLabel, GUILayout.MinWidth(256f),	GUILayout.MaxWidth(256f));
}
*/
