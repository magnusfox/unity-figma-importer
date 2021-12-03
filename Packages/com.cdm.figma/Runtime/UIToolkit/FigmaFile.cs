using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.TextCore.Text;
using UnityEngine.UIElements;

namespace Cdm.Figma.UIToolkit
{
    public class FigmaFile : Figma.FigmaFile
    {
        [SerializeField]
        private List<FontSource> _fonts = new List<FontSource>();
        
        /// <summary>
        /// Gets the font assets.
        /// </summary>
        public IList<FontSource> fonts => _fonts;

        [SerializeField]
        private FontAsset _fallbackFont;
        
        /// <summary>
        /// Gets or sets the fallback font that is used when a font mapping does not found.
        /// </summary>
        public FontAsset fallbackFont
        {
            get => _fallbackFont;
            set => _fallbackFont = value;
        }
        
        [SerializeField]
        private List<GraphicSource> _graphics = new List<GraphicSource>();
        
        /// <summary>
        /// Gets the graphic assets. 
        /// </summary>
        public IList<GraphicSource> graphics => _graphics;
        
        public bool TryGetGraphic(string graphicId, out VectorImage graphic)
        {
            var graphicSource = _graphics.FirstOrDefault(x => x.id == graphicId);
            if (graphicSource != null)
            {
                graphic = graphicSource.graphic;
                return true;
            }

            graphic = null;
            return false;
        }
        
        public bool TryGetFont(string fontName, out FontAsset font)
        {
            var fontSource = _fonts.FirstOrDefault(
                x => string.Equals(x.fontName, fontName, StringComparison.OrdinalIgnoreCase));
            if (fontSource != null && fontSource.font != null)
            {
                font = fontSource.font;
                return true;
            }

            if (fallbackFont != null)
            {
                font = fallbackFont;
                return true;
            }

            font = null;
            return false;
        }
    }
    
    [Serializable]
    public class GraphicSource
    {
        public string id;
        public VectorImage graphic;
    }
    
    [Serializable]
    public class FontSource
    {
        public string fontName;
        public FontAsset font;

        public static string GetFontName(string family, int weight, bool italic)
            => $"{family}-{(TextFontWeight) weight}{(italic ? "-Italic" : "")}";
    }
}