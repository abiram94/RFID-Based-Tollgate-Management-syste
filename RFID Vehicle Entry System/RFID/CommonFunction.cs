using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Data;
using System.Data.SqlClient;
using System.Windows.Forms;
using System.Configuration;
using System.Transactions;
using System.Data.SqlServerCe;



namespace RFID
{
    class CommonFunction
    {
        public string con = ConfigurationManager.ConnectionStrings["RFID"].ToString();
        public SqlConnection cn = new SqlConnection();


        //public SqlCeConnection sqlcn;

        //public CommonFunction()
        //{

        //    // Local Database attached with Project 
        //    sqlcn = new SqlCeConnection("Data Source = |DataDirectory|\\RFID.sdf");
        //    sqlcn.Open();


        //    // SQL Server 
        //    //sqlcn1 = new SqlConnection("Data Source= KARTHIGA\\SQLEXPRESS;Initial Catalog=TestDB; Integrated Security = true; Connection Timeout=230"); 
        //    //sqlcn1.Open();

        //    //// LocalDB\\MSSQLLocalDB
        //    //sqlcn1 = new SqlConnection("Data Source=(localdb)\\MSSQLLocalDB;AttachDbFilename=|DataDirectory|TestDB.mdf;Integrated Security=True");
        //    //sqlcn1.Open();
           
        //}
        public CommonFunction()
        {
            cn.ConnectionString = con;
            cn.Open();
        }

        public string Connection()
        {
            return con;
        }

        public DataTable ReturnDataTable(string sql)
        {
            SqlCommand cmd = new SqlCommand(sql, cn);
            SqlDataAdapter ada = new SqlDataAdapter(cmd);
            DataTable dt = new DataTable();
            ada.Fill(dt);
            return dt;
        }
        public SqlDataReader ReturnDataReader(string sql)
        {
            SqlCommand cmd = new SqlCommand(sql, cn);
            cmd.CommandTimeout = 9999999;
            SqlDataReader sda = cmd.ExecuteReader();
            return sda;
        }

        //public SqlCeDataReader ReturnsqlDatareader(string sql)
        //{
        //    SqlCeCommand sqlcmd = new SqlCeCommand(sql, sqlcn);
        //    SqlCeDataReader sqldr = sqlcmd.ExecuteReader();
        //    return sqldr;
        //}


        public void ExportToExcelDt(DataTable gridviewID, string excelFilename)
        {
            Microsoft.Office.Interop.Excel.Application objexcelapp = new Microsoft.Office.Interop.Excel.Application();
            objexcelapp.Application.Workbooks.Add(Type.Missing);
            objexcelapp.Columns.ColumnWidth = 25;
            //Microsoft.Office.Interop.Excel.Range chartRange;

            for (int i = 1; i < gridviewID.Columns.Count + 1; i++)
            {
                objexcelapp.Cells[1, i] = gridviewID.Columns[i - 1].ColumnName;
                //objexcelapp.Cells[1, i].Interior.Color = System.Drawing.Color.LightSteelBlue;               
            }
            /*For storing Each row and column value to excel sheet*/
            for (int i = 0; i < gridviewID.Rows.Count; i++)
            {
                for (int j = 0; j < gridviewID.Columns.Count; j++)
                {
                    if (gridviewID.Rows[i][j] != null)
                    {
                        objexcelapp.Cells[i + 2, j + 1] = gridviewID.Rows[i][j].ToString();
                        //objexcelapp.Cells[i + 2, j + 1].HorizontalAlignment = Microsoft.Office.Interop.Excel.XlHAlign.xlHAlignLeft;
                    }
                }
            }
            objexcelapp.Visible = true;
            objexcelapp.Columns.AutoFit();

        }
    }
}
