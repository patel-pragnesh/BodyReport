﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.ComponentModel;

namespace BodyReport.Message
{
	public class TrainingWeekKey : KeyNotifyPropertyChanged
    {
        /// <summary>
        /// UserId
        /// </summary>
		private string _userId;
		public string UserId
		{
			get { return _userId; }
			set 
			{ 
				_userId = value;
				OnPropertyChanged();
			}
		}

        /// <summary>
        /// Year
        /// </summary>
		private int _year;
		public int Year
		{
			get { return _year; }
			set 
			{ 
				_year = value;
				OnPropertyChanged();
			}
		}

        /// <summary>
        /// Week of year
        /// </summary>
		private int _weekOfYear;

		public int WeekOfYear
		{
			get { return _weekOfYear; }
			set 
			{ 
				_weekOfYear = value;
				OnPropertyChanged();
			}
		}


        /// <summary>
        /// Equals by key
        /// </summary>
        /// <returns></returns>
        public static bool IsEqualByKey(TrainingWeekKey key1, TrainingWeekKey key2)
        {
            return key1.UserId == key2.UserId && key1.Year == key2.Year && key1.WeekOfYear == key2.WeekOfYear;
        }

        public override string GetCacheKey()
        {
            return string.Format("TrainingWeekKey_{0}_{1}_{2}",
                UserId, Year.ToString(), WeekOfYear.ToString());
        }
    }

    public class TrainingWeek : TrainingWeekKey
    {
        /// <summary>
        /// Unit Type
        /// </summary>
        public TUnitType Unit
        {
            get;
            set;
        }
        /// <summary>
        /// User Height
        /// </summary>
		private double _userHeight;
		public double UserHeight
		{
			get { return _userHeight; }
			set 
			{
                if (_userHeight != value)
                {
                    _userHeight = value;
                    OnPropertyChanged();
                }
			}
		}
        /// <summary>
        /// User Weight
        /// </summary>
		private double _userWeight;
		public double UserWeight
		{
			get { return _userWeight; }
			set 
			{
                if (_userWeight != value)
                {
                    _userWeight = value;
                    OnPropertyChanged();
                }
			}
		}

        /// <summary>
		/// Modification Date
		/// </summary>
		public DateTime ModificationDate
        {
            get;
            set;
        }

        /// <summary>
        /// Week of year description
        /// </summary>
        private string _weekOfYearDescription;

        public string WeekOfYearDescription
        {
            get { return _weekOfYearDescription; }
            set
            {
                _weekOfYearDescription = value;
                OnPropertyChanged();
            }
        }

        /// <summary>
        /// User Weight
        /// </summary>
        public List<TrainingDay> TrainingDays { get; set; }
    }

    public class TrainingWeekCriteria : CriteriaField
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
        
        public override string GetCacheKey()
        {
            return string.Format("TrainingWeekCriteria_{0}_{1}_{2}",
                UserId == null ? "null" : UserId.GetCacheKey(),
                Year == null ? "null" : Year.GetCacheKey(),
                WeekOfYear == null ? "null" : WeekOfYear.GetCacheKey());
        }
    }

    public class TrainingWeekScenario : Key
    {
        private bool _manageTrainingDay;
        public bool ManageTrainingDay
        {
            get
            {
                return _manageTrainingDay;
            }
            set
            {
                _manageTrainingDay = value;
                if (_manageTrainingDay)
                    TrainingDayScenario = new TrainingDayScenario();
                else
                    TrainingDayScenario = null;
            }
        }

        public TrainingDayScenario TrainingDayScenario { get; set; }

        public TrainingWeekScenario()
        {
            ManageTrainingDay = true;
        }

        public override string GetCacheKey()
        {
            return string.Format("TrainingWeekScenario_{0}",
                ManageTrainingDay ? "1" : "0");
        }
    }
}
