namespace TTechNewsCMSClient.Models;

public class KeywordListResult
{
    public KeywordItem[] QueryResult { get; set; }

    public int PageNumber { get; set; }

    public int PageSize { get; set; }

    public int TotalCount { get; set; }
}

public class KeywordItem
{
    public int Id { get; set; }

    public string BusinessId { get; set; }

    public int Status { get; set; }

    public string Title { get; set; }

}
