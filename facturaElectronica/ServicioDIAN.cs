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

namespace facturaElectronica
{
    /// <summary>
    /// Clase para enviar documentos al servicio de la DIAN
    /// 
    /// Agradecimientos a https://es.stackoverflow.com/questions/191511/c-consumir-web-service-facturacion-electronica
    /// </summary>
    public static class ServicioDIAN
    {
        /// <summary>
        /// Empaqueta un archivo en ZIP y lo retorna como una cadena de bytes
        /// </summary>
        /// <param name="RutaArchivo">Ruta del archivo a e</param>
        /// <returns>Archivo empaquetado en memoria</returns>
        public static byte[] Empaquetar(string RutaArchivo) {
            string rutazip = Path.ChangeExtension(RutaArchivo, "zip");
            File.Delete(rutazip);
            using (ZipArchive zip = ZipFile.Open(rutazip, ZipArchiveMode.Create))
            {
                zip.CreateEntryFromFile(RutaArchivo, Path.GetFileName(RutaArchivo));
            }
            return File.ReadAllBytes(rutazip);
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
            byte[] archivo = Empaquetar(RutaArchivo);  //FileToByteArray(@"c:\temp\ws_f0890900161000000dab3.zip");
            EndpointAddress endpointAddress = new EndpointAddress(new Uri(apiUrl));
            var securityElement = SecurityBindingElement.CreateUserNameOverTransportBindingElement();
            securityElement.AllowInsecureTransport = false;
            securityElement.EnableUnsecuredResponse = true;
            securityElement.IncludeTimestamp = false;

            // var encodingElement = new MtomMessageEncodingBindingElement(MessageVersion.Soap11, Encoding.UTF8);
            var encodingElement = new TextMessageEncodingBindingElement(MessageVersion.Soap11, Encoding.UTF8);

            var transportElement = new HttpsTransportBindingElement();
            transportElement.UseDefaultWebProxy = true;
            var binding = new CustomBinding(securityElement, encodingElement, transportElement);

            facturaElectronicaPortNameClient Service = new facturaElectronicaPortNameClient(binding, endpointAddress);
            Service.ClientCredentials.UserName.UserName = IDSoftware;
            Service.ClientCredentials.UserName.Password = GetSHA256String(ClaveSoftware);

            EnvioFacturaElectronica enviofactura = new EnvioFacturaElectronica();
            enviofactura.NIT = Nit;
            enviofactura.InvoiceNumber = NumeroDoc;
            enviofactura.IssueDate = Convert.ToDateTime(FechaEmision /*Fecha de la factura*/);
            enviofactura.Document = Convert.FromBase64String(Convert.ToBase64String(archivo));

            EnvioFacturaElectronicaPeticion envioFacturaElectronicaPeticion = new EnvioFacturaElectronicaPeticion();
            envioFacturaElectronicaPeticion.EnvioFacturaElectronicaPeticion1 = enviofactura;

            AcuseRecibo acuseRecibo = new AcuseRecibo();
            acuseRecibo = Service.EnvioFacturaElectronica(envioFacturaElectronicaPeticion.EnvioFacturaElectronicaPeticion1);

            return acuseRecibo.Comments.ToString();
        }

        public static string GetSHA256String(string phrase)
        {
            SHA256CryptoServiceProvider Hasher = new SHA256CryptoServiceProvider();
            byte[] hashedDataBytes = Hasher.ComputeHash(Encoding.UTF8.GetBytes(phrase));
            return ByteArrayToString(hashedDataBytes);
            // return Convert.ToBase64String(hashedDataBytes);
        }

        public static string ByteArrayToString(byte[] ba)
        {
            return BitConverter.ToString(ba).Replace("-", "").ToLower();
        }

        /// <summary>
        /// Tomado de https://es.stackoverflow.com/questions/194903/c-wcf-error-en-respuesta-de-webservice-al-consumir-con-soap-mtom-de-facturacion
        /// </summary>
        /// <param name="stream"></param>
        /// <returns></returns>
        static byte[] RemoveSignatures(byte[] stream)
        {
            string stream2 = Encoding.UTF8.GetString(stream);
            stream2 = stream2.Replace("\0", "");

            Regex x = new Regex("(\\<SOAP-ENV:Header\\>)(.*?)(\\</SOAP-ENV:Header\\>)");
            string repl = "";
            stream2 = x.Replace(stream2, "$1" + repl + "$3");
            byte[] streamNuevo = Encoding.ASCII.GetBytes(stream2);

            return streamNuevo;
        }
    }
}
