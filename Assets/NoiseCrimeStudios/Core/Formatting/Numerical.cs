using System;
using UnityEngine;

// String.Format Notes
// http://azuliadesigns.com/string-formatting-examples/
// https://msdn.microsoft.com/en-us/library/system.string.format(v=vs.110).aspx

namespace NoiseCrimeStudios.Core.Formatting
{
    public static class Numerical
    {
#if UNITY_2019_4_OR_NEWER
        /// <summary>
        /// Returns the number of digits for a Int32 value.
        /// </summary>    
        /// <see cref="https://stackoverflow.com/questions/4483886/how-can-i-get-a-count-of-the-total-number-of-digits-in-a-number"/>
        public static int Int32_Digits_Log10( this int n ) =>
            n == 0 ? 1 : ( n > 0 ? 1 : 2 ) + ( int )Math.Log10( Math.Abs( ( double )n ) );

        /// <summary>
        /// Returns the number of digits for a Int32 value.
        /// </summary>    
        /// <see cref="https://stackoverflow.com/questions/4483886/how-can-i-get-a-count-of-the-total-number-of-digits-in-a-number"/>
        public static int Int64_Digits_Log10( this long n ) =>
            n == 0L ? 1 : ( n > 0L ? 1 : 2 ) + ( int )Math.Log10( Math.Abs( ( double )n ) );
#endif

        // Overall this has proven to be the most efficeint method to use from this page
        // https://stackoverflow.com/questions/281640/how-do-i-get-a-human-readable-file-size-in-bytes-abbreviation-using-net#4975942
        // Returns the human-readable file size for an arbitrary, 64-bit file size. However it reverts to bytes for long.MinValue
        public static string ByteCountToSuffixHumbads( long byteCount )
        {
            // Get absolute value
            long absolute_i = (byteCount < 0 ? -byteCount : byteCount);

            // Determine the suffix and readable value
            string suffix;
            double readable;

            if ( absolute_i >= 0x1000000000000000 ) // Exabyte
            {
                suffix = "EB";
                readable = ( byteCount >> 50 );
            }
            else if ( absolute_i >= 0x4000000000000 ) // Petabyte
            {
                suffix = "PB";
                readable = ( byteCount >> 40 );
            }
            else if ( absolute_i >= 0x10000000000 ) // Terabyte
            {
                suffix = "TB";
                readable = ( byteCount >> 30 );
            }
            else if ( absolute_i >= 0x40000000 ) // Gigabyte
            {
                suffix = "GB";
                readable = ( byteCount >> 20 );
            }
            else if ( absolute_i >= 0x100000 ) // Megabyte
            {
                suffix = "MB";
                readable = ( byteCount >> 10 );
            }
            else if ( absolute_i >= 0x400 ) // Kilobyte
            {
                suffix = "KB";
                readable = byteCount;
            }
            else
            {
                return string.Format( "{0:F1} B", byteCount);  // Byte
            }

            // Divide by 1024 to get fractional value
            readable /= 1024 ;

            // Return formatted number with suffix
            return string.Format( "{0:F1} {1}", readable, suffix );
        }

#if UNITY_EDITOR
        [UnityEditor.MenuItem( "Window/NoiseCrimeStudios/Validations/Support_Formatting", false, 1300 )]
        public static void TestByteCountToSuffix()
        {
            long[] testValues = new long[]{ -1, 0, 1, 1000, 10000, 100000, 1000000, int.MaxValue, int.MinValue, long.MaxValue }; //, long.MinValue };

            string result;
            
            result = "";
            for ( int i = 0; i < testValues.Length; i++ )
                result += string.Format( "ByteCountToSuffixHumbads: {0} = {1}\n", testValues[ i ], ByteCountToSuffixHumbads( testValues[ i ] ) );

            Debug.Log( result );
        }
#endif



#if ALTERNATIVES
        // Don't use these methods, they are just for reference and not as efficeint as ByteCountToSuffixHumbads()

        static	string[]    suffixArray  = { "B", "KB", "MB", "GB", "TB", "PB", "EB" }; //Longs run out around EB

        // Note: Reverts to bytes for int.MinValue & long.MinValue
        [System.Obsolete("Obsolete. For reference only. Reverts to bytes for int.MinValue & long.MinValue")]
        public static string ByteCountToSuffixNC( long byteCount )
        {		
            int         index   = 0;
            double      output  = byteCount;

            while ( output >= 1024 )
            {
                output = output / 1024.0;
                index++;
            }

            if ( index >= suffixArray.Length ) return "err";

            return string.Format( "{0:F1} {1}", output, suffixArray[ index ] );
        }


        // Note: Fails on long.MinValue - DO NOT USE!
        [System.Obsolete("Obsolete. For reference only. Fails on long.MinValue with exception error.")]
        public static string ByteCountToSuffixDeepee( long byteCount )
        {
            if ( byteCount == 0 ) return "0" + suffixArray[ 0 ];

            long bytes  = Math.Abs(byteCount);
            int index   = Convert.ToInt32(Math.Floor(Math.Log(bytes, 1024)));
            double num  = Math.Round(bytes / Math.Pow(1024, index), 1);

            return string.Format( "{0:F1} {1}", Math.Sign( byteCount ) * num, suffixArray[ index ] );
        }


        // Requires NoiseCrimeStudio's StopWatchProfiler
        public static void PerformanceTestByteCountToSuffix()
        {
            long[] testValues = new long[]{ -1, 0, 1, 1000, 10000, 100000, 1000000, int.MaxValue, int.MinValue, long.MaxValue }; //, long.MinValue };

            StopWatchProfiler.NewFrame();

            StopWatchProfiler.BeginSample( "ByteCountToSuffixHumbads" );
            for ( int p = 0; p < 100000; p++ )
            {
                for ( int i = 0; i < testValues.Length; i++ )
                    ByteCountToSuffixHumbads( testValues[ i ] );
            }
            StopWatchProfiler.EndSample();

            Debug.Log( StopWatchProfiler.GetResults() );
        }
#endif

    }

}
