//------------------------------------------------------------------------------------------------- 
// <copyright file="Optimizer.cs" company="Oliver Hallam">
// Copyright (c) Oliver Hallam.  All rights reserved.
// </copyright>
// <summary>Defines the Optimizer type.</summary>
//-------------------------------------------------------------------------------------------------

namespace Illustrious
{
    using System.Collections.Generic;
    using Mono.Cecil;
    using Mono.Cecil.Cil;

    /// <summary>
    /// Performs optimization of an assembly.
    /// </summary>
    public class Optimizer : Visitor
    {
        /// <summary>
        /// The optimization to apply.
        /// </summary>
        private readonly Optimization[] optimizations;

        /// <summary>
        /// The set of methods that have been optimized.
        /// </summary>
        private readonly HashSet<MethodDefinition> visitedMethods = new HashSet<MethodDefinition>();

        /// <summary>
        /// Initializes a new instance of the <see cref="Optimizer"/> class.
        /// </summary>
        /// <param name="optimizations">The optimizations to apply.</param>
        public Optimizer(params Optimization[] optimizations)
        {
            this.optimizations = optimizations;
        }

        /// <summary>
        /// Visits the specified method.
        /// </summary>
        /// <param name="method">The method to visit.</param>
        public override void Visit(MethodDefinition method)
        {
            if (this.visitedMethods.Contains(method))
            {
                return;
            }

            this.visitedMethods.Add(method);

            var body = method.Body;
            if (body == null)
            {
                return;
            }

            var worker = body.CilWorker;
            var instruction = body.Instructions[0];
            while (instruction != null)
            {
                if (this.ApplyOptimizations(worker, ref instruction))
                {
                    continue;
                }

                instruction = instruction.Next;
            }
        }

        /// <summary>
        /// Applies all optimizations to the specified instruction.
        /// </summary>
        /// <param name="worker">The worker.</param>
        /// <param name="instruction">
        /// The instruction to be optimized.  Receives the first changed instruction of the optimized version.
        /// </param>
        /// <returns>
        /// <see langword="true"/> if an optimization was performed; otherwise <see langword="false"/>.
        /// </returns>
        private bool ApplyOptimizations(CilWorker worker, ref Instruction instruction)
        {
            var optimizationCount = this.optimizations.Length;
            for (var i = 0; i < optimizationCount; i++)
            {
                var optimization = this.optimizations[i];
                if (optimization.OptimizeInstruction(this, worker, ref instruction))
                {
                    return true;
                }
            }

            return false;
        }
    }
}