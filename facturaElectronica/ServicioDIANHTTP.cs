using System;
using System.IO;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.ServiceModel;
using System.ServiceModel.Channels;
using facturaElectronica.DIAN.Recepcion;
using System.Security.Cryptography;
using System.IO.Compression;
using System.Text.RegularExpressions;
using System.Net;
using System.Xml;

namespace facturaElectronica
{
    /// <summary>
    /// Clase para enviar documentos al servicio de la DIAN
    /// 
    /// Usa HTTP ´puro en lugar de los componentes SOAP de .NET
    /// 
    /// </summary>
    public static class ServicioDIANHTTP
    {
        /// <summary>
        /// Empaqueta un archivo en ZIP y lo retorna como una cadena de bytes
        /// </summary>
        /// <param name="RutaArchivo">Ruta del archivo a e</param>
        /// <returns>Archivo empaquetado en memoria</returns>
        public static string Empaquetar(string RutaArchivo)
        {
            string rutazip = Path.ChangeExtension(RutaArchivo, "zip");
            File.Delete(rutazip);
            using (ZipArchive zip = ZipFile.Open(rutazip, ZipArchiveMode.Create))
            {
                zip.CreateEntryFromFile(RutaArchivo, Path.GetFileName(RutaArchivo));
            }
            return Convert.ToBase64String( File.ReadAllBytes(rutazip) );
        }


        /// <summary>
        /// Empaqueta un archivo en ZIP y lo retorna como una cadena de bytes
        /// </summary>
        /// <param name="RutaArchivo">Ruta del archivo a e</param>
        /// <returns>Archivo empaquetado en memoria</returns>
        public static string EmpaquetarEnMemoria(string RutaArchivo) {
            string fileName = Path.GetFileName(RutaArchivo);
            byte[] fileBytes = File.ReadAllBytes(RutaArchivo);
            byte[] compressedBytes;
            string fileNameZip = Path.GetFileNameWithoutExtension(RutaArchivo) + ".zip";

            using (var outStream = new MemoryStream())
            {
                using (var archive = new ZipArchive(outStream, ZipArchiveMode.Create, true))
                {
                    var fileInArchive = archive.CreateEntry(fileName, CompressionLevel.Optimal);
                    using (var entryStream = fileInArchive.Open())
                    using (var fileToCompressStream = new MemoryStream(fileBytes))
                    {
                        fileToCompressStream.CopyTo(entryStream);
                    }
                }
                compressedBytes = outStream.ToArray();
            }

            return Convert.ToBase64String(compressedBytes);

        }

        /// <summary>
        /// Envia al servicio de la DIAN
        /// </summary>
        /// <param name="IDSoftware">Id del software en el Muisca</param>
        /// <param name="ClaveSoftware">Clave PIN asignada al software en el Muisca</param>
        /// <param name="Nit">Nit del Emisor</param>
        /// <param name="NumeroDoc">Numero de la factura o nota. Debe coincidir con el XML</param>
        /// <param name="FechaEmision">Fecha del documento. Debe coincidir con el XML</param>
        /// <param name="RutaArchivo">Ruta en el disco del documento XML a enviar</param>
        /// <returns>Mensaje de aceptación o recibo de la DIAN</returns>
        public static string EnviarDocumento(string IDSoftware, string ClaveSoftware,
                                                string Nit, string NumeroDoc, DateTime FechaEmision,
                                                String RutaArchivo) {
            const string apiUrl = "https://facturaelectronica.dian.gov.co/habilitacion/B2BIntegrationEngine/FacturaElectronica";
            string archivo = EmpaquetarEnMemoria(RutaArchivo);  //FileToByteArray(@"c:\temp\ws_f0890900161000000dab3.zip");

            string fmt = "yyyy-MM-ddTHH:mm:ss";
            string mensaje = ArmarMensaje(IDSoftware, ClaveSoftware, Nit, NumeroDoc, FechaEmision.ToString(fmt), archivo);

            HttpStatusCode estadoHttp;
            string respuesta = LlamarMetodo(apiUrl, mensaje, out estadoHttp);

            XmlDocument xml = MaterializarRespuesta(respuesta);


            return xml.SelectSingleNode("//reportar:Comments", nsmgr(xml) ).InnerText;
        }

        private static XmlNamespaceManager nsmgr(XmlDocument xml)
        {
            var result = new XmlNamespaceManager(xml.NameTable);
            result.AddNamespace("reportar", "http://www.dian.gov.co/servicios/facturaelectronica/ReportarFactura" );
            result.AddNamespace("consulta", "http://www.dian.gov.co/servicios/facturaelectronica/ConsultaDocumentos" );
            result.AddNamespace("version", "http://www.dian.gov.co/servicios/facturaelectronica/VersionDespliegue");
            return result;
        }

        public static XmlDocument MaterializarRespuesta(string contenido)
        {
            Regex rx = new Regex("<SOAP-ENV:Envelope.*SOAP-ENV:Envelope>");
            var mensaje = rx.Match(contenido);
            XmlDocument doc = new XmlDocument();
            doc.LoadXml(mensaje.Value);
            return doc;
        }



        public static string LlamarMetodo(string url, string mensaje,  out HttpStatusCode status)
        {
            string res = ""; // Contenido de la respuesta
            try
            {
                // 20180704 WV: obliga a usar TLS 1.2 en las conexiones seguras
                ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12;

                // log.Debug("SOAP", "Request", mensaje);

                HttpWebRequest rq = (HttpWebRequest)WebRequest.Create(url);

                rq.Method = "POST";
                rq.ContentType = "text/xml; charset=utf-8";
                rq.Headers["SOAPAction"] = "\"\"";

                using (Stream s = rq.GetRequestStream())
                {
                    byte[] bytes = Encoding.UTF8.GetBytes(mensaje);
                    s.Write(bytes, 0, bytes.Length);
                }

                HttpWebResponse resp = (HttpWebResponse)rq.GetResponse();

//                log.Debug("SOAP", "Status", resp.StatusDescription);
                // log.Debug("SOAP", "Response Headers", resp.Headers.ToString());
                //  log.Debug("SOAP", "Response", res);
                var sr = new StreamReader(resp.GetResponseStream());
                res = sr.ReadToEnd();
                // log.Debug("SOAP", "Response Body", res);
                status = resp.StatusCode;
                return res;
            }
            catch (WebException we)
            {
                HttpWebResponse resp = we.Response as HttpWebResponse;
                if (resp == null)
                    throw;

                // log.Error("SOAP", we);

                // log.Debug("SOAP", "ERROR", res);

                var sr = new StreamReader(resp.GetResponseStream());

                res += sr.ReadToEnd();
                status = resp.StatusCode;

                // log.Debug("SOAP", "ERROR", res);
            }
            catch (Exception err)
            {
                // log.Error("SOAP", err);
                throw err;
            }

            return res;

        }

        static string ArmarMensaje(string IDSoftware, string ClaveSoftware,
                                                string Nit, string NumeroDoc, string FechaEmision,
                                                String Archivo ) {
            string msg = "<s:Envelope xmlns:s=\"http://schemas.xmlsoap.org/soap/envelope/\" xmlns:u=\"http://docs.oasis-open.org/wss/2004/01/oasis-200401-wss-wssecurity-utility-1.0.xsd\"> " +
                "<s:Header>" +
                    "<o:Security s:mustUnderstand=\"1\" xmlns:o=\"http://docs.oasis-open.org/wss/2004/01/oasis-200401-wss-wssecurity-secext-1.0.xsd\" >" +
                        "<o:UsernameToken u:Id=\"uuid-f7cfa233-a3b0-4323-9c9c-e95458f9c3d1-1\" >" +
                          "<o:Username>" + IDSoftware + "</o:Username>" +
                          "<o:Password>" + GetSHA256String(ClaveSoftware) + "</o:Password>" +
                        "</o:UsernameToken>" +
                    "</o:Security>" +
                "</s:Header>" +
                "<s:Body xmlns:xsi=\"http://www.w3.org/2001/XMLSchema-instance\" xmlns:xsd=\"http://www.w3.org/2001/XMLSchema\" >" +
                    "<EnvioFacturaElectronicaPeticion xmlns=\"http://www.dian.gov.co/servicios/facturaelectronica/ReportarFactura\" >" +
                        "<NIT>" + Nit + "</NIT>" +
                        "<InvoiceNumber>" + NumeroDoc + "</InvoiceNumber>" +
                        "<IssueDate>" + FechaEmision + "</IssueDate>" +
                        "<Document>" + Archivo + "</Document>" +
                    "</EnvioFacturaElectronicaPeticion>" +
                "</s:Body>" +
            "</s:Envelope>";
            return msg;
        }


        public static string GetSHA256String(string phrase)
        {
            SHA256CryptoServiceProvider Hasher = new SHA256CryptoServiceProvider();
            byte[] hashedDataBytes = Hasher.ComputeHash(Encoding.UTF8.GetBytes(phrase));
            return ByteArrayToString(hashedDataBytes);
            // return Convert.ToBase64String(hashedDataBytes);
        }


        static string GetSHA256Base64(string phrase)
        {
            SHA256CryptoServiceProvider Hasher = new SHA256CryptoServiceProvider();
            byte[] hashedDataBytes = Hasher.ComputeHash(Encoding.UTF8.GetBytes(phrase));
            // return ByteArrayToString(hashedDataBytes);
            return Convert.ToBase64String(hashedDataBytes);
        }

        public static string ByteArrayToString(byte[] ba)
        {
            return BitConverter.ToString(ba).Replace("-", "").ToLower();
        }


    }
}
