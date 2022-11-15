using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace UnicodeMAP.Models
{
    public class UnicodeProperty
    {
        public string Abbreviation { get; private set; }
        public string Name { get; private set; }
        public bool IsMainProperty { get; private set; }
        public List<string> LinkedProperties { get; private set; }
        public UnicodeProperty MainProperty { get; private set; }

        public UnicodeProperty(
            string abbreviation, string name, bool isMainPropery, UnicodeProperty mainPropery, List<string> linkedProperties)
        {
            if (isMainPropery && mainPropery != null)
                throw new Exception("UnicodeProperty: A MainProperty cannot have a linked Main-UnicodeProperty, it would be linking to itself!");

            if (isMainPropery && linkedProperties == null)
                throw new Exception("UnicodeProperty: A MainProperty shoudl have linked Properties!");

            Abbreviation = abbreviation;
            Name = name;

            if (isMainPropery)
            {
                IsMainProperty = isMainPropery;
                LinkedProperties = linkedProperties;
            }
            else
            {
                MainProperty = mainPropery;
            }
        }

        public override string ToString()
        {
            if (IsMainProperty)
                return $"UnicodeProperty[MAIN]: {Abbreviation},{Name}";
            else
                return $"UnicodeProperty: {Abbreviation},{Name}";
        }
    }
}
