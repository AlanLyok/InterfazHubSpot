using System;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BatchSpertaAPI.Business.Common
{
    public class Logger
    {
        private static readonly object _lock = new object();
        private static string logPathBase = ConfigurationManager.AppSettings["PathLog"]; // ruta donde quieras guardar el log

        private static string ObtenerRutaDelDia()
        {
            string carpeta = Path.GetDirectoryName(logPathBase);
            string nombreSinExt = Path.GetFileNameWithoutExtension(logPathBase);
            string ext = Path.GetExtension(logPathBase);
            string fecha = DateTime.Now.ToString("yyyyMMdd");
            return Path.Combine(carpeta, $"{nombreSinExt}_{fecha}{ext}");
        }

        public static void Log(string mensaje)
        {
            if (!string.IsNullOrEmpty(logPathBase))
            {
                string carpeta = Path.GetDirectoryName(logPathBase);
                if (!Directory.Exists(carpeta))
                    Directory.CreateDirectory(carpeta);

                string logPath = ObtenerRutaDelDia();

                lock (_lock) // evita problemas si se escribe desde varios hilos
                {
                    using (StreamWriter sw = new StreamWriter(logPath, true))
                    {
                        sw.WriteLine(DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") + " - " + mensaje);
                    }
                }
            }
        }
    }
}
