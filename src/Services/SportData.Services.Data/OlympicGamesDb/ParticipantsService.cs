namespace SportData.Services.Data.OlympicGamesDb;

using System.Threading.Tasks;

using Microsoft.EntityFrameworkCore;

using SportData.Data.Entities.OlympicGames;
using SportData.Data.Factories.Interfaces;
using SportData.Services.Data.OlympicGamesDb.Interfaces;

public class ParticipantsService : IParticipantsService
{
    private readonly IDbContextFactory dbContextFactory;

    public ParticipantsService(IDbContextFactory dbContextFactory)
    {
        this.dbContextFactory = dbContextFactory;
    }

    public async Task<Participant> AddOrUpdateAsync(Participant participant)
    {
        using var context = dbContextFactory.CreateOlympicGamesDbContext();

        var dbParticipant = await context.Participants.FirstOrDefaultAsync(x => x.AthleteId == participant.AthleteId && x.EventId == participant.EventId);
        if (dbParticipant == null)
        {
            await context.AddAsync(participant);
            await context.SaveChangesAsync();
        }
        else
        {
            var isUpdated = dbParticipant.IsUpdated(participant);
            if (isUpdated)
            {
                context.Update(dbParticipant);
                await context.SaveChangesAsync();
            }

            participant = dbParticipant;
        }

        return participant;
    }

    public async Task<Participant> GetAsync(int number, int eventId)
    {
        using var context = dbContextFactory.CreateOlympicGamesDbContext();
        var participant = await context.Participants.FirstOrDefaultAsync(x => x.Number == number && x.EventId == eventId);
        return participant;
    }

    public async Task<Participant> GetAsync(int number, int eventId, int nocId)
    {
        using var context = dbContextFactory.CreateOlympicGamesDbContext();
        var participant = await context.Participants.FirstOrDefaultAsync(x => x.Number == number && x.EventId == eventId && x.NOCId == nocId);
        return participant;
    }
}