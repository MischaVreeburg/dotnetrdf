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

#if !NO_WEB && !NO_ASP

using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Reflection;
using System.Text;
using System.Web;
using VDS.RDF.Parsing;
using VDS.RDF.Update.Protocol;
using VDS.RDF.Web.Configuration.Protocol;
using VDS.RDF.Writing;

namespace VDS.RDF.Web
{
    /// <summary>
    /// Abstract Base Class for creating SPARQL Uniform HTTP Protocol Handler implementations
    /// </summary>
    public abstract class BaseSparqlHttpProtocolHandler : IHttpHandler
    {
        /// <summary>
        /// Handler Configuration
        /// </summary>
        protected BaseProtocolHandlerConfiguration _config;

        /// <summary>
        /// Indicates that the Handler is reusable
        /// </summary>
        public bool IsReusable
        {
            get 
            {
                return true;
            }
        }

        /// <summary>
        /// Processes requests made to the Uniform HTTP Protocol endpoint and invokes the appropriate methods on the Protocol Processor that is in use
        /// </summary>
        /// <param name="context">HTTP Context</param>
        /// <remarks>
        /// <para>
        /// Implementations may override this if necessary - if the implementation is only providing additional logic such as authentication, ACLs etc. then it is recommended that the override applies its logic and then calls the base method since this base method will handle much of the error handling and sending of appropriate HTTP Response Codes.
        /// </para>
        /// </remarks>
        public virtual void ProcessRequest(HttpContext context)
        {
            this._config = this.LoadConfig(context);

            //Add our Custom Headers
            try
            {
                context.Response.Headers.Add("X-dotNetRDF-Version", Assembly.GetExecutingAssembly().GetName().Version.ToString());
            }
            catch (PlatformNotSupportedException)
            {
                context.Response.AddHeader("X-dotNetRDF-Version", Assembly.GetExecutingAssembly().GetName().Version.ToString());
            }

            //OPT: Add proper error pages?

            //Check whether we need to use authentication
            if (!HandlerHelper.IsAuthenticated(context, this._config.UserGroups, context.Request.HttpMethod)) return;

            try
            {
                //Invoke the appropriate method on our protocol processor
                switch (context.Request.HttpMethod)
                {
                    case "GET":
                        this._config.Processor.ProcessGet(context);
                        break;
                    case "PUT":
                        this._config.Processor.ProcessPut(context);
                        break;
                    case "POST":
                        this._config.Processor.ProcessPost(context);
                        break;
                    case "DELETE":
                        this._config.Processor.ProcessDelete(context);
                        break;
                    default:
                        //For any other HTTP Verb we send a 405 Method Not Allowed
                        context.Response.StatusCode = (int)HttpStatusCode.MethodNotAllowed;
                        break;
                }

                //Update the Cache as the request may have changed the endpoint
                this.UpdateConfig(context);
            }
            catch (SparqlHttpProtocolUriResolutionException)
            {
                //If URI Resolution fails we send a 400 Bad Request
                context.Response.StatusCode = (int)HttpStatusCode.BadRequest;
            }
            catch (SparqlHttpProtocolUriInvalidException)
            {
                //If URI is invalid we send a 400 Bad Request
                context.Response.StatusCode = (int)HttpStatusCode.BadRequest;
            }
            catch (NotSupportedException)
            {
                //If Not Supported we send a 405 Method Not Allowed
                context.Response.StatusCode = (int)HttpStatusCode.MethodNotAllowed;
            }
            catch (NotImplementedException)
            {
                //If Not Implemented we send a 501 Not Implemented
                context.Response.StatusCode = (int)HttpStatusCode.NotImplemented;
            }
            catch (RdfWriterSelectionException)
            {
                //If we can't select a valid Writer when returning content we send a 406 Not Acceptable
                context.Response.StatusCode = (int)HttpStatusCode.NotAcceptable;
            }
            catch (RdfParserSelectionException)
            {
                //If we can't select a valid Parser when receiving content we send a 415 Unsupported Media Type
                context.Response.StatusCode = (int)HttpStatusCode.UnsupportedMediaType;
            }
            catch (RdfParseException)
            {
                //If we can't parse the received content successfully we send a 400 Bad Request
                context.Response.StatusCode = (int)HttpStatusCode.BadRequest;
            }
            catch (Exception)
            {
                //For any other error we'll send a 500 Internal Server Error
                context.Response.StatusCode = (int)HttpStatusCode.InternalServerError;
            }
        }

        /// <summary>
        /// Loads the Handler Configuration
        /// </summary>
        /// <param name="context">HTTP Context</param>
        /// <returns></returns>
        protected abstract BaseProtocolHandlerConfiguration LoadConfig(HttpContext context);

        /// <summary>
        /// Updates the Handler Configuration
        /// </summary>
        /// <param name="context">HTTP Context</param>
        protected virtual void UpdateConfig(HttpContext context)
        {

        }
    }
}

#endif