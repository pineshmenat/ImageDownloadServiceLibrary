using System;
using System.Collections.Generic;
using System.Runtime.Serialization;

namespace ImageDownloadServiceLibrary
{
    [DataContract]
    public class Image
    {
        [DataMember]
        public int image_id { get; set; }

        [DataMember]
        public byte[] image { get; set; }

        [DataMember]
        public String source { get; set; }

        [DataMember]
        public DateTime created_date { get; set; }

        [DataMember]
        public String image_name { get; set; }
    }
}
