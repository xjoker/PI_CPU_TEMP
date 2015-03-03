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

            //使用POST方法
            postdata.Method = "POST";
            //添加一个header参数，后面的key本体我隐藏了
            postdata.Headers.Set("userkey", "userkey");

            //构造要POST的内容
            string post = "[{\"Name\":\"CPU\",\"Value\":\"" + cpu_temp + "\"}]";

            //后面都是我抄MSDN的.......
            byte[] byteArray = Encoding.UTF8.GetBytes(post);

            //写入流送出去~
            Stream dataStream = postdata.GetRequestStream();
            dataStream.Write(byteArray, 0, byteArray.Length);
            dataStream.Close();
            WebResponse response = postdata.GetResponse();
            //状态返回，正确是OK
            Console.WriteLine(((HttpWebResponse)response).StatusDescription);

            //获取服务器返回来的数据
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

            //linux下是sh，windows下是cmd.exe
            p.StartInfo.FileName = "sh";

            //几个必要的参数
            p.StartInfo.UseShellExecute = false;
            p.StartInfo.RedirectStandardInput = true;
            p.StartInfo.RedirectStandardOutput = true;
            p.StartInfo.CreateNoWindow = true;

            while (true)
            {
                Console.WriteLine("--------------- Start!  ---------------------");
                p.Start();
                
                //读取CPU温度
                string[] ct = File.ReadAllLines("/sys/class/thermal/thermal_zone0/temp");

                //这里的p是Process，执行这个命令会得到GPU的温度
                p.StandardInput.WriteLine("/opt/vc/bin/vcgencmd measure_temp");
                //要得到回显结果必须要退出控制台哦~
                p.StandardInput.WriteLine("exit");

                //得到回显结果，
                string GPU_temp = p.StandardOutput.ReadToEnd();

                //获取到的CPU温度是5位数的数字，精确到小数点后3位，做个double来储存
                double cpu_temp = Convert.ToDouble(ct[0]) / 1000;
                Console.WriteLine("当前CPU温度为： " + cpu_temp + "℃");

                //GPU温度获取到的格式是"temp=35'C"这样的，用替换字符串提取出数字
                Console.WriteLine("当前GPU温度为： " + GPU_temp.Replace("temp=", "").Replace("'C", "℃"));

                //这里调用了上传数据的方法
                postdata(cpu_temp);

                //上传数据最短间隔是10秒，所以干脆来个11秒吧
                Thread.Sleep(11000);
            }


        }


       
    }
}
