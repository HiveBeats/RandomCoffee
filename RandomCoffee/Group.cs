namespace RandomCoffee;

public class Participant
{
    private Participant(){}
    
    public Participant(string id, string username, Coffee coffee)
    {
        Id = id;
        UserName = username;
        ScheduledAt = DateTime.UtcNow;
        CoffeId = coffee.Id;
    }
    
    public string Id { get; set; }
    public string UserName { get; set; }
    public DateTime ScheduledAt { get; set; }
    public Guid CoffeId { get; set; }
    public Coffee Coffee { get; set; }
}

public class Coffee
{
    public Guid Id { get; set; }
    public DateTime? AnnouncedAt { get; set; }
    public List<Participant> CoffeeParticipants { get; set; }
    public string GroupId { get; set; }
    public Group Group { get; set; }
}

public class Group
{
    public string Id { get; set; } //chatId
    public List<Coffee> Coffees { get; set; }    
}