using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SteamBotLite
{

    public class Map
    {
        public Object Submitter { get; set; }
        public string SubmitterName { get; set; }

        public string DownloadURL { get; set; }
        public string Notes { get; set; }
        public bool Uploaded { get; set; }
        public string Filename { get; set; }
        public string SubmitterContact { get; set; }

        public bool IsOwner(Object other)
        {
            return other.ToString().Equals(Submitter.ToString());
        }
    }

}