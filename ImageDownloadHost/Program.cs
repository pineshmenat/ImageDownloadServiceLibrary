using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.ServiceModel;
namespace ImageDownloadHost
{
    class Program
    {
        static void Main(string[] args)
        {
            using (System.ServiceModel.ServiceHost host = new System.ServiceModel.ServiceHost(typeof(ImageDownloadServiceLibrary.ImageDownloadService)))
            {
                host.Open();
                Console.WriteLine("Host Started, Press any key to stop...");
                Console.ReadLine();
            }
        }
    }
}
