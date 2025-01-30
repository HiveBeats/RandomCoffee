namespace RandomCoffee;

public class Participant
{
    private Participant()
    {
    }

    public Participant(string username, Coffee coffee)
    {
        Id = Guid.NewGuid();
        UserName = username;
        ScheduledAt = DateTime.UtcNow;
        CoffeeId = coffee.Id;
    }

    public Guid Id { get; init; }
    public string UserName { get; init; }
    public DateTime ScheduledAt { get; init; }
    public Guid CoffeeId { get; init; }
    public Coffee Coffee { get; init; }
}

public class Coffee
{
    private Coffee()
    {
    }

    public Coffee(Group group)
    {
        Id = Guid.NewGuid();
        CreatedAt = DateTime.UtcNow;
        GroupId = Group.Id;
    }

    public Guid Id { get; init; }
    public DateTime CreatedAt { get; init; }
    public DateTime? AnnouncedAt { get; private set; }
    public List<Participant> CoffeeParticipants { get; init; } = new();
    public string GroupId { get; init; }
    public Group Group { get; init; }

    public void AddParticipant(Participant participant)
    {
        CoffeeParticipants.Add(participant);
    }

    private List<List<Participant>> GroupParticipants(List<Participant> participants)
    {
        var count = participants.Count;
        if (count == 0)
            return [];

        // If the number of participants is even, we need to group them into two groups
        if (count % 2 == 0) return MakePairs(participants);

        var evenParticipants = participants.Take(count - 1).ToList();
        var oddParticipant = participants.Skip(count - 1).First();
        var evenGroups = MakePairs(evenParticipants);

        evenGroups.Last().Add(oddParticipant);

        return evenGroups;

        List<List<Participant>> MakePairs(IEnumerable<Participant> guys)
        {
            return guys
                .Select((participant, index) => new {Participant = participant, Index = index})
                .GroupBy(x => x.Index / 2) // Grouping by index divided by 2
                .Select(g => g.Select(x => x.Participant).ToList()) // Selecting participants from groups
                .ToList();
        }
    }

    public List<List<Participant>> AnnounceForParticipants()
    {
        AnnouncedAt = DateTime.UtcNow;
        return GroupParticipants(CoffeeParticipants);
    }
}

public class Group
{
    private Group()
    {
    }

    public Group(string chatId)
    {
        Id = chatId;
    }

    public string Id { get; init; }
    public List<Coffee> Coffees { get; init; } = new();

    public string InviteToNewCoffee()
    {
        Coffees.Add(new Coffee(this));
        return
            $"Привет!\nХочешь на следующей неделе встретиться и попить кофе с участником из группы?\n Если да, отправь \"\\coffee\" в чат. Согласиться можно до понедельника ({DateTime.UtcNow.GetNext(DayOfWeek.Monday).ToString("dd.MM.yyyy")})";
    }

    public void AddParticipant(string username)
    {
        if (string.IsNullOrWhiteSpace(username))
            throw new ArgumentException("Username cannot be null or empty.", nameof(username));

        var coffee = Coffees.LastOrDefault(x => x.AnnouncedAt == null);
        if (coffee == null)
            throw new ApplicationException(
                $"Ты не успел подписаться на текущую встречу. В пятницу ({DateTime.UtcNow.GetNext(DayOfWeek.Friday):dd.MM.yyyy}) я объявлю сбор на следующую, не пропусти!");

        coffee.AddParticipant(new Participant(username, coffee));
    }

    public string? AnnounceCoffee()
    {
        var coffee = Coffees.LastOrDefault(x => x.AnnouncedAt == null);
        if (coffee == null)
            return null;

        var groups = coffee.AnnounceForParticipants();
        var groupStrings = string.Join(Environment.NewLine,
            groups.Select(x => string.Join(" \u2615\ufe0f ", x.Select(y => $"@{y.UserName}"))));
        return
            $"Появилась информация о кофе-встречах.\nГруппы для кофе готовы. Назначайте встречи:\n _Встречу назначает человек слева_\n{groupStrings}";
    }
}