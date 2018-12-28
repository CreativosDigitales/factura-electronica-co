using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Diagnostics;
using System.Xml;

namespace facturaElectronica
{
    public static class FirmaDIAN
    {
        /// <summary>
        /// Genera el CUFE a partir de los valores convertidos a Cadenas
        /// </summary>
        /// <param name="NumFac"></param>
        /// <param name="FecFac"></param>
        /// <param name="ValFac"></param>
        /// <param name="ValImp1"></param>
        /// <param name="ValImp2"></param>
        /// <param name="ValImp3"></param>
        /// <param name="ValPag"></param>
        /// <param name="NitOFE"></param>
        /// <param name="TipAdq"></param>
        /// <param name="NumAdq"></param>
        /// <param name="ClTec"></param>
        /// <returns></returns>
        public static string GenerarCUFE(
            string NumFac, // = Número de factura. 
            string FecFac, // = Fecha de factura en formato (Java) YYYYmmddHHMMss. 
            string ValFac, // = Valor Factura sin IVA, con punto decimal, con decimales a dos (2) dígitos, sin separadoresde miles, ni símbolo pesos.
            string ValImp1, // = Valor impuesto 01, con punto decimal, con decimales a dos (2) dígitos, sin separadores de miles, ni símbolo pesos.
            string ValImp2, // = Valor impuesto 02, con punto decimal, con decimales a dos (2) dígitos, sin separadores de miles, ni símbolo pesos.
            string ValImp3, // = Valor impuesto 03, con punto decimal, con decimales a dos (2) dígitos, sin separadores de miles, ni símbolo pesos.
            string ValPag, // = Valor a Pagar, con punto decimal, con decimales a dos (2) dígitos, sin separadores de miles, ni símbolo pesos.
            string NitOFE, // = NIT del Facturador Electrónico sin puntos ni guiones, sin digito de verificación. 
            string TipAdq, // = tipo de adquiriente, de acuerdo con la tabla Tipos de documentos de identidad del «Anexo 001 Formato estándar XML de la Factura, notas débito y notas crédito electrónicos» 
            string NumAdq, // = Número de identificación del adquirente sin puntos ni guiones, sin digito de verificación. 
            string ClTec // = Clave técnica del rango de facturación.
          )
        {
            string semilla = (NumFac +
                                FecFac +
                                ValFac +
                                "01" + ValImp1 +
                                "02" + ValImp2 +
                                "03" + ValImp3 +
                                ValPag +
                                NitOFE +
                                TipAdq + NumAdq +
                                ClTec);

            Trace.WriteLine(semilla);

            using (var sha1 = new System.Security.Cryptography.SHA1Managed())
            {
                var hash = sha1.ComputeHash(Encoding.ASCII.GetBytes(semilla));
                var sb = new StringBuilder(hash.Length * 2); // Se usa un StringBuilder por eficiencia

                foreach (byte b in hash) { sb.Append(b.ToString("x2")); }  // Convertir a cadena Hexadecimal en minusculas

                return sb.ToString();
            }

        }



        /// <summary>
        /// Genera el CUFE a partir de los valores de la factura
        /// </summary>
        /// <param name="NumFac"></param>
        /// <param name="FecFac"></param>
        /// <param name="ValFac"></param>
        /// <param name="ValImp1"></param>
        /// <param name="ValImp2"></param>
        /// <param name="ValImp3"></param>
        /// <param name="ValPag"></param>
        /// <param name="NitOFE"></param>
        /// <param name="TipAdq"></param>
        /// <param name="NumAdq"></param>
        /// <param name="ClTec"></param>
        /// <returns></returns>
        public static string GenerarCUFE(
            string NumFac, // = Número de factura. 
            DateTime FecFac, // = Fecha de factura en formato (Java) YYYYmmddHHMMss. 
            decimal ValFac, // = Valor Factura sin IVA, con punto decimal, con decimales a dos (2) dígitos, sin separadoresde miles, ni símbolo pesos.
            decimal ValImp1, // = Valor impuesto 01, con punto decimal, con decimales a dos (2) dígitos, sin separadores de miles, ni símbolo pesos.
            decimal ValImp2, // = Valor impuesto 02, con punto decimal, con decimales a dos (2) dígitos, sin separadores de miles, ni símbolo pesos.
            decimal ValImp3, // = Valor impuesto 03, con punto decimal, con decimales a dos (2) dígitos, sin separadores de miles, ni símbolo pesos.
            decimal ValPag, // = Valor a Pagar, con punto decimal, con decimales a dos (2) dígitos, sin separadores de miles, ni símbolo pesos.
            string NitOFE, // = NIT del Facturador Electrónico sin puntos ni guiones, sin digito de verificación. 
            string TipAdq, // = tipo de adquiriente, de acuerdo con la tabla Tipos de documentos de identidad del «Anexo 001 Formato estándar XML de la Factura, notas débito y notas crédito electrónicos» 
            string NumAdq, // = Número de identificación del adquirente sin puntos ni guiones, sin digito de verificación. 
            string ClTec // = Clave técnica del rango de facturación.
          )
        {
            return GenerarCUFE(NumFac,
                                FecFac.ToString("yyyyMMddHHmmss", System.Globalization.CultureInfo.InvariantCulture),
                                ValFac.ToString("0.00", System.Globalization.CultureInfo.InvariantCulture),
                                ValImp1.ToString("0.00", System.Globalization.CultureInfo.InvariantCulture),
                                ValImp2.ToString("0.00", System.Globalization.CultureInfo.InvariantCulture),
                                ValImp3.ToString("0.00", System.Globalization.CultureInfo.InvariantCulture),
                                ValPag.ToString("0.00", System.Globalization.CultureInfo.InvariantCulture),
                                NitOFE,
                                TipAdq, NumAdq,
                                ClTec);
        }

        /// <summary>
        /// Genera el CUFE para un documento (Factura o Nota) en XML
        /// </summary>
        /// <param name="xml">Documento XML</param>
        /// <param name="ClTec">Clave técnica de la resolución de facturación bajo la que se emite este documento </param>
        /// <returns></returns>
        public static string GenerarCUFEDocumento(XmlDocument xml, string ClTec) {
            var nsmgr = CrearNsmgr(xml);
            return GenerarCUFE(TextoNodo(xml, nsmgr, "/fe:Invoice/cbc:ID"),
                                FechaNodo(xml, nsmgr, "/fe:Invoice/cbc:IssueDate") + FechaNodo(xml, nsmgr, "/fe:Invoice/cbc:IssueTime"),
                                ValorNodo(xml, nsmgr, "/fe:Invoice/fe:LegalMonetaryTotal/cbc:LineExtensionAmount"),
                                ValorNodo(xml, nsmgr, "/fe:Invoice/fe:TaxTotal/fe:TaxSubtotal[cac:TaxCategory/cac:TaxScheme/cbc:ID = '01']/cbc:TaxAmount"),
                                ValorNodo(xml, nsmgr, "/fe:Invoice/fe:TaxTotal/fe:TaxSubtotal[cac:TaxCategory/cac:TaxScheme/cbc:ID = '02']/cbc:TaxAmount"),
                                ValorNodo(xml, nsmgr, "/fe:Invoice/fe:TaxTotal/fe:TaxSubtotal[cac:TaxCategory/cac:TaxScheme/cbc:ID = '03']/cbc:TaxAmount"),
                                ValorNodo(xml, nsmgr, "/fe:Invoice/fe:LegalMonetaryTotal/cbc:PayableAmount"),
                                TextoNodo(xml, nsmgr, "/fe:Invoice/fe:AccountingSupplierParty/fe:Party/cac:PartyIdentification/cbc:ID"),
                                TextoNodo(xml, nsmgr, "/fe:Invoice/fe:AccountingCustomerParty/fe:Party/cac:PartyIdentification/cbc:ID/@schemeID"),
                                TextoNodo(xml, nsmgr, "/fe:Invoice/fe:AccountingCustomerParty/fe:Party/cac:PartyIdentification/cbc:ID"),
                                ClTec);
        }

        public static string TextoNodo(XmlNode xml, XmlNamespaceManager nsmgr, string xpath) {
            var n = xml.SelectSingleNode(xpath, nsmgr);
            if (n != null)
                return n.InnerText;
            else
                return "";
        }

        public static string ValorNodo(XmlNode xml, XmlNamespaceManager nsmgr, string xpath)
        {
            var n = xml.SelectSingleNode(xpath, nsmgr);
            if (n != null)
                return n.InnerText;
            else
                return "0.00";
        }


        public static string FechaNodo(XmlNode xml, XmlNamespaceManager nsmgr, string xpath)
        {
            var n = xml.SelectSingleNode(xpath, nsmgr);
            if (n == null)
                throw new Exception("El documento no contiene el elemento fecha " + xpath);

            return n.InnerText.Replace("-","").Replace(":", "");
        }

        public static XmlNamespaceManager CrearNsmgr(XmlDocument xml)
        {
            var result = new XmlNamespaceManager(xml.NameTable);
            result.AddNamespace("reportar", "http://www.dian.gov.co/servicios/facturaelectronica/ReportarFactura");
            result.AddNamespace("consulta", "http://www.dian.gov.co/servicios/facturaelectronica/ConsultaDocumentos");
            result.AddNamespace("version", "http://www.dian.gov.co/servicios/facturaelectronica/VersionDespliegue");
            return result;
        }

    }
}
