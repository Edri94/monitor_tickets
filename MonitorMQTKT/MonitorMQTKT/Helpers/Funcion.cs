using System;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MonitorMQTKT.Funciones
{
    public class Funcion
    {
        /// <summary>
        /// escribe en el log
        /// </summary>
        /// <param name="vData"></param>
        public void Escribe(string vData, string tipo = "Mensaje")
        {
            string seccion = "escribeArchivoLOG";
            //Archivo = strlogFilePath & Format(Now(), "yyyyMMdd") & "-" & strlogFileName
            //string docPath = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);
            //string docPath = @"C:\tmp\log\";
            //string docPath = "D:\\Procesos\\TestMonitorMQTKTNet\\Procesos\\Log\\";

            if (true)
            {
                using (StreamWriter outputFile = new StreamWriter(Path.Combine(getValueAppConfig(seccion, "logFilePath"), getValueAppConfig(seccion, "logFileName")), append: true))
                {
                    vData = $"[{DateTime.Now.ToString("dd-MM-yyyy hh:mm:ss")}]  {tipo}:  {vData}";
                    Console.WriteLine(vData);
                    outputFile.WriteLine(vData);
                }

            }
        }

        /// <summary>
        /// escribe en el log
        /// </summary>
        /// <param name="vData"></param>
        public void Escribe(Exception ex, string tipo = "Error")
        {
            string vData;
            string seccion = "escribeArchivoLOG";
            //Archivo = strlogFilePath & Format(Now(), "yyyyMMdd") & "-" & strlogFileName
            //string docPath = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);
            //string docPath = @"C:\tmp\log\";
            //string docPath = "D:\\Procesos\\TestMonitorMQTKTNet\\Procesos\\Log\\";

            if (true)
            {
                using (StreamWriter outputFile = new StreamWriter(Path.Combine(getValueAppConfig(seccion, "logFilePath"), getValueAppConfig(seccion, "logFileName")), append: true))
                {
                    vData = $"[{DateTime.Now.ToString("dd-MM-yyyy hh:mm:ss")}] {(char)13}" +
                        $"*{tipo}:  {ex.Message} {(char)13}" +
                        $"*InnerException: {ex.InnerException} {(char)13}" +
                        $"*Source: {ex.Source}  {(char)13}" +
                        $"*Data: {ex.Data}  {(char)13}" +
                        $"*HelpLink: {ex.HelpLink}  {(char)13}" +
                        $"*TargetSite: {ex.TargetSite}  {(char)13}";
                    Console.Write(vData);
                    outputFile.WriteLine(vData);
                }

            }
        }

        /// <summary>
        /// Obtiene valor del parametro dado desde el app.config
        /// </summary>
        /// <param name="section">Seccion donde buscara</param>
        /// <param name="value">Valor que buscas</param>
        /// <returns></returns>
        public string getValueAppConfig(string section, string key = "")
        {
            if(key.Length >= 1)
            {
                return ConfigurationManager.AppSettings[$"{section}.{key}"];
            }
            else
            {
                return ConfigurationManager.AppSettings[$"{key}"];
            }
            
        }

        /// <summary>
        /// Obtiene fecha dependiendo el formato escogido
        /// </summary>
        /// <param name="formato"></param>
        /// <returns></returns>
        public string ObtenFechaFormato(int formato)
        {
            try
            {
                switch (formato)
                {
                    case 1:
                        return new DateTime().ToString("dd/MM/yyyy");
                    default:
                        return new DateTime().ToString("dd/MM/yyyy");
                }
            }
            catch (Exception error)
            {
                Escribe(error);
                return null;
            }
        }


        /// <summary>
        /// Escribe en el App.config en la seccion y key dada en parametros
        /// </summary>
        /// <param name="section">seccion en appsettings</param>
        /// <param name="key">key en appsetitngs</param>
        /// <param name="value">valor nuevo</param>
        public void SetParameterAppSettings(string key, string value, string section = "")
        {
            try
            {
                string appPath = System.IO.Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().Location);
                string[] appPath_arr = appPath.Split('\\');

                Escribe("Variable entrada [appPath]: " + appPath, "Mensaje");
                appPath = "";
                for (int i = 0; i < (appPath_arr.Length - 1); i++)
                {
                    appPath = appPath + "\\" + appPath_arr[i];
                }
                appPath = appPath.Substring(1, appPath.Length - 1);
                Escribe("Variable entrada [appPath]: " + appPath, "Mensaje");
                string configFile = System.IO.Path.Combine(appPath, "App.config");
                Escribe("Variable entrada [configFile]: " + configFile, "Mensaje");
                ExeConfigurationFileMap configFileMap = new ExeConfigurationFileMap();
                configFileMap.ExeConfigFilename = configFile;
                System.Configuration.Configuration config = ConfigurationManager.OpenMappedExeConfiguration(configFileMap, ConfigurationUserLevel.None);
                if (section.Length > 0)
                {
                    config.AppSettings.Settings[$"{section}.{key}"].Value = value;
                }
                else
                {
                    config.AppSettings.Settings[$"{key}"].Value = value;
                }
                config.Save();
            }
            catch (Exception ex)
            {
                Escribe(ex, "Error");
            }

        }

        /// <summary>
        /// Devuelve cadena igual a el numero de espacios dados en el parametro
        /// </summary>
        /// <param name="veces">numero de espacios</param>
        /// <returns></returns>
        public string Space(int veces)
        {
            return new String(' ', veces);
        }


        /// <summary>
        /// Devuelve una variante ( cadena ) que contiene un número especificado de caracteres del lado izquierdo de una cadena.
        /// </summary>
        /// <param name="cadena">Cadena</param>
        /// <param name="posiciones">Posiciones a tomar</param>
        /// <returns></returns>
        public string Left(string cadena, int posiciones)
        {
            return cadena.Substring(0, posiciones);
        }

        /// <summary>
        /// Devuelve una cadena que contiene un número de caracteres especificado a partir de una posición especificada de una cadena.
        /// </summary>
        /// <param name="cadena"></param>
        /// <param name="start"></param>
        /// <param name="length"></param>
        /// <returns></returns>
        public string Mid(string cadena, int start, int length)
        {
            return cadena.Substring(start, length);
        }
        /// <summary>
        /// Devuelve una variante ( cadena ) que contiene un número específico de caracteres del lado derecho de una cadena.
        /// </summary>
        /// <param name="cadena">Cadena</param>
        /// <param name="posiciones">Posiciones a tomar</param>
        /// <returns></returns>
        public string Right(string cadena, int posiciones)
        {
            return cadena.Substring((cadena.Length - posiciones), posiciones);
        }
    }
}
