using CK.Core;
using FluentAssertions;
using Microsoft.Extensions.Configuration;
using NUnit.Framework;
using System;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.Json;

namespace CK.Core.Tests
{
    [TestFixture]
    public class MutableAndImmutableConfigurationSectionTests
    {
        [Test]
        public void ImmutableConfigurationSection_captures_everything()
        {
            using var config = new ConfigurationManager();
            config["X:A"] = "a";
            config["X:Nothing"] = "";
            config["X:Section"] = "Value for Section";
            config["X:Section:A"] = "a";
            config["X:Section:B"] = null;
            config["X:Section:C:More"] = "C more";

            CheckConfiguration( config.GetSection( "X" ) );
            var immutable = new ImmutableConfigurationSection( config.GetSection( "X" ) );
            CheckConfiguration( immutable );

            // Immutable captures non existing sections (no value and no children).
            var b = immutable.TryGetSection( "Section:B" );
            b.Should().NotBeNull();
            b.Exists().Should().BeFalse();

            static void CheckConfiguration( IConfigurationSection config )
            {
                config["A"].Should().Be( "a" );
                config["Nothing"].Should().Be( "" );
                config["Section"].Should().Be( "Value for Section" );
                config["Section:A"].Should().Be( "a" );
                config["Section:B"].Should().BeNull();
                config["Section:C:More"].Should().Be( "C more" );
                var sA = config.GetSection( "A" );
                sA.Value.Should().Be( "a" );
                sA.GetChildren().Should().BeEmpty();
                var sNothing = config.GetSection( "Nothing" );
                sNothing.Value.Should().Be( "" );
                sNothing.GetChildren().Should().BeEmpty();
                var sSection = config.GetSection( "Section" );
                sSection.Value.Should().Be( "Value for Section" );
                sSection.GetChildren().Should().HaveCount( 3 );
                sSection["A"].Should().Be( "a" );
                sSection["B"].Should().BeNull();
                sSection["C"].Should().BeNull();
                sSection["C:More"].Should().Be( "C more" );
                var sSectionC = sSection.GetSection( "C" );
                var sSectionC2 = config.GetSection( "Section:C" );
                sSectionC.Should().BeEquivalentTo( sSectionC2 );
                config["Section:C:More"].Should().Be( "C more" );
                // Bad key behavior.

                sSection["::::"].Should().BeNull();
                sSection.GetSection( "::::" ).Path.Should().Be( "X:Section:::::" );
                sSection.GetSection( "::::" ).Key.Should().Be( "" );

                sSection[":A"].Should().BeNull();
                sSection.GetSection( ":A" ).Path.Should().Be( "X:Section::A" );
                sSection.GetSection( ":A" ).Key.Should().Be( "A" );

                sSection["A:"].Should().BeNull();
                sSection.GetSection( "A:" ).Path.Should().Be( "X:Section:A:" );
                sSection.GetSection( "A:" ).Key.Should().Be( "" );

                sSection["NO:WAY"].Should().BeNull();
                sSection.GetSection( "NO:WAY" ).Path.Should().Be( "X:Section:NO:WAY" );
                sSection.GetSection( "NO:WAY" ).Key.Should().Be( "WAY" );

                sSection["::NO:WAY::"].Should().BeNull();
                sSection.GetSection( "::NO:WAY::" ).Path.Should().Be( "X:Section:::NO:WAY::" );
                sSection.GetSection( "::NO:WAY::" ).Key.Should().Be( "" );
            }

        }

        [Test]
        public void MutableConfigurationSection_invalid_parameters_check()
        {
            FluentActions.Invoking( () => new MutableConfigurationSection( (IConfigurationSection?)null! ) ).Should().Throw<ArgumentNullException>();
            FluentActions.Invoking( () => new MutableConfigurationSection( (string)null! ) ).Should().Throw<ArgumentNullException>();
            FluentActions.Invoking( () => new MutableConfigurationSection( "" ) ).Should().Throw<ArgumentException>();
            FluentActions.Invoking( () => new MutableConfigurationSection( ":" ) ).Should().Throw<ArgumentException>();
            FluentActions.Invoking( () => new MutableConfigurationSection( "A::B" ) ).Should().Throw<ArgumentException>();

            FluentActions.Invoking( () => new MutableConfigurationSection( "A::B", "" ) ).Should().Throw<ArgumentException>();
            FluentActions.Invoking( () => new MutableConfigurationSection( ":", "A" ) ).Should().Throw<ArgumentException>();
            FluentActions.Invoking( () => new MutableConfigurationSection( ":A", "A" ) ).Should().Throw<ArgumentException>();
            FluentActions.Invoking( () => new MutableConfigurationSection( "A:", "A" ) ).Should().Throw<ArgumentException>();

            FluentActions.Invoking( () => new MutableConfigurationSection( "A:B", "" ) ).Should().Throw<ArgumentException>();
            FluentActions.Invoking( () => new MutableConfigurationSection( "A", "A:" ) ).Should().Throw<ArgumentException>();
            FluentActions.Invoking( () => new MutableConfigurationSection( "A", "A:B" ) ).Should().Throw<ArgumentException>();
        }

        [Test]
        public void MutableConfigurationSection_simple_tests()
        {
            var c = new MutableConfigurationSection( "X" );
            c["Y"] = "Value";
            c.GetMutableChildren().Should().HaveCount( 1 );
            c.GetMutableChildren().Single().Path.Should().Be( "X:Y" );
            c.GetMutableChildren().Single().Value.Should().Be( "Value" );
            c["Y"].Should().Be( "Value" );

            c["Z:A:B:C"] = "Another Value";
            c["Z:A:B:C"].Should().Be( "Another Value" );
            c.GetRequiredSection( "Z" ).GetRequiredSection( "A" ).GetRequiredSection( "B" ).GetRequiredSection( "C" ).Value.Should().Be( "Another Value" );

        }

        [Test]
        public void MutableConfigurationSection_cannot_set_a_value_above_an_existing_one()
        {
            var c = new MutableConfigurationSection( "X" );
            c["Z:A:B:C"] = "Another Value";
            FluentActions.Invoking( () => c["Z:A:B"] = "No way" )
                .Should().Throw<InvalidOperationException>()
                         .WithMessage( "Unable to set 'X:Z:A:B' value to 'No way' since at least 'X:Z:A:B:C' (with value 'Another Value') exists below." );

            FluentActions.Invoking( () => c["Z"] = "No way" )
                         .Should().Throw<InvalidOperationException>();
        }

        [Test]
        public void MutableConfigurationSection_cannot_set_a_value_below_an_existing_one()
        {
            var c = new MutableConfigurationSection( "Root" );
            c["Z"] = "A top Value";
            FluentActions.Invoking( () => c["Z:A"] = "No way" )
                .Should().Throw<InvalidOperationException>()
                         .WithMessage( "Unable to set 'Root:Z:A' value to 'No way' since 'Root:Z' above has value 'A top Value'." );

            FluentActions.Invoking( () => c["Z:X:Y:Z"] = "No way 2" )
                         .Should().Throw<InvalidOperationException>()
                         .WithMessage( "Unable to set 'Root:Z:X:Y:Z' value to 'No way 2' since 'Root:Z' above has value 'A top Value'." );

            // Free the Z!
            c["Z"] = null;
            c["Z:A"] = "It works now!";
            c["Z:X:Y:Z"] = "It works also here.";
        }

        [Test]
        public void MutableConfigurationSections_when_inexisting_dont_prevent_setting_a_value_above()
        {
            var c = new MutableConfigurationSection( "X" );
            var empty1 = c.GetMutableSection( "A:A:A" );
            var empty2 = c.GetMutableSection( "A:B:C" );
            var empty3 = c.GetMutableSection( "A:B:C:D:E:F" );

            c["A"] = "It works!";

            FluentActions.Invoking( () => empty1.Value = "Pouf" )
                .Should().Throw<InvalidOperationException>( "Sections MUST remain empty when below a value." )
                .WithMessage( "Unable to set 'X:A:A:A' value to 'Pouf' since 'X:A' above has value 'It works!'." );
        }

        [Test]
        public void ImmutableConfigurationSection_LookupAllSection_finds_sub_keys()
        {
            var c = new MutableConfigurationSection( "X" );
            c["Key"] = "Key";
            c["A:Key"] = "A-Key";
            c["A:A:Key"] = "A-A-Key";
            c["A:A:A:Key"] = "A-A-A-Key";
            c["A:A:A:A:Key"] = "A-A-A-A-Key";

            var i = new ImmutableConfigurationSection( c );
            var deepest = i.GetSection( "A:A:A:A" );
            deepest.LookupAllSection( "Key" ).Select( s => s.Value ).Concatenate()
                .Should().Be( "Key, A-Key, A-A-Key, A-A-A-Key, A-A-A-A-Key" );
        }

        [Test]
        public void ImmutableConfigurationSection_LookupAllSection_finds_sub_paths()
        {
            var c = new MutableConfigurationSection( "X" );
            c["In:Key"] = "Key";
            c["A:In:Key"] = "A-Key";
            c["A:A:In:Key"] = "A-A-Key";
            c["A:A:A:In:Key"] = "A-A-A-Key";
            c["A:A:A:A:In:Key"] = "A-A-A-A-Key";

            var i = new ImmutableConfigurationSection( c );
            var deepest = i.GetSection( "A:A:A:A" );
            deepest.LookupAllSection( "In:Key" ).Select( s => s.Value ).Concatenate()
                .Should().Be( "Key, A-Key, A-A-Key, A-A-A-Key, A-A-A-A-Key" );
        }

        [Test]
        public void ImmutableConfigurationSection_LookupAllSection_skips_sections_on_the_search_path_by_default()
        {
            {
                var c = new MutableConfigurationSection( "X" );
                c["Key"] = "Key";
                c["A:Key:Key"] = "A-Key";
                c["A:Key:A:Key"] = "A-A-Key";
                c["A:Key:A:A:Key"] = "A-A-A-Key";
                c["A:Key:A:A:A:Key"] = "A-A-A-A-Key";

                var i = new ImmutableConfigurationSection( c );
                var deepest = i.GetSection( "A:Key:A:A:A" );
                deepest.LookupAllSection( "Key" ).Select( s => s.Value ?? s.Path ).Concatenate()
                    .Should().Be( "Key, A-Key, A-A-Key, A-A-A-Key, A-A-A-A-Key" );
            }
            {
                var c = new MutableConfigurationSection( "X" );
                c["In:Key"] = "Key";
                c["A:In:Key:In:Key"] = "A-Key";
                c["A:In:Key:A:In:Key"] = "A-A-Key";
                c["A:In:Key:A:A:In:Key"] = "A-A-A-Key";
                c["A:In:Key:A:A:A:In:Key"] = "A-A-A-A-Key";

                var i = new ImmutableConfigurationSection( c );
                var deepest = i.GetSection( "A:In:Key:A:A:A" );
                deepest.LookupAllSection( "In:Key" ).Select( s => s.Value ?? s.Path ).Concatenate()
                    .Should().Be( "Key, A-Key, A-A-Key, A-A-A-Key, A-A-A-A-Key" );
            }
        }

        [Test]
        public void ImmutableConfigurationSection_LookupAllSection_can_return_sections_on_the_search_path()
        {
            {
                var c = new MutableConfigurationSection( "X" );
                c["Key"] = "Key";
                c["A:Key:Key"] = "A-Key";
                c["A:Key:A:Key"] = "A-A-Key";
                c["A:Key:A:A:Key"] = "A-A-A-Key";
                c["A:Key:A:A:A:Key"] = "A-A-A-A-Key";

                var i = new ImmutableConfigurationSection( c );
                var deepest = i.GetSection( "A:Key:A:A:A" );
                deepest.LookupAllSection( "Key", skipSectionsOnThisPath: false ).Select( s => s.Value ?? s.Path ).Concatenate()
                    .Should().Be( "Key, X:A:Key, A-Key, A-A-Key, A-A-A-Key, A-A-A-A-Key" );
            }
            {
                var c = new MutableConfigurationSection( "X" );
                c["In:Key"] = "Key";
                c["A:In:Key:In:Key"] = "A-Key";
                c["A:In:Key:A:In:Key"] = "A-A-Key";
                c["A:In:Key:A:A:In:Key"] = "A-A-A-Key";
                c["A:In:Key:A:A:A:In:Key"] = "A-A-A-A-Key";

                var i = new ImmutableConfigurationSection( c );
                var deepest = i.GetSection( "A:In:Key:A:A:A" );
                deepest.LookupAllSection( "In:Key", skipSectionsOnThisPath: false ).Select( s => s.Value ?? s.Path ).Concatenate()
                    .Should().Be( "Key, X:A:In:Key, A-Key, A-A-Key, A-A-A-Key, A-A-A-A-Key" );
            }
        }

        [Test]
        public void adding_json_configuration()
        {
            var c = new MutableConfigurationSection( "Root" );
            c.Exists().Should().BeFalse();
            c.AddJson( "{}" );
            c.Exists().Should().BeFalse();

            FluentActions.Invoking( () => c.AddJson( null! ) ).Should().Throw<ArgumentNullException>();
            FluentActions.Invoking( () => c.AddJson( "" ) ).Should().Throw<JsonException>();
            FluentActions.Invoking( () => c.AddJson( "e" ) ).Should().Throw<JsonException>();
            FluentActions.Invoking( () => c.AddJson( "{ " ) ).Should().Throw<JsonException>();
            FluentActions.Invoking( () => c.AddJson( """{ "p": """ ) ).Should().Throw<JsonException>();
            c.Exists().Should().BeFalse();

            c.AddJson( """{ "S": "A" }""" );
            c["S"].Should().Be( "A" );

            c.AddJson( """{ "Num": 0.258e7 }""" );
            c["Num"].Should().Be( "0.258e7" );

            c.AddJson( """{ "F": false }""" );
            c["F"].Should().Be( "False" );

            c.AddJson( """{ "F": true }""" );
            c["F"].Should().Be( "True" );

            c.AddJson( """{ "N": null }""" );
            c["N"].Should().Be( null );
            c.GetMutableChildren().Single( sub => sub.Key == "N" ).Value.Should().Be( null );
            c.GetMutableChildren().Single( sub => sub.Key == "N" ).Exists().Should().BeFalse();

            c.AddJson( """{ "EmptyArray": [] }""" );
            c["EmptyArray"].Should().Be( null );
            c.GetMutableChildren().Single( sub => sub.Key == "EmptyArray" ).Value.Should().Be( null );
            c.GetMutableChildren().Single( sub => sub.Key == "EmptyArray" ).Exists().Should().BeFalse();

            c.AddJson( """{ "O": { "One": 1, "Two": 2.0, "Three": "trois" } }""" );
            c["O:One"].Should().Be( "1" );
            c["O:Two"].Should().Be( "2.0" );
            c["O:Three"].Should().Be( "trois" );

            c.AddJson( """{ "O": { "One": "1bis", "Two": 2.1, "Three": "trois.1", "AString": ["a","b", "cde", null, 12], "AO": [{ "In": true }, { "Out": 3712 } ]  } }""" );
            c["O:One"].Should().Be( "1bis" );
            c["O:Two"].Should().Be( "2.1" );
            c["O:Three"].Should().Be( "trois.1" );
            c["O:AString:0"].Should().Be( "a" );
            c["O:AString:1"].Should().Be( "b" );
            c["O:AString:2"].Should().Be( "cde" );
            c["O:AString:3"].Should().Be( null );
            c["O:AString:4"].Should().Be( "12" );
            c["O:AO:0:In"].Should().Be( "True" );
            c["O:AO:1:Out"].Should().Be( "3712" );
        }

        [Test]
        public void adding_json_configuration_can_skip_comments_when_they_are_allowed()
        {
            var c = new MutableConfigurationSection( "Root" );
            var r = new Utf8JsonReader( Encoding.UTF8.GetBytes( """
                                                                // c1
                                                                { /*c2*/ "A"
                                                                : /*c5*/"V" //c6
                                                                //c7
                                                                "B":true }

                                                                """ ),
                                        new JsonReaderOptions { CommentHandling = JsonCommentHandling.Allow } );
            c.AddJson( ref r );
            c["A"].Should().Be( "V" );
            c["B"].Should().Be( "True" );
        }

        [Test]
        public void empty_json_objects_are_NOT_read_at_all()
        {
            // We allow the trailing commas to appear when reading from a string.
            var c = new MutableConfigurationSection( "Root" )
                    .AddJson( """
                              {
                                "Unexisting": {},
                              }
                              """ );

            var iC = new ImmutableConfigurationSection( c );

            // There is no way with the Mutable to know if "Unexisting" has been created by the reader.
            c.GetMutableSection( "Unexisting" ).Should().NotBeNull();
            // But the Immutable captures the object and exposes the TryGetSection:
            // we can see that empty objects are totally ignored.
            iC.TryGetSection( "Unexisting" ).Should().BeNull();
        }

        [Test]
        public void ShouldApplyConfiguration_works()
        {
            var c = new MutableConfigurationSection( "Root" )
                    .AddJson( """
                              {
                                "ExplicitTrue": true,
                                "ExplicitFalse": false,
                                "Existing": { "SomeValue": 0 }
                              }
                              """ );

            var iC = new ImmutableConfigurationSection( c );

            // Checks correct type inference.
            c.ShouldApplyConfiguration( "Existing", true, out var interfaceContent ).Should().BeTrue();
            interfaceContent.Should().BeOfType<MutableConfigurationSection>();
            iC.ShouldApplyConfiguration( "Existing", true, out var immutableContent ).Should().BeTrue();
            immutableContent.Should().BeOfType<ImmutableConfigurationSection>();

            c.ShouldApplyConfiguration( "Existing", false, out interfaceContent ).Should().BeTrue();
            interfaceContent.Should().NotBeNull();
            iC.ShouldApplyConfiguration( "Existing", false, out immutableContent ).Should().BeTrue();
            immutableContent.Should().NotBeNull();

            c.ShouldApplyConfiguration( "ExplicitTrue", true, out interfaceContent ).Should().BeTrue();
            interfaceContent.Should().BeNull();
            iC.ShouldApplyConfiguration( "ExplicitTrue", true, out immutableContent ).Should().BeTrue();
            immutableContent.Should().BeNull();
            c.ShouldApplyConfiguration( "ExplicitTrue", false, out interfaceContent ).Should().BeTrue();
            interfaceContent.Should().BeNull();
            iC.ShouldApplyConfiguration( "ExplicitTrue", false, out immutableContent ).Should().BeTrue();
            immutableContent.Should().BeNull();

            c.ShouldApplyConfiguration( "ExplicitFalse", true, out interfaceContent ).Should().BeFalse();
            interfaceContent.Should().BeNull();
            iC.ShouldApplyConfiguration( "ExplicitFalse", true, out immutableContent ).Should().BeFalse();
            immutableContent.Should().BeNull();
            c.ShouldApplyConfiguration( "ExplicitFalse", false, out interfaceContent ).Should().BeFalse();
            interfaceContent.Should().BeNull();
            iC.ShouldApplyConfiguration( "ExplicitFalse", false, out immutableContent ).Should().BeFalse();
            immutableContent.Should().BeNull();

            c.ShouldApplyConfiguration( "Unexisting", true, out interfaceContent ).Should().BeTrue();
            interfaceContent.Should().BeNull();
            iC.ShouldApplyConfiguration( "Unexisting", true, out immutableContent ).Should().BeTrue();
            immutableContent.Should().BeNull();
            c.ShouldApplyConfiguration( "Unexisting", false, out interfaceContent ).Should().BeFalse();
            interfaceContent.Should().BeNull();
            iC.ShouldApplyConfiguration( "Unexisting", false, out immutableContent ).Should().BeFalse();
            immutableContent.Should().BeNull();



        }
    }
}
