using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GameBoySharp;

public static class TypeConvertUtils
{
    public static string ToHex(this byte v) => $"{v:X2}";
    public static string ToHex(this ushort v) => $"{v:X4}";
    public static string ToHex(this int v) => $"{v:X8}";
    public static byte SetBit(this byte me, int i, bool value)
    {
        if (value)
            return (byte)(me | (1 << i));    // Set bit to 1
        else
            return (byte)(me & ~(1 << i));   // Clear bit to 0
    }
    public static byte SetBit(this byte me, int i, byte value)
        => me.SetBit(i, value);
}
