/*

Copyright dotNetRDF Project 2009-12
dotnetrdf-develop@lists.sf.net

------------------------------------------------------------------------

This file is part of dotNetRDF.

dotNetRDF is free software: you can redistribute it and/or modify
it under the terms of the GNU General Public License as published by
the Free Software Foundation, either version 3 of the License, or
(at your option) any later version.

dotNetRDF is distributed in the hope that it will be useful,
but WITHOUT ANY WARRANTY; without even the implied warranty of
MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
GNU General Public License for more details.

You should have received a copy of the GNU General Public License
along with dotNetRDF.  If not, see <http://www.gnu.org/licenses/>.

------------------------------------------------------------------------

dotNetRDF may alternatively be used under the LGPL or MIT License

http://www.gnu.org/licenses/lgpl.html
http://www.opensource.org/licenses/mit-license.php

If these licenses are not suitable for your intended use please contact
us at the above stated email address to discuss alternative
terms.

*/

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using VDS.RDF.Nodes;

namespace VDS.RDF.Query.Expressions.Functions.Sparql.Boolean
{
    /// <summary>
    /// Class representing the Sparql LangMatches() function
    /// </summary>
    public class LangMatchesFunction
        : BaseBinaryExpression
    {
        /// <summary>
        /// Creates a new LangMatches() function expression
        /// </summary>
        /// <param name="term">Expression to obtain the Language of</param>
        /// <param name="langRange">Expression representing the Language Range to match</param>
        public LangMatchesFunction(ISparqlExpression term, ISparqlExpression langRange)
            : base(term, langRange) { }

        /// <summary>
        /// Computes the Effective Boolean Value of this Expression as evaluated for a given Binding
        /// </summary>
        /// <param name="context">Evaluation Context</param>
        /// <param name="bindingID">Binding ID</param>
        /// <returns></returns>
        public override IValuedNode Evaluate(SparqlEvaluationContext context, int bindingID)
        {
            INode result = this._leftExpr.Evaluate(context, bindingID);
            INode langRange = this._rightExpr.Evaluate(context, bindingID);

            if (result == null)
            {
                return new BooleanNode(false);
            }
            else if (result.NodeType == NodeType.Literal)
            {
                if (langRange == null)
                {
                    return new BooleanNode(false);
                }
                else if (langRange.NodeType == NodeType.Literal)
                {
                    string range = ((ILiteralNode)langRange).Value;
                    string lang = ((ILiteralNode)result).Value;

                    if (range.Equals("*"))
                    {
                        return new BooleanNode(!lang.Equals(string.Empty));
                    }
                    else
                    {
                        return new BooleanNode(lang.Equals(range, StringComparison.OrdinalIgnoreCase) || lang.StartsWith(range + "-", StringComparison.OrdinalIgnoreCase));
                    }
                }
                else
                {
                    return new BooleanNode(false);
                }
            }
            else
            {
                return new BooleanNode(false);
            }
        }

        /// <summary>
        /// Gets the String representation of this Expression
        /// </summary>
        /// <returns></returns>
        public override string ToString()
        {
            return "LANGMATCHES(" + this._leftExpr.ToString() + "," + this._rightExpr.ToString() + ")";
        }

        /// <summary>
        /// Gets the Type of the Expression
        /// </summary>
        public override SparqlExpressionType Type
        {
            get
            {
                return SparqlExpressionType.Function;
            }
        }

        /// <summary>
        /// Gets the Functor of the Expression
        /// </summary>
        public override string Functor
        {
            get
            {
                return SparqlSpecsHelper.SparqlKeywordLangMatches;
            }
        }

        /// <summary>
        /// Transforms the Expression using the given Transformer
        /// </summary>
        /// <param name="transformer">Expression Transformer</param>
        /// <returns></returns>
        public override ISparqlExpression Transform(IExpressionTransformer transformer)
        {
            return new LangMatchesFunction(transformer.Transform(this._leftExpr), transformer.Transform(this._rightExpr));
        }
    }
}
