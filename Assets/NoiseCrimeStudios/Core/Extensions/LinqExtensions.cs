using System;
using System.Collections.Generic;
using System.Linq;

namespace NoiseCrimeStudios.Core
{
    public static class LinqExtensions
    {
        /// <summary>Helper method to choose between ascending or descending Order.</summary>
        /// <remarks>Taken from Unity's TreeViewExamples demo.</remarks>
        public static IOrderedEnumerable<T> OrderBy<T, TKey>( this IEnumerable<T> source, Func<T, TKey> selector, bool ascending )
        {
            if ( ascending )
                return source.OrderBy( selector );

            return source.OrderByDescending( selector );
        }

        /// <summary>Helper method to choose between ascending or descending ThenBy.</summary>
        /// <remarks>Taken from Unity's TreeViewExamples demo.</remarks>
        public static IOrderedEnumerable<T> ThenBy<T, TKey>( this IOrderedEnumerable<T> source, Func<T, TKey> selector, bool ascending )
        {
            if ( ascending )
                return source.ThenBy( selector );

            return source.ThenByDescending( selector );
        }
    }
}
