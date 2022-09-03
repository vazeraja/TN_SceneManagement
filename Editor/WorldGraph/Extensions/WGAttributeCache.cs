using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEditor.Experimental.GraphView;
using UnityEngine.Profiling;

namespace ThunderNut.SceneManagement.Editor {
    

    [InitializeOnLoad]
    public static class WGAttributeCache {
        static WGAttributeCache() {
            ReCacheKnownNodeTypes();
        }

        public static Dictionary<Type, List<ContextFilterableAttribute>> m_KnownNodeTypeLookupTable { get; set; }
        public static IEnumerable<Type> knownNodeTypes => m_KnownNodeTypeLookupTable.Keys;
        public static List<string> nodePathsList => GetNodePathsList();

        private static void ReCacheKnownNodeTypes() {
            Profiler.BeginSample("NodeTypeCache: Re-caching all known node types");
            m_KnownNodeTypeLookupTable = new Dictionary<Type, List<ContextFilterableAttribute>>();
            foreach (Type nodeType in TypeCache.GetTypesDerivedFrom<SceneHandle>()) {
                if (nodeType.IsAbstract) continue;
                List<ContextFilterableAttribute> filterableAttributes = new List<ContextFilterableAttribute>();
                foreach (Attribute attribute in Attribute.GetCustomAttributes(nodeType)) {
                    Type attributeType = attribute.GetType();
                    if (!attributeType.IsAbstract && attribute is ContextFilterableAttribute contextFilterableAttribute) {
                        filterableAttributes.Add(contextFilterableAttribute);
                    }
                }

                m_KnownNodeTypeLookupTable.Add(nodeType, filterableAttributes);
            }
        }

        public static List<string> GetSortedNodePathsList() {
            var sortedListItems = nodePathsList;
            sortedListItems.Sort((entry1, entry2) => {
                string[] splits1 = entry1.Split('/');
                string[] splits2 = entry2.Split('/');
                for (var i = 0; i < splits1.Length; i++) {
                    if (i >= splits2.Length)
                        return 1;
                    int value = string.Compare(splits1[i], splits2[i], StringComparison.Ordinal);
                    if (value == 0) continue;
                    // Make sure that leaves go before nodes
                    if (splits1.Length == splits2.Length || (i != splits1.Length - 1 && i != splits2.Length - 1)) return value;
                    int alphaOrder = splits1.Length < splits2.Length ? -1 : 1;
                    return alphaOrder;
                }

                return 0;
            });
            return sortedListItems;
        }

        private static List<string> GetNodePathsList() {
            List<string> list = new List<string>();
            foreach (var type in knownNodeTypes) {
                if (type.IsClass && !type.IsAbstract) {
                    var pathAttribute = GetAttributeOnNodeType<PathAttribute>(type);
                    if (pathAttribute != null) list.Add(pathAttribute.path);
                }
            }

            return list;
        }

        public static T GetAttributeOnNodeType<T>(Type nodeType) where T : ContextFilterableAttribute {
            var filterableAttributes = GetFilterableAttributesOnNodeType(nodeType);
            foreach (var attr in filterableAttributes) {
                if (attr is T searchTypeAttr) {
                    return searchTypeAttr;
                }
            }

            return null;
        }

        private static IEnumerable<ContextFilterableAttribute> GetFilterableAttributesOnNodeType(Type nodeType) {
            if (nodeType == null) {
                throw new ArgumentNullException($"Cannot get attributes on a null type");
            }

            if (m_KnownNodeTypeLookupTable.TryGetValue(nodeType, out List<ContextFilterableAttribute> filterableAttributes)) {
                return filterableAttributes;
            }
            else {
                throw new ArgumentException(
                    $"The passed in Type {nodeType.FullName} was not found in the loaded assemblies as a child class of AbstractMaterialNode");
            }
        }
    }

}