using Cdm.Figma.UI.Styles;
using UnityEngine;

namespace Cdm.Figma.UI
{
    public class GroupNodeConverter : NodeConverter<GroupNode>
    {
        protected override FigmaNode Convert(FigmaNode parentObject, GroupNode groupNode, NodeConvertArgs args)
        {
            var figmaNode = args.importer.CreateFigmaNode<FigmaNode>(groupNode);
            figmaNode.rectTransform.anchorMin = new Vector2(0, 0);
            figmaNode.rectTransform.anchorMax = new Vector2(1, 1);
            figmaNode.rectTransform.offsetMin = new Vector2(0, 0);
            figmaNode.rectTransform.offsetMax = new Vector2(0, 0);

            Node parentNode = null;
            groupNode.TraverseUp(n =>
            {
                if (n.type != NodeType.Group)
                {
                    parentNode = n;
                    return false;
                }

                return true;
            });

            if (parentNode is INodeTransform parentTransform)
            {
                groupNode.relativeTransform = parentTransform.relativeTransform;
                groupNode.size = parentTransform.size;
            }

            BuildChildren(groupNode, figmaNode, args);
            
            if (figmaNode != null && groupNode.isMask)
            {
                args.importer.LogWarning("Group node with mask is not supported.", figmaNode);
            }
            
            return figmaNode;
        }

        private static void BuildChildren(GroupNode currentNode, FigmaNode nodeObject, NodeConvertArgs args)
        {
            var children = currentNode.GetChildren();
            if (children != null)
            {
                foreach (var child in children)
                {
                    if (args.importer.TryConvertNode(nodeObject, child, args, out var childObject))
                    {
                        if (childObject != nodeObject)
                        {
                            childObject.rectTransform.SetParent(nodeObject.rectTransform, false);
                            childObject.AdjustPosition(currentNode.size);
                            
                            // Add transform style after all changes made on rect transform.
                            childObject.styles.Add(TransformStyle.GetTransformStyle(childObject.rectTransform));
                        }
                    }
                }
            }
        }
    }
}