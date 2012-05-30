#region LGPL License
/*----------------------------------------------------------------------------
* This file (Tests\CK.SharedDic.Tests\ArchitectureSharedDicReader.cs) is part of CiviKey. 
*  
* CiviKey is free software: you can redistribute it and/or modify 
* it under the terms of the GNU Lesser General Public License as published 
* by the Free Software Foundation, either version 3 of the License, or 
* (at your option) any later version. 
*  
* CiviKey is distributed in the hope that it will be useful, 
* but WITHOUT ANY WARRANTY; without even the implied warranty of
* MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the 
* GNU Lesser General Public License for more details. 
* You should have received a copy of the GNU Lesser General Public License 
* along with CiviKey.  If not, see <http://www.gnu.org/licenses/>. 
*  
* Copyright © 2007-2012, 
*     Invenietis <http://www.invenietis.com>,
*     In’Tech INFO <http://www.intechinfo.fr>,
* All rights reserved. 
*-----------------------------------------------------------------------------*/
#endregion

using System;
using NUnit.Framework;
using System.Collections.Generic;
using CK.Core;

namespace SharedDic.OverrideArchitecture
{

    public interface IReader 
    {
        int TheFunc( long l );
        
        void TheAction( string s );

        IList<string> SettablePropertyByOverride { get; }

        string SharedProperty { get; set; }

        string NormalProperty { get; set; }

        string NotOverridableFunc();

        string AnOverridableProperty { get; set; }

        IReader CreateOverride( Action<OverrideIReader> o );
    }

    public class OverrideIReader
    {
        public OverrideIReader()
        {
        }

        public OverrideIReader( IReader r )
        {
            TheFunc = r.TheFunc;
            TheAction = r.TheAction;
            SettablePropertyByOverride = r.SettablePropertyByOverride;

            GetAnOverridableProperty = SetAnOverridableProperty = Util.FuncIdentity;
        }

        public Func<long, int> TheFunc { get; set; }

        public Action<string> TheAction { get; set; }

        public IList<string> SettablePropertyByOverride { get; set; }

        string _anOverridableProperty;

        public Func<string, string> SetAnOverridableProperty { get; set; }       
        public Func<string, string> GetAnOverridableProperty { get; set; }

        public string AnOverridableProperty 
        {
            get { return GetAnOverridableProperty( _anOverridableProperty ); }
            set { _anOverridableProperty = SetAnOverridableProperty( _anOverridableProperty ); }
        }

    }


    public class Reader : IReader
    {
        // This is required for handling Shared properties.
        Reader _baseReader;
        string _sharedProperty;

        OverrideIReader _o;

        public Reader()
        {
            _o = new OverrideIReader();
            _o.SettablePropertyByOverride = new List<string>();
            _o.TheFunc = DefaultFunc;
            _o.TheAction = DefaultAction;
            _baseReader = this;
            _sharedProperty = "I'm one and only one...";
        }

        private Reader( Reader baseReader, OverrideIReader o )
        {
            _baseReader = baseReader;
            _o = o;
            NormalProperty = baseReader.NormalProperty;
        }

        public IList<string> SettablePropertyByOverride { get { return _o.SettablePropertyByOverride; } }

        public string NormalProperty { get; set; }

        public string SharedProperty { get { return _baseReader._sharedProperty; } set { _baseReader._sharedProperty = value; } }

        public string NotOverridableFunc()
        {
            return "This implementation will always be used.";
        }

        public int TheFunc( long l )
        {
            return _o.TheFunc( l );
        }

        public void TheAction( string s )
        {
            _o.TheAction( s );
        }

        int DefaultFunc( long l )
        {
            return (int)(l % 12);
        }
        
        void DefaultAction( string s )
        {
        }

        public string AnOverridableProperty 
        {
            get { return _o.AnOverridableProperty; }
            set { _o.AnOverridableProperty = value; } 
        }

        
        public IReader CreateOverride( Action<OverrideIReader> o )
        {
            OverrideIReader ov = new OverrideIReader( this );
            o( ov );
            return new Reader( this, ov );
        }
    }

    [TestFixture]
    public class ArchitectureSharedDicReader
    {

        [Test]
        public void PluggableFunction()
        {
            Reader r = new Reader();

            Assert.That( r.TheFunc( 13 ) == 1 );

            IReader r2 = r.CreateOverride( o => o.SettablePropertyByOverride = null );

            IReader r3 = r.CreateOverride( o => 
            {
                o.TheFunc = l => r.TheFunc( l ) * 3; 
                o.TheAction = s => Console.WriteLine( s );
                o.SettablePropertyByOverride = new List<string>(); 
            } );

            Assert.That( r2.TheFunc( 13 ) == 1 );
            Assert.That( r3.TheFunc( 13 ) == 3 );

        }

    }
}
