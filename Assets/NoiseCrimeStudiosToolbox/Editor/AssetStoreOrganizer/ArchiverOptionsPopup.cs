using UnityEditor;
using UnityEngine;

namespace NoiseCrimeStudios.Toolbox.AssetStoreOrganizer
{    
    public class ArchiverOptionsPopup : PopupWindowContent
    {
        private readonly OrganizerEditor m_packageLibraryEditor;

        private readonly GUIContent m_archiveContent   = new GUIContent("Archive Shown Packages",  "Archive the current filtered packages" );
        private readonly GUIContent m_fileOpsContent   = new GUIContent("Disable File Operations", "Prevents file operations ( for debugging )" );
        private readonly GUIContent m_logPathsContent  = new GUIContent("Log Store Paths",         "Logs paths to console for debugging" );
       
        public ArchiverOptionsPopup( OrganizerEditor editor )
        {
            m_packageLibraryEditor = editor;
        }

        public override Vector2 GetWindowSize()
        {
            return new Vector2( 256f, 160f );
        }

        public override void OnGUI( Rect rect )
        {
            GUILayout.Label( "Archive Options", EditorStyles.boldLabel );

            m_packageLibraryEditor.DisableFileOperations =
                EditorGUILayout.Toggle( m_fileOpsContent, !m_packageLibraryEditor.DisableFileOperations );

            if ( GUILayout.Button( m_archiveContent ) )
                m_packageLibraryEditor.StartArchiveSourceLibraryProcess();

            GUILayout.Space( 8f );

            if ( GUILayout.Button( m_logPathsContent ) )
                OrganizerPaths.LogToConsole();

            if ( Event.current.type == EventType.ScrollWheel )
                editorWindow.Close();
        }

        /*
        public override void OnOpen() { Debug.Log( "Popup opened: " + this ); }
        public override void OnClose() { Debug.Log( "Popup closed: " + this ); }
        */
    }
}
