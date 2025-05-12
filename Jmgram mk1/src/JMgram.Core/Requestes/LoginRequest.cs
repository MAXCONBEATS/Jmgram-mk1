using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Jmgram_mk1.src.JMgram.Core.Requestes
{
    public class LoginRequest
    {
        public required string Phone { get; set; }
        public required string Password { get; set; }
    }
}
