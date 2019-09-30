using System;
using System.Windows.Forms;
using JCommon;
using JCommon.Extensions;
using JCommon.SD.Core.Events;

namespace Tests
{
    public partial class SDDownloader : Form
    {
        public SDDownloader()
        {
            InitializeComponent();
            SuperDownloader.OnStarted += OnStart;
            SuperDownloader.OnReceived += OnRecived;
            SuperDownloader.OnProgress += OnProgress;
            SuperDownloader.OnCompleted += OnDownloadDone;
            SuperDownloader.OnStopped += OnStoped;
        }

        bool isDownloading = false;

        private void StartDownloading(object sender, EventArgs e)
        {
            if (!isDownloading)
            {
                isDownloading = true;
                SuperDownloader.Start(URL.Text);
                button1.Text = "Stop";
            }
            else
            {
                isDownloading = false;
                SuperDownloader.Stop();
                button1.Text = "Download";
            }
        }

        private void OnProgress(SDDataReceivedEventArgs args)
        {
            if (InvokeRequired)
            {
                Invoke(new MethodInvoker(() => OnProgress(args)));
                return;
            }
            StatusLabel.Text = "Speed: " + SuperDownloader.DownloadSpeed.BitesToFormatedString() + "/sec.";
            RemTimeLabel.Text = "Remaining time: " + SuperDownloader.RemainingTime.SecondsToDateFormat() + "";
            label1.Text = SuperDownloader.DownloadedSize.BitesToFormatedString()+"/"+ SuperDownloader.TotalFileSize.BitesToFormatedString();
            progressBar1.Value = SuperDownloader.DownloadProgress;
        }

        private void OnStoped(SDEventArgs args)
        {
            if (InvokeRequired)
            {
                Invoke(new MethodInvoker(() => OnStoped(args)));
                return;
            }
            isDownloading = false;
            button1.Text = "Download";

            StatusLabel.Text = "Speed: 0 MB/s";
            RemTimeLabel.Text = "Remaining time: 00:00:00";
            label1.Text = "";
            progressBar1.Value = 0;
        }

        private void OnDownloadDone(SDEventArgs args)
        {
            if (InvokeRequired)
            {
                Invoke(new MethodInvoker(() => OnDownloadDone(args)));
                return;
            }
            isDownloading = false;
            SuperDownloader.Stop();
            button1.Text = "Download";

            StatusLabel.Text = "Speed: 0 MB/s";
            RemTimeLabel.Text = "Remaining time: 00:00:00";
            label1.Text = "";
            progressBar1.Value = 0;
        }

        private void OnRecived(SDDataReceivedEventArgs args)
        {

        }

        private void OnStart(SDStartedEventArgs args)
        {

        }
    }
}
