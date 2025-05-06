namespace Tutorial8.Models.DTOs;

public class ClientTripDTO
{
    public int Id { get; set; }
    public string FirstName { get; set; }
    public string LastName { get; set; }
    /*public string RegisteredAt{ get; set; }
    public string PaymentDate{ get; set; }*/
    public List<TripClientDTO> Trips { get; set; }
}

public class TripClientDTO
{
    public int Id { get; set; }
    public string Name { get; set; }
    public String Description { get; set; }
    public DateTime DateFrom { get; set; }
    public DateTime DateTo{ get; set; }
    public int MaxPeople{ get; set; }
    public int RegisteredAt{ get; set; }
    public int PaymentDate{ get; set; }
}