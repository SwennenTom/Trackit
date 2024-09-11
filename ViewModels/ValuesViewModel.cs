using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Trackit.Models;

namespace Trackit.ViewModels
{
    class ValuesViewModel
    {
        private readonly Tracker _tracker;
        public ValuesViewModel(Tracker tracker)
        {
            _tracker = tracker;
        }
    }
}
