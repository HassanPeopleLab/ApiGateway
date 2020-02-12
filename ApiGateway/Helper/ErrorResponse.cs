﻿using System.IO;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;

namespace ApiGateway.Helper
{
    public static class ErrorResponse
    {
        public static async Task<HttpResponseMessage> Construct(HttpResponseMessage response, byte loginService)
        {
            LoginParsers.ILoginParser login = null;
            switch (loginService)
            {
                case 1: // test token
                    login = new LoginParsers.TestToken();
                    break;
            }

            return await login.WrapResponseError(response);
                        
        }


        public static HttpResponseMessage Create(Microsoft.AspNetCore.Http.HttpRequest request, int code, string message)
        {
            string requestContent = getRequestContent(request);
            string requestID = getRequestID(requestContent);

            string soap_envelope = addSoapEnvelope(setResponse(code, message, requestID));

            HttpResponseMessage errorMessage = new HttpResponseMessage
            {
                //StatusCode = HttpStatusCode.NotFound,

                Content = new StringContent(soap_envelope, Encoding.UTF8, "text/xml"),

                StatusCode = System.Net.HttpStatusCode.OK
            
            };
            return errorMessage;
        }

        private static string addSoapEnvelope(StringBuilder sbxmlOut)
        {
            StringBuilder sb1 = new StringBuilder();
            sb1.Append("<?xml version=\"1.0\" encoding=\"utf-8\"?>");
            sb1.Append("<string xmlns=\"http://yeap.local/public/ws\">");
            sb1.Append(sbxmlOut.Replace("<", "&lt;").Replace(">", "&gt;").ToString());
            sb1.Append("</string>");

            return sb1.ToString();
        }
        private static StringBuilder setResponse(int code, string description, string requestID)
        {

            ApiGateway.Helper.Xml.Login.Response response = new Helper.Xml.Login.Response();
            response.ID = requestID;
            Helper.Xml.Login.ResponseResult result = new Helper.Xml.Login.ResponseResult();
            result.Codice = code;
            result.Descrizione = description;
            ApiGateway.Helper.Xml.Login.ResponseData data = null;

            response.Result = result;
            response.Data = data;

            System.Text.StringBuilder sbxmlOut = new System.Text.StringBuilder();
            XmlSerializer serOUT = new XmlSerializer(typeof(ApiGateway.Helper.Xml.Login.Response));
            StringWriter rdr_out = new StringWriter(sbxmlOut);
            serOUT.Serialize(rdr_out, response);

            return sbxmlOut;
        }
        private static string getRequestID(string requestContent){
            
            // getting request ID from request content
            string requestID = "";
            string xml_raw = requestContent.Substring(requestContent.IndexOf("<"));
            System.Xml.XmlDocument xmlDoc = new System.Xml.XmlDocument();
            xmlDoc.LoadXml(xml_raw);
            System.Xml.XmlNode request_node = xmlDoc.SelectSingleNode("/Request");
            if (request_node != null)
            {
                requestID = request_node.Attributes["ID"].InnerText;
            }

            return requestID;
        }
        private static string getRequestContent(Microsoft.AspNetCore.Http.HttpRequest request)
        {
            // getting request content
            Stream receiveStream = request.Body;
            StreamReader readStream = new StreamReader(receiveStream, Encoding.UTF8);
            string requestContent = readStream.ReadToEnd();

            return requestContent;
        }

    }
}
