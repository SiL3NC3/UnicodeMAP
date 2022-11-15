using UnicodeMAP.Logic;

namespace UnicodeMAP.Models
{
    public class Character
    {
        public string Code { get; set; }
        public bool ColorBlend { get; set; }
        public string Icon { get; set; }
        public string Name { get; set; }
        public string Text
        {
            get { return $"{Name.ToUpper()}"; }
        }
    }
}