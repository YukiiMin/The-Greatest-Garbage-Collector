using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GarbageCollection.Common.DTOs
{
    public class ResetPasswordData
    {
        public string Email { get; set; } = null!;
        public string Password { get; set; } = null!;
        public string Otp { get; set; } = null!;
    }
}
