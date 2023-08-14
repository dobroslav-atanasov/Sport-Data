namespace SportData.Services.Data.CrawlerStorageDb.Interfaces;

using SportData.Data.Entities.Crawlers;

public interface IGroupsService
{
    Task AddOrUpdateGroupAsync(Group group);

    Task<Group> GetGroupAsync(Guid identifier);

    Task<Group> GetGroupAsync(int crawlerId, string name);

    Task AddGroupAsync(Group group);

    Task UpdateGroupAsync(Group newGroup, Group oldGroup);

    Task<IList<string>> GetGroupNamesAsync(int crawlerId);
}