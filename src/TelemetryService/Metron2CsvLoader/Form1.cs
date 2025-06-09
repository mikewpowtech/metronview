using CsvHelper;
using Metron2Client;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Globalization;
using System.IO;
using System.Text;
using System.Windows.Forms;
using Utilities;

namespace Metron2CsvLoader
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
        }

        private void ShowFileChooser_Click(object sender, EventArgs e)
        {
            if (!string.IsNullOrWhiteSpace(Path.Text))
                OpenFileDialog.FileName = Path.Text.Trim();
            DialogResult result = OpenFileDialog.ShowDialog();
            if (DialogResult.OK == result)
                Path.Text = OpenFileDialog.FileName;
        }

        private void PerformImport(string path)
        {
            string uniqueId = ConfigurationManager.AppSettings["uniqueId"];
            string server = ConfigurationManager.AppSettings["server"];
            int port = int.Parse(ConfigurationManager.AppSettings["port"]);
            string secret = ConfigurationManager.AppSettings["secret"];
            string pin = ConfigurationManager.AppSettings["pin"];

            // Create the RTU we're going to use to send the data - a Metron 2 with six channels
            Metron2 rtu = new Metron2(uniqueId, server, port) { Secret = secret, Pin = pin };
            for (int s = 0; s < 6; s++)
                rtu.AddSensor(new AnalogueSensor(s + 1, $"T{s + 1}"));
            using (TextReader tr = File.OpenText(path))
            using (CsvReader csv = new CsvReader(tr, CultureInfo.InvariantCulture))
            {
                while (csv.Read())
                {
                    LogBox.AppendText(csv.Context.Parser.RawRecord);
                    if (!csv.TryGetField(0, out DateTime dateRecordedUtc))
                    {
                        LogBox.AppendText("Couldn't parse date and time, skipping record");
                        LogBox.AppendText(Environment.NewLine);
                    }
                    else
                    {
                        IList<NumericReading> readings = new List<NumericReading>();
                        for (int s = 1; s <= 6; s++)
                        {
                            if (!csv.TryGetField(s, out double value))
                                value = 0.0;
                            readings.Add(new NumericReading() { Channel = s, Value = value, IsAlarm = false, DateRecordedUtc = dateRecordedUtc });
                        }
                        try
                        {
                            rtu.ConnectAndSendReadings(readings, false);
                        }
                        catch (Exception ex)
                        {
                            LogBox.AppendText(ToExceptionDetails(ex));
                            LogBox.AppendText(Environment.NewLine);
                        }
                        Application.DoEvents();
                    }
                }
            }
            LogBox.AppendText(Environment.NewLine);
            LogBox.AppendText("Import completed, check for errors above.");
        }

        private string ToExceptionDetails(Exception ex)
        {
            StringBuilder sb = new StringBuilder();
            sb.Append(ex.GetType().FullName);
            sb.Append(": ");
            sb.AppendLine(ex.Message);
            sb.AppendLine(ex.StackTrace);
            return sb.ToString();
        }

        private void Path_TextChanged(object sender, EventArgs e)
        {
            ImportCsv.Enabled = !string.IsNullOrWhiteSpace(Path.Text);
        }

        private void ImportCsv_Click(object sender, EventArgs e)
        {
            PerformImport(Path.Text.Trim());
        }
    }
}
