using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MonitorMQTKT.Data
{
    public class ConexionBD
    {

        #region FIELDS

        private SqlConnection sqlCnn;
        private CommandType commandType = CommandType.StoredProcedure;
        private SqlCommand cmd;
        private SqlDataReader dReader;
        private SqlParameter[] parametersArray;
        private string strCmd;
        private string strCnn;
        private bool parametersContains = false;
        private bool activeReader = false;
        private bool activeConnection = false;

        #endregion

        #region CONSTRUCTOR

        public ConexionBD(string cnn)
        {
            strCnn = cnn;
        }

        #endregion

        #region PROPERTYS

        public bool ActiveConnection
        {
            set
            {
                activeConnection = value;
                if (activeConnection)
                    OpenConnection();
                else
                    CloseConnection();
            }
        }
        public string ConnectionString
        {
            get { return strCnn; }
            set { strCnn = value; }
        }
        public CommandType CommandType
        {
            get { return commandType; }
            set { commandType = value; }
        }
        public bool ParametersContains
        {
            set { parametersContains = value; }
        }

        #endregion

        #region METHODS

        //Lógica de Conexión
        private void OpenConnection()
        {
            if (sqlCnn == null)
                sqlCnn = new SqlConnection();

            if (sqlCnn.State == ConnectionState.Closed)
            {
                sqlCnn.ConnectionString = strCnn;
                sqlCnn.Open();
            }
        }

        /// <summary>
        /// Método que abre la conexión de BD y abre una nueva transacción
        /// </summary>
        /// <param name="transactionName">Nombre de la transacción que se abrirá</param>
        /// <returns>Objeto SqlTransaction</returns>
        public SqlTransaction OpenTransaction(String transactionName)
        {
            if (sqlCnn == null)
                sqlCnn = new SqlConnection();

            if (sqlCnn.State == ConnectionState.Closed)
            {
                sqlCnn.ConnectionString = strCnn;
                sqlCnn.Open();
            }
            return sqlCnn.BeginTransaction(IsolationLevel.Serializable, transactionName);
        }

        private void CloseConnection()
        {
            if (sqlCnn != null)
            {
                if (activeReader) CloseDataReader();
                if (sqlCnn.State != ConnectionState.Closed)
                {
                    sqlCnn.Close();
                }
            }
        }

        /// <summary>
        /// Método que realiza el commit de la transacción especificada y cierra la conexión
        /// </summary>
        /// <param name="transaction">SqlTransaction sobre el que se realizará el commit</param>
        public void CommitTransaction(SqlTransaction transaction)
        {
            if (sqlCnn != null)
            {
                if (sqlCnn.State != ConnectionState.Closed)
                {
                    transaction.Commit();
                    sqlCnn.Close();
                }
            }
        }

        /// <summary>
        /// Método que sirve para realizar un rollback sobre una transacción especificada
        /// </summary>
        /// <param name="transaction">transacción sobre la que se llevará acabo el rollback</param>
        public void RollbackTransaction(SqlTransaction transaction)
        {
            if (sqlCnn != null)
            {
                if (sqlCnn.State != ConnectionState.Closed)
                {
                    transaction.Rollback();
                    sqlCnn.Close();
                }
            }
        }

        //Lógica de comandos
        public void CloseDataReader()
        {
            if (dReader != null)
            {
                if (!dReader.IsClosed)
                    dReader.Close();
                dReader.Dispose();
                activeReader = false;
            }
        }
        private void PrepareCommand(string strCommand)
        {

            if (cmd == null)
            {
                cmd = new SqlCommand(strCommand, sqlCnn);
            }
            else
            {
                cmd.Connection = sqlCnn;
                cmd.CommandText = strCommand;
            }
            cmd.CommandType = commandType;
            if (parametersContains)
            {
                cmd.Parameters.Clear();
                cmd.Parameters.AddRange(parametersArray);
            }
            else
                cmd.Parameters.Clear();
        }

        /// <summary>
        /// Método que prepara el comando que será ejecutado sobre una transacción
        /// </summary>
        /// <param name="strCommand">comando</param>
        /// <param name="transaction">transacción</param>
        private void PrepareCommand(string strCommand, SqlTransaction transaction)
        {

            if (cmd == null)
            {
                cmd = new SqlCommand(strCommand, sqlCnn, transaction);
            }
            else
            {
                cmd.Connection = sqlCnn;
                cmd.CommandText = strCommand;
            }
            cmd.CommandType = commandType;
            if (parametersContains)
            {
                cmd.Parameters.Clear();
                cmd.Parameters.AddRange(parametersArray);
            }
            else
                cmd.Parameters.Clear();
        }

        //Lógica de parámetros
        public void AddParameters(params SqlParameter[] Parameters)
        {
            parametersArray = Parameters;
            int count = parametersArray.Length;
            if (count > 0)
                parametersContains = true;
            else
                parametersContains = false;
        }
        public SqlParameter CreateParameters(string param, SqlDbType type, ParameterDirection direction, object value)
        {
            SqlParameter p = new SqlParameter();
            p.ParameterName = param;
            p.Direction = direction;
            p.SqlDbType = type;
            if (value == null)
            {
                p.Value = DBNull.Value;
                p.IsNullable = true;
            }
            else
                p.Value = value;
            return p;
        }
        public Object GetOutputParam(string parameterId)
        {
            if ((cmd != null) && (cmd.Parameters.Contains(parameterId)))
            {
                return cmd.Parameters[parameterId].Value;
            }
            else
                throw new DbHelperException("Parametro o Commando requerido inexistente.", new Exception());
        }

        //Ejecución de Comandos

        public DataTable ExecuteDataTable(string strCommand)
        {
            if (!activeConnection) OpenConnection();
            PrepareCommand(strCommand);
            SqlDataAdapter da = new SqlDataAdapter(cmd);
            DataTable dt = new DataTable();
            try
            {
                da.Fill(dt);
            }
            catch (SqlException e)
            {
                if (!activeConnection) CloseConnection();
                throw new DbHelperException("Error llenando DataTable.", e);
            }
            if (!activeConnection) CloseConnection();

            return dt;
        }
        public Object ExecuteScalar(string strCommand)
        {
            Object Value;
            if (!activeConnection) OpenConnection();
            PrepareCommand(strCommand);
            try
            {
                Value = cmd.ExecuteScalar();
            }
            catch (SqlException e)
            {
                if (!activeConnection) CloseConnection();
                throw new DbHelperException("Error ejecutando un Scalar.", e);
            }
            if (!activeConnection) CloseConnection();

            return Value;
        }
        public int ExecuteNonQuery(string strCommand)
        {
            int Value;
            if (!activeConnection) OpenConnection();
            PrepareCommand(strCommand);
            try
            {
                Value = cmd.ExecuteNonQuery();
            }
            catch (SqlException e)
            {
                if (!activeConnection) CloseConnection();
                throw new DbHelperException("Error ejecutando un NonQuery.", e);
            }
            if (!activeConnection) CloseConnection();

            return Value;
        }

        /// <summary>
        /// Sobre carga del método para hacer que se puedan ejecutar SPS sobre una sola transacción administrada en código
        /// </summary>
        /// <param name="strCommand"> cadena de texto que es el comando</param>
        /// <param name="transaction">transacción</param>
        /// <returns>valor de retorno</returns>
        public int ExecuteNonQuery(string strCommand, SqlTransaction transaction)
        {
            int Value;
            //if (!activeConnection) OpenConnection();
            PrepareCommand(strCommand, transaction);
            cmd.Transaction = transaction;
            try
            {
                Value = cmd.ExecuteNonQuery();
            }
            catch (SqlException e)
            {
                RollbackTransaction(transaction);
                //if (!activeConnection) 
                throw new DbHelperException("Error ejecutando un NonQuery.", e);
            }
            //if (!activeConnection) CloseConnection();

            return Value;
        }

        public SqlDataReader ExecuteDataReader(string strCommand)
        {
            if (!activeConnection) throw new DbHelperException("ExecuteDataReader requiere de ActiveConnection = true", new Exception());
            PrepareCommand(strCommand);
            try
            {
                cmd.CommandTimeout = 0;
                dReader = cmd.ExecuteReader();
            }
            catch (SqlException e)
            {
                if (!activeConnection) CloseConnection();
                throw new DbHelperException("Error ejecutando un DataReader.", e);
            }
            activeReader = true;
            strCmd = strCommand;

            return dReader;
        }
        public class DbHelperException : Exception
        {
            private string dbhMessage;

            public DbHelperException(string message, Exception e)
                : base(e.Message, e)
            {
                dbhMessage = message;
            }

            public override string Message
            {
                get
                {
                    return dbhMessage + "\n" + base.Message;
                }
            }
        }

        #endregion

        public class Db_Helper : Exception
        {
            private string strMessage;

            public Db_Helper(string message, Exception e)
                : base(e.Message, e)
            {
                strMessage = message;
            }

            public override string Message
            {
                get
                {
                    return strMessage + "\n" + base.Message;
                }
            }
        }

    }
}
