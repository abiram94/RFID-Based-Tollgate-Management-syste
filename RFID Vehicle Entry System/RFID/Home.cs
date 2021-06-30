using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.Configuration;
using System.Data.SqlClient;
using System.Transactions;
using System.Threading;


namespace RFID
{
    public partial class Home : Form
    {
        string sql;
        SqlDataReader drr;
        CommonFunction db = new CommonFunction();
        SqlTransaction trnsql;
        SqlConnection sqlcn = new SqlConnection();
        string vehiclenumber = "";
        bool inflag = false;
        DataTable dt = new DataTable();
        DataGridView dgvexcel = new DataGridView();

        System.Windows.Forms.Timer timerHideLabel = new System.Windows.Forms.Timer();

        public Home()
        {
            InitializeComponent();
        }

        private void button1_Click(object sender, EventArgs e)
        {
            groupBox1.Visible = true;
        }

        private void button3_Click(object sender, EventArgs e)
        {
            groupBox1.Visible = false;
        }

        private void button2_Click(object sender, EventArgs e)
        {
            savevehicle();
        }

        private void savevehicle()
        {
            if (sqlcn.State == ConnectionState.Closed)
            {
                sqlcn.Open();
            }

            else
            {
                sqlcn.Close();
                sqlcn.Open();
            }

            trnsql = sqlcn.BeginTransaction();

            sql = "SELECT Count (*) FROM RFID_Testmaster  Where RFIDnumber='"+ textBox2.Text.Trim() +"'";
            SqlCommand cmd = new SqlCommand(sql, sqlcn, trnsql);
            int count = Convert.ToInt32(cmd.ExecuteScalar());
            if (count == 0)
            {
                sql = "INSERT INTO RFID_Testmaster (RFIDnumber, number) VALUES (@RFIDnumber, @number)";
            }
            else
            {
                DialogResult result = MessageBox.Show("Already RFID Number Allocated to another vehicle, Doyou want to change ? ", "RFID", MessageBoxButtons.YesNo, MessageBoxIcon.Information);
                if (result == DialogResult.Yes)
                {
                    sql = "UPDATE RFID_Testmaster SET number = @number Where  RFIDnumber = @RFIDnumber";
                }
                else if (result == DialogResult.No)
                {
                    return;
                }
               
            }
            cmd = new SqlCommand(sql);
            cmd.Connection = sqlcn;
            cmd.Transaction = trnsql;
            cmd.Parameters.AddWithValue("@RFIDnumber", Convert.ToInt32(textBox2.Text.ToString()));
            cmd.Parameters.AddWithValue("@number", textBox3.Text.ToString());
            int success = cmd.ExecuteNonQuery();
            if (success > 0)
            {
                trnsql.Commit();
                MessageBox.Show("Vehicle Successfully added");
                textBox2.Text = "";
                textBox3.Text = "";
                groupBox1.Visible = false;
            }
            else
            {
                MessageBox.Show("Problem in adding vehicle");
            }
        }

        private void textBox1_TextChanged(object sender, EventArgs e)
        {
            if (textBox1.Text.Length == 10)
            {
                vehicleentry();
            }
        }

        
        private void findnumber()
        {
            sql = "SELECT number FROM RFID_Testmaster Where RFIDnumber='"+textBox1.Text.Trim()+"'";
            drr = db.ReturnDataReader(sql);
            if (drr.Read())
            {
                vehiclenumber = drr["number"].ToString();
                drr.Close();
            }

            else
            {
                MessageBox.Show("Vehicle Not yet Registered");
                return;
            }

        }

        private void vehicleentry()
        {
            inflag = false;
            findnumber();
            if (sqlcn.State == ConnectionState.Closed)
            {
                sqlcn.Open();
            }

            else
            {
                sqlcn.Close();
                sqlcn.Open();
            }

            trnsql = sqlcn.BeginTransaction();

            sql = "SELECT Count (*) AS count FROM RFID_Testdata  Where RFIDnumber='" + textBox1.Text.Trim() + "' AND dateofentry='" + DateTime.Now.ToShortDateString() +"'";
            SqlCommand cmd = new SqlCommand(sql, sqlcn, trnsql);
            int count = Convert.ToInt32(cmd.ExecuteScalar());
            if (count % 2 == 0)
            {
                inflag = true;
                sql = "INSERT INTO RFID_Testdata (RFIDnumber, dateofentry, entrystatus,entrytime) VALUES (@RFIDnumber, @dateofentry, @entrystatus,@entrytime)";
            }
            else
            {
                sql = "INSERT INTO RFID_Testdata (RFIDnumber, dateofentry, entrystatus,entrytime) VALUES (@RFIDnumber, @dateofentry, @entrystatus,@entrytime)";
                
            }

            cmd = new SqlCommand(sql);
            cmd.Connection = sqlcn;
            cmd.Transaction = trnsql;
            if (inflag == true)
            {
                cmd.Parameters.AddWithValue("@RFIDnumber", Convert.ToInt32(textBox1.Text.ToString()));
                cmd.Parameters.AddWithValue("@dateofentry",Convert.ToDateTime(DateTime.Now.ToShortDateString()));
                cmd.Parameters.AddWithValue("@entrystatus", "IN");
                cmd.Parameters.AddWithValue("@entrytime", Convert.ToDateTime(DateTime.Now.ToString()));
            }
            else
            {
                cmd.Parameters.AddWithValue("@RFIDnumber", Convert.ToInt32(textBox1.Text.ToString()));
                cmd.Parameters.AddWithValue("@dateofentry", Convert.ToDateTime(DateTime.Now.ToShortDateString()));
                cmd.Parameters.AddWithValue("@entrystatus", "OUT");
                cmd.Parameters.AddWithValue("@entrytime", Convert.ToDateTime(DateTime.Now.ToString()));
            
            }
            
            int success = cmd.ExecuteNonQuery();
            if (success > 0)
            {
                trnsql.Commit();
                if (inflag == true)
                {
                    lblstatus.ForeColor = Color.Green;
                    lblstatus.Visible = true;
                    lblstatus.Text = vehiclenumber + " IS IN";
                    timerHideLabel.Interval = 5000; // Five seconds.
                    timerHideLabel.Tick += TimerHideLabel_Tick;
                    timerHideLabel.Start();

                    //lblstatus.Text = "";
                    //textBox1.Text = "";
                    

                }
                else
                {
                    lblstatus.ForeColor = Color.Red;
                    lblstatus.Visible = true;
                    lblstatus.Text = vehiclenumber + " IS OUT";
                    timerHideLabel.Interval = 5000; // Five seconds.
                    timerHideLabel.Tick += TimerHideLabel_Tick;
                    timerHideLabel.Start();
                    //lblstatus.Text = "";
                    //textBox1.Text = "";
                   
                }
                
            }
            else
            {
                MessageBox.Show("Problem in vehicle Entry");
                return;
            }
        }

        private void fillgrid()
        {
            Cursor.Current = Cursors.WaitCursor;
            sql = " SELECT       RFID_Testdata.RFIDnumber AS [RFID Tag Number], RFID_Testmaster.number AS [Vehicle Number], RFID_Testdata.dateofentry AS [Date of Entry], RFID_Testdata.entrystatus AS [Type of Entry],  " +
                    "                          RFID_Testdata.entrytime AS [Time of Entry] " +
                    " FROM            RFID_Testdata INNER JOIN " +
                    "                          RFID_Testmaster ON RFID_Testdata.RFIDnumber = RFID_Testmaster.RFIDnumber Where RFID_Testdata.dateofentry BETWEEN ('" + dateTimePicker1.Text.ToString() + "') AND ('" + dateTimePicker2.Text.ToString() + "') Order by RFID_Testdata.entrytime DESC ";
            dt = db.ReturnDataTable(sql);
            dgvexcel.DataSource = dt;
        }

        private void button4_Click(object sender, EventArgs e)
        {
            groupBox2.Visible = true;

           
        }

        private void button5_Click(object sender, EventArgs e)
        {
            groupBox2.Visible = false;
        }

        private void button6_Click(object sender, EventArgs e)
        {
            Cursor.Current = Cursors.WaitCursor;
            fillgrid();
            db.ExportToExcelDt(dt, "Vehicle Movement");
            groupBox2.Visible = false;
        }

        private void TimerHideLabel_Tick(object sender, EventArgs e)
        {
            lblstatus.Text = "";
            textBox1.Text = "";
            timerHideLabel.Stop();
        }

        private void Home_Load(object sender, EventArgs e)
        {
            sqlcn.ConnectionString = db.Connection();
            sqlcn.Open();
            lblstatus.Visible = false;
            groupBox2.Visible = false;
        }
        



    }  

    
}
