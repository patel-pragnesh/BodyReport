﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace BodyReport.Message
{
    public class BodyExerciseKey : Key
    {
        /// <summary>
        /// Exercise Id
        /// </summary>
        public int Id { get; set; } = 0;

        public override string GetCacheKey()
        {
            return string.Format("BodyExerciseKey_{0}", Id.ToString());
        }
    }

    public class BodyExercise : BodyExerciseKey
    {
        /// <summary>
        /// Exercise Name
        /// </summary>
        public string Name { get; set; } = string.Empty;
        /// <summary>
        /// Exercise Picture name
        /// </summary>
        public string ImageName { get; set; } = string.Empty;
        /// <summary>
        /// Muscle Id
        /// </summary>
        public int MuscleId { get; set; } = 0;
    }

    public class BodyExerciseCriteria : CriteriaField
    {
        /// <summary>
        /// Exercise Id
        /// </summary>
        public IntegerCriteria Id { get; set; }

        /// <summary>
        /// Exercise Name
        /// </summary>
        public StringCriteria Name { get; set; }

        /// <summary>
        /// Muscle Id
        /// </summary>
        public IntegerCriteria MuscleId { get; set; }

        public override string GetCacheKey()
        {
            return string.Format("BodyExerciseCriteria_{0}_{1}_{2}",
                Id == null ? "null" : Id.GetCacheKey(),
                Name == null ? "null" : Name.GetCacheKey(),
                MuscleId == null ? "null" : MuscleId.GetCacheKey());
        }
    }
}
