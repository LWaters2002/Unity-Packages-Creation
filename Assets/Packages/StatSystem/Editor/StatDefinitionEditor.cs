using System;
using System.Reflection;
using UnityEditor;
using UnityEngine;
using Object = UnityEngine.Object;

namespace StatSystem.Editor
{
    [CustomEditor(typeof(StatDefinition))]
    public class StatDefinitionEditor : UnityEditor.Editor
    {
        private StatDefinition item { get { return target as StatDefinition; } }

        public override Texture2D RenderStaticPreview(string assetPath, Object[] subAssets, int width, int height)
        {
            if (item.icon == null) return base.RenderStaticPreview(assetPath, subAssets, width, height);
            
            var t = GetType("UnityEditor.SpriteUtility");
            if (t == null) return base.RenderStaticPreview(assetPath, subAssets, width, height);
                
            MethodInfo method = t.GetMethod("RenderStaticPreview", new[] { typeof(Sprite), typeof(Color), typeof(int), typeof(int) });
            if (method == null) return base.RenderStaticPreview(assetPath, subAssets, width, height);
            object ret = method.Invoke("RenderStaticPreview", new object[] { item.icon, Color.white, width, height });
            if (ret is Texture2D texture2D)
                return texture2D;
            return base.RenderStaticPreview(assetPath, subAssets, width, height);
        }

        private static Type GetType(string typeName)
        {
            var type = Type.GetType(typeName);
            if (type != null)
                return type;

            var currentAssembly = Assembly.GetExecutingAssembly();
            var referencedAssemblies = currentAssembly.GetReferencedAssemblies();
            foreach (var assemblyName in referencedAssemblies)
            {
                var assembly = Assembly.Load(assemblyName);
                if (assembly != null)
                {
                    type = assembly.GetType(typeName);
                    if (type != null)
                        return type;
                }
            }
            return null;
        }
    }
}