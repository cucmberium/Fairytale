using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Fairytale.Demo
{
    class Program
    {
        static void Main(string[] args)
        {
            string s = System.IO.File.ReadAllText(@"example.json");
            System.Diagnostics.Stopwatch sw = new System.Diagnostics.Stopwatch();
            sw.Start();

            for (int i = 0; i < 10000; i++)
                Fairytale.JsonDeserializer.Deserialize(s);
            
            sw.Stop();
            Console.WriteLine(sw.Elapsed);

            sw.Reset();

            sw.Start();

            for (int i = 0; i < 10000; i++)
                Newtonsoft.Json.JsonConvert.DeserializeObject(s);

            sw.Stop();
            Console.WriteLine(sw.Elapsed);

            sw.Reset();

            sw.Start();

            var parser = new System.Text.Json.JsonParser();
            for (int i = 0; i < 10000; i++)
                parser.Parse(s);

            sw.Stop();
            Console.WriteLine(sw.Elapsed);
            Console.ReadLine();
        }
    }
}
