namespace BeastieHunter.Models.ViewModels
{
    public class DashboardViewModel
    {
        public Company Company { get; set; }

        public List<Project> Projects { get; set; }

        public List<Ticket> Ticket { get; set; }

        public List<BTUser> Members { get; set; }



    }
}
