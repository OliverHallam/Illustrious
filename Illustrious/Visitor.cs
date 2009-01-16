//------------------------------------------------------------------------------------------------- 
// <copyright file="Visitor.cs" company="Oliver Hallam">
// Copyright (c) Oliver Hallam.  All rights reserved.
// </copyright>
//-------------------------------------------------------------------------------------------------

namespace Illustrious
{
    using Mono.Cecil;

    /// <summary>
    /// Base class for visitors which scan assemblies.
    /// </summary>
    public class Visitor
    {
        /// <summary>
        /// Visits the specified assembly.
        /// </summary>
        /// <param name="assembly">The assembly to visit.</param>
        public virtual void Visit(AssemblyDefinition assembly)
        {
            var modules = assembly.Modules;
            var moduleCount = modules.Count;
            for (var i = 0; i < moduleCount; i++)
            {
                var module = modules[i];
                this.Visit(module);
            }
        }

        /// <summary>
        /// Visits the specified module.
        /// </summary>
        /// <param name="module">The module to visit.</param>
        public virtual void Visit(ModuleDefinition module)
        {
            var types = module.Types;
            var typeCount = types.Count;
            for (var i = 0; i < typeCount; i++)
            {
                var type = types[i];
                this.Visit(type);
            }
        }

        /// <summary>
        /// Visits the specified type.
        /// </summary>
        /// <param name="type">The type to visit.</param>
        public virtual void Visit(TypeDefinition type)
        {
            var nestedTypes = type.NestedTypes;
            var typeCount = nestedTypes.Count;
            for (var i = 0; i < typeCount; i++)
            {
                var nestedType = nestedTypes[i];
                this.Visit(nestedType);
            }

            var constructors = type.Constructors;
            var constructorCount = constructors.Count;
            for (var i = 0; i < constructorCount; i++)
            {
                var constructor = constructors[i];
                this.Visit(constructor);
            }

            var methods = type.Methods;
            var methodCount = methods.Count;
            for (var i = 0; i < methodCount; i++)
            {
                var method = methods[i];
                this.Visit(method);
            }

            var properties = type.Properties;
            var propertyCount = properties.Count;
            for (var i = 0; i < propertyCount; i++)
            {
                var property = properties[i];
                this.Visit(property);
            }
        }
        
        /// <summary>
        /// Visits the specified method.
        /// </summary>
        /// <param name="method">The method to visit.</param>
        public virtual void Visit(MethodDefinition method)
        {
        }

        /// <summary>
        /// Visits the specified property.
        /// </summary>
        /// <param name="property">The property to visit.</param>
        public virtual void Visit(PropertyDefinition property)
        {
            var getMethod = property.GetMethod;
            if (getMethod != null)
            {
                this.Visit(getMethod);
            }

            var setMethod = property.SetMethod;
            if (setMethod != null)
            {
                this.Visit(setMethod);
            }
        }
    }
}