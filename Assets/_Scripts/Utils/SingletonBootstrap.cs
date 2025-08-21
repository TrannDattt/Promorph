using System;
using System.Linq;
using System.Reflection;
using UnityEngine;

namespace Promorph.Utils
{
    public static class SingletonBootstrap
    {
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
        private static void InitializeAllSingletons()
        {
            // var assemblies = AppDomain.CurrentDomain.GetAssemblies();
            var userAssemblies = AppDomain.CurrentDomain.GetAssemblies()
            .Where(a => a.GetName().Name == "Assembly-CSharp");

            foreach (var assembly in userAssemblies)
            {
                var singletonTypes = assembly.GetTypes()
                    .Where(t => t.IsClass && !t.IsAbstract)
                    .Where(t => IsSubclassOfRawGeneric(typeof(PersistentSingleton<>), t));

                foreach (var type in singletonTypes)
                {
                    var instanceProperty = type.GetProperty("Instance", BindingFlags.Public | BindingFlags.Static | BindingFlags.FlattenHierarchy);
                    if (instanceProperty != null)
                    {
                        var currentInstance = instanceProperty.GetValue(null);
                        if (currentInstance == null)
                        {
                            var _ = instanceProperty.GetValue(null);
                        }
                    }
                }
            }
        }

        private static bool IsSubclassOfRawGeneric(Type generic, Type toCheck)
        {
            while (toCheck != null && toCheck != typeof(object))
            {
                var cur = toCheck.IsGenericType ? toCheck.GetGenericTypeDefinition() : toCheck;
                if (generic == cur)
                {
                    return true;
                }
                toCheck = toCheck.BaseType;
            }
            return false;
        }
    }
}