using System;
using System.Collections.Generic;
using System.Linq;
using System.ServiceModel;
using System.Text;
using System.Threading.Tasks;

namespace DBHost
{
    class Program
    {
        static void Main(string[] args)
        {
            using(var host = new ServiceHost(typeof(DBService.DBConnection)))
            {
                host.Open();
                Console.WriteLine("Host started");
                Console.ReadLine();
            }
            Console.ReadLine();
        }
    }
}
