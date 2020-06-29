using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace RTL.API.Models
{
    public class RTLDBSettings : IRTLDBSettings
    {
        public string ShowsCollectionName { get; set; }
        public string ConnectionString { get; set; }
        public string DBName { get; set; }
    }

    public interface IRTLDBSettings
    {
        string ShowsCollectionName { get; set; }
        string ConnectionString { get; set; }
        string DBName { get; set; }
    }
}
