using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SQLite;

namespace Trackit.Models
{
    public class TrackerSettings
    {
        [PrimaryKey, AutoIncrement]
        public int settings_id { get; set; }

        [Indexed]
        public int tracker_id { get; set; }

        public double min_threshhold { get; set; }
        public double max_threshold { get; set; }
    }
}
