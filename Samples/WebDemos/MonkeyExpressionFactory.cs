﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using VDS.RDF;
using VDS.RDF.Query.Expressions;

namespace WebDemos
{
    public class MonkeyExpressionFactory : ISparqlCustomExpressionFactory
    {
        public bool TryCreateExpression(Uri u, List<ISparqlExpression> args, out ISparqlExpression expr)
        {
            Graph g = new Graph();
            expr = new NodeExpressionTerm(g.CreateLiteralNode("Monkey"));
            return true;
        }
    }
}