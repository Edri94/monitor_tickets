using System.Windows.Forms;
using MonitorMQTKT.Funciones;
using System;
using IBM.WMQ;
using System.Collections.Generic;

namespace MonitorMQTKT
{
    public partial class FrmMonitor : Form
    {
        private bool ErrorEjecucion;
        private bool ActivoProcFuncAuto;        // Variable para determinar si se desea ejecutar el proceso del Monitoreo
        private bool ModoMonitor;               // Variable para determinar el modo de operacion del monitor
        private bool DetallesShow;              // Variable para determinar el status de los detalles
        private bool bCambioManual;

        // ***** Para realizar el monitoreo de bitacoras
        private int miTipoMonitoreo;
        private int miTotalMonitor;
        private int ItemProceso;
        private int ItemSeleccion;
        private int intlBitacoras;

        private double MensajesMQ;

        private Funcion funcion;
        //private MqSeries mqSeries;
        private MqMonitorTicket monitorTicket;

        public FrmMonitor()
        {
            //Inicializando variables-----------
            funcion = new Funcion();
            //mqSeries = new MqSeries(); //Solo se podran usar mnetodos que hereden de esta clase
            monitorTicket = new MqMonitorTicket();
            //----------------------------------
            InitializeComponent();
        }


        private void  GuardarLog()
        {
            funcion.Escribe("El siguiente reporte se genera a partir del botón 'Guardar el Registro de Operaciones' o cuando ha cambiado el dia de monitoreo o cuando se ha pulsado el botón 'Salir' del Monitor.");
            funcion.Escribe("---------  Reporte del estado de los procesos  ---------");
            funcion.Escribe("*********  Registro de operaciones procesadas  *********");
            funcion.Escribe("   Respuestas (HOST->NT) registradas proceso de Monitoreo");
            //'Escribe "       > Duración del CICLO[seg] : " & IntRecepResMonitor
            funcion.Escribe("   Solicitudes (NT->HOST) registradas proceso de Funcionarios y Autorizaciones");
            //'Escribe( "       > Duración del CICLO[min] : " & IntEnvioMsgMonitor
            funcion.Escribe("---------  Fin del reporte del estado de los procesos  ---------");
            funcion.Escribe("");
        }

        private bool ResetMonitor()
        {
            bool reset_monitor = false;

            try
            {
                funcion.Escribe ("Respaldo del estado del monitor " + funcion.ObtenFechaFormato(1));

                GuardarLog();

                if(monitorTicket.QUEUE != null)
                {
                    if(monitorTicket.QUEUE.IsOpen)
                    {
                        monitorTicket.CerrarColaMQ();
                    }
                }

                monitorTicket.DesconectarMQ();

                if(monitorTicket.ConectarMQ(monitorTicket.strMQManager))
                {
                    monitorTicket.blnConectado = true;
                }
                else
                {
                    funcion.Escribe("Falla en Monitor < Error al conectarse con la MQ > : " + funcion.ObtenFechaFormato(1));
                    funcion.Escribe("Detalles : " + monitorTicket.QUEUE.ReasonCode + " " + monitorTicket.QUEUE.ReasonName);
                }


            }
            catch (Exception error)
            {
                txtErrores.Text += error.Message;
                funcion.Escribe("Falla en Monitor < Error al conectarse con la MQ > : " + funcion.ObtenFechaFormato(1));
                funcion.Escribe("Detalles : " + monitorTicket.QMGR.ReasonCode + " " + monitorTicket.QMGR.ReasonName);
                funcion.Escribe(error);
            }

            return reset_monitor;
        }

        private void FrmMonitor_Load(object sender, EventArgs e)
        {
            //strArchivoIni = App.Path & "\MonitorMQTKT.ini"
            
            if(monitorTicket.Inicia())
            {
                bCambioManual = false;
            }

            ModoMonitor = (monitorTicket.intgModoMonitor == 1) ? true : false;
            ActivoProcFuncAuto = (monitorTicket.intgActv_FuncAuto == 1) ? true : false;

            funcion.Escribe("Aplicación Monitor iniciado : " + funcion.ObtenFechaFormato(1));

            CargaInfMonitoreo();

        }


        private void CargaInfMonitoreo()
        {
            miTotalMonitor = Int32.Parse(funcion.getValueAppConfig("PMONITOREOS"));
        }

        private void FrmMonitor_FormClosing(object sender, FormClosingEventArgs e)
        {
            Detener();

            if(monitorTicket.QUEUE != null)
            {
                if (monitorTicket.QUEUE.IsOpen) monitorTicket.CerrarColaMQ();
            }

            monitorTicket.DesconectarMQ();

            monitorTicket.QUEUE = null;
            monitorTicket.QMGR = null;
        }

        private void Detener()
        {
            try
            {
                if(monitorTicket.QUEUE != null)
                {
                    if (monitorTicket.QUEUE.IsOpen) monitorTicket.CerrarColaMQ();
                }

                if(monitorTicket.DesconectarMQ())
                {
                    monitorTicket.blnConectado = false;
                }
            }
            catch(MQException ex)
            {
                funcion.Escribe(ex);
                funcion.Escribe("" + monitorTicket.QUEUE.ReasonCode + " " + monitorTicket.QUEUE.ReasonName);
                funcion.Escribe("Falla en Monitor < Falla en el cierre de MQ-Series > : " + funcion.ObtenFechaFormato(1));
            }
            catch (Exception ex)
            {
                funcion.Escribe(ex);
            }
        }

        private void Iniciar()
        {
            int i_row;

            try
            {
                if(monitorTicket.ConectarMQ(monitorTicket.strMQManager))
                {
                    monitorTicket.blnConectado = true;
                }
                else
                {
                    funcion.Escribe("Falla en Monitor < Falla en el cierre de MQ-Series > : " + funcion.ObtenFechaFormato(1)); ;
                    funcion.Escribe("" + monitorTicket.QUEUE.ReasonCode + " " + monitorTicket.QUEUE.ReasonName);
                }

                if(ModoMonitor ==  true)
                {
                    funcion.Escribe("Monitor iniciado en modo de monitoreo: " + funcion.ObtenFechaFormato(1));

                    if(!ActivoProcFuncAuto)
                    {
                        funcion.Escribe("El procesos de Funcionarios-Autorizaciones se encuentra en estado inactivo");
                    }
                    else
                    {
                        funcion.Escribe("El procesos de Funcionarios-Autorizaciones se encuentra en estado activo");
                    }
                }
                else
                {
                    funcion.Escribe("Monitor iniciado en modo de procesamiento: " + funcion.ObtenFechaFormato(1));
                }

                tmrRestar.Enabled = true;
                tmrRestar.Interval = monitorTicket.intgtmrRestar * 1000;

                TimeSpan Diff_dates = Convert.ToDateTime(monitorTicket.FechaRestar).Subtract(monitorTicket.date);
                if (Diff_dates.Days != 1)
                {
                    monitorTicket.FechaRestar = monitorTicket.date.AddDays(1).ToString();
                }
                funcion.SetParameterAppSettings("RestarMonitor", monitorTicket.FechaRestar);
            }
            catch (Exception ex)
            {
                funcion.Escribe("Error en la conexion con el servidor MQ: " + monitorTicket.QUEUE.ReasonCode + " " + monitorTicket.QUEUE.ReasonName);
                funcion.Escribe(ex);
            }
        }

        private void tmrRestar_Tick(object sender, EventArgs e)
        {
            tmrRestar.Enabled = false;

            funcion.Escribe("****************************************************");
            funcion.Escribe("****************************************************");
            funcion.Escribe("Ejecución del Timer tmrRestar : " + monitorTicket.currentDate.ToString());
            funcion.Escribe("valor de Modfunciones.date: " + monitorTicket.date.ToString());
            funcion.Escribe("valor de ModFunciones.FechaRestar: " + monitorTicket.FechaRestar.ToString());
            funcion.Escribe("valor de Convert.ToDateTime(ModFunciones.FechaRestar: " + Convert.ToDateTime(monitorTicket.FechaRestar.ToString()));
            funcion.Escribe("****************************************************");
            funcion.Escribe("****************************************************");
            if (monitorTicket.date > Convert.ToDateTime(monitorTicket.FechaRestar))
            {
                ResetMonitor();

                monitorTicket.FechaRestar = monitorTicket.date.ToString();

                funcion.SetParameterAppSettings("RestarMonitor", monitorTicket.FechaRestar);

                funcion.Escribe("Aplicación Monitor iniciado : " + monitorTicket.currentDate, "Mensaje");
                funcion.Escribe("Monitor iniciado en modo de procesamiento: " + monitorTicket.currentDate, "Mensaje");
            }

            tmrRestar.Enabled = true;
        }

        private void tmrMonitorMQTKT_Tick(object sender, EventArgs e)
        {
            monitorTicket.dblCiclosBitacoras += 10;
            monitorTicket.dblCiclosTKTMQ += 10;
            monitorTicket.dblCiclosFuncionarios += 10;
            monitorTicket.dblCiclosAutorizaciones += 10;

            funcion.Escribe("Ejecución del Timer tmrMonitorMQTKT_Tick : " + monitorTicket.currentDate);

            funcion.Escribe("Valor de intgMonitor : " + monitorTicket.intgMonitor);
            funcion.Escribe("Valor de strFormatoTiempoBitacoras : " + monitorTicket.strFormatoTiempoBitacoras);
            funcion.Escribe("valor de dblCiclosBitacoras : " + monitorTicket.dblCiclosBitacoras);
            funcion.Escribe("Valor de intTiempoBitacoras : " + monitorTicket.intTiempoBitacoras);

            if (monitorTicket.intgMonitor == 1)
            {
                if (monitorTicket.strFormatoTiempoBitacoras != "S")
                {
                    if (monitorTicket.dblCiclosBitacoras >= (monitorTicket.intTiempoBitacoras * 60))
                    {

                        funcion.Escribe("Ejecutando if TmrBitacora()... ");
                        TmrBitacora();
                        monitorTicket.dblCiclosBitacoras = 0;
                    }
                }
                else
                {
                    if (monitorTicket.dblCiclosBitacoras >= monitorTicket.intTiempoBitacoras)
                    {

                        funcion.Escribe("Ejecutando else TmrBitacora()... ");
                        TmrBitacora();
                        monitorTicket.dblCiclosBitacoras = 0;
                    }
                }
            }
        }

        private void TmrBitacora()
        {
            double Ld_CodigoExecNTHOST;
            string EjecutableMSG;
            int icont;
            string Ejecutable;
            List<string> Parametro = new List<string>(); ;
            int intlBitacoras;
            string sValor;
            string[] vntBitacora;

            for (intlBitacoras = 1; intlBitacoras < miTotalMonitor; intlBitacoras++)
            {
                sValor = funcion.getValueAppConfig("PARAMETRO" + intlBitacoras);
                vntBitacora = sValor.Split(',');

                if(Int32.Parse(vntBitacora[0]) == 1)
                {
                    Parametro.AddRange(funcion.getValueAppConfig(vntBitacora[1]).Split(','));
                    Ejecutable = Parametro[0];

                    if(Ejecutable == "M")
                    {
                        //Dim MensajesMQ As Object
                        //Set MensajesMQ = CreateObject("MensajesMQ.cMensajes")
                    }
                    else
                    {
                        //Dim MensajesMQ As Object
                        //Set MensajesMQ = CreateObject("MensajesMQ.cMensajes")
                    }
                }
            }
        }

    }
}
