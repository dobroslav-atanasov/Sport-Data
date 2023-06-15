namespace SportData.Converters;

using Microsoft.Extensions.Logging;

public abstract class BaseConverter
{
    public BaseConverter(ILogger<BaseConverter> logger)
    {
        this.Logger = logger;
    }

    protected ILogger<BaseConverter> Logger { get; }

    protected abstract Task ProcessGroupAsync(Group group);
}