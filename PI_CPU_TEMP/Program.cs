using System;
using System.Diagnostics;
using System.IO;
using System.Net;
using System.Text;
using System.Threading;


namespace PI_CPU_TEMP
{
    class Program
    {

        public static void postdata(double cpu_temp)
        {
            HttpWebRequest postdata = (HttpWebRequest)WebRequest.Create("http://www.lewei50.com/api/V1/gateway/UpdateSensors/01");

            postdata.Method = "POST";
            postdata.Headers.Set("userkey", "userkey");
            string post = "[{\"Name\":\"CPU\",\"Value\":\"" + cpu_temp + "\"}]";
            byte[] byteArray = Encoding.UTF8.GetBytes(post);
            Console.WriteLine("--------------- Ready Post ---------------------");
            Stream dataStream = postdata.GetRequestStream();
            dataStream.Write(byteArray, 0, byteArray.Length);
            dataStream.Close();
            Console.WriteLine("--------------- dataStream ---------------------");
            WebResponse response = postdata.GetResponse();
            Console.WriteLine(((HttpWebResponse)response).StatusDescription);
            dataStream = response.GetResponseStream();
            StreamReader reader = new StreamReader(dataStream);
            string responseFromServer = reader.ReadToEnd();
            Console.WriteLine(responseFromServer);
            reader.Close();
            dataStream.Close();
            response.Close();
        }
        static void Main(string[] args)
        {
            Process p = new Process();
            p.StartInfo.FileName = "sh";
            p.StartInfo.UseShellExecute = false;
            p.StartInfo.RedirectStandardInput = true;
            p.StartInfo.RedirectStandardOutput = true;
            p.StartInfo.CreateNoWindow = true;

            while (true)
            {
                Console.WriteLine("--------------- Start!  ---------------------");
                p.Start();
                string[] ct = File.ReadAllLines("/sys/class/thermal/thermal_zone0/temp");
                p.StandardInput.WriteLine("/opt/vc/bin/vcgencmd measure_temp");
                p.StandardInput.WriteLine("exit");
                string GPU_temp = p.StandardOutput.ReadToEnd();

                double cpu_temp = Convert.ToDouble(ct[0]) / 1000;
                Console.WriteLine("当前CPU温度为： " + cpu_temp + "℃");
                Console.WriteLine("当前GPU温度为： " + GPU_temp.Replace("temp=", "").Replace("'C", "℃"));

                postdata(cpu_temp);

                Thread.Sleep(11000);
            }


        }


       
    }
}
