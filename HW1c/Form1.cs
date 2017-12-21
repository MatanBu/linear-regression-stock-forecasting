using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Threading;
using System.Globalization;

namespace HW1c
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
        }

        // Get a web response.
        private string GetWebResponse(string url)
        {
            // Make a WebClient.
            WebClient web_client = new WebClient();

            // Get the indicated URL.
            Stream response = web_client.OpenRead(url);

            // Read the result.
            using (StreamReader stream_reader = new StreamReader(response))
            {
                // Get the results.
                string result = stream_reader.ReadToEnd();

                // Close the stream reader and its underlying stream.
                stream_reader.Close();

                // Return the result.
                return result;
            }
        }
   
        // Get the stock prices.
        private void button1_Click(object sender, EventArgs e)
        {
            this.Cursor = Cursors.WaitCursor;
            Application.DoEvents();

            // Build the URL.
            string url = "";
            url = textBox3.Text;

            if (url != "")
            {
                // Prepend the base URL.
                const string base_url =
                    "http://chart.finance.yahoo.com/table.csv?s=@&a=0&b=1&c=2010&d=%&e=$&f=#&g=d&ignore=.csv";
                url = url.Replace("%", ((DateTime.Now.Month) - 1).ToString());
                url = url.Replace("$", DateTime.Now.Day.ToString());
                url = url.Replace("#", DateTime.Now.Year.ToString());

                // Get the response.
                try
                {
                    // Get the web response.
                    string result = GetWebResponse(url);
                    Console.WriteLine(result.Replace("\\r\\n", "\r\n"));

                    // Pull out the current prices.
                    string[] lines = result.Split(
                        new char[] { '\r', '\n' },
                        StringSplitOptions.RemoveEmptyEntries);

                    double[] prices = new double[lines.Length - 1];
                    DateTime[] dates = new DateTime[lines.Length - 1];

                    for (int i = lines.Length - 1, j = 1; i > 0; i--, j++)
                    {
                        prices[i - 1] = double.Parse(lines[j].Split(',')[4]);
                        dates[i - 1] = DateTime.Parse(lines[j].Split(',')[0]);
                    }

                    chart1.Series[0].Points.DataBindXY(dates, prices);
                    chart1.ChartAreas["ChartArea1"].AxisY.IsStartedFromZero = false;

                    calc(prices, dates);
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Error!\nPlease check stock symbol and try again.", "Read Error",
                        MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
                    groupBox1.Visible = false;

                    foreach (var series in chart1.Series)
                    {
                        series.Points.Clear();
                    }
                }
            }
            this.Cursor = Cursors.Default;
        }

        private void calc(double[] valuesList, DateTime[] dates)
        {
            int count = valuesList.Length;

            double sumY = 0, yTag = 0, xGag = 0, yGag = 0, a = 0, b = 0, sumXY = 0, y = 0, yMinusYTag = 0;
            int sumX = 0;
            DateTime forcastDay;
            uint sumXSquare = 0, xGagSquare = 0, sumYMinusYTagSquare = 0;

            for (int i = 1; i < count; i++)
            {
                sumXY += (i * valuesList[i - 1]);
                sumX += i;
                sumY += valuesList[i - 1];
                sumXSquare += (uint)Math.Pow(i, 2);
            }
            xGag = sumX / count;
            yGag = sumY / count;

            xGagSquare = (uint)Math.Pow(xGag, 2.0);

            b = ((sumXY - (count * xGag * yGag)) / (sumXSquare - (count * xGagSquare)));
            a = yGag - (b * xGag);

            forcastDay = dateTimePicker1.Value;

            DateTime startD = DateTime.Today.Date;
            DateTime endD = forcastDay.Date;

            double calcBusinessDays =
            ((endD - startD).TotalDays * 5 -
            (startD.DayOfWeek - endD.DayOfWeek) * 2) / 7;

            if (endD.DayOfWeek == DayOfWeek.Saturday) calcBusinessDays--;
            if (startD.DayOfWeek == DayOfWeek.Sunday) calcBusinessDays--;

            int dayForCalc = count + (int)calcBusinessDays;

            y = a + b * dayForCalc;
            y = Math.Round(y, 3);

            for (int i = 1; i < count; i++)
            {
                yTag = a + b * i;
                yMinusYTag = (Convert.ToDouble(valuesList[i - 1]) - yTag);
                sumYMinusYTagSquare += (uint)Math.Pow(yMinusYTag, 2.0);
            }

            double[] newVal = new double[count];

            for (int i = 0; i < count; i++)
            {
                newVal[i] = (a + b * i);
            }

            double currentVal = Math.Round(valuesList.Last(), 3);

            textBox1.Text = y.ToString();
            textBox2.Text = currentVal.ToString();

            chart1.Series[1].Points.DataBindXY(dates, newVal);

            label3.Text = textBox3.Text.ToString();
            label4.Text = dateTimePicker1.Text.ToString();
            groupBox1.Visible = true;

        }

        private void textBox3_KeyPress(object sender, KeyPressEventArgs e)
        {
            if (System.Text.Encoding.UTF8.GetByteCount(new char[] { e.KeyChar }) > 1)
            {
                e.Handled = true;
            }
        }
    }
}

