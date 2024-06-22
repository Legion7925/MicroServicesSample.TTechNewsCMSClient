namespace TTechNewsCMSClient.Models;

public class CreateNewsModel
{
    public Guid BusunessId { get; set; } = Guid.NewGuid();
    public string Title { get; set; }
    public string Description { get; set; }
    public string Body { get; set; }
    public List<Guid> KeywordsId { get; set; }
}

public class CreateNewsViewModel
{
    public CreateNewsModel SaveModel { get; set; }

    public Dictionary<string , string> Keywords { get; set; } = new Dictionary<string , string>();
}

