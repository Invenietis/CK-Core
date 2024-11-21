using System;
using System.ComponentModel;
using System.Globalization;

namespace CK.Core;

[TypeConverter( typeof( Converter ) )]
public readonly partial struct NormalizedPath : IConvertible
{
    sealed class Converter : TypeConverter
    {
        public override bool CanConvertFrom( ITypeDescriptorContext? context, Type sourceType )
        {
            return sourceType == typeof( string );
        }

        public override bool CanConvertTo( ITypeDescriptorContext? context, Type? destinationType )
        {
            return destinationType == typeof( string );
        }

        public override object? ConvertFrom( ITypeDescriptorContext? context, CultureInfo? culture, object value )
        {
            return value is string s ? new NormalizedPath( s ) : default;
        }

        public override object? ConvertTo( ITypeDescriptorContext? context, CultureInfo? culture, object? value, Type destinationType )
        {
            return value is NormalizedPath p ? p.ToString() : string.Empty;
        }
    }

    #region IConvertible implementation
    static T ThrowNotSupported<T>()
    {
        var ex = ThrowNotSupported( typeof( T ) );
        return (T)ex;
    }

    static object ThrowNotSupported( Type type )
    {
        throw new InvalidCastException( $"Converting a NormalizedPath to '{type}' is not supported." );
    }

    TypeCode IConvertible.GetTypeCode() => TypeCode.Object;
    bool IConvertible.ToBoolean( IFormatProvider? provider ) => ThrowNotSupported<bool>();
    char IConvertible.ToChar( IFormatProvider? provider ) => ThrowNotSupported<char>();
    sbyte IConvertible.ToSByte( IFormatProvider? provider ) => ThrowNotSupported<sbyte>();
    byte IConvertible.ToByte( IFormatProvider? provider ) => ThrowNotSupported<byte>();
    short IConvertible.ToInt16( IFormatProvider? provider ) => ThrowNotSupported<short>();
    ushort IConvertible.ToUInt16( IFormatProvider? provider ) => ThrowNotSupported<ushort>();
    int IConvertible.ToInt32( IFormatProvider? provider ) => ThrowNotSupported<int>();
    uint IConvertible.ToUInt32( IFormatProvider? provider ) => ThrowNotSupported<uint>();
    long IConvertible.ToInt64( IFormatProvider? provider ) => ThrowNotSupported<long>();
    ulong IConvertible.ToUInt64( IFormatProvider? provider ) => ThrowNotSupported<ulong>();
    float IConvertible.ToSingle( IFormatProvider? provider ) => ThrowNotSupported<float>();
    double IConvertible.ToDouble( IFormatProvider? provider ) => ThrowNotSupported<double>();
    decimal IConvertible.ToDecimal( IFormatProvider? provider ) => ThrowNotSupported<decimal>();
    DateTime IConvertible.ToDateTime( IFormatProvider? provider ) => ThrowNotSupported<DateTime>();
    string IConvertible.ToString( IFormatProvider? provider ) => Path;

    object IConvertible.ToType( Type conversionType, IFormatProvider? provider )
    {
        if( conversionType == typeof( NormalizedPath ) )
        {
            return this;
        }
        if( conversionType == typeof( string ) )
        {
            return Path;
        }
        return ThrowNotSupported( conversionType );
    }
    #endregion 
}
