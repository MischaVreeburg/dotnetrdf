﻿/*

Copyright Robert Vesse 2009-10
rvesse@vdesign-studios.com

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
using VDS.RDF.Query.Patterns;

namespace VDS.RDF.Query.Algebra
{
    /// <summary>
    /// Represents a BINDINGS modifier on a SPARQL Query
    /// </summary>
    public class Bindings : ISparqlAlgebra
    {
        private BindingsPattern _bindings;
        private ISparqlAlgebra _pattern;

        /// <summary>
        /// Creates a new BINDINGS modifier
        /// </summary>
        /// <param name="bindings">Bindings</param>
        /// <param name="pattern">Pattern</param>
        public Bindings(BindingsPattern bindings, ISparqlAlgebra pattern)
        {
            this._bindings = bindings;
            this._pattern = pattern;
        }

        /// <summary>
        /// Evaluates the BINDINGS modifier
        /// </summary>
        /// <param name="context">Evaluation Context</param>
        /// <returns></returns>
        public BaseMultiset Evaluate(SparqlEvaluationContext context)
        {
            //Setup the Input Multiset appropriately
            context.InputMultiset = new Multiset(this._bindings);

            //Evalute the Pattern
            BaseMultiset results = this._pattern.Evaluate(context);

            //If the result is Null/Identity/Empty
            if (results is NullMultiset || results is IdentityMultiset || results.IsEmpty)
            {
                context.OutputMultiset = results;
                return results;
            }
            else
            {
                //Modify the Results based on the Solution Mapping
                foreach (int id in results.SetIDs.ToList())
                {
                    if (!this._bindings.Accepts(results[id]))
                    {
                        results.Remove(id);
                    }
                }
                context.OutputMultiset = results;
                return results;
            }
        }

        /// <summary>
        /// Gets the Variables used in the Algebra
        /// </summary>
        public IEnumerable<String> Variables
        {
            get
            {
                return this._pattern.Variables.Distinct();
            }
        }

        /// <summary>
        /// Gets the Bindings 
        /// </summary>
        public BindingsPattern BindingsPattern
        {
            get
            {
                return this._bindings;
            }
        }

        /// <summary>
        /// Gets the Inner Algebra
        /// </summary>
        public ISparqlAlgebra InnerAlgebra
        {
            get
            {
                return this._pattern;
            }
        }

        /// <summary>
        /// Gets the String representation of the Algebra
        /// </summary>
        /// <returns></returns>
        public override string ToString()
        {
            return "Bindings(" + this._pattern.ToString() + ")";
        }

        /// <summary>
        /// Converts the Algebra back to a SPARQL Query
        /// </summary>
        /// <returns></returns>
        public SparqlQuery ToQuery()
        {
            SparqlQuery q = this._pattern.ToQuery();
            if (this._bindings != null)
            {
                q.Bindings = this._bindings;
            }
            return q;
        }

        public GraphPattern ToGraphPattern()
        {
            throw new NotSupportedException("A Bindings() cannot be converted to a GraphPattern");
        }
    }
}
