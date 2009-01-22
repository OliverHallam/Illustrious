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
        /// Visits the specified assembly definition.
        /// </summary>
        /// <param name="assemblyDefinition">The assembly definition to visit.</param>
        public virtual void Visit(AssemblyDefinition assemblyDefinition)
        {
            var modules = assemblyDefinition.Modules;
            var moduleCount = modules.Count;
            for (var i = 0; i < moduleCount; i++)
            {
                var module = modules[i];
                this.Visit(module);
            }
        }

        /// <summary>
        /// Visits the specified module definition.
        /// </summary>
        /// <param name="moduleDefinition">The module definition to visit.</param>
        public virtual void Visit(ModuleDefinition moduleDefinition)
        {
            var types = moduleDefinition.Types;
            var typeCount = types.Count;
            for (var i = 0; i < typeCount; i++)
            {
                var type = types[i];
                this.Visit(type);
            }
        }

        /// <summary>
        /// Visits the specified type definition.
        /// </summary>
        /// <param name="typeDefinition">The type definition to visit.</param>
        public virtual void Visit(TypeDefinition typeDefinition)
        {
            var nestedTypes = typeDefinition.NestedTypes;
            var typeCount = nestedTypes.Count;
            for (var i = 0; i < typeCount; i++)
            {
                var nestedType = nestedTypes[i];
                this.Visit(nestedType);
            }

            var constructors = typeDefinition.Constructors;
            var constructorCount = constructors.Count;
            for (var i = 0; i < constructorCount; i++)
            {
                var constructor = constructors[i];
                this.Visit(constructor);
            }

            var methods = typeDefinition.Methods;
            var methodCount = methods.Count;
            for (var i = 0; i < methodCount; i++)
            {
                var method = methods[i];
                this.Visit(method);
            }

            var properties = typeDefinition.Properties;
            var propertyCount = properties.Count;
            for (var i = 0; i < propertyCount; i++)
            {
                var property = properties[i];
                this.Visit(property);
            }
        }
        
        /// <summary>
        /// Visits the specified method definition.
        /// </summary>
        /// <param name="methodDefinition">The method definition to visit.</param>
        public virtual void Visit(MethodDefinition methodDefinition)
        {
        }

        /// <summary>
        /// Visits the specified property definition.
        /// </summary>
        /// <param name="propertyDefinition">The property definition to visit.</param>
        public virtual void Visit(PropertyDefinition propertyDefinition)
        {
            var getMethod = propertyDefinition.GetMethod;
            if (getMethod != null)
            {
                this.Visit(getMethod);
            }

            var setMethod = propertyDefinition.SetMethod;
            if (setMethod != null)
            {
                this.Visit(setMethod);
            }
        }
    }
}