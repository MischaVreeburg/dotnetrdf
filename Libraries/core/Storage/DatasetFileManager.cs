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

#if !NO_STORAGE

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.IO;
using VDS.RDF.Configuration;
using VDS.RDF.Parsing;
using VDS.RDF.Storage.Params;
using VDS.RDF.Writing;

namespace VDS.RDF.Storage
{
    /// <summary>
    /// Allows you to treat an RDF Dataset File - NQuads, TriG or TriX - as a read-only generic store
    /// </summary>
    public class DatasetFileManager : IQueryableGenericIOManager, IConfigurationSerializable
    {
        private TripleStore _store = new TripleStore();
        private bool _ready = false;
        private String _filename;

        /// <summary>
        /// Creates a new Dataset File Manager
        /// </summary>
        /// <param name="filename">File to load from</param>
        /// <param name="async">Whether to load asynchronously</param>
        public DatasetFileManager(String filename, bool async)
        {
            if (!File.Exists(filename)) throw new RdfStorageException("Cannot connect to a Dataset File that doesn't exist");
            this._filename = filename;

            if (async)
            {
                Thread asyncLoader = new Thread(new ThreadStart(delegate { this.Initialise(filename); }));
                asyncLoader.IsBackground = true;
                asyncLoader.Start();
            }
            else
            {
                this.Initialise(filename);
            }
        }

        /// <summary>
        /// Internal helper method for loading the data
        /// </summary>
        /// <param name="filename">File to load from</param>
        private void Initialise(String filename)
        {
            try
            {
                IStoreReader reader = MimeTypesHelper.GetStoreParser(MimeTypesHelper.GetMimeType(Path.GetExtension(filename)));
                reader.Load(this._store, new StreamParams(filename));

                this._ready = true;
            }
            catch (RdfException rdfEx)
            {
                throw new RdfStorageException("An Error occurred while trying to read the Dataset File", rdfEx);
            }
        }

        /// <summary>
        /// Makes a query against the in-memory copy of the Stores data
        /// </summary>
        /// <param name="sparqlQuery">SPARQL Query</param>
        /// <returns></returns>
        public object Query(string sparqlQuery)
        {
            return this._store.ExecuteQuery(sparqlQuery);
        }

        /// <summary>
        /// Loads a Graph from the Dataset
        /// </summary>
        /// <param name="g">Graph to load into</param>
        /// <param name="graphUri">URI of the Graph to load</param>
        public void LoadGraph(IGraph g, Uri graphUri)
        {
            if (graphUri == null)
            {
                foreach (Uri u in WriterHelper.StoreDefaultGraphURIs.Select(s => new Uri(s)))
                {
                    if (this._store.HasGraph(u))
                    {
                        g.Merge(this._store.Graph(u));
                        return;
                    }
                }
            }
            else
            {
                if (g.IsEmpty) g.BaseUri = graphUri;
                if (this._store.HasGraph(graphUri))
                {
                    g.Merge(this._store.Graph(graphUri));
                }
            }
        }

        /// <summary>
        /// Loads a Graph from the Dataset
        /// </summary>
        /// <param name="g">Graph to load into</param>
        /// <param name="graphUri">URI of the Graph to load</param>
        public void LoadGraph(IGraph g, string graphUri)
        {
            if (graphUri.Equals(String.Empty))
            {
                this.LoadGraph(g, (Uri)null);
            }
            else
            {
                this.LoadGraph(g, new Uri(graphUri));
            }
        }

        /// <summary>
        /// Throws an error since this Manager is read-only
        /// </summary>
        /// <param name="g">Graph to save</param>
        /// <exception cref="RdfStorageException">Always thrown since this Manager provides a read-only connection</exception>
        public void SaveGraph(IGraph g)
        {
            throw new RdfStorageException("The DatasetFileManager provides a read-only connection");
        }

        /// <summary>
        /// Throws an error since this Manager is read-only
        /// </summary>
        /// <param name="graphUri">Graph URI</param>
        /// <param name="additions">Triples to be added</param>
        /// <param name="removals">Triples to be removed</param>
        public void UpdateGraph(Uri graphUri, IEnumerable<Triple> additions, IEnumerable<Triple> removals)
        {
            throw new RdfStorageException("The DatasetFileManager provides a read-only connection");
        }

        /// <summary>
        /// Throws an error since this Manager is read-only
        /// </summary>
        /// <param name="graphUri">Graph URI</param>
        /// <param name="additions">Triples to be added</param>
        /// <param name="removals">Triples to be removed</param>
        public void UpdateGraph(string graphUri, IEnumerable<Triple> additions, IEnumerable<Triple> removals)
        {
            throw new RdfStorageException("The DatasetFileManager provides a read-only connection");
        }

        /// <summary>
        /// Returns that Updates are not supported since this is a read-only connection
        /// </summary>
        public bool UpdateSupported
        {
            get 
            {
                return false;
            }
        }

        public void DeleteGraph(Uri graphUri)
        {
            throw new RdfStorageException("The DatasetFileManager provides a read-only connection");
        }

        public void DeleteGraph(String graphUri)
        {
            throw new RdfStorageException("The DatasetFileManager provides a read-only connection");
        }

        public bool DeleteSupported
        {
            get
            {
                return false;
            }
        }

        /// <summary>
        /// Returns that the Manager is ready if the underlying file has been loaded
        /// </summary>
        public bool IsReady
        {
            get
            {
                return this._ready;
            }
        }

        /// <summary>
        /// Returns that the Manager is read-only
        /// </summary>
        public bool IsReadOnly
        {
            get
            {
                return true;
            }
        }

        /// <summary>
        /// Disposes of the Manager
        /// </summary>
        public void Dispose()
        {
            this._store.Dispose();
        }

        /// <summary>
        /// Gets the String representation of the Connection
        /// </summary>
        /// <returns></returns>
        public override string ToString()
        {
            return "[Dataset File] " + this._filename;
        }

        /// <summary>
        /// Serializes the connection's configuration
        /// </summary>
        /// <param name="context">Configuration Serialization Context</param>
        public void SerializeConfiguration(ConfigurationSerializationContext context)
        {
            INode manager = context.NextSubject;
            INode rdfType = context.Graph.CreateUriNode(new Uri(RdfSpecsHelper.RdfType));
            INode rdfsLabel = context.Graph.CreateUriNode(new Uri(NamespaceMapper.RDFS + "label"));
            INode dnrType = ConfigurationLoader.CreateConfigurationNode(context.Graph, ConfigurationLoader.PropertyType);
            INode genericManager = ConfigurationLoader.CreateConfigurationNode(context.Graph, ConfigurationLoader.ClassGenericManager);
            INode file = ConfigurationLoader.CreateConfigurationNode(context.Graph, ConfigurationLoader.PropertyFromFile);

            context.Graph.Assert(new Triple(manager, rdfType, genericManager));
            context.Graph.Assert(new Triple(manager, rdfsLabel, context.Graph.CreateLiteralNode(this.ToString())));
            context.Graph.Assert(new Triple(manager, dnrType, context.Graph.CreateLiteralNode(this.GetType().FullName)));
            context.Graph.Assert(new Triple(manager, file, context.Graph.CreateLiteralNode(this._filename)));
        }
    }
}

#endif