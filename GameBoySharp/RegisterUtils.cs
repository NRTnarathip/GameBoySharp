using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GameBoySharp
{
    public static class RegisterUtils
    {
        public static bool IsRegisterByName(string name)
        {
            switch (name)
            {
                case "A":
                case "B":
                case "C":
                case "D":
                case "E":
                case "F":
                case "H":
                case "L":
                case "AF":
                case "BC":
                case "DE":
                case "HL":
                    return true;
                default:
                    return false;
            }
        }

    }
}
