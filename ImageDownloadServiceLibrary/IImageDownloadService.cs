using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.ServiceModel;
using System.Text;

namespace ImageDownloadServiceLibrary
{
    [ServiceContract]
    public interface IImageDownloadService
    {
        [OperationContract]
        string GetData(int value);
    }
}
