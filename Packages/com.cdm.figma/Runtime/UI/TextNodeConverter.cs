using TMPro;
using UnityEngine;

namespace Cdm.Figma.UI
{
    [CreateAssetMenu(fileName = nameof(TextNodeConverter), menuName = AssetMenuRoot + "Text", order = 20)]
    public class TextNodeConverter : NodeConverter<TextNode>
    {
        public override NodeObject Convert(Node node, NodeConvertArgs args)
        {
            
            var textNode = (TextNode) node;

            var nodeObject = VectorNodeConverter.Convert(textNode, args);
            var text = nodeObject.gameObject.AddComponent<TextMeshProUGUI>();
            text.text = textNode.characters;
            text.paragraphSpacing = textNode.style.paragraphSpacing;
            // TODO: textNode.style.paragraphIndent;
            // TODO:  textNode.style.listSpacing;
            // TODO: text.fontWeight = textNode.style.fontWeight;
            
            if (textNode.style.italic)
            {
                text.fontStyle |= FontStyles.Italic;
            }
            
            return nodeObject;
        }
    }
}