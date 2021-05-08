using System.Windows.Forms;
using DiztinGUIsh.Properties;
using DiztinGUIsh.util;

namespace DiztinGUIsh.window.dialog
{
    public struct ProgressReport
    {
        [CanBeNull] public string Text;
        public int Progress;
    }
    
    public partial class ProgressDialog : Form, IProgressBarView
    {
        private string textOverride;
        private bool isMarquee;

        public string TextOverride
        {
            get => textOverride;
            set
            {
                textOverride = value;
                UpdateProgressText();
            }
        }

        public bool IsMarquee
        {
            get => isMarquee;
            set
            {
                isMarquee = value;
                if (value)
                    progressBar1.Style = isMarquee ? ProgressBarStyle.Marquee : default;
            }
        }

        private int progress;
        public int Progress
        {
            get { return progress; }
            set
            {
                progress = value;
                OnProgressChanged();
            }
        }

        private void OnProgressChanged() => UpdateProgressText();

        public ProgressDialog()
        {
            InitializeComponent();
            progressBar1.Value = 0;
            progressBar1.Maximum = 100;
        }

        public float PercentDone => (int)(100 * (progressBar1.Value / (float)progressBar1.Maximum));

        private void UpdateProgressText() => lblStatusText.Text = TextOverride ?? $@"{PercentDone}%";

        public void UpdateProgress(ProgressReport report)
        {
            this.InvokeIfRequired(() =>
            {
                TextOverride = report.Text;
                Progress = report.Progress;
            });
        }
    }
}

