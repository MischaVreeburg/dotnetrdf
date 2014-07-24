﻿using System;
using System.Collections.Generic;
using System.Linq;
using VDS.RDF.Query.Algebra;
using VDS.RDF.Query.Compiler;
using VDS.RDF.Query.Engine;
using VDS.RDF.Query.Engine.Algebra;
using VDS.RDF.Query.Results;

namespace VDS.RDF.Query.Processors
{
    public class AlgebraQueryProcessor
        : IQueryProcessor
    {
        public AlgebraQueryProcessor(IQueryCompiler compiler, IAlgebraExecutor executor)
        {
            if (compiler == null) throw new ArgumentNullException("compiler");
            if (executor == null) throw new ArgumentNullException("executor");
            this.Compiler = compiler;
            this.Executor = executor;
        }

        public IQueryCompiler Compiler { get; private set; }

        public IAlgebraExecutor Executor { get; private set; }

        public IQueryResult Execute(IQuery query)
        {
            IAlgebra algebra = this.Compile(query);
            IExecutionContext context = new QueryExecutionContext(query);
            IEnumerable<ISolution> solutions = this.Execute(algebra, context);

            switch (query.QueryType)
            {
                case QueryType.Ask:
                    return new QueryResult(solutions.Any());
                case QueryType.Construct:
                    // TODO Implement CONSTRUCT template processing
                    throw new NotImplementedException("CONSTRUCT queries are not yet supported");
                case QueryType.Describe:
                case QueryType.DescribeAll:
                    // TODO Implement DESCRIBE processing
                    throw new NotImplementedException("DESCRIBE queries are not yet supported");
                case QueryType.Select:
                case QueryType.SelectAll:
                case QueryType.SelectAllDistinct:
                case QueryType.SelectAllReduced:
                case QueryType.SelectDistinct:
                case QueryType.SelectReduced:
                    return new QueryResult(new StreamingTabularResults(new AlgebraExecutionResultStream(algebra, solutions)));
                default:
                    throw new RdfQueryException("Unexpected query type encountered");
            }
        }

        public void Execute(IQuery query, QueryCallback callback, object state)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Compiles the query into algebra
        /// </summary>
        /// <param name="query">Query</param>
        /// <returns></returns>
        protected virtual IAlgebra Compile(IQuery query)
        {
            return this.Compiler.Compile(query);
        }

        /// <summary>
        /// Executes the algebra
        /// </summary>
        /// <param name="algebra">Algebra</param>
        /// <param name="context">Execution Context</param>
        /// <returns></returns>
        protected virtual IEnumerable<ISolution> Execute(IAlgebra algebra, IExecutionContext context)
        {
            return this.Executor.Execute(algebra, context);
        }
    }
}
