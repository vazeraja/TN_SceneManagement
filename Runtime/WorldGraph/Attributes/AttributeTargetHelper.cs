using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using Debug = UnityEngine.Debug;

namespace ThunderNut.SceneManagement {
    
    public static class AttributeTargetHelper<TAttribute> where TAttribute : Attribute
    {
        /// <summary>
        /// Map of attributes and their respective targets
        /// </summary>
        private static Dictionary<TAttribute, object> targetMap;

        /// <summary>
        /// List of assemblies that should not be rescanned for types.
        /// </summary>
        private static List<string> skipAssemblies;

        /// <summary>
        /// Adds an attribute and it's target to the dictionary
        /// </summary>
        /// <param name="attribute"></param>
        /// <param name="item"></param>
        private static void Add(TAttribute attribute, object item)
        {
            targetMap.Add(attribute, item);
        }

        /// <summary>
        /// Scans an assembly for all instances of the attribute.
        /// </summary>
        /// <param name="assembly"></param>
        private static void ScanAssembly(Assembly assembly)
        {
            const BindingFlags memberInfoBinding = BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Static | BindingFlags.Instance;

            if (!skipAssemblies.Contains(assembly.FullName))
            {
                skipAssemblies.Add(assembly.FullName);

                Debug.Log("Loading attribute targets for " + typeof(TAttribute).Name + " from assembly " + assembly.FullName);

                foreach (TAttribute attr in assembly.GetCustomAttributes(typeof(TAttribute), false))
                    Add(attr, assembly);

                foreach (Type type in assembly.GetTypes())
                {
                    foreach (TAttribute attr in type.GetCustomAttributes(typeof(TAttribute), false))
                        Add(attr, type);

                    foreach (MemberInfo member in type.GetMembers(memberInfoBinding))
                    {
                        foreach (TAttribute attr in member.GetCustomAttributes(typeof(TAttribute), false))
                            Add(attr, member);

                        if (member.MemberType == MemberTypes.Method)
                            foreach (var parameter in ((MethodInfo)member).GetParameters())
                                foreach (TAttribute attr in parameter.GetCustomAttributes(typeof(TAttribute), false))
                                    Add(attr, parameter);
                    }
                }
            }

            foreach (var assemblyName in assembly.GetReferencedAssemblies())
            {
                if (!skipAssemblies.Contains(assemblyName.FullName))
                    ScanAssembly(Assembly.Load(assemblyName));
            }
        }

        /// <summary>
        /// Returns the target of an attribute.
        /// </summary>
        /// <param name="attribute">The attribute for which a target is sought</param>
        /// <returns>The target of the attribute - either an Assembly, Type or MemberInfo instance.</returns>
        public static object GetTarget(TAttribute attribute)
        {
            object result;
            if (!targetMap.TryGetValue(attribute, out result))
            {
                // Since types can be loaded at any time, recheck that all assemblies are included...
                // Walk up the stack in a last-ditch effort to find instances of the attribute.
                StackTrace stackTrace = new StackTrace();           // get call stack
                StackFrame[] stackFrames = stackTrace.GetFrames();  // get method calls (frames)

                // write call stack method names
                foreach (StackFrame stackFrame in stackFrames)
                {
                    Console.WriteLine(stackFrame.GetMethod().Name);   // write method name
                    ScanAssembly(stackFrame.GetMethod().GetType().Assembly);
                }

                if (!targetMap.TryGetValue(attribute, out result))
                    throw new InvalidProgramException("Cannot find assembly referencing attribute");
            }
            return result;
        }

        /// <summary>
        /// Static constructor for type.
        /// </summary>
        static AttributeTargetHelper()
        {
            targetMap = new Dictionary<TAttribute, object>();

            // Do not load any assemblies reference by the assembly which declares the attribute, since they cannot possibly use the attribute
            skipAssemblies = new List<string>(typeof(TAttribute).Assembly.GetReferencedAssemblies().Select(c => c.FullName)) {
                // Skip common system assemblies
                "System.Core, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089",
                "System.Security, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a",
                "System.Xml, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089",
                "System.Data.SqlXml, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089",
                "System.Configuration, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a",
                "System.Numerics, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089"
            };
            
            // Scan the entire application
            ScanAssembly(Assembly.GetEntryAssembly());
        }

    }


    /// <summary>
    /// Extends attributes so that their targets can be discovered
    /// </summary>
    public static class AttributeTargetHelperExtension
    {
        /// <summary>
        /// Gets the target of an attribute
        /// </summary>
        /// <typeparam name="TAttribute"></typeparam>
        /// <param name="attribute">The attribute for which a target is sought</param>
        /// <returns>The target of the attribute - either an Assembly, Type or MemberInfo instance.</returns>
        public static object GetTarget<TAttribute>(this TAttribute attribute) where TAttribute : Attribute
        {
            return AttributeTargetHelper<TAttribute>.GetTarget(attribute);
        }
    }

}