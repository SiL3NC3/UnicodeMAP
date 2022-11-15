using UnicodeMAP.Logic;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Unicode;

namespace UnicodeMAP.Models
{
    public class UnicodeBlock
    {
        public UnicodeCodePointRange CodePointRange { get; private set; }
        public string Name { get; set; }

        public UnicodeBlock(string name, string codeRange)
        {
            Name = name;

            var segments = codeRange.Split("..");
            var startCode = segments[0].Trim();
            var endCode = segments[1].Trim();
            CodePointRange = new UnicodeCodePointRange(Helper.HexToInt(startCode), Helper.HexToInt(endCode));
        }
    }
}
