﻿using BodyReport.Resources;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace BodyReport.Areas.User.ViewModels
{
    public class TrainingExerciseViewModel
    {
        [Required(ErrorMessageResourceName = TRS.THE_P0_FIELD_IS_REQUIRED, ErrorMessageResourceType = typeof(Translation))]
        public string UserId { get; set; }

        [Required(ErrorMessageResourceName = TRS.THE_P0_FIELD_IS_REQUIRED, ErrorMessageResourceType = typeof(Translation))]
        public int Year { get; set; }

        [Required(ErrorMessageResourceName = TRS.THE_P0_FIELD_IS_REQUIRED, ErrorMessageResourceType = typeof(Translation))]
        public int WeekOfYear { get; set; }

        [Required(ErrorMessageResourceName = TRS.THE_P0_FIELD_IS_REQUIRED, ErrorMessageResourceType = typeof(Translation))]
        public int DayOfWeek { get; set; }

        [Required(ErrorMessageResourceName = TRS.THE_P0_FIELD_IS_REQUIRED, ErrorMessageResourceType = typeof(Translation))]
        public int TrainingDayId { get; set; }

        [Required(ErrorMessageResourceName = TRS.THE_P0_FIELD_IS_REQUIRED, ErrorMessageResourceType = typeof(Translation))]
        [Display(Name = TRS.BODY_EXERCISES, ResourceType = typeof(Translation))]
        public int BodyExerciseId { get; set; }
        
        [Required(ErrorMessageResourceName = TRS.THE_P0_FIELD_IS_REQUIRED, ErrorMessageResourceType = typeof(Translation))]
        [Display(Name = TRS.MUSCULAR_GROUP, ResourceType = typeof(Translation))]
        public int MuscularGroupId { get; set; }

        [Required(ErrorMessageResourceName = TRS.THE_P0_FIELD_IS_REQUIRED, ErrorMessageResourceType = typeof(Translation))]
        [Display(Name = TRS.MUSCLE, ResourceType = typeof(Translation))]
        public int MuscleId { get; set; }

        [Required(ErrorMessageResourceName = TRS.THE_P0_FIELD_IS_REQUIRED, ErrorMessageResourceType = typeof(Translation))]
        [Range(0, 300, ErrorMessageResourceName = TRS.THE_FIELD_P0_SHOULD_BE_A_NUMBER_BETWEEN_P1_AND_P2, ErrorMessageResourceType = typeof(Translation))]
        [Display(Name = TRS.REST_TIME, ResourceType = typeof(Translation))]
        public int RestTime { get; set; }

        [Display(Name = TRS.REPS, ResourceType = typeof(Translation))]
        public List<int> Reps { get; set; } = new List<int>();

        public string BodyExerciseName { get; set; }
        public string BodyExerciseImage { get; set; }
        public List<Tuple<int, int>> TupleSetReps { get; set; }
    }
}