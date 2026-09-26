using System.Security.Cryptography;
namespace MyFundex.BuildingBlocks.Ids;
public static class Uuid7
{
    public static Guid NewGuid()
    {
        Span<byte> b=stackalloc byte[16]; RandomNumberGenerator.Fill(b);
        var ms=DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();
        b[0]=(byte)(ms>>40);b[1]=(byte)(ms>>32);b[2]=(byte)(ms>>24);b[3]=(byte)(ms>>16);b[4]=(byte)(ms>>8);b[5]=(byte)ms;
        b[6]=(byte)((b[6]&0x0F)|0x70); b[8]=(byte)((b[8]&0x3F)|0x80);
        // Guid(byte[]) has mixed-endian fields; create from RFC bytes explicitly.
        return new Guid(((int)b[0]<<24)|((int)b[1]<<16)|((int)b[2]<<8)|b[3],(short)((b[4]<<8)|b[5]),(short)((b[6]<<8)|b[7]),b[8],b[9],b[10],b[11],b[12],b[13],b[14],b[15]);
    }
}
