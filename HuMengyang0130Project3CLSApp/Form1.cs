using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Xml;

namespace HuMengyang0130Project3CLSApp
{
    public partial class frmCreative : Form
    {
        private Icon m_ready = new Icon(SystemIcons.WinLogo,40,40);
        private Icon m_info = new Icon(SystemIcons.Information, 40, 40);
        private Icon m_error = new Icon(SystemIcons.Error, 40, 40);

        public frmCreative()
        {
            InitializeComponent();
        }

        private void tabPage1_Click(object sender, EventArgs e)
        {

        }

        private void frmCreative_Load(object sender, EventArgs e)
        {
            txtSource.Text = "D:\\Creative\\Source\\";
            txtProcessedFile.Text = "D:\\Creative\\Processed\\";
            txtDest.Text = "D:\\Creative\\Destination\\";
            opsGenerateLog.Checked = true;
        }

        private void btnOk_Click(object sender, EventArgs e)
        {
            //source text box validation
            if (!Directory.Exists(txtSource.Text))
            {
                errorMessage.SetError(txtSource, "Invalid Source Directory");
                txtSource.Focus();
                tabControl1.SelectedTab = tabSource;
                return;
            }
            else
                errorMessage.SetError(txtSource, "");

            //processedFile text box validation
            if (!Directory.Exists(txtProcessedFile.Text))
            {
                errorMessage.SetError(txtProcessedFile, "Invalid Processed File Directory");
                txtProcessedFile.Focus();
                tabControl1.SelectedTab = tabSource;
                return;
            }
            else
                errorMessage.SetError(txtProcessedFile, "");

            //destination test box validation
            if (!Directory.Exists(txtDest.Text))
            {
                errorMessage.SetError(txtDest, "Invalid Destination Directory");
                txtDest.Focus();
                tabControl1.SelectedTab = tabDest;
                return;
            }
            else
                errorMessage.SetError(txtDest, "");

            //end validation

            //activate watching directory
            watchDir.EnableRaisingEvents = true;
            watchDir.Path = txtSource.Text;

            //code for notifycation
            mnuNotify.Icon = m_ready;
            mnuNotify.Visible = true;
            this.ShowInTaskbar = false;
            this.Hide();
        }

        private void txtSource_KeyUp(object sender, KeyEventArgs e)
        {
            if (Directory.Exists(txtSource.Text))
            {
                txtSource.BackColor = Color.White;
            }
            else
            { 
                txtSource.BackColor= Color.Pink;
            }

          

        }

        private void txtProcessedFile_KeyUp(object sender, KeyEventArgs e)
        {
            if (Directory.Exists(txtProcessedFile.Text))
            {
                txtProcessedFile.BackColor = Color.White;
            }
            else
            {
                txtProcessedFile.BackColor = Color.Pink;
            }
        }

        private void txtDest_KeyUp(object sender, KeyEventArgs e)
        {

            if (Directory.Exists(txtDest.Text))
            {
                txtDest.BackColor = Color.White;
            }
            else
            {
                txtDest.BackColor = Color.Pink;
            }
        }

      

        private void mnuConfigure_Click(object sender, EventArgs e)
        {
            mnuNotify.Visible= false;
            this.ShowInTaskbar= true;
            this.Show();
        }

        private void mnuExit_Click(object sender, EventArgs e)
        {
            Application.Exit();
        }

        private void mnuNotify_DoubleClick(object sender, EventArgs e)
        {
            mnuNotify.Visible = false;
            this.ShowInTaskbar = true;
            this.Show();
        }

        private void watchDir_Created(object sender, FileSystemEventArgs e)
        {
            watchDir.EnableRaisingEvents = false;
            mnuNotify.Icon = m_info;
            mnuNotify.Text = "Processed: " + e.Name;
            //to access the Word application
            Microsoft.Office.Interop.Word.Application wdApp = new Microsoft.Office.Interop.Word.Application();
            object optional = System.Reflection.Missing.Value;
            //writrer for XML
            XmlTextWriter xmlTextWriter = XmlTextWriter(txtDest.Text + "summary",null);
            try
            {
                Microsoft.Office.Interop.Word.Document doc = new Microsoft.Office.Interop.Word.Document();
                object filename = e.Name;
                doc = wdApp.Documents.Open(ref filename, ref optional, ref optional, ref optional, ref optional, ref optional, ref optional,
                    ref optional, ref optional, ref optional, ref optional);
                Microsoft.Office.Interop.Word.Range wdRange;
                wdRange = doc.Paragraphs[2].Range;

                string strMemo, strAmount;
                int intParacount;
                strMemo = wdRange.Text;
                strMemo = strMemo.Substring(15, 4);
                intParacount = doc.Paragraphs.Count;
                intParacount = intParacount - 2;

                wdRange = doc.Paragraphs[intParacount].Range;
                Object count = -1;
                Object wdCharecter = "1";

                wdRange.MoveEnd(ref wdCharecter, ref count);
                strAmount = wdRange.Text;

                strAmount = strAmount.Substring(23);
                //write xml
                xmlTextWriter.Formatting = Formatting.Indented;
                xmlTextWriter.WriteDocType("Sales", null, null, null);
                xmlTextWriter.WriteComment("Summary od sales at Creative Learning");
                xmlTextWriter.WriteStartElement("Sales");
                xmlTextWriter.WriteStartElement(Convert.ToString(DateTime.Today));
                xmlTextWriter.WriteElementString("Memo", strMemo);
                xmlTextWriter.WriteElementString("Amount", strAmount);
                xmlTextWriter.WriteEndElement();
                xmlTextWriter.WriteEndElement();
                mnuNotify.Icon = m_ready;
            }
            catch (Exception ex)
            {
                mnuNotify.Icon = m_error;
                mnuNotify.Text = "Error in " + e.Name;
                if (opsGenerateLog.Checked == true)
                {
                    eventLog1.WriteEntry(e.Name + ": " + ex.Message);
                }
            }
            finally { 
                xmlTextWriter.Flush();
                xmlTextWriter.Close();
                wdApp.Quit(ref optional,ref optional,ref optional);
                wdApp = null;
                watchDir.EnableRaisingEvents = true;
            }
        tryAgain:
            try
            {
                File.Move(e.FullPath, txtProcessedFile.Text + e.Name);
            }
            catch {
                goto tryAgain;
            }
        }

        private XmlTextWriter XmlTextWriter(string v, object p)
        {
            throw new NotImplementedException();
        }

        private void btnRefresh_Click(object sender, EventArgs e)
        {
            lstEvents.Items.Clear();
            eventLog1.Log = "Application";
            eventLog1.MachineName = ".";
            foreach (EventLogEntry logEntry in eventLog1.Entries)
            {
                if (logEntry.Source == "CreativeLearning") { 
                lstEvents.Items.Add(logEntry.Message);
                }

            }
        }

        private void btnViewSummary_Click(object sender, EventArgs e)
        {
            StreamReader strRead;
            try
            {
                strRead = new StreamReader(txtDest.Text + "Summary.xml");
                MessageBox.Show(strRead.ReadToEnd(), txtDest.Text + "Summary", MessageBoxButtons.OK,
                    MessageBoxIcon.Information);
                strRead.Close();
            }
            catch (Exception ex)
            {
                MessageBox.Show("An Error was returned : "+ex.Message+" Please check the destination folder for summary");
            }
        }
    }
}
