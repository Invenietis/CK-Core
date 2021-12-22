# ROSpanMatcher: the "Match and Forward" pattern

## Macth and Forward
This handy little parsing pattern allows you to combine parsing functions much more easily and efficiently
than the standard TryParse methods. A few explanations below.

Static TryParse methods accept a `string` (and now rather a `ReadOnlySpan<char>`), return a boolean and the out
parameter of the parsed value on success: 

```csharp
public struct UInt32
{
  public static bool TryParse( ReadOnlySpan<char> s, out uint result )
  {
    //...
  }
}
```

It's convenient... but this pattern has a big problem: 
 - the string doesn't support a suffix: TryParse("5454 ", out _ ) fails because of the final little blank.
 - this doesn't tell us how many characters have been matched (because they only match what you provide).

This prohibits chaining these methods together to parse small, not too complex grammars in a simple way: in fact,
they cannot be combined but rather used with "splits" on specific delimiting characters (which, besides limiting the
flexibility of the patterns, systematically introduces several consecutive passes on the string).

The Matcher pattern does the same thing except that the principle is to advance a read head (or at least to be able to advance a
read head knowing the length of the match) in order to chain the calls and "advance in the grammar". 

Below is an example of a simple code that parses a LED state (Color and State) and advances a `ReadOnlySpan<char>` read head
passed by reference (otherwise the head would not advance!).

(If you're not familiar with `ReadOnlySpan` `Span` and other `Memory`, please read this: https://docs.microsoft.com/en-us/archive/msdn-magazine/2018/january/csharp-all-about-span-exploring-a-new-net-mainstay.)

This first method simply handles one character.

```csharp
public static bool TryMatch( ref ReadOnlySpan<char> h, out LEDColor color )
{
    color = default;
    if( h.Length == 0 ) return false;
    switch( h[0] )
    {
        case 'W': color = LEDColor.White; break;
        case 'R': color = LEDColor.Red; break;
        case 'G': color = LEDColor.Green; break;
        case 'B': color = LEDColor.Blue; break;
        case 'Y': color = LEDColor.Yellow; break;
        case 'M': color = LEDColor.Magenta; break;
        case 'C': color = LEDColor.Cyan; break;
        default: return false;
    };
    h = h.Slice( 1 );
    return true;
}
```

Now, a LEDState is a color and a possible combination of Off, Blink and/or Transparent modifiers (in this order separated by commas
without any white space):

```csharp
public static bool TryMatch( ref ReadOnlySpan<char> h, out LEDState state )
{
    state = default;
    if( TryMatch( ref h, out LEDColor color ) )
    {
        state = state.SetColor( color );
        if( h.Length > 3 )
        {
            state = state.Switch( !h.TryMatch( ",Off" ) )
                            .SetBlinking( !h.TryMatch( ",Blink" ) )
                            .SetOpacity( !h.TryMatch( ",Transparent" ) );
        }
        return true;
    }
    return false;
}
```

The model also handles a somewhat special state: the "Background" which is the fact that the LED must be in
the default state defined by the LED strip. In code this is represented by a `Nullable<LEDState>` (a null `LEDState?`):
```csharp
public static bool TryMatch( ref ReadOnlySpan<char> h, out LEDState? state )
{
    state = null;
    if( h.TryMatch( "Background" ) )
    {
        return true;
    }
    if( TryMatch( ref h, out LEDState s ) )
    {
        state = s;
        return true;
    }
    return false;
}
```

Here we can start to see the combinations at work.
What about the standard .Net TryParse? It is simply a match followed by the end of the chain:

```csharp
/// <summary>
/// Tries to parse a nullable <see cref="LEDState"/> (see <see cref="LEDExtensions.ToParseable(LEDState?)"/>).
/// The "Background" string is the null state.
/// </summary>
/// <param name="s">The string to parse.</param>
/// <param name="state">The resulting state.</param>
/// <returns>True on success, false is not matched.</returns>
public static bool TryParse( ReadOnlySpan<char> s, out LEDState? state ) => TryMatch( ref s, out state ) && s.IsEmpty;
```

It's always the same piece of code to write (CodeGeneration here would be great...).
To conclude this sample on composition, a "piece of LED strip":

```csharp
/// <summary>
/// Tries to parse a pattern: a pipe separated list of <see cref="LEDState?"/> (see <see cref="TryMatch(ref ReadOnlySpan{char}, out LEDState?)"/>).
/// </summary>
/// <param name="h">The head.</param>
/// <param name="pattern">The resulting pattern.</param>
/// <returns>True on success, false is not matched.</returns>
public static bool TryMatch( ref ReadOnlySpan<char> h, [NotNullWhen(true)]out List<LEDState?>? pattern )
{
    pattern = null;
    while( TryMatch( ref h, out LEDState? s ) )
    {
        if( pattern == null ) pattern = new List<LEDState?>();
        pattern.Add( s );
        if( !h.TryMatch( '|' ) ) return true;
    }
    return false;
}
```

## More complex grammars: exposing detailed syntax errors

Previous examples are simple. However when combining them, even an error in one Color character may be hard to spot in a complex
grammar. Feedback to the user is crucial in any complex system. Without error management, the readability and maintainability of
the combinations of these small TryMatch functions become unusable in practice (a user cannot be satisfied with a terse "Invalid Syntax",
he must be told where and what is invalid).

Below is an example of the simple color match that explicit its error:

```csharp
public static bool TryMatch( ref ROSpanCharMatcher m, out LEDColor color )
{
    color = default;
    if( m.Head.Length > 0 )
    {
        switch( m.Head[0] )
        {
            case 'W': color = LEDColor.White; break;
            case 'R': color = LEDColor.Red; break;
            case 'G': color = LEDColor.Green; break;
            case 'B': color = LEDColor.Blue; break;
            case 'Y': color = LEDColor.Yellow; break;
            case 'M': color = LEDColor.Magenta; break;
            case 'C': color = LEDColor.Cyan; break;
            default: goto error; 
        };
        m.Head = m.Head.Slice( 1 );
        return m.ClearExpectations();
    }
    error:
    return m.AddExpectation( "Color char expected: W, R, G, B, Y, M, C." );
}
```

