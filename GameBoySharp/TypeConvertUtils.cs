using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GameBoySharp;

public static class TypeConvertUtils
{
    public static string ToHex(this byte v) => $"{v:X}";

    public static string ToHex(this ushort v) => $"{v:X}";
}
