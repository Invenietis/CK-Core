# FormattedString

A [`FormattedString`](FormattedString.cs) is a capture of an interpolated string:
- `Text` is the usual final interpolated string.
- `Placeholders` are all the occurences (Start,Length) of the formatted placeholders.
- `PlaceholderContents` is a `IEnumerable<ReadOnlyMemory<char>` of the formatted contents.
- `Culture` is the `CultureInfo` that has been used to format placeholders' content.

The `GetFormatString()` method computes the corresponding
[composite format string](https://learn.microsoft.com/en-us/dotnet/standard/base-types/composite-formatting):
positional placeholders {0}, {1} etc. for each placeholder.

The purpose of this format string is not to rewrite the message with other contents, it is to ease globalization
process by providing the message's format in order to translate it into different languages.

```csharp
    var enUS = CultureInfo.GetCultureInfo( "en-US" );
    var frFR = CultureInfo.GetCultureInfo( "fr-FR" );

    var d = new DateTime( 2023, 07, 27, 23, 59, 59, 999, DateTimeKind.Utc );
    var value = 37.12;

    var inAmerica = new FormattedString( enUS, $"Date: {d:F}, V: {value:C}" );
    var inFrance = new FormattedString( frFR, $"Date: {d:F}, V: {value:C}" );

    inAmerica.Text.Should().Be( "Date: Thursday, July 27, 2023 11:59:59 PM, V: $37.12" );
    inFrance.Text.Should().Be( "Date: jeudi 27 juillet 2023 23:59:59, V: 37,12 €" );
    inAmerica.GetFormatString().Should().Be( "Date: {0}, V: {1}" )
        .And.Be( inFrance.GetFormatString() );

    inAmerica.GetPlaceholderContents().Select( a => a.ToString() )
            .Should().BeEquivalentTo( new[] { "Thursday, July 27, 2023 11:59:59 PM", "$37.12" } );

    inFrance.GetPlaceholderContents().Select( a => a.ToString() )
            .Should().BeEquivalentTo( new[] { "jeudi 27 juillet 2023 23:59:59", "37,12 €" } );

    inFrance.Culture.Should().BeSameAs( frFR );
    inAmerica.Culture.Should().BeSameAs( enUS );
```

`FormattedString` supports both simple and versioned serialization. Their primary usage is
to be the message of the [`ResultMessage`](../ResultMessage/README.md) helper.







