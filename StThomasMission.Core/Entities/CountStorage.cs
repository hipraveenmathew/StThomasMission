using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StThomasMission.Core.Entities
{
    public class CountStorage
    {
        public int Id { get; set; }
        public string EntityName { get; set; } = string.Empty;
        public int EntityCount { get; set; }
    }
}
