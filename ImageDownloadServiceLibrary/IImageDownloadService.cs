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

        [OperationContract]
        bool saveTodaysBingWallpaper();

        [OperationContract]
        List<Image> getBingWallpapers();

        [OperationContract]
        List<Image> getInstagramImages();

        [FaultContract(typeof(InvalidUrlFault))]
        [OperationContract]
        bool downloadInstagramImage(String url);
    }
}
