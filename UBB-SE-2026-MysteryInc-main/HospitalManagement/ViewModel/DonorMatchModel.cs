namespace HospitalManagement.ViewModel;

internal class DonorMatchModel
{
    public string FirstName { get; set; } = null!;

    public string LastName { get; set; } = null!;

    public string Cnp { get; set; } = null!;

    public string BloodType { get; set; } = null!;

    public string RhFactor { get; set; } = null!;

    public int Score { get; set; }
}
