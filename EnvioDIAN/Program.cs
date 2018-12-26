using facturaElectronica;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EnvioDIAN
{
    class Program
    {
        static void Main(string[] args)
        {

            string IDSoftware = "12345678-1234-1234-1234-1234567890123456";
            string ClaveSoftware = "clave-creada-en-el-muisca";
            string Nit = "123456789";
            string NumeroDoc = "PRUE980000000";
            DateTime FechaEmision = new DateTime(2018,12,12);
            String RutaArchivo = @"face_f09009711530000000001.xml";

            Console.WriteLine("Listo para enviar");
            Console.ReadLine();
            try
            {
                Console.WriteLine(ServicioDIANHTTP.EnviarDocumento(IDSoftware, ClaveSoftware, Nit, NumeroDoc, FechaEmision, RutaArchivo));
            }
            catch (Exception e) {
                ShowException(e);
            }
            Console.ReadLine();
        }

        static void ShowException(Exception e)
        {
            Console.WriteLine(e.ToString());
        //    Console.WriteLine(e.Message);
        //    Console.WriteLine(e.StackTrace);
            if (e.InnerException != null)
                ShowException(e.InnerException);
        }
    }
}
