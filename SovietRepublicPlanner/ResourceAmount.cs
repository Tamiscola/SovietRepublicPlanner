// Resource Amount in Recipes
struct ResourceAmount
{
    public ResourceAmount(Resource resource, double amount, TimePeriod period)
    {
        this.Resource = resource;
        this.Amount = amount;
        this.Period = period; 
    }
    public Resource Resource {  get; set; }
    public double Amount { get; set; }
    public TimePeriod Period { get; set; }  // Day, Week, Month, Year
}