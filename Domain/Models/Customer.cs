using System.Collections.Generic;
using Identity.Reposi;

namespace Domain.Models
{
    public class Customer
    {
        public string Id { get; set; }
        public List<Job> Jobs { get; set; }
    }
}