using System.Collections.Generic;
using UnityEngine;

namespace Cdm.Figma.UI
{
    public interface INodeConverter
    {
        bool CanConvert(Node node, NodeConvertArgs args);
        FigmaNode Convert(FigmaNode parentObject, Node node, NodeConvertArgs args);
    }

    public class NodeConvertArgs
    {
        public FigmaImporter importer { get; }
        public FigmaFile file { get; }

        public Dictionary<string, ComponentNode> componentPropertyAssignments { get; } =
            new Dictionary<string, ComponentNode>();

        public List<Material> generatedMaterials { get; } = new List<Material>();

        public Dictionary<string, Sprite> generatedSprites { get; } = new Dictionary<string, Sprite>();

        public NodeConvertArgs(FigmaImporter importer, FigmaFile file)
        {
            this.importer = importer;
            this.file = file;
        }
    }
}