using System.Collections.Generic;
using System.Text;
using UnityEngine;

namespace NoiseCrimeStudios.Core.Utilities
{
    /// <summary>
    /// Stringbuilder based logging with features for use via Debug.Log.
    /// </summary>
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
        private readonly StringBuilder   m_output                  = new StringBuilder();

        /// <summary>List of indices at which to break up Debug.Log statements.</summary>
        private readonly List<int>       m_stringBreakIndices      = new List<int>();

        /// <summary>Character limit at which point to automatically add a Hard break into the BreakIndices.</summary>
        private readonly int            m_stringBreakCharLimit    = 14000;

        /// <summary>Cache the previous break index.</summary>
        private int                     m_previousBreakIndex      = 0;

        // Allows instances to hold a flag to check whether to call instance methods. e.g. if ( instance.ExternalEnableLogging ) output.writeline()
        public  bool                    ExternalEnableLogging { get; private set; }
        

        public StringBuilderDebugLog( bool enableLogging = true, int breakCharCount = 14000 )
        {
            ExternalEnableLogging   = enableLogging;
            m_stringBreakCharLimit    = breakCharCount;
        }
        
        public override string ToString()
        {
            return m_output.ToString();
        }

        public void AppendLine()
        {
            CheckForStringBreak( 2 );
            m_output.AppendLine();                 
        }

        public void AppendLine( string value )
        {            
            CheckForStringBreak( value.Length + 2 + m_indentLevel * m_indentSize ); 
            m_output.Append( Indent );
            m_output.AppendLine( value );
        }

        public void AppendFormat( string format, params object[] args )
        {
            string value = string.Format( format, args );            
            CheckForStringBreak( value.Length + 2 + m_indentLevel * m_indentSize ); 

            m_output.Append( Indent );
            m_output.AppendFormat( value );
            m_output.AppendLine();
        }

        public void Append( string value )
        {
            CheckForStringBreak( value.Length + 2 + m_indentLevel * m_indentSize );
            m_output.Append( Indent );
            m_output.Append( value );
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
            m_output.Length       = 0;
            m_previousBreakIndex  = 0;
            m_stringBreakIndices.Clear();
            IndentReset();
        }

        private void LogStatement( int previousIndex, int length, LogType logType )
        {
            switch ( logType )
            {
                case LogType.Warning:
                    Debug.LogWarning( m_output.ToString( previousIndex, length) );
                    break;
                case LogType.Error:
                    Debug.LogError( m_output.ToString( previousIndex, length) );
                    break;
                default:
                    Debug.Log( m_output.ToString( previousIndex, length) );
                    break;
            }
        }

        public void LogToConsole( bool clearLogOnExit = true, bool disableStackTraceLog = false, LogType logType = LogType.Log )
        {            
            int                 previousIndex       = 0;
            StackTraceLogType   previousTraceType   = Application.GetStackTraceLogType( logType );

            if ( disableStackTraceLog ) /// DISABLE
                Application.SetStackTraceLogType( logType, StackTraceLogType.None );
            
            // Print contents to console 
            for ( int i = 0; i < m_stringBreakIndices.Count; i++ )
            {
                LogStatement( previousIndex, m_stringBreakIndices[ i ] - previousIndex, logType );                
                previousIndex = m_stringBreakIndices[ i ] ;
            }

            // Print remainder to console  
            if ( m_output.Length - 1 - previousIndex > 0 )
                LogStatement( previousIndex, m_output.Length - 1 - previousIndex, logType );                      
            
            if ( disableStackTraceLog ) // RESTORE
                Application.SetStackTraceLogType( logType, previousTraceType );
            
            if ( clearLogOnExit )
                Clear();
        }


        /// <summary>Inserts a Hard Break at current output length to split up results into debug.log statements.</summary>
        public void InsertHardBreak()
        {
            m_previousBreakIndex = m_output.Length;
            m_stringBreakIndices.Add( m_previousBreakIndex );
        }

        /// <summary>Inserts a Hard Break at current output length ONLY if breakBufferLength exceeds max character limit.</summary>        
        public void InsertSoftBreak( int breakBufferLength = 1024 )
        {
            if ( m_output.Length - m_previousBreakIndex + breakBufferLength < m_stringBreakCharLimit )
                return;

            m_previousBreakIndex = m_output.Length;
            m_stringBreakIndices.Add( m_previousBreakIndex );
        }

        /// <summary>Check if appending the predicted number of characters will exceed next break index.</summary>
        private void CheckForStringBreak( int predictedCharacterLength = 0 )
        {
            if ( m_output.Length + predictedCharacterLength - m_previousBreakIndex < m_stringBreakCharLimit )
                return;

            m_previousBreakIndex = m_output.Length;
            m_stringBreakIndices.Add( m_previousBreakIndex );  
        }



        #region Indents
        private int		m_indentLevel = 0;
        private	int		m_indentSize  = 4;    // 16 is default Unity value		
        private string	Indent { get; set; }

        public void IndentReset( int size = 4 )
        {
            m_indentLevel = 0;
            m_indentSize  = size;
            Indent      = new string( ' ', m_indentLevel * m_indentSize );
        }

        public void IndentIncrement( int level = 1 )
        {
            m_indentLevel += level;
            Indent = new string( ' ', m_indentLevel * m_indentSize );
        }

        public void IndentDecrement( int level = 1 )
        {
            m_indentLevel -= level;
            if ( m_indentLevel < 0 )
                m_indentLevel = 0;
            Indent = new string( ' ', m_indentLevel * m_indentSize );
        }
        #endregion


        #region Validation
        /// <summary>Simple test to validate everything works as expected.</summary>  
        public static void ValidateOutput()
        {
            StringBuilderDebugLog helper = new StringBuilderDebugLog();

            for ( int i = 0; i < 8192; i++ )            
                helper.AppendFormat( "{0:D4}+", i );
            
            helper.LogToConsole();
        }
        #endregion
    }
}
