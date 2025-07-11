using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Numerics;
using System.Reflection.Emit;
using System.Reflection.Metadata;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace _2D_Engine_Sokov
{
    internal class XMLParser
    {
        public GameLevel current_level;
        public GameLevel loading_level;

        public GameLevel LoadLevel(string path)
        {
            XDocument doc = XDocument.Load(path);
            XElement root = doc.Element("Level");

            return loading_level;
        }

    }
}
