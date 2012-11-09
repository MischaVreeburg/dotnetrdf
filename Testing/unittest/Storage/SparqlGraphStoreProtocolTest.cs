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

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using VDS.RDF.Parsing;
using VDS.RDF.Storage;
using VDS.RDF.Writing;
using VDS.RDF.Writing.Formatting;

namespace VDS.RDF.Test.Storage
{
    [TestClass]
    public class SparqlGraphStoreProtocolTest
    {
        private NTriplesFormatter _formatter = new NTriplesFormatter();

        public static SparqlHttpProtocolConnector GetConnection()
        {
            if (!TestConfigManager.GetSettingAsBoolean(TestConfigManager.UseIIS))
            {
                Assert.Inconclusive("Test Config marks IIS as unavailable, cannot run test");
            }
            return new SparqlHttpProtocolConnector(TestConfigManager.GetSetting(TestConfigManager.LocalGraphStoreUri));
        }

        [TestMethod]
        public void StorageSparqlUniformHttpProtocolSaveGraph()
        {
            try
            {
                Options.UriLoaderCaching = false;

                Graph g = new Graph();
                FileLoader.Load(g, "Turtle.ttl");
                g.BaseUri = new Uri("http://example.org/sparqlTest");

                //Save Graph to SPARQL Uniform Protocol
                SparqlHttpProtocolConnector sparql = SparqlGraphStoreProtocolTest.GetConnection();
                sparql.SaveGraph(g);
                Console.WriteLine("Graph saved to SPARQL Uniform Protocol OK");

                //Now retrieve Graph from SPARQL Uniform Protocol
                Graph h = new Graph();
                sparql.LoadGraph(h, "http://example.org/sparqlTest");

                Console.WriteLine();
                foreach (Triple t in h.Triples)
                {
                    Console.WriteLine(t.ToString(this._formatter));
                }

                GraphDiffReport diff = g.Difference(h);
                if (!diff.AreEqual)
                {
                    Console.WriteLine();
                    Console.WriteLine("Graphs are different - should be 1 difference due to New Line Normalization");
                    Console.WriteLine("Added Triples");
                    foreach (Triple t in diff.AddedTriples)
                    {
                        Console.WriteLine(t.ToString(this._formatter));
                    }
                    Console.WriteLine("Removed Triples");
                    foreach (Triple t in diff.RemovedTriples)
                    {
                        Console.WriteLine(t.ToString(this._formatter));
                    }

                    Assert.IsTrue(diff.AddedTriples.Count() == 1, "Should only be 1 Triple difference due to New Line normalization");
                    Assert.IsTrue(diff.RemovedTriples.Count() == 1, "Should only be 1 Triple difference due to New Line normalization");
                    Assert.IsFalse(diff.AddedMSGs.Any(), "Should not be any MSG differences");
                    Assert.IsFalse(diff.RemovedMSGs.Any(), "Should not be any MSG differences");
                }
            }
            finally
            {
                Options.UriLoaderCaching = true;
            }
        }

        [TestMethod]
        public void StorageSparqlUniformHttpProtocolLoadGraph()
        {
            try
            {
                Options.UriLoaderCaching = false;
                //Ensure that the Graph will be there using the SaveGraph() test
                StorageSparqlUniformHttpProtocolSaveGraph();

                Graph g = new Graph();
                FileLoader.Load(g, "Turtle.ttl");
                g.BaseUri = new Uri("http://example.org/sparqlTest");

                //Try to load the relevant Graph back from the Store
                SparqlHttpProtocolConnector sparql = SparqlGraphStoreProtocolTest.GetConnection();

                Graph h = new Graph();
                sparql.LoadGraph(h, "http://example.org/sparqlTest");

                Console.WriteLine();
                foreach (Triple t in h.Triples)
                {
                    Console.WriteLine(t.ToString(this._formatter));
                }

                GraphDiffReport diff = g.Difference(h);
                if (!diff.AreEqual)
                {
                    Console.WriteLine();
                    Console.WriteLine("Graphs are different - should be 1 difference due to New Line Normalization");
                    Console.WriteLine("Added Triples");
                    foreach (Triple t in diff.AddedTriples)
                    {
                        Console.WriteLine(t.ToString(this._formatter));
                    }
                    Console.WriteLine("Removed Triples");
                    foreach (Triple t in diff.RemovedTriples)
                    {
                        Console.WriteLine(t.ToString(this._formatter));
                    }

                    Assert.IsTrue(diff.AddedTriples.Count() == 1, "Should only be 1 Triple difference due to New Line normalization (added)");
                    Assert.IsTrue(diff.RemovedTriples.Count() == 1, "Should only be 1 Triple difference due to New Line normalization (removed)");
                    Assert.IsFalse(diff.AddedMSGs.Any(), "Should not be any MSG differences");
                    Assert.IsFalse(diff.RemovedMSGs.Any(), "Should not be any MSG differences");
                }
            }
            finally
            {
                Options.UriLoaderCaching = true;
            }
        }

        [TestMethod]
        public void StorageSparqlUniformHttpProtocolGraphExists()
        {
            try
            {
                Options.UriLoaderCaching = false;
                //Ensure that the Graph will be there using the SaveGraph() test
                StorageSparqlUniformHttpProtocolSaveGraph();

                //Check the Graph exists in the Store
                SparqlHttpProtocolConnector sparql = SparqlGraphStoreProtocolTest.GetConnection();
                Assert.IsTrue(sparql.HasGraph("http://example.org/sparqlTest"));
            }
            finally
            {
                Options.UriLoaderCaching = true;
            }
        }

        [TestMethod]
        public void StorageSparqlUniformHttpProtocolDeleteGraph()
        {
            try
            {
                Options.UriLoaderCaching = false;
                StorageSparqlUniformHttpProtocolSaveGraph();

                SparqlHttpProtocolConnector sparql = SparqlGraphStoreProtocolTest.GetConnection();
                sparql.DeleteGraph("http://example.org/sparqlTest");

                //Give SPARQL Uniform Protocol time to delete stuff
                Thread.Sleep(1000);

                try
                {
                    Graph g = new Graph();
                    sparql.LoadGraph(g, "http://example.org/sparqlTest");

                    //If we do get here without erroring then the Graph should be empty
                    Assert.IsTrue(g.IsEmpty, "If the Graph loaded without error then it should have been empty as we deleted it from the store");
                }
                catch (Exception ex)
                {
                    Console.WriteLine("Errored as expected since the Graph was deleted");
                    TestTools.ReportError("Error", ex);
                }
            }
            finally
            {
                Options.UriLoaderCaching = true;
            }
        }

        [TestMethod]
        public void StorageSparqlUniformHttpProtocolAddTriples()
        {
            try
            {
                Options.UriLoaderCaching = false;

                StorageSparqlUniformHttpProtocolSaveGraph();

                Graph g = new Graph();
                g.Retract(g.Triples.Where(t => !t.IsGroundTriple));
                FileLoader.Load(g, "InferenceTest.ttl");

                SparqlHttpProtocolConnector sparql = SparqlGraphStoreProtocolTest.GetConnection();
                sparql.UpdateGraph("http://example.org/sparqlTest", g.Triples, null);

                Graph h = new Graph();
                sparql.LoadGraph(h, "http://example.org/sparqlTest");

                foreach (Triple t in h.Triples)
                {
                    Console.WriteLine(t.ToString(this._formatter));
                }

                Assert.IsTrue(g.IsSubGraphOf(h), "Retrieved Graph should have the added Triples as a Sub Graph");
            }
            finally
            {
                Options.UriLoaderCaching = true;
            }
        }

        [TestMethod]
        public void StorageSparqlUniformHttpProtocolRemoveTriples()
        {
            try
            {
                Options.UriLoaderCaching = false;
                Graph g = new Graph();
                FileLoader.Load(g, "InferenceTest.ttl");

                try
                {
                    SparqlHttpProtocolConnector sparql = SparqlGraphStoreProtocolTest.GetConnection();
                    sparql.UpdateGraph("http://example.org/sparqlTest", null, g.Triples);

                    Assert.Fail("SPARQL Uniform HTTP Protocol does not support removing Triples");
                }
                catch (RdfStorageException storeEx)
                {
                    Console.WriteLine("Got an error as expected");
                    TestTools.ReportError("Storage Error", storeEx);
                }
                catch (NotSupportedException ex)
                {
                    Console.WriteLine("Got a Not Supported error as expected");
                    TestTools.ReportError("Not Supported", ex);
                }
            }
            finally
            {
                Options.UriLoaderCaching = true;
            }
        }

        [TestMethod]
        public void StorageSparqlUniformHttpProtocolPostCreate()
        {
            SparqlHttpProtocolConnector connector = SparqlGraphStoreProtocolTest.GetConnection();

            HttpWebRequest request = (HttpWebRequest)WebRequest.Create(TestConfigManager.GetSetting(TestConfigManager.LocalGraphStoreUri));
            request.Method = "POST";
            request.ContentType = "application/rdf+xml";

            Graph g = new Graph();
            FileLoader.Load(g, "InferenceTest.ttl");

            using (StreamWriter writer = new StreamWriter(request.GetRequestStream()))
            {
                RdfXmlWriter rdfxmlwriter = new RdfXmlWriter();
                rdfxmlwriter.Save(g, writer);
                writer.Close();
            }

            using (HttpWebResponse response = (HttpWebResponse)request.GetResponse())
            {
                //Should get a 201 Created response
                if (response.StatusCode == HttpStatusCode.Created)
                {
                    if (response.Headers["Location"] == null) Assert.Fail("A Location: Header containing the URI of the newly created Graph should have been returned");
                    Uri graphUri = new Uri(response.Headers["Location"]);

                    Console.WriteLine("New Graph URI is " + graphUri.ToString());

                    Console.WriteLine("Now attempting to retrieve this Graph from the Store");
                    Graph h = new Graph();
                    connector.LoadGraph(h, graphUri);

                    TestTools.ShowGraph(h);

                    Assert.AreEqual(g, h, "Graphs should have been equal");
                }
                else
                {
                    Assert.Fail("A 201 Created response should have been received but got a " + (int)response.StatusCode + " response");
                }
                response.Close();
            }
        }

        [TestMethod]
        public void StorageSparqlUniformHttpProtocolPostCreateMultiple()
        {
            SparqlHttpProtocolConnector connector = SparqlGraphStoreProtocolTest.GetConnection();

            Graph g = new Graph();
            FileLoader.Load(g, "InferenceTest.ttl");

            List<Uri> uris = new List<Uri>();
            for (int i = 0; i < 10; i++)
            {
                HttpWebRequest request = (HttpWebRequest)WebRequest.Create(TestConfigManager.GetSetting(TestConfigManager.LocalGraphStoreUri));
                request.Method = "POST";
                request.ContentType = "application/rdf+xml";

                using (StreamWriter writer = new StreamWriter(request.GetRequestStream()))
                {
                    RdfXmlWriter rdfxmlwriter = new RdfXmlWriter();
                    rdfxmlwriter.Save(g, writer);
                    writer.Close();
                }

                using (HttpWebResponse response = (HttpWebResponse)request.GetResponse())
                {
                    //Should get a 201 Created response
                    if (response.StatusCode == HttpStatusCode.Created)
                    {
                        if (response.Headers["Location"] == null) Assert.Fail("A Location: Header containing the URI of the newly created Graph should have been returned");
                        Uri graphUri = new Uri(response.Headers["Location"]);
                        uris.Add(graphUri);

                        Console.WriteLine("New Graph URI is " + graphUri.ToString());

                        Console.WriteLine("Now attempting to retrieve this Graph from the Store");
                        Graph h = new Graph();
                        connector.LoadGraph(h, graphUri);

                        Assert.AreEqual(g, h, "Graphs should have been equal");
                        Console.WriteLine("Graphs were equal as expected");
                    }
                    else
                    {
                        Assert.Fail("A 201 Created response should have been received but got a " + (int)response.StatusCode + " response");
                    }
                    response.Close();
                }
                Console.WriteLine();
            }

            Assert.IsTrue(uris.Distinct().Count() == 10, "Should have generated 10 distinct URIs");
        }
    }
}
