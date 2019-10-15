using Domain.Models;
using Identity.Reposi;

namespace Identity.Dtos
{
    public class JobsDto
    {
        public int Hours { get; set; }
        public string Description { get; set; }

        public Customer Customer { get; set; }
        public User User { get; set; }
    }
}