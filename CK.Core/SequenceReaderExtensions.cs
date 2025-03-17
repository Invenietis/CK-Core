using System.Buffers;

namespace CK.Core;

/// <summary>
/// To be removed when https://github.com/dotnet/runtime/issues/30580 is implemented.
/// </summary>
public static class SequenceReaderExtensions
{
    /// <inheritdoc cref="System.Buffers.SequenceReaderExtensions.TryReadBigEndian(ref SequenceReader{byte}, out short)"/>
    public static bool TryReadBigEndian( ref this SequenceReader<byte> reader, out ushort value )
    {
        var res = reader.TryReadBigEndian( out short tmp );
        value = (ushort)tmp;
        return res;
    }
    /// <inheritdoc cref="System.Buffers.SequenceReaderExtensions.TryReadBigEndian(ref SequenceReader{byte}, out int)"/>
    public static bool TryReadBigEndian( ref this SequenceReader<byte> reader, out uint value )
    {
        var res = reader.TryReadBigEndian( out int tmp );
        value = (uint)tmp;
        return res;
    }
    /// <inheritdoc cref="System.Buffers.SequenceReaderExtensions.TryReadBigEndian(ref SequenceReader{byte}, out long)"/>
    public static bool TryReadBigEndian( ref this SequenceReader<byte> reader, out ulong value )
    {
        var res = reader.TryReadBigEndian( out long tmp );
        value = (ulong)tmp;
        return res;
    }
    /// <inheritdoc cref="System.Buffers.SequenceReaderExtensions.TryReadBigEndian(ref SequenceReader{byte}, out short)"/>
    public static bool TryReadLittleEndian( ref this SequenceReader<byte> reader, out ushort value )
    {
        var res = reader.TryReadLittleEndian( out short tmp );
        value = (ushort)tmp;
        return res;
    }
    /// <inheritdoc cref="System.Buffers.SequenceReaderExtensions.TryReadBigEndian(ref SequenceReader{byte}, out int)"/>
    public static bool TryReadLittleEndian( ref this SequenceReader<byte> reader, out uint value )
    {
        var res = reader.TryReadLittleEndian( out int tmp );
        value = (uint)tmp;
        return res;
    }
    /// <inheritdoc cref="System.Buffers.SequenceReaderExtensions.TryReadBigEndian(ref SequenceReader{byte}, out long)"/>
    public static bool TryReadLittleEndian( ref this SequenceReader<byte> reader, out ulong value )
    {
        var res = reader.TryReadLittleEndian( out long tmp );
        value = (ulong)tmp;
        return res;
    }
}
