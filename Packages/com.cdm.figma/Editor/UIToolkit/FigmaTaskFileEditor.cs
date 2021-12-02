using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Unity.VectorGraphics.Editor;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;

namespace Cdm.Figma.UIToolkit
{
    [CustomEditor(typeof(FigmaTaskFile))]
    public class FigmaTaskFileEditor : Cdm.Figma.FigmaTaskFileEditor
    {
        protected override string GetImplementationName() => nameof(UIToolkit);

        protected string[] GetNodeGraphicTypes()
        {
            return new[] {NodeType.Vector, NodeType.Ellipse, NodeType.Line, NodeType.Polygon, NodeType.Star};
        }

        protected override async Task OnAfterFigmaFileSaved(
            Figma.FigmaTaskFile taskFile, Figma.FigmaFile file, FigmaFileContent fileContent)
        {
            await SaveVectorGraphicsAsync((FigmaTaskFile) taskFile, (FigmaFile) file, fileContent);
        }
        
        private async Task SaveVectorGraphicsAsync(FigmaTaskFile taskFile, FigmaFile file, FigmaFileContent fileContent)
        {
            if (!file.graphics.Any())
                return;
            
            var graphics = 
                await FigmaApi.GetImageAsync(new FigmaImageRequest(taskFile.personalAccessToken, file.id)
                {
                    ids = file.graphics.Select(x => x.id).ToArray(),
                    format = "svg",
                    svgIncludeId = false,
                    svgSimplifyStroke = true
                });
            
            var directory = Path.Combine("Assets", taskFile.graphicsPath);
            Directory.CreateDirectory(directory);

            foreach (var graphic in graphics)
            {
                if (graphic.Value != null)
                {
                    var fileName = $"{graphic.Key.Replace(":", "-").Replace(";", "_")}.svg";
                    
                    var path = Path.Combine(Application.dataPath, taskFile.graphicsPath, fileName);
                    await File.WriteAllBytesAsync(path, graphic.Value);

                    var graphicPath = Path.Combine("Assets", taskFile.graphicsPath, fileName);
                    AssetDatabase.ImportAsset(graphicPath, ImportAssetOptions.ForceSynchronousImport);

                    var svgImporter = (SVGImporter) AssetImporter.GetAtPath(graphicPath);
                    svgImporter.PreserveSVGImageAspect = true;
                    svgImporter.SvgType = SVGType.UIToolkit;

                    EditorUtility.SetDirty(svgImporter);
                    svgImporter.SaveAndReimport();
                    
                    var vectorImage = AssetDatabase.LoadAssetAtPath<VectorImage>(graphicPath);
                    var graphicSource = file.graphics.First(x => x.id == graphic.Key);
                    graphicSource.graphic = vectorImage;
                }
                else
                {
                    Debug.LogWarning($"Graphic could not be rendered: {graphic.Key}");
                }
            }
            
            EditorUtility.SetDirty(file);
        }
    }
}