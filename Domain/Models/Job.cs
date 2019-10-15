using Identity.Reposi;

namespace Domain.Models
{
    public class Job
    {
        public int Hours { get; set; }
        public string Description { get; set; }
         public string UserId { get; set; }
        public User User { get; set; }
        public string CustomerId { get; set; }
        public Customer Customer { get; set; }

    }
}