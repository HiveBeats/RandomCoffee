namespace RandomCoffee;

public class Participant
{
    private Participant(){}
    
    public Participant(string id, string username, Coffee coffee)
    {
        Id = id;
        UserName = username;
        ScheduledAt = DateTime.UtcNow;
        CoffeeId = coffee.Id;
    }
    
    public string Id { get; set; }
    public string UserName { get; set; }
    public DateTime ScheduledAt { get; set; }
    public Guid CoffeeId { get; set; }
    public Coffee Coffee { get; set; }
}

public class Coffee
{
    public Coffee(Group group)
    {
        Id = Guid.NewGuid();
        CreatedAt = DateTime.UtcNow;
        GroupId = Group.Id;
    }
    public Guid Id { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? AnnouncedAt { get; set; }
    public List<Participant> CoffeeParticipants { get; set; } = new List<Participant>();
    public string GroupId { get; set; }
    public Group Group { get; set; }

    public void AddParticipant(Participant participant)
    {
        CoffeeParticipants.Add(participant);
    }
}

public class Group
{
    public Group(string chatId)
    {
        Id = chatId;
    }
    
    public string Id { get; set; }
    public List<Coffee> Coffees { get; set; } = new List<Coffee>();

    public void AddCoffee()
    {
        Coffees.Add(new Coffee(this));
    }

    public void AddParticipant(string username)
    {
        if (string.IsNullOrWhiteSpace(username))
            throw new ArgumentException("Username cannot be null or empty.", nameof(username));
        
        var coffee = Coffees.LastOrDefault(x => x.AnnouncedAt == null);
        if (coffee == null)
            throw new ApplicationException("Ты не успел подписаться на текущую встречу. В понедельник я объявлю сбор на следующую, не пропусти!");
        coffee.AddParticipant(new Participant(Guid.NewGuid().ToString(), username, coffee));
    }
    
    private List<List<Participant>> GroupParticipants(List<Participant> participants)
    {
        int count = participants.Count;
        if (count == 0)
            return [];
        
        // If the number of participants is even, we need to group them into two groups
        if (count % 2 == 0)
        {
            return MakePairs(participants);
        }

        var evenParticipants = participants.Take(count - 1).ToList();
        var oddParticipant = participants.Skip(count - 1).First();
        var evenGroups = MakePairs(evenParticipants);
            
        evenGroups.Last().Add(oddParticipant);

        return evenGroups;

        List<List<Participant>> MakePairs(IEnumerable<Participant> guys)
        {
            return guys
                .Select((participant, index) => new { Participant = participant, Index = index })
                .GroupBy(x => x.Index / 2) // Grouping by index divided by 2
                .Select(g => g.Select(x => x.Participant).ToList()) // Selecting participants from groups
                .ToList();
        }
    }

    public string? AnnounceCoffee()
    {
        var coffee = Coffees.LastOrDefault(x => x.AnnouncedAt == null);
        if (coffee == null)
            return null;
        
        coffee.AnnouncedAt = DateTime.UtcNow;
        
        var groups = GroupParticipants(coffee.CoffeeParticipants);
        var groupStrings = string.Join(Environment.NewLine, groups.Select(x => string.Join(" \u2615\ufe0f ", x.Select(y => $"@{y.UserName}"))));
        return
            $"Появилась информация о кофе-встречах.\nГруппы для кофе готовы. Назначайте встречи:\n _Встречу назначает человек слева_\n{groupStrings}";
    }
}