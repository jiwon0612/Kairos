using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Kairos.Shared.DTOs
{
    public class SetDueDateRequest
    {
        public DateTime? DueDate { get; set; }
        public bool HasDueTime { get; set; }
    }
}
