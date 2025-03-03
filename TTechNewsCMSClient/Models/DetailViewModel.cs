﻿namespace NewCMSClient.Models.NewsViewModels;


public class NewsDetailViewModel
{
    public int id { get; set; }
    public string title { get; set; }
    public string description { get; set; }
    public string body { get; set; }
    public List<KeywordViewModel> keywords { get; set; }
    public DateTime insertDate { get; set; }
}

public class KeywordViewModel
{
    public Guid KeywordId { get; set; }

    public string KeywordTitle { get; set; }
}

