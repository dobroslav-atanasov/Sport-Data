namespace SportData.Services.Data.SportDataDb;

using System.Threading.Tasks;

using Microsoft.EntityFrameworkCore;

using SportData.Data.Entities.OlympicGames;
using SportData.Data.Factories.Interfaces;
using SportData.Data.Repositories;
using SportData.Services.Data.SportDataDb.Interfaces;

public class ParticipantsService : IParticipantsService
{
    //private readonly SportDataRepository<Participant> repository;
    private readonly IDbContextFactory dbContextFactory;

    public ParticipantsService(SportDataRepository<Participant> repository, IDbContextFactory dbContextFactory)
    {
        //this.repository = repository;
        this.dbContextFactory = dbContextFactory;
    }

    public async Task<Participant> AddOrUpdateAsync(Participant participant)
    {
        using var context = this.dbContextFactory.CreateSportDataDbContext();
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

        //var dbParticipant = await repository.GetAsync(x => x.AthleteId == participant.AthleteId && x.EventId == participant.EventId);
        //if (dbParticipant == null)
        //{
        //    await repository.AddAsync(participant);
        //    await repository.SaveChangesAsync();
        //}
        //else
        //{
        //    var isUpdated = dbParticipant.IsUpdated(participant);
        //    if (isUpdated)
        //    {
        //        repository.Update(dbParticipant);
        //        await repository.SaveChangesAsync();
        //    }
        //
        //     participant = dbParticipant;
        //}

        //return participant;
    }

    public async Task<Participant> GetAsync(int number, int eventId)
    {
        using var context = this.dbContextFactory.CreateSportDataDbContext();
        var participant = await context.Participants.FirstOrDefaultAsync(x => x.Number == number && x.EventId == eventId);
        return participant;
    }

    public async Task<Participant> GetAsync(int number, int eventId, int nocId)
    {
        using var context = this.dbContextFactory.CreateSportDataDbContext();
        var participant = await context.Participants.FirstOrDefaultAsync(x => x.Number == number && x.EventId == eventId && x.NOCId == nocId);
        return participant;
    }
}