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
        public static GUIStyle  LabelBgLightLeft;
        public static GUIStyle  LabelBgLightRight;
        public static GUIStyle  LabelBgDarkLeft;
        public static GUIStyle  LabelBgDarkRight;
        public static GUIStyle  LabelBgStdLeft;
        public static GUIStyle  LabelBgStdRight;

        public static GUIStyle  LabelNormLeft;
        public static GUIStyle  LabelNormRight;
        public static GUIStyle  LabelBoldLeft;
        public static GUIStyle  LabelBoldRight;

        public static GUIStyle  LabelNormIcon;
        #endregion

        public static GUIStyle  FullButton;

        public static GUIStyle  MiniButtonDark;
        public static GUIStyle  MiniButtonLight;

        public static GUIStyle  TextAreaImage;

        public static GUIStyle  ScopeDark;
        public static GUIStyle  ScopeLight;
        public static GUIStyle  ScopeStd;             

        public static Color     ColorContentNormal  = Color.white;
        public static Color     ColorContentError   = new Color( 0.5f, 0f, 0f );
        public static Color     ColorContentWarning = IsUsingDarkSkinMode ? Color.yellow : new Color( 0.6f, 0.4f, 0.0f );
        public static Color     ColorContentGreen   = IsUsingDarkSkinMode ? Color.green  : new Color( 0.0f, 0.6f, 0.0f );
        public static Color     ColorTextNormal     = IsUsingDarkSkinMode ? Color.white  : Color.black;

        // How to destroy these?
        public static Texture2D BackgroundTxDark;
        public static Texture2D BackgroundTxStd;
        public static Texture2D BackgroundTxLight;

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
            BackgroundTxDark    = EditorGUIMethods.CreateTexture( 8, 8, IsUsingDarkSkinMode ? new Color32( 48, 48, 48, 255 ) : new Color32( 154, 154, 154, 255 ) );
            BackgroundTxLight   = EditorGUIMethods.CreateTexture( 8, 8, IsUsingDarkSkinMode ? new Color32( 64, 64, 64, 255 ) : new Color32( 193, 193, 193, 255 ) );
            BackgroundTxStd     = EditorGUIMethods.CreateTexture( 8, 8, IsUsingDarkSkinMode ? new Color32( 56, 56, 56, 255 ) : new Color32( 162, 162, 162, 255 ) );
            //	backgroundTextureSelect	= IMGUIMethods.CreateTexture(8,8, new Color32(56,56,156,255));

            RectOffset margin  = new RectOffset(0,0,0,0);
            RectOffset padding = new RectOffset(2,2,2,2);

            DefineStyle( ref FullButton, GUI.skin.button, FontStyle.Normal, TextAnchor.MiddleCenter, null, null, margin, new RectOffset( 2, 2, 2, 2 ) );

            DefineStyle( ref ScopeLight,        EditorStyles.label, FontStyle.Normal,   TextAnchor.MiddleCenter, BackgroundTxLight, null, margin, padding );
            DefineStyle( ref ScopeStd,          EditorStyles.label, FontStyle.Normal,   TextAnchor.MiddleCenter, BackgroundTxStd, null, margin, padding );
            DefineStyle( ref ScopeDark,         EditorStyles.label, FontStyle.Normal,   TextAnchor.MiddleCenter, BackgroundTxDark, null, margin, padding );

            DefineStyle( ref MiniButtonLight,   EditorStyles.miniButton, EditorStyles.miniButton.fontStyle, EditorStyles.miniButton.alignment, BackgroundTxLight );
            DefineStyle( ref MiniButtonDark,    EditorStyles.miniButton, EditorStyles.miniButton.fontStyle, EditorStyles.miniButton.alignment, BackgroundTxDark );

            DefineStyle( ref LabelBgLightLeft,  EditorStyles.label, FontStyle.Normal,   TextAnchor.MiddleLeft, BackgroundTxLight );
            DefineStyle( ref LabelBgLightRight, EditorStyles.label, FontStyle.Normal,   TextAnchor.MiddleRight, BackgroundTxLight );
            DefineStyle( ref LabelBgStdLeft,    EditorStyles.label, FontStyle.Normal,   TextAnchor.MiddleLeft, BackgroundTxStd );
            DefineStyle( ref LabelBgStdRight,   EditorStyles.label, FontStyle.Normal,   TextAnchor.MiddleRight, BackgroundTxStd );
            DefineStyle( ref LabelBgDarkLeft,   EditorStyles.label, FontStyle.Normal,   TextAnchor.MiddleLeft, BackgroundTxDark );
            DefineStyle( ref LabelBgDarkRight,  EditorStyles.label, FontStyle.Normal,   TextAnchor.MiddleRight, BackgroundTxDark );

            DefineStyle( ref LabelNormLeft,     EditorStyles.label, FontStyle.Normal,   TextAnchor.MiddleLeft );
            DefineStyle( ref LabelNormRight,    EditorStyles.label, FontStyle.Normal,   TextAnchor.MiddleRight );
            DefineStyle( ref LabelBoldLeft,     EditorStyles.label, FontStyle.Bold,     TextAnchor.MiddleLeft );
            DefineStyle( ref LabelBoldRight,    EditorStyles.label, FontStyle.Bold,     TextAnchor.MiddleRight );
            DefineStyle( ref LabelNormIcon,     EditorStyles.label, FontStyle.Normal,   TextAnchor.MiddleCenter, null, null, null, new RectOffset( 2, 2, 2, 2 ) );

            TextAreaImage = new GUIStyle( EditorStyles.textArea )
            {
                imagePosition = ImagePosition.ImageOnly,
                alignment = TextAnchor.MiddleCenter
            };
        }

        private static void DefineStyle( ref GUIStyle style, GUIStyle from, FontStyle font, TextAnchor anchor, Texture2D normalBG = null, RectOffset border = null, RectOffset margin = null, RectOffset padding = null )
        {
            if ( null == border )
                border = from.border;
            if ( null == margin )
                margin = from.margin;
            if ( null == padding )
                padding = from.padding;

            style = new GUIStyle( from )
            {
                fontStyle = font,
                alignment = anchor,
                border = border,
                margin = margin,
                padding = padding
            };

            if ( null != normalBG )
                style.normal.background = normalBG;
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
