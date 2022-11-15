using UnicodeMAP.Logic;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;

namespace UnicodeMAP.Models
{
    public class UnicodeCharacter : Character
    {
        public UnicodeProperty Property { get; set; }

        public UnicodeCharacter(string code, string name, UnicodeProperty property)
        {
            ColorBlend = false;
            Icon = Helper.GetUnicodeDisplayText(code);
            Code = code;
            Name = name;
            Property = property;
        }
    }
}
