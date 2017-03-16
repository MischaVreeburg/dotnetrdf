/*
dotNetRDF is free and open source software licensed under the MIT License

-----------------------------------------------------------------------------

Copyright (c) 2009-2012 dotNetRDF Project (dotnetrdf-developer@lists.sf.net)

Permission is hereby granted, free of charge, to any person obtaining a copy
of this software and associated documentation files (the "Software"), to deal
in the Software without restriction, including without limitation the rights
to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
copies of the Software, and to permit persons to whom the Software is furnished
to do so, subject to the following conditions:

The above copyright notice and this permission notice shall be included in all
copies or substantial portions of the Software.

THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR 
IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY, 
FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY,
WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN
CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.
*/

#if !NO_PROCESS

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Xunit;
using VDS.RDF.Writing;
using VDS.RDF.XunitExtensions;

namespace VDS.RDF.Writing
{

    public class GraphVizTests
    {
        [SkippableFact]
        public void WritingGraphViz1()
        {
            if (!TestConfigManager.GetSettingAsBoolean(TestConfigManager.UseGraphViz))
            {
                throw new SkipTestException("Test Config marks GraphViz as unavailable, test cannot be run");
            }

            Graph g = new Graph();
            g.LoadFromEmbeddedResource("VDS.RDF.Configuration.configuration.ttl");

            GraphVizWriter writer = new GraphVizWriter();
            writer.Save(g, "WritingGraphViz1.dot");
        }

#if !NO_PROCESS
        [SkippableFact]
        public void WritingGraphViz2()
        {
            if (!TestConfigManager.GetSettingAsBoolean(TestConfigManager.UseGraphViz))
            {
                throw new SkipTestException("Test Config marks GraphViz as unavailable, test cannot be run");
            }

            Graph g = new Graph();
            g.LoadFromEmbeddedResource("VDS.RDF.Configuration.configuration.ttl");

            GraphVizGenerator generator = new GraphVizGenerator("svg");
            generator.Generate(g, "WritingGraphViz2.svg", false);
        }

        [SkippableFact]
        public void WritingGraphViz3()
        {
            if (!TestConfigManager.GetSettingAsBoolean(TestConfigManager.UseGraphViz))
            {
                throw new SkipTestException("Test Config marks GraphViz as unavailable, test cannot be run");
            }

            Graph g = new Graph();
            g.LoadFromEmbeddedResource("VDS.RDF.Configuration.configuration.ttl");

            GraphVizGenerator generator = new GraphVizGenerator("png");
            generator.Generate(g, "WritingGraphViz3.png", false);
        }
#endif
    }
}

#endif