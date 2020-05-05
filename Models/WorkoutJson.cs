using System;
using System.Collections.Generic;

using System.Globalization;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace Fitness_Diary.Models
{
    public class WorkoutJson
    {
        [JsonProperty("core")]
        public List<Set> Core { get; set; }

        [JsonProperty("glutes")]
        public List<Set> Glutes { get; set; }

        [JsonProperty("hiits")]
        public List<Set> Hiits { get; set; }
    }

    public class Set
    {
        [JsonProperty("workout")]
        public string Workout { get; set; }

        [JsonProperty("reps")]
        public string Reps { get; set; }
    }
}
