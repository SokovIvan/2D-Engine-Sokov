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
        /*public GameLevel LoadLevel(string path)
        {
            XDocument doc = XDocument.Load(path);
            XElement root = doc.Element("Level");

            return new GameLevel(
                name: root.Attribute("Name").Value,
                width: int.Parse(root.Attribute("Width").Value),
                height: int.Parse(root.Attribute("Height").Value),
                objects: root.Element("Objects").Elements().Select(ParseObject).ToList(),
                ui: root.Element("UI").Elements().Select(ParseUI).ToList()
            );
        }
        private GameObject ParseObject(XElement xml)
        {
            var type = xml.Name.LocalName;
            var obj = type switch
            {
                "Player" => new Player(),
                "Sprite" => new Sprite(xml.Attribute("Type").Value),
                _ => throw new ArgumentException($"Unknown object type: {type}")
            };

            obj.Position = new Vector2(
                float.Parse(xml.Attribute("X").Value),
            float.Parse(xml.Attribute("Y").Value)
            );

            obj.Texture = Content.Load<Texture2D>(xml.Attribute("Texture").Value);
            return obj;
        }
        private UIElement ParseUI(XElement xml)
        {
            var element = new UIElement(xml.Attribute("Type").Value)
            {
                Bounds = new Rectangle(
                    int.Parse(xml.Attribute("X").Value),
                    int.Parse(xml.Attribute("Y").Value),
                    int.Parse(xml.Attribute("Width").Value),
                    int.Parse(xml.Attribute("Height").Value)
                )
            };

            if (element.Type == "Button")
                element.OnClick += () => GameEventManager.Dispatch(xml.Attribute("OnClick").Value);

            return element;
        }*/
    }
}
