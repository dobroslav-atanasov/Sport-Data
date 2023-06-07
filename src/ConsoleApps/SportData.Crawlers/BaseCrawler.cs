﻿namespace SportData.Crawlers;

using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

using SportData.Data.Entities.Crawlers;
using SportData.Data.Models.Http;
using SportData.Services.Interfaces;

public abstract class BaseCrawler
{
    public BaseCrawler(ILogger<BaseCrawler> logger, IConfiguration configuration, IHttpService httpService)
    {
        Logger = logger;
        Configuration = configuration;
        HttpService = httpService;
    }

    protected ILogger<BaseCrawler> Logger { get; }

    protected IConfiguration Configuration { get; }

    protected IHttpService HttpService { get; }

    public abstract Task StartAsync();

    protected async Task ProcessGroupAsync(HttpModel httpModel)
    {
        var name = this.GetNameFromUrl(httpModel);
        var document = this.CreateDocument(httpModel);

        var group = new Group
        {
            Name = name,
            CrawlerId = this.CrawlerId.Value,
            Documents = new List<Document> { document }
        };

        await this.GroupsService.AddOrUpdateGroupAsync(group);
    }

    protected async Task ProcessGroupAsync(HttpModel httpModel, ICollection<Document> documents)
    {
        var name = this.GetNameFromUrl(httpModel);

        var group = new Group
        {
            Name = name,
            CrawlerId = this.CrawlerId.Value,
            Documents = documents
        };

        await this.GroupsService.AddOrUpdateGroupAsync(group);
    }

    protected Document CreateDocument(HttpModel httpModel)
    {
        var name = this.GetNameFromUrl(httpModel);
        var document = new Document
        {
            Name = $"{name}.html".ToLower(),
            Format = httpModel.MimeType,
            Url = httpModel.Url,
            Content = httpModel.Bytes,
            Encoding = httpModel.Encoding.BodyName,
            Order = 1
        };

        return document;
    }

    protected virtual string GetNameFromUrl(HttpModel httpModel)
    {
        var uri = httpModel.Uri;
        var nameParts = uri.AbsolutePath.Split(new[] { "/" }, StringSplitOptions.RemoveEmptyEntries).ToArray();
        var name = string.Join("_", nameParts);
        name = name.Replace(".html", string.Empty);

        return name;
    }

    protected string CreateUrl(string link, string mainUrl)
    {
        var mainUri = new Uri(mainUrl);
        var uri = new Uri(mainUri, link);

        return uri.AbsoluteUri;
    }
}