using System.Runtime.Serialization;

namespace ImageDownloadServiceLibrary
{
    [DataContract]
    class InvalidUrlFault
    {
        [DataMember]
        public string Error { get; set; }

        [DataMember]
        public string Details { get; set; }

    }
}