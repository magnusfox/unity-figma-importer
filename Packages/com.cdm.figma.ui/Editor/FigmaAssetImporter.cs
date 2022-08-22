using System;
using System.Linq;
using System.Reflection;
using Cdm.Figma.Editor;
using Cdm.Figma.Utils;
using TMPro;
using UnityEditor;
using UnityEditor.AssetImporters;
using UnityEngine;

namespace Cdm.Figma.UI.Editor
{
    [ScriptedImporter(1, Extension)]
    public class FigmaAssetImporter : FigmaAssetImporterBase
    {
        [SerializeField]
        private FontSource[] _fonts;

        /// <summary>
        /// Gets the font assets.
        /// </summary>
        public FontSource[] fonts => _fonts;

        [SerializeField]
        private TMP_FontAsset _fallbackFont;

        /// <summary>
        /// Gets or sets the fallback font that is used when a font mapping does not found.
        /// </summary>
        public TMP_FontAsset fallbackFont
        {
            get => _fallbackFont;
            set => _fallbackFont = value;
        }

        [SerializeField]
        private float _pixelsPerUnit = 100f;

        public float pixelsPerUnit
        {
            get => _pixelsPerUnit;
            set => _pixelsPerUnit = value;
        }

        [SerializeField]
        private ushort _gradientResolution = 128;

        public ushort gradientResolution
        {
            get => _gradientResolution;
            set => _gradientResolution = value;
        }

        [SerializeField]
        private int _textureSize = 1024;

        public int textureSize
        {
            get => _textureSize;
            set => _textureSize = value;
        }

        [SerializeField]
        private TextureWrapMode _wrapMode = TextureWrapMode.Repeat;

        public TextureWrapMode wrapMode
        {
            get => _wrapMode;
            set => _wrapMode = value;
        }

        [SerializeField]
        private FilterMode _filterMode = FilterMode.Bilinear;

        public FilterMode filterMode
        {
            get => _filterMode;
            set => _filterMode = value;
        }

        [SerializeField]
        private int _sampleCount = 4;

        public int sampleCount
        {
            get => _sampleCount;
            set => _sampleCount = value;
        }

        protected override void OnAssetImporting(AssetImportContext ctx, IFigmaImporter figmaImporter,
            FigmaFile figmaFile)
        {
            base.OnAssetImporting(ctx, figmaImporter, figmaFile);

            UpdateFonts((FigmaImporter)figmaImporter, figmaFile);
        }

        protected override void OnAssetImported(AssetImportContext ctx, IFigmaImporter figmaImporter,
            FigmaFile figmaFile, Figma.FigmaDesign figmaDesign)
        {
            base.OnAssetImported(ctx, figmaImporter, figmaFile, figmaDesign);

            // Add imported page game objects to the asset.
            var design = (FigmaDesign)figmaDesign;

            // Add figma nodes.
            design.document.TraverseDfs(node =>
            {
                ctx.AddObjectToAsset($"{node.nodeID}", node.gameObject);
                return true;
            });

            var importer = (FigmaImporter)figmaImporter;

            // Add generated objects to the asset.
            foreach (var generatedAsset in importer.generatedAssets)
            {
                ctx.AddObjectToAsset(generatedAsset.Key, generatedAsset.Value);
            }

            // Register dependency assets.
            foreach (var dependencyAsset in importer.dependencyAssets)
            {
                ctx.DependsOnSourceAsset(AssetDatabase.GetAssetPath(dependencyAsset.Value));
            }
        }

        protected override IFigmaImporter GetFigmaImporter()
        {
            var figmaImporter = new FigmaImporter()
            {
                failOnError = false,
                spriteOptions = new SpriteGenerateOptions()
                {
                    pixelsPerUnit = pixelsPerUnit,
                    gradientResolution = gradientResolution,
                    textureSize = textureSize,
                    wrapMode = wrapMode,
                    filterMode = filterMode,
                    sampleCount = sampleCount
                }
            };

            figmaImporter.AddDefaultNodeConverters();
            figmaImporter.AddDefaultComponentConverters();

            SearchAndAddComponentConverters(figmaImporter);
            SearchAndAddNodeConverters(figmaImporter);

            SearchAndAddCompoundComponents(figmaImporter);

            return figmaImporter;
        }

        private void SearchAndAddCompoundComponents(FigmaImporter figmaImporter)
        {
            var compoundComponents = AppDomain.CurrentDomain.GetAssemblies()
                .SelectMany(a => a.GetTypes().Where(t => t.IsDefined(typeof(FigmaComponentAttribute))));

            foreach (var type in compoundComponents)
            {
                if (typeof(FigmaBehaviour).IsAssignableFrom(type))
                {
                    var figmaComponentAttribute =
                        (FigmaComponentAttribute)Attribute.GetCustomAttribute(type, typeof(FigmaComponentAttribute));

                    var typeId = figmaComponentAttribute.typeId;
                    figmaImporter.componentConverters.Add(new CompoundComponentConverter(typeId, type));
                }
                else
                {
                    Debug.LogError($"Cannot add compound component '{type.FullName}' to {nameof(FigmaImporter)}. " +
                                   $"Compound component must inherit from {typeof(FigmaBehaviour).FullName}");
                }
            }
        }

        private void SearchAndAddComponentConverters(FigmaImporter figmaImporter)
        {
            var componentConverters = AppDomain.CurrentDomain.GetAssemblies()
                .SelectMany(a => a.GetTypes().Where(t => t.IsDefined(typeof(FigmaComponentConverterAttribute))));

            foreach (var type in componentConverters)
            {
                if (typeof(ComponentConverter).IsAssignableFrom(type))
                {
                    figmaImporter.componentConverters.Add((ComponentConverter)Activator.CreateInstance(type));
                }
                else
                {
                    Debug.LogError($"Cannot add component converter '{type.FullName}' to {nameof(FigmaImporter)}. " +
                                   $"Component converter must inherit from {typeof(ComponentConverter).FullName}");
                }
            }
        }

        private void SearchAndAddNodeConverters(FigmaImporter figmaImporter)
        {
            var nodeConverters = AppDomain.CurrentDomain.GetAssemblies()
                .SelectMany(a => a.GetTypes().Where(t => t.IsDefined(typeof(FigmaNodeConverterAttribute))));

            foreach (var type in nodeConverters)
            {
                if (typeof(NodeConverter).IsAssignableFrom(type))
                {
                    figmaImporter.nodeConverters.Add((NodeConverter)Activator.CreateInstance(type));
                }
                else
                {
                    Debug.LogError($"Cannot add node converter '{type.FullName}' to {nameof(FigmaImporter)}. " +
                                   $"Node converter must inherit from {typeof(NodeConverter).FullName}");
                }
            }
        }

        private void UpdateFonts(FigmaImporter figmaImporter, FigmaFile file)
        {
            if (importSettingsMissing)
            {
                _fonts = null;
            }

            var usedFonts = file.GetUsedFonts();
            var oldFonts = _fonts;
            _fonts = new FontSource[usedFonts.Length];

            for (var i = 0; i < _fonts.Length; i++)
            {
                _fonts[i] = new FontSource(usedFonts[i], null);

                // Restore previously assigned fonts.
                if (oldFonts != null)
                {
                    var oldFontIndex = Array.FindIndex(oldFonts, x => x.fontName == _fonts[i].fontName);
                    if (oldFontIndex >= 0)
                    {
                        _fonts[i].font = oldFonts[oldFontIndex].font;
                    }
                }
            }

            figmaImporter.fonts.AddRange(_fonts);
            figmaImporter.fallbackFont = fallbackFont;
        }
    }
}