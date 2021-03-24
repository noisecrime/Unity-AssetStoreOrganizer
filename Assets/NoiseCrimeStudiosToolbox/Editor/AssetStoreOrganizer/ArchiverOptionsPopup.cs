using UnityEditor;
using UnityEngine;

namespace NoiseCrimeStudios.Toolbox.AssetStoreOrganizer
{    
    public class ArchiverOptionsPopup : PopupWindowContent
    {
        private OrganizerEditor packageLibraryEditor;

        private GUIContent archiveContent   = new GUIContent("Archive Shown Packages",  "Archive the current filtered packages" );
        private GUIContent fileOpsContent   = new GUIContent("Disable File Operations", "Prevents file operations ( for debugging )" );
        private GUIContent logPathsContent  = new GUIContent("Log Store Paths",         "Logs paths to console for debugging" );
       
        public ArchiverOptionsPopup( OrganizerEditor editor )
        {
            packageLibraryEditor = editor;
        }

        public override Vector2 GetWindowSize()
        {
            return new Vector2( 256f, 160f );
        }
        
        public override void OnGUI( Rect rect )
        {
            GUILayout.Label( "Archive Options", EditorStyles.boldLabel );

            packageLibraryEditor.disableFileOperations = 
                EditorGUILayout.Toggle( fileOpsContent, !packageLibraryEditor.disableFileOperations );
            
            if ( GUILayout.Button( archiveContent ) )
                packageLibraryEditor.StartArchiveSourceLibraryProcess();

            GUILayout.Space(8f);

            if ( GUILayout.Button( logPathsContent ) )
                OrganizerPaths.LogToConsole();

            if( Event.current.type == EventType.ScrollWheel ) editorWindow.Close();
        }

        /*
        public override void OnOpen() { Debug.Log( "Popup opened: " + this ); }
        public override void OnClose() { Debug.Log( "Popup closed: " + this ); }
        */
    }
}
