﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Data.Entity.Database;
using Machete.Domain;

namespace Machete.Data
{
    public class MacheteInitializer : DropCreateDatabaseIfModelChanges<MacheteContext>
    {
        protected override void Seed(MacheteContext macheteDB)
        {
            //Initialize Lookups
            new List<Race>
            {
                //TODO:LOW How do I localize thses lookup strings?       
                new Race { racelabel = "Afroamerican" },
                new Race { racelabel = "Asian"},
                new Race { racelabel = "Caucausian" },
                new Race { racelabel = "Hawaiian" },
                new Race { racelabel = "Latino" },
                new Race { racelabel = "Native American"},
                new Race { racelabel = "Other"}
            }.ForEach(u => macheteDB.Races.Add(u));

            new List<Language>
            {
                new Language { language = "English", level="conversational"},
                new Language { language = "English", level="limited"},
                new Language { language = "English", level="none"}
            }.ForEach(u => macheteDB.Languages.Add(u));

            new List<Neighborhood>
            {
                new Neighborhood { neighborhood = "Seattle"},
                new Neighborhood { neighborhood = "Capitol Hill"},
                new Neighborhood { neighborhood = "Central District"},
                new Neighborhood { neighborhood = "South Park"},
                new Neighborhood { neighborhood = "Kent"}, 
                new Neighborhood { neighborhood = "Auburn"}
            }.ForEach(u => macheteDB.Neighborhoods.Add(u));

            new List<Income>
            {
                new Income { incomelabel = "Less than $10,000"},
                new Income { incomelabel = "Between $10,000 and $25,000"},
                new Income { incomelabel = "Above $25,000"}
            }.ForEach(u => macheteDB.Incomes.Add(u));

            new List<Skill>
            {
                new Skill {skill = "painting", level = "apprentice"},
                new Skill {skill = "painting", level = "competent"},
                new Skill {skill = "painting", level = "master"},
                new Skill {skill = "construction", level = "apprentice"},
                new Skill {skill = "construction", level = "competent"},
                new Skill {skill = "construction", level = "master"}
            }.ForEach(u => macheteDB.Skills.Add(u));
        }
    }
}