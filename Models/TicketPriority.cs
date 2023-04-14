using System.ComponentModel;

namespace BeastieHunter.Models
{
    public class TicketPriority
    {
        public int Id { get; set; }

        [DisplayName("Priority")]
        public string? Name { get; set; }
    }
}
