using System;

namespace CK.Core
{
    /// <summary>
    /// Minimal ref struct that captures a <see cref="Parsed"/> and <see cref="Remainder"/>.
    /// </summary>
    public ref struct ROParseResult
    {
        /// <summary>
        /// Gets the original text.
        /// </summary>
        public readonly ReadOnlySpan<char> Text;

        /// <summary>
        /// Gets the remainder to parse.
        /// </summary>
        public readonly ReadOnlySpan<char> Remainder => Text.Slice( ParsedLength );

        /// <summary>
        /// Gets the parsed part. Empty when <see cref="Success"/> is true.
        /// </summary>
        public readonly ReadOnlySpan<char> Parsed => Text.Slice( 0, ParsedLength );

        /// <summary>
        /// The parsed length. 0 when <see cref="Success"/> is false.
        /// </summary>
        public readonly int ParsedLength;

        /// <summary>
        /// Gets whether the <see cref="Parsed"/> is not empty.
        /// </summary>
        public bool Success => ParsedLength != 0;

        /// <summary>
        /// Returns true or throws a <see cref="FormatException"/>.
        /// </summary>
        /// <param name="message">The exception's message.</param>
        /// <returns>Always true.</returns>
        /// <exception cref="FormatException">When <see cref="Success"/> is false.</exception>
        public bool SuccessOrThrowFormatException( string message ) => Success ? true : throw new FormatException( message );

        /// <summary>
        /// Returns true or throws a <see cref="FormatException"/>.
        /// </summary>
        /// <returns>Always true.</returns>
        /// <exception cref="FormatException">When <see cref="Success"/> is false.</exception>
        public bool SuccessOrThrowFormatException() => Success ? true : throw new FormatException();

        /// <summary>
        /// Implicit conversion to boolean of <see cref="Success"/>.
        /// </summary>
        /// <param name="r">The result.</param>
        public static implicit operator bool( ROParseResult r ) => r.Success;

        /// <summary>
        /// Returns this ROParseResult if the parse succeeded and <see cref="Text"/> has been fully parsed (or not) or a failed one.
        /// </summary>
        /// <param name="atEnd">True if the Text must be fully parsed.</param>
        /// <returns>This ROParseResult if the parse succeeded and <see cref="Text"/> has been fully parsed (or not) or a failed one.</returns>
        public bool AndAtEnd( bool atEnd ) => Success && (atEnd == (Text.Length == ParsedLength)) ? this : new ROParseResult( Text, 0 );

        /// <summary>
        /// Returns this ROParseResult if the parse succeeded and <see cref="Text"/> has been fully parsed or a failed one.
        /// </summary>
        /// <returns>This ROParseResult if the parse succeeded and <see cref="Text"/> has been fully parsed or a failed one.</returns>
        public ROParseResult AndAtEnd() => Success && Text.Length == ParsedLength ? this : new ROParseResult( Text, 0 );

        /// <summary>
        /// Returns true if <see cref="Success"/> is false or there is no <see cref="Remainder"/> to parse.
        /// </summary>
        /// <returns>True if <see cref="Success"/> is false or there is no <see cref="Remainder"/> to parse.</returns>
        public bool OrAtEnd() => Success || Remainder.Length == 0;

        /// <summary>
        /// Initializes a new <see cref="ROParseResult"/>.
        /// </summary>
        /// <param name="origin">The original text to parse.</param>
        /// <param name="parsed">The number of parsed characters.</param>
        public ROParseResult( ReadOnlySpan<char> origin, int parsed )
        {
            Text = origin;
            ParsedLength = parsed;
        }
    }

}
