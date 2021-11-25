using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Cdm.Figma.UIToolkit
{
    public static class ComponentState
    {
        public const string Default = "Default";
        public const string Hover = "Hover";
        public const string Press = "Press";
        public const string Disabled = "Disabled";
    }
    
    public abstract class ComponentConverter : NodeConverter
    {
        [SerializeField]
        private string _typeId;

        public string typeId => _typeId;

        [SerializeField]
        private List<ComponentProperty> _properties = new List<ComponentProperty>();

        public IReadOnlyList<ComponentProperty> properties => _properties;
        
        private void OnEnable()
        {
            if (string.IsNullOrWhiteSpace(_typeId))
            {
                _typeId = GetDefaultTypeId();
                
#if UNITY_EDITOR
                UnityEditor.EditorUtility.SetDirty(this);
#endif
            }
            
            if (!_properties.Any())
            {
                _properties.AddRange(GetVariants());
                
#if UNITY_EDITOR
                UnityEditor.EditorUtility.SetDirty(this);
#endif
            }
        }

        private void Reset()
        {
            _typeId = GetDefaultTypeId();
            _properties.Clear();
            _properties.AddRange(GetVariants());
            
#if UNITY_EDITOR
            UnityEditor.EditorUtility.SetDirty(this);
#endif
        }

        protected abstract string GetDefaultTypeId();
        protected abstract ISet<ComponentProperty> GetVariants();
        
        public override bool CanConvert(FigmaImporter importer, FigmaFile file, Node node)
        {
            return node?.name.Contains($"{importer.typePrefix}{typeId}") ?? false;
        }
    }
}