using Oracle.ManagedDataAccess.Client;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ConsoleApplication1
{
    class Program
    {
        private static OracleConnection conn;
        private static OracleCommand cmd;
        private static OracleDataAdapter da;
        private static DataSet ds;

        public Program()
        {
            //
            // TODO: Add constructor logic 
            //
        }

        public static string NewConnection()
        {
            try
            {
                //string oradb = " Data Source = XE; User Id = hr; Password = hr; ";
                string oradb = "Data Source=(DESCRIPTION="
+ "(ADDRESS_LIST=(ADDRESS=(PROTOCOL=TCP)(HOST=10.194.15.187)(PORT=1521)))"
+ "(CONNECT_DATA=(SERVER=DEDICATED)(SERVICE_NAME=XE)));"
+ "User Id=tc_profiler;Password=tc_profiler;";
                conn = new OracleConnection(oradb);
                conn.Open();
            }
            catch (OracleException e)
            {
                return e.Message;
            }

            return conn.State.ToString();
        }

        public static DataTable GetEmployees()
        {
            string SQL = " SELECT TNAME, TLOC FROM TESTCASE WHERE TNAME like 'a%'";
            cmd = new OracleCommand(SQL, conn);
            cmd.CommandType = CommandType.Text;
            da = new OracleDataAdapter(cmd);
            ds = new DataSet();

            da.Fill(ds);
            return ds.Tables[0];
        }

        public static void Terminate()
        {
            conn.Close();
        }

        static void Main(string[] args)
        {
            string strConn = Program.NewConnection();
            DataTable emp = Program.GetEmployees();
            for (int i = 0; i < emp.Rows.Count; i++)
            {
                DataRow row = emp.Rows[i];
                //Print first name and last name
                Console.WriteLine(row["TNAME"]+ "\t\t " + row["TLOC"]);
            }
            Program.Terminate();

            Console.Read();
        }

    }
}
