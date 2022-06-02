using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace ImageCarousel
{
    public partial class frmMain : Form
    {
        private List<FileInfo> files = new();
        bool started = false;
        int timerInterval = 0;
        int timerRest = 0;
        int imgCount = 0;
        int imgCounter = 0;
        string folder = "";
        int stopAfter = -1;
        public frmMain()
        {
            InitializeComponent();
            CreateIntervals();
            CreateStopAfterList();
        }

        private void CreateIntervals()
        {
            List<Interval> intervals = new List<Interval>();
            intervals.Add(new ("30 Seconds", 30));
            intervals.Add(new("45 Seconds", 30));
            intervals.Add(new("1 Minute", 60));
            intervals.Add(new("1.5 Minutes", 90));
            intervals.Add(new("2 Minutes", 120));
            intervals.Add(new("3 Minutes", 180));
            intervals.Add(new("5 Minutes", 300));
            intervals.Add(new("7 Minutes", 420));
            intervals.Add(new("10 Minutes", 600));
            cmbInterval.DataSource = intervals;
            cmbInterval.SelectedIndex = 0;
        }

        private void CreateStopAfterList()
        {
            cmbStopAfter.Items.AddRange(new string[] {"10","20","30","40","all" });
            cmbStopAfter.SelectedIndex = cmbStopAfter.Items.Count - 1;
        }

        private void HandleImages()
        {
            imgCounter += 1;

            if (stopAfter>0 && (imgCounter == stopAfter || imgCounter>stopAfter))
            {
                StopTask();
                MessageBox.Show("You have reached your goal.", "Information", MessageBoxButtons.OK,
                    MessageBoxIcon.Information);
                return;
            }

            if (files.Count == 0)
            {
                StopTask();
                MessageBox.Show("Please select an image folder.", "Information", MessageBoxButtons.OK,
                    MessageBoxIcon.Information);
                return;
            }
            Random random = new Random();
            int i = random.Next(0, files.Count - 1);
            string file = files[i].FullName;

            loadImage(file);


            //files.RemoveAt(i);
        }

        private void loadImage(string file)
        {
            try
            {
                picMain.Load(file);
                picMain.SizeMode = PictureBoxSizeMode.Zoom;
            }
            catch (Exception ex)
            {
                MessageBox.Show("Display error: "+ ex.Message,"Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }

        }

        private void btnSelFolder_Click(object sender, EventArgs e)
        {
            files.Clear();
            folder = "";
            lblActual.Text = "0";
            lblTotal.Text = "0";
            FolderBrowserDialog dialog = new FolderBrowserDialog();
            dialog.ShowDialog();
            folder = dialog.SelectedPath;
            if (folder != "")
            {
                BrowseFolder(folder);
            }
        }

        private void BrowseFolder(string folder)
        {
            DirectoryInfo dir = new DirectoryInfo(folder);
            files.AddRange(dir.GetFiles().Where(f => f.Extension == ".jpeg" || f.Extension == ".jpg" || f.Extension == ".bmp" || f.Extension == ".png").ToList());
            if (chkSubFolders.Checked)
            {
                foreach (DirectoryInfo _dir in dir.GetDirectories())
                {
                    BrowseFolder(_dir.FullName);
                }
            }
            imgCount = files.Count;
            GetInfo();
        }

        private void btnStart_Click(object sender, EventArgs e)
        {
            stopAfter = -1;

            if (cmbStopAfter.SelectedIndex < cmbStopAfter.Items.Count - 1)
            {
                stopAfter = Convert.ToInt16(cmbStopAfter.SelectedItem);
            }
            timerInterval = 0;
            if (cmbInterval.SelectedIndex > -1)
            {
                Interval i = (Interval)cmbInterval.SelectedItem;
                timerInterval = i.Seconds;
            }

            cmbInterval.Enabled = true;
            cmbStopAfter.Enabled = true;
            chkSubFolders.Enabled = true;
            btnSelFolder.Enabled = true;

            if (!started)
            {
                cmbInterval.Enabled = false;
                cmbStopAfter.Enabled = false;
                chkSubFolders.Enabled = false;
                btnSelFolder.Enabled = false;
                HandleImages();
                if (files.Count == 0) return;
                timerMain.Interval = timerInterval * 1000;
                timerRest = timerInterval - 1;
                timerPic.Interval = 1000;
                progBar.Value = 100;
                timerMain.Start();
                timerPic.Start();
                started = true;
                btnStart.Text = "STOP";
                GetInfo();
                return;
            }

            if (started)
            {
                StopTask();
                return;
            }
        }

        private void StopTask()
        {
            timerMain.Stop();
            timerPic.Stop();
            progBar.Value = 0;
            imgCounter = 0;
            GetInfo();
            btnStart.Text = "START";
            picMain.Image = null;
            started = false;
        }

        private void timerMain_Tick(object sender, EventArgs e)
        {
            TickMain();
        }

        private void TickMain()
        {
            timerPic.Stop();
            progBar.Value = 100;
            HandleImages();
            GetInfo();
            timerRest = timerInterval - 1;
            timerPic.Start();
        }

        private void timerPic_Tick(object sender, EventArgs e)
        {
            timerRest -= 1;
            if(timerRest>0) progBar.Value = Convert.ToInt16(Convert.ToSingle(100) / timerInterval * timerRest);

        }

        private void GetInfo()
        {
            lblTotal.Text = imgCount.ToString();
            lblActual.Text = imgCounter.ToString();
        }

        private void chkSubFolders_CheckedChanged(object sender, EventArgs e)
        {
            files.Clear();
            if (folder != "") BrowseFolder(folder);
        }

        private void btnSkip_Click(object sender, EventArgs e)
        {
            imgCounter -= 1;
            timerMain.Stop();
            TickMain();
            timerMain.Start();

        }

        private void exitToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Close();
        }

        private void infoToolStripMenuItem_Click(object sender, EventArgs e)
        {
            frmInfo frm = new frmInfo();
            frm.ShowDialog();
        }
    }

    class Interval
    {
        public string Description { get; set; }
        public int Seconds { get; set; }

        public Interval(string description, int seconds)
        {
            Description = description;
            Seconds = seconds;  
        }
    }

    
}
