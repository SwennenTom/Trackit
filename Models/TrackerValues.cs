using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SQLite;

namespace Trackit.Models
{
    public class TrackerValues
    {
        [PrimaryKey, AutoIncrement]
        public int value_id { get; set; }

        [Indexed]
        public int tracker_id { get; set; }

        public double value { get; set; }
        public DateTime date { get; set; }
    }
}
