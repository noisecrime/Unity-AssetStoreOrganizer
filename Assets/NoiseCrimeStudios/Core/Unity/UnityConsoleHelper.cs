namespace NoiseCrimeStudios.Core.UnitySupport
{
#if !UNITY_EDITOR

    public static class UnityConsoleHelper
    {
        public static void ClearConsoleSimple() { }
        public static void ClearConsole() { }
    }

#else

    public static class UnityConsoleHelper
    {
        public static void ClearConsoleSimple()
        {
            var method      = System.Type.GetType("UnityEditor.LogEntries, UnityEditor.dll");
            var clearMethod = method.GetMethod("Clear", System.Reflection.BindingFlags.Static | System.Reflection.BindingFlags.Public);
            clearMethod.Invoke( null, null );
        }


        public static void ClearConsole( string callerName = null )
        {
#if UNITY_2017_4_OR_NEWER
            string typeName = "UnityEditor.LogEntries";
#else
            string typeName = "UnityEditorInternal.LogEntries";
#endif
            var assembly    = System.Reflection.Assembly.GetAssembly( typeof( UnityEditor.Editor ) );
            var type        = assembly.GetType( typeName );
            var method      = type.GetMethod( "Clear", System.Reflection.BindingFlags.Static | System.Reflection.BindingFlags.Public );
            method.Invoke( null, null );

            if ( null != callerName )
                UnityEngine.Debug.Log( " Console Cleared By Code - " + callerName );
        }
    }
#endif    
}


/*
// Old Code For Future Reference
// typeof(UnityEditor.Editor).Assembly.GetType("UnityEditorInternal.LogEntries").GetMethod("Clear", BindingFlags.Static | BindingFlags.Public).Invoke(null, null);	

#if UNITY_2017_4_OR_NEWER
    var assembly    = System.Reflection.Assembly.GetAssembly( typeof( UnityEditor.Editor ) );
    var type        = assembly.GetType ("UnityEditor.LogEntries");
    var method      = type.GetMethod ("Clear", System.Reflection.BindingFlags.Static | System.Reflection.BindingFlags.Public);
    method.Invoke( null, null );
#elif UNITY_2017_1_OR_NEWER
    var assembly    = System.Reflection.Assembly.GetAssembly( typeof( UnityEditor.SceneView ) );
    var type        = assembly.GetType ("UnityEditorInternal.LogEntries");
    var method      = type.GetMethod ("Clear", System.Reflection.BindingFlags.Static | System.Reflection.BindingFlags.Public);
    method.Invoke( new object(), null );		
#else
    var assembly	= System.Reflection.Assembly.GetAssembly( typeof( UnityEditor.ActiveEditorTracker ) );   
    var type        = assembly.GetType("UnityEditorInternal.LogEntries");
    var method      = type.GetMethod ("Clear", System.Reflection.BindingFlags.Static | System.Reflection.BindingFlags.Public);
    method.Invoke( new object(), null );        
#endif
*/
