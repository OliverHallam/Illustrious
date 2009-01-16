//------------------------------------------------------------------------------------------------- 
// <copyright file="Optimizer.cs" company="Oliver Hallam">
// Copyright (c) Oliver Hallam.  All rights reserved.
// </copyright>
// <summary>Defines the Optimizer type.</summary>
//-------------------------------------------------------------------------------------------------

namespace Inliner
{
    using System.Collections.Generic;
    using Mono.Cecil;

    /// <summary>
    /// Performs optimization of an assembly.
    /// </summary>
    public class Optimizer : Visitor
    {
        /// <summary>
        /// The optimization to apply.
        /// </summary>
        private readonly InlineFunctionCallOptimization optimization;

        /// <summary>
        /// The set of methods that have been optimized.
        /// </summary>
        private readonly HashSet<MethodDefinition> visitedMethods = new HashSet<MethodDefinition>();

        /// <summary>
        /// Initializes a new instance of the <see cref="Optimizer"/> class.
        /// </summary>
        /// <param name="optimization">The optimization to apply.</param>
        public Optimizer(InlineFunctionCallOptimization optimization)
        {
            this.optimization = optimization;
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
                if (this.optimization.OptimizeInstruction(this, worker, ref instruction))
                {
                    continue;
                }

                instruction = instruction.Next;
            }
        }
    }
}
