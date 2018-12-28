using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using facturaElectronica;

namespace Pruebas
{
    [TestClass]
    public class UnitTest1
    {
        [TestMethod]
        public void ProbarCUFEvalores()
        {
            string cufe = FirmaDIAN.GenerarCUFE(
            "323200000129", // = Número de factura. 
            new DateTime(2015, 08, 12, 06, 11, 31), // = Fecha de factura en formato (Java) YYYYmmddHHMMss. 
            1109376.00m, // = Valor Factura sin IVA, con punto decimal, con decimales a dos (2) dígitos, sin separadoresde miles, ni símbolo pesos.
            0.00m, // = Valor impuesto 01, con punto decimal, con decimales a dos (2) dígitos, sin separadores de miles, ni símbolo pesos.
            45928.16m, // = Valor impuesto 02, con punto decimal, con decimales a dos (2) dígitos, sin separadores de miles, ni símbolo pesos.
            107165.72m, // = Valor impuesto 03, con punto decimal, con decimales a dos (2) dígitos, sin separadores de miles, ni símbolo pesos.
            1296705.20m, // = Valor IVA, con punto decimal, con decimales a dos (2) dígitos, sin separadores de miles, ni símbolo pesos.
            "700085371", // = NIT del Facturador Electrónico sin puntos ni guiones, sin digito de verificación. 
            "31", // = tipo de adquiriente, de acuerdo con la tabla Tipos de documentos de identidad del «Anexo 001 Formato estándar XML de la Factura, notas débito y notas crédito electrónicos» 
            "800199436", // = Número de identificación del adquirente sin puntos ni guiones, sin digito de verificación. 
            "693ff6f2a553c3646a063436fd4dd9ded0311471" // = Clave técnica del rango de facturación.
          );

            Assert.AreEqual("77c35e565a8d8f9178f2c0cb422b067091c1d760", cufe);
        }

        [TestMethod]
        public void ProbarCUFEcadenas()
        {
            string cufe = FirmaDIAN.GenerarCUFE(
            "323200000129", // = Número de factura. 
            "20150812061131", // = Fecha de factura en formato (Java) YYYYmmddHHMMss. 
            "1109376.00", // = Valor Factura sin IVA, con punto decimal, con decimales a dos (2) dígitos, sin separadoresde miles, ni símbolo pesos.
            "0.00", // = Valor impuesto 01, con punto decimal, con decimales a dos (2) dígitos, sin separadores de miles, ni símbolo pesos.
            "45928.16", // = Valor impuesto 02, con punto decimal, con decimales a dos (2) dígitos, sin separadores de miles, ni símbolo pesos.
            "107165.72", // = Valor impuesto 03, con punto decimal, con decimales a dos (2) dígitos, sin separadores de miles, ni símbolo pesos.
            "1296705.20", // = Valor IVA, con punto decimal, con decimales a dos (2) dígitos, sin separadores de miles, ni símbolo pesos.
            "700085371", // = NIT del Facturador Electrónico sin puntos ni guiones, sin digito de verificación. 
            "31", // = tipo de adquiriente, de acuerdo con la tabla Tipos de documentos de identidad del «Anexo 001 Formato estándar XML de la Factura, notas débito y notas crédito electrónicos» 
            "800199436", // = Número de identificación del adquirente sin puntos ni guiones, sin digito de verificación. 
            "693ff6f2a553c3646a063436fd4dd9ded0311471" // = Clave técnica del rango de facturación.
          );

            Assert.AreEqual("77c35e565a8d8f9178f2c0cb422b067091c1d760", cufe);
        }
    }
}
