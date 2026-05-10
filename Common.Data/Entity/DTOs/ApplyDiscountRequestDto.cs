namespace Common.Data.Entity.DTOs;

public sealed class ApplyDiscountRequestDto
{
    public decimal BasePrice { get; set; }

    public int Discount { get; set; }
}