using Microsoft.AspNetCore.Mvc;
using NewCMSClient.Models.NewsViewModels;
using Newtonsoft.Json;
using System.Net.Http;
using System.Text;
using TTechNewsCMSClient.Models;

namespace TTechNewsCMSClient.Controllers
{
    public class NewsController : Controller
    {
        private readonly IHttpClientFactory _httpClientFactory;
        public NewsController(IHttpClientFactory httpClientFactory)
        {
            _httpClientFactory = httpClientFactory;
        }
        public async Task<IActionResult> Index()
        {
            var newsClient = _httpClientFactory.CreateClient("news");
            string result = await newsClient.GetStringAsync("api/News/GetList");
            NewsListModel newsList = JsonConvert.DeserializeObject<NewsListModel>(result);

            return View(newsList);
        }

        public async Task<IActionResult> Detail(long id)
        {
            var newsClient = _httpClientFactory.CreateClient("news");
            string result = await newsClient.GetStringAsync($"api/News/GetDetail?NewsId={id}");
            NewsDetailViewModel newsDetail = JsonConvert.DeserializeObject<NewsDetailViewModel>(result);

            return View(newsDetail);
        }

        public async Task<IActionResult> Save()
        {
            var biClient = _httpClientFactory.CreateClient("bi");
            string keywordAsString = await biClient.GetStringAsync("api/Keywords/SearchTitleAndStatus");
            KeywordListResult keywordListResult = JsonConvert.DeserializeObject<KeywordListResult>(keywordAsString);
            CreateNewsViewModel createNewsViewModel = new CreateNewsViewModel();

            foreach (var keyword in keywordListResult.QueryResult)
            {
                createNewsViewModel.Keywords.Add(keyword.BusinessId , keyword.Title);
            }
            return View(createNewsViewModel);
        }

        [HttpPost]
        public async Task<IActionResult> Save([Bind(Prefix = "SaveModel")]CreateNewsModel model)
        {
            var newsClient = _httpClientFactory.CreateClient("news");
            var httpContent = new StringContent(JsonConvert.SerializeObject(model) , Encoding.UTF8, "application/json");
            var result = await newsClient.PostAsync("api/News", httpContent);
            return result.IsSuccessStatusCode ? Redirect("Index") : Redirect ("Save");
        }
    }
}
