using IBM.WMQ;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


namespace Bitacoras
{
    public class Bitacora
    {
        private string ArchivoIni;
        private string lsCommandLine;

        // Variables para el registro de los valores del header IH
        private string strFuncionHost; // Valor que indica el programa que invocara el CICSBRIDGE
        private string strHeaderTagIni; // Bandera que indica el comienzo del Header
        private string strIDProtocol; // Identificador  del protocolo (PS9)
        private string strLogical; // Terminal Lógico Asigna Arquitectura ASTA
        private string strAccount; // Terminal Contable (CR Contable)
        private string strUser; // Usuario. Debe ser diferente de espacios
        private string strSeqNumber; // Número de Secuencia (indicador de paginación)
        private string strTXCode; // Función específica Asigna Arquitectura Central
        private string strUserOption; // Tecla de función (no aplica)
        private string strCommit; // Indicador de commit: Permite realizar commit
        private string strMsgType; // Tipo de mensaje: Nuevo requerimiento
        private string strProcessType; // Tipo de proceso: on line
        private string strChannel; // Canal Asigna Arquitectura Central
        private string strPreFormat; // Indicador de preformateo: Arquitectura no deberá de preformatear los datos
        private string strLenguage; // Idioma: Español
        private string strHeaderTagEnd; // Bandera que indica el final del header

        // Variables para el registro de los valores del header ME
        private string strMETAGINI; // Bandera que indica el comienzo del mensaje
        private string strMsgTypeCole; // Tipo de mensaje: Copy
        private string strMETAGEND; // Bandera que indica el fin del mensaje

        // Variables para el registro de los valores Default
        string strColectorMaxLeng; // Maxima longitud del COLECTOR
        string strMsgMaxLeng; // Maxima longitud del del bloque ME
        string strPS9MaxLeng; // Maxima longitud del formato PS9
        string strReplyToMQ; // MQueue de respuesta para HOST
        string strFuncionSQL; // Funcion a ejecutar al recibir la respuesta
        string strRndLogTerm; // Indica que el atributo Logical Terminal es 

        private string Gs_MQManager;   // MQManager de Escritura
        private string Gs_MQQueueEscritura;   // MQQueue de Escritura
        public DateTime gsAccesoActual;   // Fecha/Hora actual del sistema. La tomamos del servidor NT y no de SQL porque precisamente el

        MqSeries mqSeries;
        public Bitacora()
        {
            //mqSeries = new MqSeries();
        }

        public void ProcesarBitacora(string ruta, string strParametros)
        {
            string[] parametros;
            string ls_MsgVal = "";

            try
            {
                //ArchivoIni = strRutaIni + @"\Bitacoras.ini";
                ConfiguraFileLog("escribeArchivoLOG");
                gsAccesoActual = DateTime.Now;

                lsCommandLine = strParametros.Trim();

                if (lsCommandLine.Equals("") != false)
                {
                    parametros = lsCommandLine.Split('-');

                    Gs_MQManager = parametros[0].Trim();
                    Gs_MQQueueEscritura = parametros[1].Trim();
                    strFuncionSQL = parametros[3];
                }
                else
                {
                    ObtenerInfoMq();
                }


                ConfiguraHeader_IH_ME();

                if (!ValidaInfoMQ(ls_MsgVal))
                {
                    Escribe("Se presentó un error en la función ValidaInfoMQ invocada desde el MAIN: " + ls_MsgVal + ". Función SQL: " + strFuncionSQL);
                    return;
                }

                ProcesoBDtoMQQUEUE();

                Escribe("Termina proceso bitácoras. Función SQL: " + strFuncionSQL);


            }
            catch (Exception Err)
            {
                //mqSeries.MQDesconectar(mqSeries.queueManager, mqSeries.queue);
                Escribe("Termina el acceso a la aplicación Bitácoras porque se presentó un error en la función MAIN. Función SQL: " + strFuncionSQL + ". Error. " + Err.Data + "-" + Err.Message);
            }
        }

        private void ConfiguraFileLog(string section)
        {
            string strlogFileName = getValueAppConfig(section, "logFileName");
            string strlogFilePath = getValueAppConfig(section, "logFilePath");

            bool Mb_GrabaLog = true;
        }

        private void ObtenerInfoMq()
        {
            string section = "mqSeries";
            Gs_MQManager = getValueAppConfig(section, "MQManager"); 
            Gs_MQQueueEscritura = getValueAppConfig(section, "MQEscritura");
            strFuncionSQL = getValueAppConfig(section, "FGBitacora"); 
        }

        private void ConfiguraHeader_IH_ME()
        {
            string section = "headerih";
            strFuncionHost = getValueAppConfig(section, "PRIMERVALOR"); 
            strHeaderTagIni = getValueAppConfig(section, "IHTAGINI");
            strIDProtocol = getValueAppConfig(section, "IDPROTOCOL");
            strLogical = getValueAppConfig(section, "FGBitacora");
            strAccount = getValueAppConfig(section, "ACCOUNT");
            strUser = getValueAppConfig(section, "User");
            strSeqNumber = getValueAppConfig(section, "SEQNUMBER");
            strTXCode = getValueAppConfig(section, "TXCODE");
            strUserOption = getValueAppConfig(section, "TXCODE");
            strCommit = getValueAppConfig(section, "Commit");
            strMsgType =getValueAppConfig(section, "MSGTYPE"); 
            strProcessType =getValueAppConfig(section, "PROCESSTYPE"); 
            strChannel =getValueAppConfig(section, "CHANNEL"); 
            strPreFormat =getValueAppConfig(section, "PREFORMATIND"); 
            strLenguage =getValueAppConfig(section, "LANGUAGE"); 
            strHeaderTagEnd =getValueAppConfig(section, "IHTAGEND");

            section = "headerme";

            strMETAGINI = getValueAppConfig(section,"METAGINI");
            strMsgTypeCole = getValueAppConfig(section,"TIPOMSG");
            strMETAGEND = getValueAppConfig(section,"METAGEND");

            section = "defaultValues";

            strColectorMaxLeng = getValueAppConfig(section,"COLMAXLENG");
            strMsgMaxLeng = getValueAppConfig(section,"MSGMAXLENG");
            strPS9MaxLeng = getValueAppConfig(section,"PS9MAXLENG");
            strReplyToMQ = getValueAppConfig(section,"ReplyToQueue");
            strRndLogTerm = getValueAppConfig(section,"RandomLogTerm");
        }

        private bool ValidaInfoMQ(string ps_MsgVal)
        {
            bool validaInfoMQ = false;
            string ls_msg = "";

            if (Gs_MQManager.Trim() == "")
            {
                ls_msg = ls_msg + (ls_msg.Length > 0 ? ((char)13).ToString() : "") + "Falta MQ Manager envio.";
            }
            if (Gs_MQQueueEscritura.Trim() == "")
            {
                ls_msg = ls_msg + (ls_msg.Length > 0 ? ((char)13).ToString() : "") + "Falta MQ Queue envio.";
            }
            if (ls_msg.Trim() == "")
            {
                validaInfoMQ = true;
            }

            ps_MsgVal = ls_msg;

            return validaInfoMQ;

        }

        private void ProcesoBDtoMQQUEUE()
        {
            string Ls_MensajeMQ;
            string Ls_MsgColector;

            string sFechaEnvio;
            string sEnvioConse;
            string sMensajeEnvio = "";
          

            try
            {
                mqSeries = new MqSeries();
                Escribe("");
                Escribe("Inicia envío de mensajes a Host: " + gsAccesoActual + " Función SQL: " + strFuncionSQL);

                if (mqSeries.MQConectar(Gs_MQManager, mqSeries.queueManager) == "")
                {
                    mqSeries.blnConectado = true;
                }
                else
                {
                    Escribe("Fallo conexión MQ-Manager " + Gs_MQManager + ": " + mqSeries.queueManager.ReasonCode + " - " + mqSeries.queueManager.ReasonName);
                }

                sFechaEnvio = Left(DateTime.Now.ToString("yyyymmddhhnnss") + Space(26), 26);
                sEnvioConse = Left(getValueAppConfig("valorTk14", "TKCONSECUTIVO") + Space(1), 1);

                Ls_MsgColector = (strFuncionSQL + new String(' ', 8)).Substring(0, 8);

                if (Ls_MsgColector.Length > 0)
                {
                    Ls_MensajeMQ = ASTA_ENTRADA(Ls_MsgColector);
                    if (Ls_MensajeMQ != "")
                    {
                        Escribe("Mensaje Enviado: " + Ls_MensajeMQ);

                        if (mqSeries.MQEnviarMsg(mqSeries.queueManager, Gs_MQQueueEscritura, mqSeries.queue, mqSeries.queueMessage, Ls_MensajeMQ, strReplyToMQ))
                        {
                            sMensajeEnvio = (sEnvioConse + 1).ToString();

                            if (Int32.Parse(sMensajeEnvio) > 9)
                            {
                                sMensajeEnvio = ((char)(1)).ToString();
                            }
                            SetParameterAppSettings("valorTk14", "TKCONSECUTIVO", sMensajeEnvio);
                        }
                        else
                        {
                            Escribe("Se ha presentado un error al escribir la solicitud en la MQ QUEUE:");
                        }
                    }
                    else
                    {
                        Escribe("Se ha presentado un error durante el armado del formato PS9 funcion ASTA_ENTRADA.Colector: " + Ls_MsgColector);
                    }
                }
                else
                {
                    Escribe("Se ha presentado un error al armar el Layout TKT14. No existe longitud en el Colector");
                }

                mqSeries.MQDesconectar(mqSeries.queueManager, mqSeries.queue);

                Escribe("Envio de solicitures TKT -> Host Terminado");
                Escribe("Solicitudes enviadas a MQ: " + sMensajeEnvio);
            }
            catch (Exception ex)
            {
                Escribe("Se presentó un error durante la ejecución de la función ProcesoBDtoMQQUEUE : " + ex.Message);
            }

        }

        private string ASTA_ENTRADA(string strMsgColector)
        {
            string ASTA_ENTRADA;

            string ls_TempColectorMsg;
            string ls_BloqueME;
            int ln_longCOLECTOR;
            int ln_AccTerminal;

            try
            {
                ls_TempColectorMsg = strMsgColector;

                if (ls_TempColectorMsg.Length > Int32.Parse(strColectorMaxLeng))
                {
                    Escribe("La longitud del colector supera el maximo permitido");
                    //GoTo ErrorASTA
                }

                ls_BloqueME = Left((strMETAGINI + Space(4)).Trim(), 4);
                ls_BloqueME = ls_BloqueME + Right("0000" + (ls_TempColectorMsg.Length.ToString()), 4);
                ls_BloqueME = ls_BloqueME + Left((strMsgTypeCole.Trim() + Space(5)), 5);
                ls_BloqueME = ls_BloqueME + ls_TempColectorMsg;
                ls_BloqueME = ls_BloqueME + Left(strMETAGEND + Space(5), 5);

                if (ls_BloqueME.Length > Int32.Parse(strMsgMaxLeng))
                {
                    Escribe("La longitud del Bloque ME supera el maximo permitido");
                    //GoTo ErrorASTA
                }

                ASTA_ENTRADA = Left(strFuncionHost.Trim() + Space(8), 8);
                ASTA_ENTRADA = ASTA_ENTRADA + Left(strHeaderTagIni.Trim() + Space(4), 4);
                ASTA_ENTRADA = ASTA_ENTRADA + Left(strIDProtocol.Trim() + Space(2), 2);

                if (strRndLogTerm.Trim().Equals("1"))
                {
                    ln_AccTerminal = 0;
                    do
                    {
                        //new  Random(DateTime.Now.Second);
                        var Rnd = new Random(DateTime.Now.Second * 1000);
                        ln_AccTerminal = Rnd.Next();

                    } while (ln_AccTerminal > 0 && ln_AccTerminal < 2000);

                    ASTA_ENTRADA = ASTA_ENTRADA + Left((ln_AccTerminal.ToString("D4") + Space(8)), 8);
                }
                else
                {
                    ASTA_ENTRADA = ASTA_ENTRADA + Left(strAccount.Trim() + Space(8), 8);
                }

                ASTA_ENTRADA = ASTA_ENTRADA + Left(strAccount.Trim() + Space(8), 8);
                ASTA_ENTRADA = ASTA_ENTRADA + Left(strUser.Trim() + Space(8), 8);
                ASTA_ENTRADA = ASTA_ENTRADA + Left(strSeqNumber.Trim() + Space(8), 8);
                ASTA_ENTRADA = ASTA_ENTRADA + Left(strTXCode.Trim() + Space(8), 8);
                ASTA_ENTRADA = ASTA_ENTRADA + Left(strUserOption.Trim() + Space(8), 8);

                ln_longCOLECTOR = 65 + ls_BloqueME.Length;

                if (ln_longCOLECTOR > Int32.Parse(strPS9MaxLeng.Trim()))
                {
                    Escribe("La longitud del Layout PS9 supera el maximo permitido");
                    //GoTo ErrorASTA
                }

                ASTA_ENTRADA = ASTA_ENTRADA + Right("00000" + ln_longCOLECTOR.ToString(), 5);
                ASTA_ENTRADA = ASTA_ENTRADA + Left(strCommit.Trim() + Space(1), 1);
                ASTA_ENTRADA = ASTA_ENTRADA + Left(strMsgType.Trim() + Space(1), 1);
                ASTA_ENTRADA = ASTA_ENTRADA + Left(strProcessType.Trim() + Space(1), 1);
                ASTA_ENTRADA = ASTA_ENTRADA + Left(strChannel.Trim() + Space(2), 2);
                ASTA_ENTRADA = ASTA_ENTRADA + Left(strPreFormat.Trim() + Space(1), 1);
                ASTA_ENTRADA = ASTA_ENTRADA + Left(strLenguage.Trim() + Space(1), 1);
                ASTA_ENTRADA = ASTA_ENTRADA + Left(strHeaderTagEnd.Trim() + Space(5), 5);
                ASTA_ENTRADA = ASTA_ENTRADA + ls_BloqueME;


                return ASTA_ENTRADA;
            }
            catch (Exception ex)
            {
                mqSeries.Escribe(ex.Message);
                return ex.Message;
            }
        }

        //**********************************FUNCIONES EXTRAS********************************** 

        /// <summary>
        /// Obtiene valor del parametro dado desde el app.config
        /// </summary>
        /// <param name="section">Seccion donde buscara</param>
        /// <param name="value">Valor que buscas</param>
        /// <returns></returns>
        private string getValueAppConfig(string section, string key)
        {
            return ConfigurationManager.AppSettings[$"{section}.{key}"]; ;
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
        /// Devuelve una variante ( cadena ) que contiene un número específico de caracteres del lado derecho de una cadena.
        /// </summary>
        /// <param name="cadena">Cadena</param>
        /// <param name="posiciones">Posiciones a tomar</param>
        /// <returns></returns>
        public string Right(string cadena, int posiciones)
        {
            return cadena.Substring((cadena.Length - posiciones), posiciones);
        }

        /// <summary>
        /// Escribe en el App.config en la seccion y key dada en parametros
        /// </summary>
        /// <param name="section">seccion en appsettings</param>
        /// <param name="key">key en appsetitngs</param>
        /// <param name="value">valor nuevo</param>
        public void SetParameterAppSettings(string section, string key, string value)
        {
            string appPath = System.IO.Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().Location);
            string[] appPath_arr = appPath.Split('\\');

            appPath = "";
            for (int i = 0; i < (appPath_arr.Length - 2); i++)
            {
                appPath = appPath + "\\" + appPath_arr[i];
            }
            appPath = appPath.Substring(1, appPath.Length - 1);

            string configFile = System.IO.Path.Combine(appPath, "App.config");
            ExeConfigurationFileMap configFileMap = new ExeConfigurationFileMap();
            configFileMap.ExeConfigFilename = configFile;
            System.Configuration.Configuration config = ConfigurationManager.OpenMappedExeConfiguration(configFileMap, ConfigurationUserLevel.None);
            config.AppSettings.Settings[$"{section}.{key}"].Value = value;

            config.Save();
        }

        /// <summary>
        /// escribe en el log
        /// </summary>
        /// <param name="vData"></param>
        public void Escribe(string vData)
        {
            //Archivo = strlogFilePath & Format(Now(), "yyyyMMdd") & "-" & strlogFileName
            //string docPath = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);
            //string docPath = @"C:\tmp\log\";
            string docPath = "D:\\Procesos\\TestMonitorMQTKTNet\\Procesos\\Log\\";

            if (true)
            {
                using (StreamWriter outputFile = new StreamWriter(Path.Combine(docPath, "log.txt"),append:true))
                {
                    vData = "[" + DateTime.Now.ToString("dd-MM-yyyy hh:mm:ss") + "]  Error: " + vData ;
                    Console.WriteLine(vData);
                    outputFile.WriteLine(vData);
                }

            }
        }
    }
}
