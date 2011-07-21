using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;

namespace CK.Keyboard
{
	public partial class KeyboardContext
	{
        /// <summary>
        /// Computes a unique name (suffixed with '(n)' where n is a number) given 
        /// a function that check existence of proposed names.
        /// It relies on <see cref="R.KeyboardAutoNumPattern"/> and <see cref="R.KeyboardAutoNumRegex"/> resources
        /// to offer culture dependant naming.
        /// </summary>
        /// <param name="newName">Proposed name.</param>
        /// <param name="currentName">Current name (null if none).</param>
        /// <param name="exists">Function that check the existence.</param>
        /// <returns>A unique name based on proposed name.</returns>
        internal static string EnsureUnique( string newName, string currentName, Predicate<string> exists )
        {
            string nCleaned = Regex.Replace( newName, R.KeyboardAutoNumRegex, String.Empty );
            string n = nCleaned;
            if ( n != currentName )
            {
                int autoNum = 1;
                while ( n != currentName && exists( n ) )
                {
                    n = String.Format( R.KeyboardAutoNumPattern, nCleaned, autoNum++ );
                }
            }
            return n;
        }
	}
}
