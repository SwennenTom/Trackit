using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SQLite;

namespace Trackit.Models
{
    public class Tracker
    {
        [PrimaryKey, AutoIncrement]
        public int tracker_id { get; set; }

        public string name { get; set; }
        public string description { get; set; }

        [Indexed]
        public DateTime created_at { get; set; }
    }
}
