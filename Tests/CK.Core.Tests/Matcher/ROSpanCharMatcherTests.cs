using System;
using System.Linq;
using FluentAssertions;
using NUnit.Framework;

namespace CK.Core.Tests
{
    [TestFixture]
    public class ROSpanCharMatcherTests
    {
        [Test]
        public void simple_char_matching()
        {
            string s = "ABCD";
            var m = new ROSpanCharMatcher( s );
            m.TryMatch( 'a' ).Should().BeFalse();
            m.HasError.Should().BeTrue();
            m.TryMatch( 'A' ).Should().BeTrue();
            m.HasError.Should().BeFalse();
            m.TryMatch( 'A' ).Should().BeFalse();
            m.HasError.Should().BeTrue();
            m.TryMatch( 'B' ).Should().BeTrue();
            m.HasError.Should().BeFalse();
            m.TryMatch( 'C' ).Should().BeTrue();
            m.HasError.Should().BeFalse();

            m.TryMatch( 'D' ).Should().BeTrue();
            m.HasError.Should().BeFalse();
            m.GetErrors().Should().BeEmpty();

            m.TryMatch( 'D' ).Should().BeFalse();
            m.HasError.Should().BeTrue();
            m.GetErrors().Single().Expectation.Should().Be( "Character 'D'" );
        }

        [TestCase( "abcdef", 0xABCDEFUL )]
        [TestCase( "12abcdef", 0x12ABCDEFUL )]
        [TestCase( "12abcdef12abcdef", 0x12abcdef12abcdefUL )]
        [TestCase( "00000000FFFFFFFF", 0xFFFFFFFFUL )]
        [TestCase( "FFFFFFFFFFFFFFFF", 0xFFFFFFFFFFFFFFFFUL )]
        public void matching_hex_number( string s, ulong v )
        {
            var m = new ROSpanCharMatcher( s );
            m.TryMatchHexNumber( out ulong value ).Should().BeTrue();
            value.Should().Be( v );
            m.Head.Length.Should().Be( 0 );
        }

        [TestCase( "0|", 0x0UL, '|' )]
        [TestCase( "AG", 0xAUL, 'G' )]
        [TestCase( "cd", 0xCUL, 'd' )]
        public void matching_hex_number_one_digit( string s, ulong v, char end )
        {
            var m = new ROSpanCharMatcher( s );
            m.TryMatchHexNumber( out ulong value, 1, 1 ).Should().BeTrue();
            value.Should().Be( v );
            m.HasError.Should().BeFalse();
            m.Head.Length.Should().BePositive();
            m.Head[0].Should().Be( end );
        }

        [TestCase( "not a hex." )]
        [TestCase( "FA12 but we want 5 digits min." )]
        public void matching_hex_number_failures( string s )
        {
            var m = new ROSpanCharMatcher( s );
            m.TryMatchHexNumber( out ulong value, 5, 5 ).Should().BeFalse();
            m.Head.Length.Should().Be( s.Length );
            m.GetErrors().Single().Expectation.Should().Be( "5 digits hexadecimal number" );
            m.GetErrors().Single().CallerName.Should().Be( "TryMatchHexNumber" );
        }

        [Test]
        public void matching_texts_and_whitespaces()
        {
            string s = " AB  \t\r C";
            var m = new ROSpanCharMatcher( s );

            m.TryMatch( "A" ).Should().BeFalse();
            m.TrySkipWhiteSpaces().Should().BeTrue();
            m.TryMatch( "A" ).Should().BeTrue();
            m.TryMatch( "B" ).Should().BeTrue();

            m.TrySkipWhiteSpaces( 6 ).Should().BeFalse();
            m.HasError.Should().BeTrue();
            m.GetErrors().Single().Expectation.Should().Be( "At least 6 white space(s)" );

            m.TrySkipWhiteSpaces( 5 ).Should().BeTrue();
            m.HasError.Should().BeFalse();
            m.GetErrors().Should().BeEmpty();

            m.TrySkipWhiteSpaces().Should().BeFalse();
            m.HasError.Should().BeTrue();
            m.GetErrors().Single().Expectation.Should().Be( "At least one white space" );

            m.TryMatch( "c", StringComparison.OrdinalIgnoreCase ).Should().BeTrue();
            m.Head.IsEmpty.Should().BeTrue();

            m.TryMatch( "A" ).Should().BeFalse();
            m.TrySkipWhiteSpaces().Should().BeFalse();
        }

        [Test]
        public void matching_integers()
        {
            var m = new ROSpanCharMatcher( "X3712Y" );
            m.TryMatch( 'X' ).Should().BeTrue();
            m.TryMatchInt32( out var i ).Should().BeTrue();
            i.Should().Be( 3712 );
            m.TryMatch( 'Y' ).Should().BeTrue();
        }

        [Test]
        public void matching_digits()
        {
            var m = "X  012345678901234567890123456789  Y".AsSpan();
            m.TryMatch( 'X' ).Should().BeTrue();
            m.SkipWhiteSpaces().Should().BeTrue();

            m.TryMatchDigits( out var digits ).Should().BeTrue();
            digits.ToString().Should().Be( "012345678901234567890123456789" );

            m.SkipWhiteSpaces().Should().BeTrue();
            m.TryMatch( 'Y' ).Should().BeTrue();
        }

        [TestCase( "00003712 -000435 056", "AllowLeadingZeros" )]
        [TestCase( "3712 -435 56", "" )]
        public void matching_integers_with_min_max_values( string s, string withZeros )
        {
            bool allowZeros = withZeros == "AllowLeadingZeros";
            var m = new ROSpanCharMatcher( "3712 -435 56" );
            int i;
            m.TryMatchInt32( out i, -500, -400, allowZeros ).Should().BeFalse();
            m.TryMatchInt32( out i, 0, 3712, allowZeros ).Should().BeTrue();
            i.Should().Be( 3712 );
            m.TrySkipWhiteSpaces().Should().BeTrue();
            m.TryMatchInt32( out i, 0, allowLeadingZeros: allowZeros ).Should().BeFalse();
            m.TryMatchInt32( out i, -500, -400, allowZeros ).Should().BeTrue();
            i.Should().Be( -435 );
            m.TrySkipWhiteSpaces().Should().BeTrue();
            m.TryMatchInt32( out i, 1000, 2000, allowZeros ).Should().BeFalse();
            m.TryMatchInt32( out i, 56, 56, allowZeros ).Should().BeTrue();
            i.Should().Be( 56 );
            m.Head.IsEmpty.Should().BeTrue();
        }

        delegate T ROSpanCharMatcherFunc<T>( ROSpanCharMatcher m );

        [Test]
        public void match_methods_must_set_an_error()
        {
            var m = new ROSpanCharMatcher( "~" );

            CheckMatchError( m, m => m.TryMatch( 'a' ), "Character 'a'" );
            CheckMatchError( m, m => m.TryMatchHexNumber( out _ ), "Hexadecimal number (1 to 16 digits)" );
            CheckMatchError( m, m => m.TryMatchHexNumber( out _, 2, 6 ), "Hexadecimal number (2 to 6 digits)" );
            CheckMatchError( m, m => m.TryMatchHexNumber( out _, 3, 3 ), "3 digits hexadecimal number" );
            CheckMatchError( m, m => m.TryMatchInt32( out _ ), "Signed integer between -2147483648 and 2147483647 (without leading zeros)" );
            CheckMatchError( m, m => m.TryMatchInt32( out _, 0, 500 ), "Integer between 0 and 500 (without leading zeros)" );
            CheckMatchError( m, m => m.TryMatchInt32( out _, -2, -1 ), "Signed integer between -2 and -1 (without leading zeros)" );
            CheckMatchError( m, m => m.TryMatchInt32( out _, 0, 500, allowLeadingZeros: true ), "Integer between 0 and 500" );
            CheckMatchError( m, m => m.TryMatchInt32( out _, -2, -1, allowLeadingZeros: true ), "Signed integer between -2 and -1" );
            CheckMatchError( m, m => m.TryMatchGuid( out _ ), "Guid" );
            CheckMatchError( m, m => m.TryMatchDouble( out _ ), "Floating number" );
            CheckMatchError( m, m => m.TrySkipWhiteSpaces(), "At least one white space" );
        }

        static void CheckMatchError( ROSpanCharMatcher m, ROSpanCharMatcherFunc<bool> fail, string message )
        {
            int len = m.Head.Length;
            fail( m ).Should().BeFalse();
            m.HasError.Should().BeTrue();
            m.Head.Length.Should().Be( len );
            m.GetErrors().Should().HaveCount( 1 );
            m.GetErrors().Single().Expectation.Should().Be( message );
            m.ClearExpectations();
        }

        [TestCase( "0", 0 )]
        [TestCase( "2.3", 2.3 )]
        [TestCase( "9876978", 9876978 )]
        [TestCase( "-9876978", -9876978 )]
        [TestCase( "0.0", 0 )]
        [TestCase( "0.00", 0 )]
        [TestCase( "0.34", 0.34 )]
        [TestCase( "-4e+5", -4e5 )]
        [TestCase( "4E5", 4E5 )]
        [TestCase( "4.0E5", 4E5 )]
        [TestCase( "29380.34e98", 29380.34e98 )]
        [TestCase( "29380.34E98", 29380.34E98 )]
        [TestCase( "-80.34e-98", -80.34e-98 )]
        [TestCase( "-80.34E+98", -80.34E98 )]
        public void matching_double_values( string s, double d )
        {
            var m = new ROSpanCharMatcher( "P" + s + "S" );
            m.TryMatch( 'P' ).Should().BeTrue();
            m.TrySkipDouble().Should().BeTrue();
            m.TryMatch( 'S' ).Should().BeTrue();
            m.Head.IsEmpty.Should().BeTrue();

            m.Head = m.AllText.Slice( 1 );
            m.TryMatchDouble( out var parsed ).Should().BeTrue();
            parsed.Should().BeApproximately( d, 1f );
            m.TryMatch( 'S' ).Should().BeTrue();
            m.Head.IsEmpty.Should().BeTrue();
        }

        [TestCase( "N" )]
        [TestCase( "D" )]
        [TestCase( "B" )]
        [TestCase( "P" )]
        [TestCase( "X" )]
        public void matching_the_5_forms_of_guid( string form )
        {
            var id = Guid.NewGuid();
            string sId = id.ToString( form );
            {
                string s = sId;
                var m = new ROSpanCharMatcher( s );
                Guid readId;
                m.TryMatchGuid( out readId ).Should().BeTrue();
                readId.Should().Be( id );
            }
            {
                string s = "S" + sId;
                var m = new ROSpanCharMatcher( s );
                Guid readId;
                m.TryMatch( 'S' ).Should().BeTrue();
                m.TryMatchGuid( out readId ).Should().BeTrue();
                readId.Should().Be( id );
            }
            {
                string s = "S" + sId + "T";
                var m = new ROSpanCharMatcher( s );
                Guid readId;
                m.TryMatch( 'S' ).Should().BeTrue();
                m.TryMatchGuid( out readId ).Should().BeTrue();
                readId.Should().Be( id );
                m.TryMatch( 'T' ).Should().BeTrue();
            }
            sId = sId.Remove( sId.Length - 1 );
            {
                string s = sId;
                var m = new ROSpanCharMatcher( s );
                Guid readId;
                m.TryMatchGuid( out readId ).Should().BeFalse();
                m.Head.Length.Should().Be( m.AllText.Length );
            }
            sId = id.ToString().Insert( 3, "K" ).Remove( 4 );
            {
                string s = sId;
                var m = new ROSpanCharMatcher( s );
                Guid readId;
                m.TryMatchGuid( out readId ).Should().BeFalse();
                m.Head.Length.Should().Be( m.AllText.Length );
            }
        }


        [Test]
        public void OpenExpectations_are_pooled_objects()
        {
            var m = new ROSpanCharMatcher( "" );
            var g1 = m.OpenExpectations();
            g1.Dispose();
            var g1Back = m.OpenExpectations();
            g1Back.Should().BeSameAs( g1 );
            g1Back.Dispose();

            g1 = m.OpenExpectations();
            var g2 = m.OpenExpectations();
            var g3 = m.OpenExpectations();
            var g4 = m.OpenExpectations();
            var g5 = m.OpenExpectations();

            g5.Dispose();
            g4.Dispose();
            g3.Dispose();
            g2.Dispose();
            g1.Dispose();

            g1Back = m.OpenExpectations();
            var g2Back = m.OpenExpectations();
            var g3Back = m.OpenExpectations();
            var g4Back = m.OpenExpectations();
            var g5Back = m.OpenExpectations();

            g1Back.Should().BeSameAs( g1 );
            g2Back.Should().BeSameAs( g2 );
            g3Back.Should().BeSameAs( g3 );
            g4Back.Should().BeSameAs( g4 );
            g5Back.Should().BeSameAs( g5 );
        }

        [Test]
        public void Mismatch_OpenExpectations_Dispose_throw_InvalidOpertionException()
        {
            var m = new ROSpanCharMatcher( "" );
            var g1 = m.OpenExpectations();
            var g2 = m.OpenExpectations();
            FluentActions.Invoking( () => g1.Dispose() ).Should().Throw<InvalidOperationException>();
            g2.Dispose();
            g1.Dispose();
        }

        [Test]
        public void Expectation_and_error_management_with_OpenExpectations()
        {
            const string defaultGroupName = nameof( Expectation_and_error_management_with_OpenExpectations );

            var m = new ROSpanCharMatcher( "" );
            m.HasError.Should().BeFalse();
            using( m.OpenExpectations() )
            {
                m.HasError.Should().BeFalse( "Newly opened group has no error." );
                m.ClearExpectations();
            }
            m.HasError.Should().BeFalse( "ClearExpectations has been called in group." );
            using( m.OpenExpectations() )
            {
                m.HasError.Should().BeFalse();
            }
            m.HasError.Should().BeTrue( "ClearExpectations has NOT been called in group: the error is the caller name since there's no explicit expect string." );
            m.GetErrors().Single().Expectation.Should().Be( defaultGroupName );

            m.ClearExpectations();
            m.HasError.Should().BeFalse();
            m.GetErrors().Should().BeEmpty();

            using( m.OpenExpectations() )
            {
                m.HasError.Should().BeFalse();
                m.AddExpectation( "1" );
                m.ClearExpectations();

                m.HasError.Should().BeFalse();
                m.GetErrors().Should().BeEmpty();
            }
            m.HasError.Should().BeFalse();
            m.GetErrors().Should().BeEmpty();

            using( m.OpenExpectations() )
            {
                m.HasError.Should().BeFalse();
                m.AddExpectation( "1" );
                m.HasError.Should().BeTrue();
                using( m.OpenExpectations( "sub" ) )
                {
                    m.HasError.Should().BeFalse( "No error in this group." );
                }
                m.HasError.Should().BeTrue();
                m.GetErrors().Select( e => e.Expectation ).Should().BeEquivalentTo( new[] { "1", "sub" } );
            }
            m.HasError.Should().BeTrue();
            m.GetErrors().Select( e => e.Expectation ).Should().BeEquivalentTo( new[] { defaultGroupName, "1", "sub" } );

            using( m.OpenExpectations( "G2" ) )
            {
                m.HasError.Should().BeFalse();
                m.AddExpectation( "2" );
                m.HasError.Should().BeTrue();
                using( m.OpenExpectations( "sub2" ) )
                {
                    m.HasError.Should().BeFalse();
                    m.AddExpectation( "E" );
                }
                m.HasError.Should().BeTrue();
                m.GetErrors().Select( e => e.Expectation ).Should().BeEquivalentTo( new[] { "2", "sub2", "E" } );
            }
            m.HasError.Should().BeTrue();
            m.GetErrors().Select( e => e.Expectation ).Should().BeEquivalentTo( new[] { defaultGroupName, "1", "sub", "G2", "2", "sub2", "E" } );
        }

        [Test]
        public void when_SingleExpectationMode_is_true_only_the_last_expectation_is_kept()
        {
            var m = new ROSpanCharMatcher( "" );
            m.SingleExpectationMode.Should().BeFalse( "The default is false." );
            m.SingleExpectationMode = true;

            m.AddExpectation( "A" );
            m.AddExpectation( "B" );
            m.AddExpectation( "C" );

            m.HasError.Should().BeTrue();
            m.GetErrors().Single().Expectation.Should().Be( "C" );

            m.SingleExpectationMode = false;

            m.AddExpectation( "A" );
            m.AddExpectation( "B" );

            m.HasError.Should().BeTrue();
            m.GetErrors().Select( e => e.Expectation ).Should().BeEquivalentTo( new[] { "C", "A", "B" } );

            m.SingleExpectationMode = true;

            m.HasError.Should().BeTrue();
            m.GetErrors().Single().Expectation.Should().Be( "B" );

        }

        [Test]
        public void above_SingleExpectationMode_initially_propagates_to_OpenExpectations_groups()
        {
            var m = new ROSpanCharMatcher( "" );
            m.SingleExpectationMode.Should().BeFalse();
            m.SingleExpectationMode = true;

            using( m.OpenExpectations( "Thing" ) )
            {
                m.SingleExpectationMode.Should().BeTrue();
                m.AddExpectation( "A" );
                m.AddExpectation( "B" );
                m.AddExpectation( "C" );
                m.HasError.Should().BeTrue();
                m.GetErrors().Single().Expectation.Should().Be( "C" );
            }
            m.HasError.Should().BeTrue();
            m.GetErrors().Single().Expectation.Should().Be( "Thing" );

            m.AddExpectation( "A" );
            m.AddExpectation( "B" );
            m.AddExpectation( "C" );

            m.HasError.Should().BeTrue();
            m.GetErrors().Single().Expectation.Should().Be( "C" );

            m.ClearExpectations();
            m.GetErrors().Should().BeEmpty();

            using( m.OpenExpectations( "Thing1" ) )
            {
                m.HasError.Should().BeFalse();
                m.SingleExpectationMode.Should().BeTrue();
                m.AddExpectation( "A1" );
                m.AddExpectation( "B1" );
                m.AddExpectation( "C1" );
                m.HasError.Should().BeTrue();
                m.GetErrors().Single().Expectation.Should().Be( "C1" );
                using( m.OpenExpectations( "Thing2" ) )
                {
                    m.HasError.Should().BeFalse();
                    m.SingleExpectationMode.Should().BeTrue();
                    m.AddExpectation( "A2" );
                    m.AddExpectation( "B2" );
                    m.AddExpectation( "C2" );
                    m.HasError.Should().BeTrue();
                    m.GetErrors().Single().Expectation.Should().Be( "C2" );
                }
                m.HasError.Should().BeTrue();
                m.GetErrors().Single().Expectation.Should().Be( "Thing2" );
            }
            m.HasError.Should().BeTrue();
            m.GetErrors().Single().Expectation.Should().Be( "Thing1" );
        }

        [Test]
        public void SingleExpectationMode_can_be_freely_changed_in_any_OpenExpectations_group()
        {
            var m = new ROSpanCharMatcher( "" );
            m.SingleExpectationMode.Should().BeFalse();
            m.SingleExpectationMode = true;

            using( m.OpenExpectations( "Thing" ) )
            {
                m.SingleExpectationMode.Should().BeTrue();
                m.SingleExpectationMode = false;
                m.AddExpectation( "A" );
                m.AddExpectation( "B" );
                m.AddExpectation( "C" );
                m.HasError.Should().BeTrue();
                m.GetErrors().Select( e => e.Expectation ).Should().BeEquivalentTo( "A", "B", "C" );
            }
            m.HasError.Should().BeTrue();
            m.GetErrors().Single().Expectation.Should().Be( "Thing" );

            m.SingleExpectationMode = false;
            using( m.OpenExpectations( "Thing2" ) )
            {
                m.SingleExpectationMode.Should().BeFalse();
                m.AddExpectation( "A" );
                m.AddExpectation( "B" );
                m.AddExpectation( "C" );
                m.HasError.Should().BeTrue();
                m.GetErrors().Select( e => e.Expectation ).Should().BeEquivalentTo( "A", "B", "C" );

                m.SingleExpectationMode = true;
                m.GetErrors().Single().Expectation.Should().Be( "C" );
            }

            m.HasError.Should().BeTrue();
            m.GetErrors().Select( e => e.Expectation ).Should().BeEquivalentTo( "Thing", "Thing2", "C" );
        }

    }
}
