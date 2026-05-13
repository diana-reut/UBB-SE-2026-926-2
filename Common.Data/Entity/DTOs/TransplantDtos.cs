namespace Common.API.Controllers
{
    public class CreateWaitlistRequestDto
    {
        public int ReceiverId { get; set; }
        public string OrganType { get; set; } = string.Empty;
    }

    public class AssignDonorDto
    {
        public int DonorId { get; set; }
        public float FinalScore { get; set; }
    }
}
