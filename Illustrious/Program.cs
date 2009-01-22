//------------------------------------------------------------------------------------------------- 
// <copyright file="Program.cs" company="Oliver Hallam">
// Copyright (c) Oliver Hallam.  All rights reserved.
// </copyright>
// <summary>Defines the Program type.</summary>
//-------------------------------------------------------------------------------------------------

namespace Illustrious
{
    using System.Globalization;
    using Mono.Cecil;
    using Optimizations;
    
    /// <summary>
    /// Contains the application entry point.
    /// </summary>
    public static class Program
    {
        /// <summary>
        /// The application entry point.
        /// </summary>
        /// <param name="arguments">The command line arguments.</param>
        public static void Main(string[] arguments)
        {
            // TODO: sanitize arguments
            var assemblyPath = arguments[0];

            var assembly = AssemblyFactory.GetAssembly(assemblyPath);

            var configuration = new OptimizationConfiguration();

            var branchToReturn = new BranchToReturn();
            var inlineFunctionCall = new InlineFunctionCall(configuration);
            var removeDegenerateBranch = new RemoveDegenerateBranch();
            var retargetDoubleBranch = new RetargetDoubleBranch();
            var removeNop = new RemoveNop();

            var rewriter = new Optimizer(
                branchToReturn, 
                inlineFunctionCall, 
                removeDegenerateBranch, 
                retargetDoubleBranch, 
                removeNop);

            rewriter.Visit(assembly);

            var assemblyName = assembly.Name.Name;
            var assemblyKind = assembly.Kind;

            // TODO: accept output file name from command line
            var outputFileName = string.Format(
                CultureInfo.InvariantCulture,
                "{0}.inlined.{1}",
                assemblyName,
                assemblyKind == AssemblyKind.Dll ? "dll" : "exe");

            AssemblyFactory.SaveAssembly(assembly, outputFileName);
        }
    }
}