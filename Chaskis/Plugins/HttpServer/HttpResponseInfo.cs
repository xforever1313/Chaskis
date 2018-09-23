//
//          Copyright Seth Hendrick 2018.
// Distributed under the Boost Software License, Version 1.0.
//    (See accompanying file LICENSE_1_0.txt or copy at
//          http://www.boost.org/LICENSE_1_0.txt)
//

using System.IO;
using System.Net;
using System.Text;
using System.Xml;

namespace Chaskis.Plugins.HttpServer
{
    /// <summary>
    /// Contains information to send back to the client.
    /// </summary>
    public class HttpResponseInfo
    {
        // ---------------- Constructor ----------------

        public HttpResponseInfo()
        {
            this.Message = string.Empty;
        }

        // ---------------- Properties ----------------

        /// <summary>
        /// Our internal response status.
        /// </summary>
        public HttpResponseStatus ResponseStatus { get; set; }

        /// <summary>
        /// The message we want to tell the client.
        /// </summary>
        public string Message { get; set; }

        /// <summary>
        /// The equivalent HTTP Status Code we want to send to the client.
        /// </summary>
        public HttpStatusCode HttpStatusCode
        {
            get
            {
                switch( this.ResponseStatus )
                {
                    case HttpResponseStatus.ClientError:
                        return HttpStatusCode.BadRequest;

                    case HttpResponseStatus.ServerError:
                        return HttpStatusCode.InternalServerError;

                    case HttpResponseStatus.Ok:
                        return HttpStatusCode.OK;

                    default:
                        // This means we are not handling something on the server.
                        return HttpStatusCode.InternalServerError;
                }
            }
        }

        /// <summary>
        /// The content type we want to respond as.
        /// </summary>
        public ContentType ContentType { get; set; }

        /// <summary>
        /// The content type string to send over as a response.
        /// </summary>
        public string ContentTypeString
        {
            get
            {
                switch( this.ContentType )
                {
                    case ContentType.Xml:
                        return "text/xml";

                    default:
                        return "text/plain";
                }
            }
        }

        // ---------------- Functions ----------------

        public string ToXml()
        {
            XmlDocument doc = new XmlDocument();

            // Create declaration.
            XmlDeclaration dec = doc.CreateXmlDeclaration( "1.0", "UTF-8", null );

            // Add declaration to document.
            XmlElement root = doc.DocumentElement;
            doc.InsertBefore( dec, root );

            // Root Node
            XmlElement rootNode = doc.CreateElement( "chaskis_http_response" );

            // Create Response Status node
            {
                XmlElement statusNode = doc.CreateElement( "status" );
                statusNode.InnerText = this.ResponseStatus.ToString();
                rootNode.AppendChild( statusNode );
            }

            // Create Message Node
            {
                XmlElement messageNode = doc.CreateElement( "message" );
                messageNode.InnerText = this.Message ?? string.Empty;
                rootNode.AppendChild( messageNode );
            }

            doc.InsertAfter( rootNode, dec );

            using( StringWriter stringWriter = new StringWriter() )
            {
                doc.Save( stringWriter );

                return stringWriter.GetStringBuilder().ToString();
            }
        }

        public string ToPlainText()
        {
            StringBuilder builder = new StringBuilder();

            builder.AppendLine( "Chaskis HTTP Server Response:" );
            builder.AppendLine( "\t- Response Status: " + this.ResponseStatus );
            builder.AppendLine( "\t- Message: " + this.Message );

            return builder.ToString();
        }

        public override string ToString()
        {
            switch( ContentType )
            {
                case ContentType.Xml:
                    return this.ToXml();

                default:
                    return this.ToPlainText();
            }
        }
    }
}
