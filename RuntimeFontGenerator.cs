using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework.Graphics;
using SpriteFontPlus;

namespace _2D_Engine_Sokov
{
    public static class RuntimeFontGenerator
    {
        public static SpriteFont CreateFont(GraphicsDevice graphicsDevice) {
            var fontBakeResult = TtfFontBaker.Bake(File.ReadAllBytes(@"C:\\Windows\\Fonts\arial.ttf"),
        25,
        1024,
        1024,
        new[]
        {
        CharacterRange.BasicLatin,
        CharacterRange.Latin1Supplement,
        CharacterRange.LatinExtendedA,
        CharacterRange.Cyrillic
        }
    );

            SpriteFont font = fontBakeResult.CreateSpriteFont(graphicsDevice);
            return font;
        }
    }
}