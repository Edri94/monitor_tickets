using System.Windows.Forms;
using MonitorMQTKT.Funciones;
using System;

namespace MonitorMQTKT
{
    public partial class FrmMonitor : Form
    {
        bool ErrorEjecucion;
        bool ActivoProcFuncAuto;        // Variable para determinar si se desea ejecutar el proceso del Monitoreo
        bool ModoMonitor;               // Variable para determinar el modo de operacion del monitor
        bool DetallesShow;              // Variable para determinar el status de los detalles
        bool bCambioManual;              

        // ***** Para realizar el monitoreo de bitacoras
        int miTipoMonitoreo;
        int miTotalMonitor;
        int ItemProceso;
        int ItemSeleccion;

        double MensajesMQ;

        Funcion funcion;
        MqSeries mqSeries;

        public FrmMonitor()
        {
            //Inicializando variables-----------
            funcion = new Funcion();
            mqSeries = new MqSeries();
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

                if(mqSeries.QUEUE != null)
                {
                    if(mqSeries.QUEUE.IsOpen)
                    {
                        mqSeries.CerrarColaMQ();
                    }
                }

                mqSeries.DesconectarMQ();

                if(mqSeries.ConectarMQ(mqSeries.strMQManager))
                {
                    mqSeries.blnConectado = true;
                }
                else
                {
                    funcion.Escribe("Falla en Monitor < Error al conectarse con la MQ > : " + funcion.ObtenFechaFormato(1));
                    funcion.Escribe("Detalles : " + mqSeries.QUEUE.ReasonCode + " " + mqSeries.QUEUE.ReasonName);
                }


            }
            catch (Exception error)
            {
                txtErrores.Text += error.Message;
                funcion.Escribe(error);
            }

            return reset_monitor;
        }

        
    }
}
