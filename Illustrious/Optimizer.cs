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

            var worker = new OptimizationWorker(this, body);
            while (worker.MoveNext())
            {
                this.ApplyOptimizations(worker);
            }
        }

        /// <summary>
        /// Applies all optimizations to the specified instruction.
        /// </summary>
        /// <param name="worker">The worker to apply optimizations on.</param>
        private void ApplyOptimizations(OptimizationWorker worker)
        {
            var optimizationCount = this.optimizations.Length;
            for (var i = 0; i < optimizationCount; i++)
            {
                var optimization = this.optimizations[i];
                optimization.OptimizeInstruction(worker);
                if (worker.WasOptimized)
                {
                    return;
                }
            }
        }
    }
}