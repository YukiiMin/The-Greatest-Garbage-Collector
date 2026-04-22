using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GarbageCollection.Common.Models
{
    public class EmailOtp
    {
        public Guid Id { get; set; }
        public string Email { get; set; } = null!;
        public string OtpCode { get; set; } = null!;
        public DateTime ExpiresAt { get; set; }
        public bool IsUsed { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
<<<<<<< HEAD
        public int Count { get; set; }
        public DateTime? UpdatedAt { get; set; }
=======
>>>>>>> 2b44a62e233f1c93c71d628b9c07ab83abfea1a0
    }
}
