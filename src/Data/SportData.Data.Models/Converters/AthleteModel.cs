﻿namespace SportData.Data.Models.Converters;

using SportData.Data.Entities.Enumerations;

public class AthleteModel
{
    public int Number { get; set; }

    public FinishStatus FinishStatus { get; set; }

    public string Name { get; set; }
}