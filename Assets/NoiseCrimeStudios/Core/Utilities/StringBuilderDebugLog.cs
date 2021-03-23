using System.Text;
using System.Collections.Generic;
using UnityEngine;

// TODO: Search for 'PrintToConsole' in existin scripts and replace with this?

namespace NoiseCrimeStudios.Core
{
    /// <summary>Stringbuilder based logging with features for use via Debug.Log.</summary>
    /// <remarks> 
    /// Max characters in unity Debug.Log is about 16384.
    /// As we don't know how many additional characters there will be we have to guess a max value.
    /// We could build the string to add with string.format and know in advance max characters but
    /// this might not be as optimal as StringBuilder?
    ///
    /// Truncation Prevention via String Breaks:
    /// String Breaks ( similar to Page Break ) help to avoid truncation of Debug.Log by splitting up the output.
    /// You can specify the max character limit to be extracted from output.ToString() per Debug.Log().
    /// The SoftBreak option will insert a Hard Break if the next N characters will exceed Max Character limit.
    ///
    /// Indentation
    /// Supports simple indentation via increment and decrement methods.
    /// </remarks>
    public class StringBuilderDebugLog
    {
        private StringBuilder   output                  = new StringBuilder();

        /// <summary>List of indices at which to break up Debug.Log statements.</summary>
        private List<int>       stringBreakIndices      = new List<int>();

        /// <summary>Character limit at which point to automatically add a Hard break into the BreakIndices.</summary>
        private int             stringBreakCharLimit    = 14000;

        /// <summary>Cache the previous break index.</summary>
        private int             previousBreakIndex      = 0;

        // Allows instances to hold a flag to check whether to call instance methods. e.g. if ( instance.ExternalEnableLogging ) output.writeline()
        public  bool            ExternalEnableLogging { get; private set; }
        

        public StringBuilderDebugLog( bool enableLogging = true, int breakCharCount = 14000 )
        {
            ExternalEnableLogging   = enableLogging;
            stringBreakCharLimit    = breakCharCount;
        }
        
		public override string ToString()
        {
            return output.ToString();
        }

        public void AppendLine()
		{
            CheckForStringBreak( 2 );
			output.AppendLine();                 
		}

		public void AppendLine( string value )
		{            
            CheckForStringBreak( value.Length + 2 + indentLevel * indentSize ); 
            output.Append( Indent );
			output.AppendLine( value );
		}

		public void AppendFormat( string format, params object[] args )
		{
            string value = string.Format( format, args );            
            CheckForStringBreak( value.Length + 2 + indentLevel * indentSize ); 

            output.Append( Indent );
			output.AppendFormat( value );
			output.AppendLine();
		}

        /*
        public void WriteString( string format, params object[] args )
		{
            string value = string.Format( format, args );            
            CheckForStringBreak( value.Length );             
			output.AppendFormat( value );
		}*/

        
        /// <summary>Clears the output and resets internal break markers and indents.</summary>
        public void Clear()
		{
            // https://stackoverflow.com/questions/1709471/best-way-to-clear-contents-of-nets-stringbuilder#1709537
            output.Length       = 0;
            previousBreakIndex  = 0;
            stringBreakIndices.Clear();
			IndentReset();
		}

        private void LogStatement( int previousIndex, int length, LogType logType )
        {
            switch ( logType )
            {
                case LogType.Warning:
                    Debug.LogWarning( output.ToString( previousIndex, length) );
                    break;
                case LogType.Error:
                    Debug.LogError( output.ToString( previousIndex, length) );
                    break;
                default:
                    Debug.Log( output.ToString( previousIndex, length) );
                    break;
            }
        }

		public void LogToConsole( bool disableStackTraceLog = false, bool clearOnLog = true, LogType logType = LogType.Log )
		{            
            int                 previousIndex       = 0;
            StackTraceLogType   previousTraceType   = Application.GetStackTraceLogType( logType );

            if ( disableStackTraceLog ) /// DISABLE
                Application.SetStackTraceLogType( logType, StackTraceLogType.None );
            
            // Print contents to console 
            for ( int i = 0; i < stringBreakIndices.Count; i++ )
            {
                LogStatement( previousIndex, stringBreakIndices[ i ] - previousIndex, logType );                
                previousIndex = stringBreakIndices[ i ] ;
            }

            // Print remainder to console  
            if ( output.Length - 1 - previousIndex > 0 )
                LogStatement( previousIndex, output.Length - 1 - previousIndex, logType );                      
            
            if ( disableStackTraceLog ) // RESTORE
                Application.SetStackTraceLogType( logType, previousTraceType );
            
			if ( clearOnLog )
                Clear();
		}


        /// <summary>Inserts a Hard Break at current output length to split up results into debug.log statements.</summary>
        public void InsertHardBreak()
        {
            previousBreakIndex = output.Length;
            stringBreakIndices.Add( previousBreakIndex );
        }

        /// <summary>Inserts a Hard Break at current output length ONLY if breakBufferLength exceeds max character limit.</summary>        
        public void InsertSoftBreak( int breakBufferLength = 1024 )
        {
            if ( output.Length - previousBreakIndex + breakBufferLength < stringBreakCharLimit ) return;

            previousBreakIndex = output.Length;
            stringBreakIndices.Add( previousBreakIndex );
        }

        /// <summary>Check if appending the predicted number of characters will exceed next break index.</summary>
        private void CheckForStringBreak( int predictedCharacterLength = 0 )
        {
            if ( output.Length + predictedCharacterLength - previousBreakIndex < stringBreakCharLimit ) return;

            previousBreakIndex = output.Length;
            stringBreakIndices.Add( previousBreakIndex );  
        }



        #region Indents
        private int		indentLevel = 0;
		private	int		indentSize  = 4;    // 16 is default Unity value		
		private string	Indent { get; set; }

		public void IndentReset( int size = 4 )
		{
			indentLevel = 0;
            indentSize  = size;
			Indent      = new string( ' ', indentLevel * indentSize );
		}

		public void IndentIncrement( int level = 1 )
		{
			indentLevel += level;
			Indent = new string( ' ', indentLevel * indentSize );
		}

		public void IndentDecrement( int level = 1 )
		{
			indentLevel -= level;
			if ( indentLevel < 0 ) indentLevel = 0;
			Indent = new string( ' ', indentLevel * indentSize );
		}
        #endregion


        #region Validation
        /// <summary>Simple test to validate everything works as expected.</summary>  
        public static void ValidateOutput()
        {
            StringBuilderDebugLog helper = new StringBuilderDebugLog();

            for ( int i = 0; i < 8192; i++ )            
                helper.AppendFormat( "{0:D4}+", i );
            
            helper.LogToConsole(true);
        }
        #endregion
    }
}
