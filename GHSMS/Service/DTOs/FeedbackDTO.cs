using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Service.DTOs
{
    public class CreateFeedbackDto
    {
        public int UserId { get; set; }
        public int RelatedServiceId { get; set; }
        public byte Rating { get; set; }
        public string Comment { get; set; } = string.Empty;
    }

    public class FeedbackDto
    {
        public int FeedbackId { get; set; }
        public int UserId { get; set; }
        public int RelatedServiceId { get; set; }
        public byte Rating { get; set; }
        public string Comment { get; set; } = string.Empty;
    }
}
