﻿using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Threading.Tasks;

namespace BodyReport.Message
{
    public class TrainingExerciseKey : Key
    {
        /// <summary>
        /// UserId
        /// </summary>
        public string UserId { get; set; }
        /// <summary>
        /// Year
        /// </summary>
        public int Year { get; set; }
        /// <summary>
        /// Week of year
        /// </summary>
        public int WeekOfYear { get; set; }
        /// <summary>
        /// Day of week
        /// </summary>
        public int DayOfWeek { get; set; }
        /// <summary>
        /// Training Day Id
        /// </summary>
        public int TrainingDayId { get; set; }
        /// <summary>
        /// Id of training exercise
        /// </summary>
        public int Id { get; set; }

        public TrainingExerciseKey()
        { }

        public TrainingExerciseKey(TrainingExerciseKey key)
        {
            UserId = key.UserId;
            Year = key.Year;
            WeekOfYear = key.WeekOfYear;
            DayOfWeek = key.DayOfWeek;
            TrainingDayId = key.TrainingDayId;
            Id = key.Id;
        }

        public TrainingExerciseKey Clone()
        {
            var copy = new TrainingExerciseKey();
            copy.UserId = UserId;
            copy.Year = Year;
            copy.WeekOfYear = WeekOfYear;
            copy.DayOfWeek = DayOfWeek;
            copy.TrainingDayId = TrainingDayId;
            copy.Id = Id;
            return copy;
        }

        /// <summary>
        /// Equals by key
        /// </summary>
        /// <returns></returns>
        public static bool IsEqualByKey(TrainingExerciseKey key1, TrainingExerciseKey key2)
        {
            return key1.UserId == key2.UserId && key1.Year == key2.Year && key1.WeekOfYear == key2.WeekOfYear &&
                   key1.DayOfWeek == key2.DayOfWeek && key1.TrainingDayId == key2.TrainingDayId && key1.Id == key2.Id;
        }

        public override string GetCacheKey()
        {
            return string.Format("TrainingExerciseKey_{0}_{1}_{2}_{3}_{4}_{5}",
                UserId, Year.ToString(), WeekOfYear.ToString(), DayOfWeek.ToString(), TrainingDayId.ToString(), Id.ToString());
        }
    }

    public class TrainingExercise : TrainingExerciseKey
    {
        /// <summary>
        /// Id of body exercise
        /// </summary>
        public int BodyExerciseId { get; set; }
        /// <summary>
        /// Rest time (second)
        /// </summary>
        public int RestTime { get; set; }
        /// <summary>
        /// Eccentric Contraction Tempo (second)
        /// </summary>
        public int EccentricContractionTempo { get; set; }
        /// <summary>
        /// Stretch Position Tempo (second)
        /// </summary>
        public int StretchPositionTempo { get; set; }
        /// <summary>
        /// Concentric Contraction Tempo (second)
        /// </summary>
        public int ConcentricContractionTempo { get; set; }
        /// <summary>
        /// contracted Position Tempo (second)
        /// </summary>
        public int ContractedPositionTempo { get; set; }
        /// <summary>
		/// Modification Date
		/// </summary>
		public DateTime ModificationDate
        {
            get;
            set;
        }

        /// <summary>
        /// Version number of object for internal use
        /// 0: initial value
        /// 1: tempo values
        /// </summary>
        [DefaultValue(0)]
        [JsonProperty(DefaultValueHandling = DefaultValueHandling.Populate)] // poupalte default value attribute if not present
        public int ObjectVersionNumber { get; set; } = 1;

        public List<TrainingExerciseSet> TrainingExerciseSets { get; set; }

        public TrainingExercise()
        {
        }

        public TrainingExercise(TrainingExerciseKey key) : base(key)
        {
        }

        new public TrainingExercise Clone()
        {
            var copy = new TrainingExercise(this);
            copy.BodyExerciseId = BodyExerciseId;
            copy.RestTime = RestTime;
            copy.ModificationDate = ModificationDate;
            if (TrainingExerciseSets != null)
            {
                copy.TrainingExerciseSets = new List<TrainingExerciseSet>();
                foreach (var trainingExerciseSet in TrainingExerciseSets)
                {
                    if (trainingExerciseSet == null)
                        copy.TrainingExerciseSets.Add(null);
                    else
                        copy.TrainingExerciseSets.Add(trainingExerciseSet.Clone());
                }
            }
            return copy;
        }
    }

    public class TrainingExerciseCriteria : CriteriaField
    {
        /// <summary>
        /// User Id
        /// </summary>
        public StringCriteria UserId { get; set; }

        /// <summary>
        /// Year
        /// </summary>
        public IntegerCriteria Year { get; set; }

        /// <summary>
        /// Week Of Year
        /// </summary>
        public IntegerCriteria WeekOfYear { get; set; }

        /// <summary>
        /// Day Of Year
        /// </summary>
        public IntegerCriteria DayOfWeek { get; set; }

        /// <summary>
        /// Training Day Id
        /// </summary>
        public IntegerCriteria TrainingDayId { get; set; }
        
        /// <summary>
        /// Training Exercise Id
        /// </summary>
        public IntegerCriteria Id { get; set; }

        /// <summary>
        /// Body Exercise Id
        /// </summary>
        public IntegerCriteria BodyExerciseId { get; set; }

        public override string GetCacheKey()
        {
            return string.Format("TrainingExerciseCriteria_{0}_{1}_{2}_{3}_{4}_{5}_{6}",
                UserId == null ? "null" : UserId.GetCacheKey(),
                Year == null ? "null" : Year.GetCacheKey(),
                WeekOfYear == null ? "null" : WeekOfYear.GetCacheKey(),
                DayOfWeek == null ? "null" : DayOfWeek.GetCacheKey(),
                TrainingDayId == null ? "null" : TrainingDayId.GetCacheKey(),
                Id == null ? "null" : Id.GetCacheKey(),
                BodyExerciseId == null ? "null" : BodyExerciseId.GetCacheKey());
        }
    }
}
