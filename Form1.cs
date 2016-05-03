using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Data.SqlClient;
using System.Web;


namespace Descrizioni_Tabelle
{
    public partial class Form1 : Form
    {
        public enum ComboState  {enabled,disabled};
        public ComboState comboBox3State;
        string server1 = "srvexperience\\SQL2008";
        string server2 = "localhost";
        public static string database1 = "";
        public static string database2 = "";
        SqlConnection connessione1;
        SqlConnection connessione2;
        public static int MAXJOINS = 10;
        string username = "sa";
        string password = "sa1";
        DataTable dt1, dt2;
        public Form1()
        {
            InitializeComponent();
            textBoxServer1.Text = server1;
            textBoxServer2.Text = server2;
            comboBox3.DrawMode = DrawMode.OwnerDrawFixed;
            comboBox3.DrawItem += new DrawItemEventHandler(comboBox3_DrawItem);
            /* mostro il form del loading */
            Loading load = new Loading();
            load.Show(this);
            creaConnessioni();
            comboBox1.DropDownStyle = ComboBoxStyle.DropDownList;
            comboBox2.DropDownStyle = ComboBoxStyle.DropDownList;
            comboBox3.DropDownStyle = ComboBoxStyle.DropDownList;
            //seleziono quello che mi serve
            for (int i = 0; i < comboBox1.Items.Count; i++)
            {
                if (((String)(comboBox1.Items[i])).Equals("EVO_4S_DB"))
                    comboBox1.SelectedIndex = i;
            }
            for (int i = 0; i < comboBox2.Items.Count; i++)
            {
                if (((String)(comboBox2.Items[i])).Equals("4SNE_Krn"))
                    comboBox2.SelectedIndex = i;
            }
            caricaTabelle();
            load.Hide();
            comboBox3State = ComboState.disabled;
        }
        public void creaConnessioni()
        {
            connessione1 = new SqlConnection("Server=" + server1 + ";" +
                                             "Database=" + database1 + ";" +
                                             "User Id=" + username + ";" +
                                             "Password=" + password + ";");
            connessione2 = new SqlConnection("Server=" + server2 + ";" +
                                             "Database=" + database2 + ";" +
                                             "User Id=" + username + ";" +
                                             "Password=" + password + ";");
            try
            {
                connessione1.Open();
                connessione2.Open();
                dt1 = connessione1.GetSchema("Databases");
                dt2 = connessione2.GetSchema("Databases");
                foreach (DataRow row in dt1.Rows)
                    comboBox1.Items.Add(row["Database_name"]);
                foreach (DataRow row in dt2.Rows)
                    comboBox2.Items.Add(row["Database_name"]);
            }
            catch (Exception e)
            {
                MessageBox.Show(e.ToString(), "Errore", MessageBoxButtons.OK, MessageBoxIcon.Error);
                Application.Exit();
            }
        }
        public void caricaTabelle()
        {
            comboBox3.Items.Clear();
            connessione1.Close();
            connessione1.ConnectionString = "Server=" + server1 + ";" +
                                            "Database=" + comboBox1.SelectedItem + ";" +
                                            "User Id=" + username + ";" +
                                            "Password=" + password + ";";
            connessione1.Open();
            DataTable dataTable = connessione1.GetSchema("Tables");
            foreach (DataRow row in dataTable.Rows)
                comboBox3.Items.Add(row["TABLE_NAME"]);
            comboBox3.Sorted = true;
            try
            {
                comboBox3.SelectedIndex = 0;
            }
            catch (Exception)
            {
                
                
            }
            connessione2.Close();
            connessione2.ConnectionString = "Server=" + server2 + ";" +
                                            "Database=" + comboBox2.SelectedItem + ";" +
                                            "User Id=" + username + ";" +
                                            "Password=" + password + ";";
            connessione2.Open();

        }

        private static String[] getStringDescrizioneCampi(String stringa)
        {
            String[] ritorno = System.Text.RegularExpressions.Regex.Split(stringa, "&amp;");
            return ritorno;
        }

        private static String creaStringaDaMemorizzare(String textbox)
        {
            string ritorno = "";
            for (int i = 0; i < textbox.Length; i++)
                if (textbox[i] == ']')
                {
                    i += 3;
                    while (textbox[i] != '\n')
                    {
                        ritorno += textbox[i];
                        i++;
                    }
                    ritorno += "&amp;";
                }
            return ritorno;
        }


        private void comboBox3_SelectedIndexChanged(object sender, EventArgs e)
        {
 
            SqlCommand cmd;
            SqlDataReader reader;

            /*carico il nuovo contenuto della descrizione */
            cmd = new SqlCommand("SELECT * FROM TableDescriptions WHERE TableName ='" + comboBox3.SelectedItem + "'", connessione2);
            reader = null;
            try
            {
                reader = cmd.ExecuteReader();
                if (reader.Read())
                {
                    richTextBox2.Text = "";
                    richTextBox1.Text = HttpUtility.HtmlDecode(reader["Descrizion"].ToString());
                    if (reader["DescCampi"].ToString().Equals("") || reader["DescCampi"].ToString().Equals(null))  //creo il campo per la prima volta
                    {
                        SqlCommand command = new SqlCommand("SELECT * FROM " + comboBox3.SelectedItem + " WHERE 1=0", connessione1);
                        SqlDataReader lettore = command.ExecuteReader();
                        DataTable dataTavolo = new DataTable();
                        dataTavolo.Load(lettore);
                        int numColonne = dataTavolo.Columns.Count;
                        for (int i = 0; i < numColonne; i++)
                            richTextBox2.Text += "[" + dataTavolo.Columns[i].ColumnName + "]: \n";
                        reader.Close();
                    }
                    else    //il campo è già creato e lo leggo
                    {
                        SqlCommand command = new SqlCommand("SELECT * FROM " + comboBox3.SelectedItem + " WHERE 1=0", connessione1);
                        SqlDataReader lettore = command.ExecuteReader();
                        DataTable dataTavolo = new DataTable();
                        if (reader.HasRows)
                            dataTavolo.Load(lettore);
                        int numColonne = dataTavolo.Columns.Count;
                        String[] stringhe = getStringDescrizioneCampi(reader["DescCampi"].ToString());
                        for (int i = 0; i < numColonne; i++)
                        {
                            richTextBox2.Text += "[" + dataTavolo.Columns[i].ColumnName + "]: ";
                            try
                            {
                                richTextBox2.Text += stringhe[i];
                            }
                            catch
                            {
                            }
                            richTextBox2.Text += "\n";
                        }
                        reader.Close();
                    }                    
                }
                else
                {
                    richTextBox1.Text = "";
                    richTextBox2.Text = "";
                    SqlCommand command = new SqlCommand("SELECT * FROM " + comboBox3.SelectedItem + " WHERE 1=0", connessione1);
                    SqlDataReader lettore = command.ExecuteReader();
                    DataTable dataTavolo = new DataTable();
                    dataTavolo.Load(lettore);
                    int numColonne = dataTavolo.Columns.Count;
                    for (int i = 0; i < numColonne; i++)
                        richTextBox2.Text += "[" + dataTavolo.Columns[i].ColumnName + "]: \n";
                    reader.Close();
                }
                    
                reader.Close();
            }
            catch (Exception ex)
            {
                
            }
            comboBox3State = ComboState.enabled;
        }

        private void comboBox3_DropDown(object sender, EventArgs e)
        {
            if (comboBox3State == ComboState.disabled)
                return;  /*controllo preliminare
                  
            /* salvo il vecchio contenuto della richtextbox */
            /* prima vedo se esiste già */
            
            SqlCommand cmd = new SqlCommand("SELECT * FROM TableDescriptions WHERE TableName='" + comboBox3.SelectedItem + "'", connessione2);
            SqlDataReader reader = null;
            try
            {
                reader = cmd.ExecuteReader();
                if (reader.Read())
                {  //Se la tabella è già inserita
                    cmd = new SqlCommand("UPDATE TableDescriptions SET Descrizion='" + 
                                           HttpUtility.HtmlEncode(richTextBox1.Text) + 
                                           "',DescCampi='"+creaStringaDaMemorizzare(richTextBox2.Text)+"' WHERE TableName='" + reader["TableName"] + "'", connessione2);
                    reader.Close();
                    reader = cmd.ExecuteReader();
                    reader.Close();
                }
                else
                { //altrimenti la creo
                    //ottengo prima il progressivo id
                    cmd = new SqlCommand("SELECT MAX(id) AS Id FROM TableDescriptions",connessione2);
                    reader.Close();
                    reader = cmd.ExecuteReader();
                    reader.Read();
                    int ID;
                    try
                    {
                        ID = Int32.Parse(reader["Id"].ToString());
                    }
                    catch
                    {
                        ID = 0;
                    }
                    
                    reader.Close();
                    ID++;
                    cmd = new SqlCommand("INSERT INTO TableDescriptions(Id,TableName,Descrizion,DescCampi) VALUES ('" 
                                        + ID + "','" + comboBox3.SelectedItem + 
                                        "','" + HttpUtility.HtmlEncode(richTextBox1.Text) + "','"+creaStringaDaMemorizzare(richTextBox2.Text)+"')",connessione2);
                    reader = cmd.ExecuteReader();
                    reader.Close();
                }
            }
            catch (SqlException ex)
            {
                MessageBox.Show(ex.Message+" "+cmd.CommandText);
                reader.Close();
            }
            reader.Close();
        }
        private void comboBox3_DrawItem(object sender, DrawItemEventArgs e) {    

            Font font = comboBox3.Font;
            Brush brush = Brushes.Black;
            string text = null;
            try
            {
                text = comboBox3.Items[e.Index].ToString();
            }
            catch
            {
            }
            
            SqlCommand cmd = new SqlCommand("SELECT * FROM TableDescriptions WHERE TableName='" + text + "'", connessione2);
            SqlDataReader reader = null;
            reader = cmd.ExecuteReader();
            if (reader.Read()) //se esiste allora lo metto in grassetto
            {
                font = new Font(font, FontStyle.Bold);
                e.Graphics.DrawString(text, font, brush, e.Bounds);
            }
            else 
                e.Graphics.DrawString(text, font, Brushes.Gray, e.Bounds);    
            reader.Close();
            
        }

        private void buttonServer_Click(object sender, EventArgs e)
        {
            server1 = textBoxServer1.Text;
            server2 = textBoxServer2.Text;
            creaConnessioni();
            caricaTabelle();
        }

        private void comboBox1_SelectedIndexChanged(object sender, EventArgs e)
        {
            caricaTabelle();
        }

        private void buttonElimina_Click(object sender, EventArgs e)
        {
            SqlCommand cmd = new SqlCommand("SELECT * FROM TableDescriptions WHERE TableName='" + comboBox3.SelectedItem + "'", connessione2);
            SqlDataReader reader = null;
            try
            {
                reader = cmd.ExecuteReader();
                if (reader.Read())
                {  //Se la tabella è già inserita
                    if (!richTextBox1.Text.Equals(""))
                    {
                        if (MessageBox.Show("Sei sicuro di volere eliminare queste informazioni?", "Conferma eliminazione", MessageBoxButtons.YesNo, MessageBoxIcon.Question)==DialogResult.No)
                            return;
                    }
                        
                    cmd = new SqlCommand("DELETE FROM TableDescriptions WHERE TableName='" + reader["TableName"] + "'", connessione2);
                    reader.Close();
                    reader = cmd.ExecuteReader();
                    reader.Close();                    
                }
            }
            catch (SqlException ex)
            {

            }
            reader.Close();
            comboBox3.Show();
            comboBox3State = ComboState.disabled;
            richTextBox1.Text = "";
            richTextBox2.Text = "";
        }
    }
}
