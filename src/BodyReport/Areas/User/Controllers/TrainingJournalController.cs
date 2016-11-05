﻿using Microsoft.Extensions.Logging;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using BodyReport.Message;
using BodyReport.Message.Web;
using BodyReport.Framework;
using BodyReport.Areas.User.ViewModels;
using BodyReport.Framework.Exceptions;
using BodyReport.Models;
using BodyReport.Resources;
using BodyReport.ServiceLayers.Interfaces;

namespace BodyReport.Areas.User.Controllers
{
    [Authorize(Roles = "User,Admin")]
    [Area("User")]
    public class TrainingJournalController : MvcController
    {
        /// <summary>
        /// Logger
        /// </summary>
        private static ILogger _logger = WebAppConfiguration.CreateLogger(typeof(TrainingJournalController));
        /// <summary>
        /// Service layer users
        /// </summary>
        private readonly IUsersService _usersService;
        /// <summary>
        /// Service layer userInfos
        /// </summary>
        private readonly IUserInfosService _userInfosService;
        /// <summary>
        /// Service layer TrainingWeeks
        /// </summary>
        private readonly ITrainingWeeksService _trainingWeeksService;
        /// <summary>
        /// Service layer TrainingDays
        /// </summary>
        private readonly ITrainingDaysService _trainingDaysService;
        /// <summary>
        /// Service layer ITrainingExercises
        /// </summary>
        private readonly ITrainingExercisesService _trainingExercisesService;
        /// <summary>
        /// Service layer BodyExercises
        /// </summary>
        private readonly IBodyExercisesService _bodyExercisesService;
        /// <summary>
        /// Service layer muscles
        /// </summary>
        private readonly IMusclesService _musclesService;
        /// <summary>
        /// Service layer muscularGroups
        /// </summary>
        private readonly IMuscularGroupsService _muscularGroupsService;

        public TrainingJournalController(UserManager<ApplicationUser> userManager,
                                         IUsersService usersService,
                                         IUserInfosService userInfosService,
                                         ITrainingWeeksService trainingWeeksService,
                                         ITrainingDaysService trainingDaysService,
                                         ITrainingExercisesService trainingExercisesService,
                                         IBodyExercisesService bodyExercisesService,
                                         IMusclesService musclesService,
                                         IMuscularGroupsService muscularGroupsService) : base(userManager)
        {
            _usersService = usersService;
            _userInfosService = userInfosService;
            _trainingWeeksService = trainingWeeksService;
            _trainingDaysService = trainingDaysService;
            _trainingExercisesService = trainingExercisesService;
            _bodyExercisesService = bodyExercisesService;
            _musclesService = musclesService;
            _muscularGroupsService = muscularGroupsService;
        }

        private DayOfWeek GetCurrentDayOfWeek(int? dayOfWeekSelected, TimeZoneInfo timeZoneInfo)
        {
            if (dayOfWeekSelected.HasValue && dayOfWeekSelected >= 0 && dayOfWeekSelected <= 6)
                return (DayOfWeek)dayOfWeekSelected.Value; // TODO Manage Time with world position of user
            else
                return TimeZoneInfo.ConvertTime(DateTime.Now, timeZoneInfo).DayOfWeek;
        }

        //
        // GET: /User/TrainingJournal/Index
        [HttpGet]
        public IActionResult Index(string userId, int year, int weekOfYear, int? dayOfWeekSelected)
        {
            var viewModel = new List<TrainingWeekViewModel>();

            var searchCriteria = new TrainingWeekCriteria() { UserId = new StringCriteria() { Equal = SessionUserId } };
            var scenario = new TrainingWeekScenario() { ManageTrainingDay = false };
            var trainingWeekList = _trainingWeeksService.FindTrainingWeek(searchCriteria, scenario);

            if (trainingWeekList != null)
            {
                foreach (var trainingWeek in trainingWeekList)
                {
                    viewModel.Add(TransformTrainingWeekToViewModel(trainingWeek));
                }
            }

            return View(viewModel);
        }


        // Create a training journal
        // GET: /User/TrainingJournal/Create
        [HttpGet]
        public IActionResult Create()
        {
            var userInfo = _userInfosService.GetUserInfo(new UserInfoKey() { UserId = SessionUserId });
            if (userInfo == null)
                userInfo = new UserInfo();

            var timeZoneInfo = TimeZoneMapper.GetTimeZoneByOlsonName(userInfo.TimeZoneName);
            if (timeZoneInfo == null)
                timeZoneInfo = TimeZoneInfo.Local;
            DateTime dateTime = TimeZoneInfo.ConvertTime(DateTime.Now, timeZoneInfo);

            var trainingWeek = new TrainingWeek();
            trainingWeek.UserId = SessionUserId;
            trainingWeek.Year = dateTime.Year;
            trainingWeek.WeekOfYear = CultureInfo.CurrentCulture.Calendar.GetWeekOfYear(dateTime, CalendarWeekRule.FirstFullWeek, DayOfWeek.Monday);
            trainingWeek.UserHeight = userInfo.Height;
            trainingWeek.UserWeight = userInfo.Weight;
            trainingWeek.Unit = userInfo.Unit;

            ViewBag.UserUnit = userInfo.Unit;
            return View(TransformTrainingWeekToViewModel(trainingWeek));
        }

        // Create a training journal week
        // GET: /User/TrainingJournal/Create
        [HttpPost]
        public IActionResult Create(TrainingWeekViewModel viewModel)
        {
            if (ModelState.IsValid)
            {
                if (string.IsNullOrWhiteSpace(viewModel.UserId) || viewModel.Year == 0 || viewModel.Year == 0 || SessionUserId != viewModel.UserId)
                    return View(viewModel);

                //Verify valide week of year
                if (viewModel.WeekOfYear > 0 && viewModel.WeekOfYear <= 52 && 
                    (viewModel.Unit == (int)TUnitType.Imperial || viewModel.Unit == (int)TUnitType.Metric))
                {
                    var trainingWeek = TransformViewModelToTrainingWeek(viewModel);
                    var trainingWeekKey = new TrainingWeekKey() { UserId = trainingWeek.UserId, Year = trainingWeek.Year, WeekOfYear = trainingWeek.WeekOfYear };
                    var trainingWeekScenario = new TrainingWeekScenario() { ManageTrainingDay = false };
                    var existTrainingWeek = _trainingWeeksService.GetTrainingWeek(trainingWeekKey, trainingWeekScenario);

                    if (existTrainingWeek != null)
                    {
                        ModelState.AddModelError(string.Empty, string.Format(Translation.P0_ALREADY_EXIST, Translation.TRAINING_WEEK));
                        return View(viewModel);
                    }

                    trainingWeek = _trainingWeeksService.CreateTrainingWeek(trainingWeek);
                    if (trainingWeek == null)
                    {
                        ModelState.AddModelError(string.Empty, Translation.IMPOSSIBLE_TO_CREATE_NEW_TRAINING_JOURNAL);
                        return View(viewModel);
                    }

                    return RedirectToAction("View", new { userId = trainingWeek.UserId, year = trainingWeek.Year, weekOfYear = trainingWeek.WeekOfYear });
                }
            }

            return View(viewModel);
        }


        // Edit a training journals
        // GET: /User/TrainingJournal/Edit
        [HttpGet]
        public IActionResult Edit(string userId, int year, int weekOfYear)
        {
            if (string.IsNullOrWhiteSpace(userId) || year == 0 || weekOfYear == 0 || SessionUserId != userId)
                return RedirectToAction("Index");

            ViewBag.UserUnit = GetUserUnit(userId);
            var key = new TrainingWeekKey()
            {
                UserId = userId,
                Year = year,
                WeekOfYear = weekOfYear
            };
            var trainingWeekScenario = new TrainingWeekScenario()
            {
                ManageTrainingDay = true,
                TrainingDayScenario = new TrainingDayScenario() { ManageExercise = true }
            };
            var trainingJournal = _trainingWeeksService.GetTrainingWeek(key, trainingWeekScenario);
            if (trainingJournal == null) // no data found
                return RedirectToAction("Index");

            return View(TransformTrainingWeekToViewModel(trainingJournal));
        }

        // Edit a training journal week
        // GET: /User/TrainingJournal/Create
        [HttpPost]
        public IActionResult Edit(TrainingWeekViewModel viewModel)
        {
            if (ModelState.IsValid)
            {
                ViewBag.UserUnit = GetUserUnit(viewModel.UserId);
                if (string.IsNullOrWhiteSpace(viewModel.UserId) || viewModel.Year == 0 || viewModel.WeekOfYear == 0 || SessionUserId != viewModel.UserId)
                    return View(viewModel);

                //Verify valide week of year
                if (viewModel.WeekOfYear > 0 && viewModel.WeekOfYear <= 52 &&
                    (viewModel.Unit == (int)TUnitType.Imperial || viewModel.Unit == (int)TUnitType.Metric))
                {
                    var trainingWeek = TransformViewModelToTrainingWeek(viewModel);
                    var trainingWeekKey = new TrainingWeekKey() { UserId = trainingWeek.UserId, Year = trainingWeek.Year, WeekOfYear = trainingWeek.WeekOfYear };
                    var trainingWeekScenario = new TrainingWeekScenario() { ManageTrainingDay = false };
                    var existTrainingWeek = _trainingWeeksService.GetTrainingWeek(trainingWeekKey, trainingWeekScenario);

                    if (existTrainingWeek == null)
                    {
                        ModelState.AddModelError(string.Empty, string.Format(Translation.P0_NOT_EXIST, Translation.TRAINING_WEEK));
                        return View(viewModel);
                    }

                    //Create data in database. No need transaction, only header
                    trainingWeekScenario = new TrainingWeekScenario() { ManageTrainingDay = false };
                    trainingWeek = _trainingWeeksService.UpdateTrainingWeek(trainingWeek, trainingWeekScenario);

                    if (trainingWeek == null)
                    {
                        ModelState.AddModelError(string.Empty, string.Format(Translation.IMPOSSIBLE_TO_UPDATE_P0, Translation.TRAINING_JOURNAL));
                        return View(viewModel);
                    }

                    return RedirectToAction("View", new { userId = trainingWeek.UserId, year = trainingWeek.Year, weekOfYear = trainingWeek.WeekOfYear });
                }
            }

            return View(viewModel);
        }

        // Copy a training week
        // GET: /User/TrainingJournal/Copy
        [HttpGet]
        public IActionResult Copy(string userId, int year, int weekOfYear)
        {
            if (string.IsNullOrWhiteSpace(userId) || year == 0 || weekOfYear == 0 || SessionUserId != userId)
                return RedirectToAction("Index");

            ViewBag.UserUnit = GetUserUnit(userId);
            var key = new TrainingWeekKey()
            {
                UserId = userId,
                Year = year,
                WeekOfYear = weekOfYear
            };
            var trainingWeekScenario = new TrainingWeekScenario()
            {
                ManageTrainingDay = true,
                TrainingDayScenario = new TrainingDayScenario() { ManageExercise = true }
            };
            var trainingWeek = _trainingWeeksService.GetTrainingWeek(key, trainingWeekScenario);
            if (trainingWeek == null) // no data found
                return RedirectToAction("Index");
            
            var userInfo = _userInfosService.GetUserInfo(new UserInfoKey() { UserId = SessionUserId });
            if (userInfo == null)
                userInfo = new UserInfo();
            var timeZoneInfo = TimeZoneMapper.GetTimeZoneByOlsonName(userInfo.TimeZoneName);
            if (timeZoneInfo == null)
                timeZoneInfo = TimeZoneInfo.Local;
            DateTime dateTime = TimeZoneInfo.ConvertTime(DateTime.Now, timeZoneInfo);
            int nextWeekOfYear = CultureInfo.CurrentCulture.Calendar.GetWeekOfYear(dateTime, CalendarWeekRule.FirstFullWeek, DayOfWeek.Monday);

            if (dateTime.Year == year && nextWeekOfYear == weekOfYear
                && nextWeekOfYear < 52)
            {
                nextWeekOfYear++;
            }

            var viewModel = new CopyTrainingWeekViewModel()
            {
                UserId = userId,
                OriginWeekOfYear = weekOfYear,
                OriginYear = year,
                Year = dateTime.Year,
                WeekOfYear = nextWeekOfYear
            };

            //Need for refresh WeekOfYear in CopyTrainingWeekViewModel. Why? i don't understand on this page.
            ModelState.Clear();
            return View(viewModel);
        }

        // Edit a training journal week
        // GET: /User/TrainingJournal/Copy
        [HttpPost]
        public IActionResult Copy(CopyTrainingWeekViewModel viewModel)
        {
            if (ModelState.IsValid)
            {
                ViewBag.UserUnit = GetUserUnit(viewModel.UserId);
                if(viewModel == null)
                    return View(viewModel);

                try
                {
                    var copyTrainingWeek = new CopyTrainingWeek()
                    {
                        UserId = viewModel.UserId,
                        OriginYear = viewModel.OriginYear,
                        OriginWeekOfYear = viewModel.OriginWeekOfYear,
                        Year = viewModel.Year,
                        WeekOfYear = viewModel.WeekOfYear
                    };
                    TrainingWeek trainingWeek;
                    if (!_trainingWeeksService.CopyTrainingWeek(SessionUserId, copyTrainingWeek, out trainingWeek))
                        return View(viewModel);

                    return RedirectToAction("View", new { userId = trainingWeek.UserId, year = trainingWeek.Year, weekOfYear = trainingWeek.WeekOfYear });
                }
                catch (ErrorException error)
                {
                    ModelState.AddModelError(string.Empty, error.Message);
                    return View(viewModel);
                }
            }

            return View(viewModel);
        }
        
        // Delete a training journals
        // GET: /User/TrainingJournal/Delete
        [HttpGet]
        public IActionResult Delete(string userId, int year, int weekOfYear, bool confirmation = false)
        {
            if (confirmation)
            {
                var actionResult = RedirectToAction("View", "TrainingJournal", new { Area = "User" });
                if (string.IsNullOrWhiteSpace(userId) || year == 0 || weekOfYear == 0 || SessionUserId != userId)
                    return actionResult;
                
                var key = new TrainingWeekKey()
                {
                    UserId = userId,
                    Year = year,
                    WeekOfYear = weekOfYear
                };
                var trainingWeekScenario = new TrainingWeekScenario() { ManageTrainingDay = false };
                var trainingWeek = _trainingWeeksService.GetTrainingWeek(key, trainingWeekScenario);
                if (trainingWeek == null)
                    return actionResult;

                using (var transaction = _dbContext.Database.BeginTransaction())
                {
                    try
                    {
                        _trainingWeeksService.DeleteTrainingWeek(trainingWeek);
                        transaction.Commit();
                    }
                    catch (Exception exception)
                    {
                        _logger.LogCritical("Unable to delete training week", exception);
                        transaction.Rollback();
                        throw exception;
                    }
                }
                
                return actionResult;
            }
            else
            {
                string title = Translation.TRAINING_WEEK;
                string message = Translation.ARE_YOU_SURE_YOU_WANNA_DELETE_THIS_ELEMENT_PI;
                string returnUrlYes = Url.Action("Delete", "TrainingJournal", new { Area = "User", userId = userId, year = year, weekOfYear = weekOfYear, confirmation = true });
                string returnUrlNo = Url.Action("View", "TrainingJournal", new { Area = "User" });
                return RedirectToAction("Confirm", "Message", new { Area = "Site", title = title, message = message, returnUrlYes = returnUrlYes, returnUrlNo = returnUrlNo });
            }
        }

        //
        // GET: /User/TrainingJournal/View
        [HttpGet]
        public IActionResult View(string userId, int year, int weekOfYear, int? dayOfWeekSelected)
        {
            var userInfo = _userInfosService.GetUserInfo(new UserInfoKey() { UserId = userId });
            if (userInfo == null)
                userInfo = new UserInfo();

            var timeZoneInfo = TimeZoneMapper.GetTimeZoneByOlsonName(userInfo.TimeZoneName);
            if (timeZoneInfo == null)
                timeZoneInfo = TimeZoneInfo.Local;

            DayOfWeek currentDayOfWeek = GetCurrentDayOfWeek(dayOfWeekSelected, timeZoneInfo);
            if (!dayOfWeekSelected.HasValue)
                dayOfWeekSelected = (int)currentDayOfWeek;

            var trainingWeekKey = new TrainingWeekKey()
            {
                UserId = userId,
                Year = year,
                WeekOfYear = weekOfYear
            };

            var trainingWeekScenario = new TrainingWeekScenario()
            {
                ManageTrainingDay = true,
                TrainingDayScenario = new TrainingDayScenario() { ManageExercise = true }
            };
            var trainingWeek = _trainingWeeksService.GetTrainingWeek(trainingWeekKey, trainingWeekScenario);

            if (trainingWeek == null)
                return RedirectToAction("Index");

            //Unit viewer convertion
            string userIdViewer = SessionUserId;
            var viewerUnit = GetUserUnit(userIdViewer);
            var userUnit = GetUserUnit(userId);
            trainingWeek.UserHeight = Utils.TransformLengthToUnitSytem(userUnit, viewerUnit, trainingWeek.UserHeight);
            trainingWeek.UserWeight = Utils.TransformWeightToUnitSytem(userUnit, viewerUnit, trainingWeek.UserWeight);
            
            var trainingWeekViewModel = TransformTrainingWeekToViewModel(trainingWeek);
            List<TrainingDayViewModel> trainingDayViewModels = null;
            List<TrainingExerciseViewModel> trainingExerciseViewModels = null;
            if (trainingWeek != null && trainingWeek.TrainingDays != null && trainingWeek.TrainingDays.Count > 0)
            {
                trainingDayViewModels = new List<TrainingDayViewModel>();
                foreach (var trainingDay in trainingWeek.TrainingDays)
                {
                    trainingDayViewModels.Add(TransformTrainingDayToViewModel(trainingDay, userInfo));

                    if (dayOfWeekSelected.HasValue && trainingDay.DayOfWeek == dayOfWeekSelected)
                    { // Get only current
                        var trainingExercises = FindTrainingExercise(trainingDay);
                        if(trainingExercises != null)
                        {
                            foreach (var trainingExercise in trainingExercises)
                            {
                                //Convert user Unit to viewer unit
                                if (trainingExercise.TrainingExerciseSets != null)
                                {
                                    foreach (var set in trainingExercise.TrainingExerciseSets)
                                        set.Weight = Utils.TransformWeightToUnitSytem(userUnit, viewerUnit, set.Weight);
                                }

                                if (trainingExerciseViewModels == null)
                                    trainingExerciseViewModels = new List<TrainingExerciseViewModel>();
                                trainingExerciseViewModels.Add(TransformTrainingExerciseToViewModel(trainingExercise));
                            }
                        }
                    }
                }
            }

            ViewBag.CurrentDayOfWeek = currentDayOfWeek;
            ViewBag.ViewerUnit = viewerUnit;
            ViewBag.Editable = userId == userIdViewer;
            return View(new Tuple<TrainingWeekViewModel, List<TrainingDayViewModel>, List<TrainingExerciseViewModel>>(trainingWeekViewModel, trainingDayViewModels, trainingExerciseViewModels));
        }

        private List<TrainingExercise> FindTrainingExercise(TrainingDay trainingDay)
        {
            if (trainingDay == null)
                return null;

            var criteria = new TrainingExerciseCriteria()
            {
                Year = new IntegerCriteria() { Equal = trainingDay.Year },
                WeekOfYear = new IntegerCriteria() { Equal = trainingDay.WeekOfYear },
                DayOfWeek = new IntegerCriteria() { Equal = trainingDay.DayOfWeek },
                TrainingDayId = new IntegerCriteria() { Equal = trainingDay.TrainingDayId },
                UserId = new StringCriteria() { Equal = trainingDay.UserId }
            };
            
            return _trainingExercisesService.FindTrainingExercise(criteria);
        }

        private TrainingWeek TransformViewModelToTrainingWeek(TrainingWeekViewModel viewModel)
        {
            TrainingWeek trainingJournal = new TrainingWeek();

            trainingJournal.UserId = viewModel.UserId;
            trainingJournal.Year = viewModel.Year;
            trainingJournal.WeekOfYear = viewModel.WeekOfYear;
            trainingJournal.UserHeight = viewModel.UserHeight;
            trainingJournal.UserWeight = viewModel.UserWeight;
            trainingJournal.Unit = Utils.IntToEnum<TUnitType>(viewModel.Unit);

            return trainingJournal;
        }

        private TrainingWeekViewModel TransformTrainingWeekToViewModel(TrainingWeek trainingWeek)
        {
            TrainingWeekViewModel trainingJournalVM = new TrainingWeekViewModel();

            trainingJournalVM.UserId = trainingWeek.UserId;
            trainingJournalVM.Year = trainingWeek.Year;
            trainingJournalVM.WeekOfYear = trainingWeek.WeekOfYear;
            trainingJournalVM.UserHeight = trainingWeek.UserHeight;
            trainingJournalVM.UserWeight = trainingWeek.UserWeight;
            trainingJournalVM.Unit = (int)trainingWeek.Unit;
            
            var user = _usersService.GetUser(new UserKey() { Id = trainingWeek.UserId });
            if (user != null)
                trainingJournalVM.UserName = user.Name;

            return trainingJournalVM;
        }

        private TrainingDayViewModel TransformTrainingDayToViewModel(TrainingDay trainingDay, UserInfo userInfo)
        {
            var result = new TrainingDayViewModel()
            {
                UserId = trainingDay.UserId,
                Year = trainingDay.Year,
                WeekOfYear = trainingDay.WeekOfYear,
                DayOfWeek = trainingDay.DayOfWeek,
                TrainingDayId = trainingDay.TrainingDayId
            };

            //convert date to user timezone
            var timeZoneInfo = TimeZoneMapper.GetTimeZoneByOlsonName(userInfo.TimeZoneName);
            if (timeZoneInfo == null)
                timeZoneInfo = TimeZoneInfo.Local;
            result.BeginHour = TimeZoneInfo.ConvertTime(trainingDay.BeginHour, timeZoneInfo);
            result.EndHour = TimeZoneInfo.ConvertTime(trainingDay.EndHour, timeZoneInfo);

            return result;
        }

        private TrainingExerciseViewModel TransformTrainingExerciseToViewModel(TrainingExercise trainingExercise)
        {
            var bodyExercise = _bodyExercisesService.GetBodyExercise(new BodyExerciseKey() { Id = trainingExercise.BodyExerciseId });

            var viewModel = new TrainingExerciseViewModel()
            {
                UserId = trainingExercise.UserId,
                Year = trainingExercise.Year,
                WeekOfYear = trainingExercise.WeekOfYear,
                DayOfWeek = trainingExercise.DayOfWeek,
                TrainingDayId = trainingExercise.TrainingDayId,
                TrainingExerciseId = trainingExercise.Id,
                BodyExerciseId = trainingExercise.BodyExerciseId,
                RestTime = trainingExercise.RestTime,
                BodyExerciseName = bodyExercise != null && !string.IsNullOrWhiteSpace(bodyExercise.Name) ? bodyExercise.Name : string.Empty,
                BodyExerciseImage = string.Format("/images/bodyexercises/{0}.png", trainingExercise.BodyExerciseId)
            };

            viewModel.TupleSetReps = new List<Tuple<int, int, double>>();
            if (trainingExercise.TrainingExerciseSets != null)
            {
                foreach (var set in trainingExercise.TrainingExerciseSets)
                    viewModel.TupleSetReps.Add(new Tuple<int, int, double>(set.NumberOfSets, set.NumberOfReps, set.Weight));
            }

            return viewModel;
        }

        // Create a training day
        // GET: /User/TrainingJournal/CreateTrainingDay
        [HttpGet]
        public IActionResult CreateTrainingDay(string userId, int year, int weekOfYear, int dayOfWeek)
        {
            if (string.IsNullOrWhiteSpace(userId) || year == 0 || weekOfYear == 0 || dayOfWeek < 0 || dayOfWeek > 6 || SessionUserId != userId)
                return RedirectToAction("Index");
            
            var userInfo = _userInfosService.GetUserInfo(new UserInfoKey() { UserId = SessionUserId });
            if (userInfo == null)
                return RedirectToAction("View", new { userId = userId, year = year, weekOfYear = weekOfYear, dayOfWeek = dayOfWeek });
            
            var trainingDayCriteria = new TrainingDayCriteria()
            {
                UserId = new StringCriteria() { Equal = userId },
                Year = new IntegerCriteria() { Equal = year },
                WeekOfYear = new IntegerCriteria() { Equal = weekOfYear }
            };
            var trainingDayScenario = new TrainingDayScenario()
            {
                ManageExercise = false
            };
            var trainingDayList = _trainingDaysService.FindTrainingDay(trainingDayCriteria, trainingDayScenario);

            int trainingDayId = 0;
            if (trainingDayList != null && trainingDayList.Count > 0)
                trainingDayId = trainingDayList.Max(td => td.TrainingDayId) + 1;

            var trainingDay = new TrainingDay()
            {
                UserId = userId,
                Year = year,
                WeekOfYear = weekOfYear,
                DayOfWeek = dayOfWeek,
                TrainingDayId = trainingDayId
            };

            ViewBag.UserUnit = userInfo.Unit;
            return View(TransformTrainingDayToViewModel(trainingDay, userInfo));
        }
        
        // Create a training day
        // GET: /User/TrainingJournal/CreateTrainingDay
        [HttpPost]
        public IActionResult CreateTrainingDay(TrainingDayViewModel viewModel)
        {
            if (ModelState.IsValid)
            {
                if (string.IsNullOrWhiteSpace(viewModel.UserId) || viewModel.Year == 0 || viewModel.WeekOfYear == 0 ||
                    viewModel.DayOfWeek < 0 || viewModel.DayOfWeek > 6 || SessionUserId != viewModel.UserId)
                    return View(viewModel);

                //Verify trainingWeek exist
                var trainingWeekKey = new TrainingWeekKey()
                {
                    UserId = viewModel.UserId,
                    Year = viewModel.Year,
                    WeekOfYear = viewModel.WeekOfYear
                };

                var trainingWeekScenario = new TrainingWeekScenario() { ManageTrainingDay = false };
                var trainingWeek = _trainingWeeksService.GetTrainingWeek(trainingWeekKey, trainingWeekScenario);

                if (trainingWeek == null)
                {
                    ModelState.AddModelError(string.Empty, string.Format(Translation.P0_NOT_EXIST, Translation.TRAINING_WEEK));
                    return View(viewModel);
                }

                //Verify valid week of year
                if (viewModel.WeekOfYear > 0 && viewModel.WeekOfYear <= 52)
                {
                    var trainingDay = ControllerUtils.TransformViewModelToTrainingDay(viewModel);
                    trainingDay = _trainingDaysService.CreateTrainingDay(trainingDay);
                    if (trainingDay != null)
                    {
                        return RedirectToAction("View", new { userId = trainingDay.UserId, year = trainingDay.Year, weekOfYear = trainingDay.WeekOfYear, dayOfWeekSelected = trainingDay.DayOfWeek });
                    }
                }
            }

            return View(viewModel);
        }

        // Edit a training day
        // GET: /User/TrainingJournal/EditTrainingDay
        [HttpGet]
        public IActionResult EditTrainingDay(string userId, int year, int weekOfYear, int dayOfWeek, int trainingDayId)
        {
            if (string.IsNullOrWhiteSpace(userId) || year == 0 || weekOfYear == 0 || dayOfWeek < 0 || dayOfWeek > 6 || trainingDayId == 0 || SessionUserId != userId)
                return RedirectToAction("Index");
            
            var key = new TrainingDayKey()
            {
                UserId = userId,
                Year = year,
                WeekOfYear = weekOfYear,
                DayOfWeek = dayOfWeek,
                TrainingDayId = trainingDayId
            };
            var trainingDayScenario = new TrainingDayScenario() { ManageExercise = true };
            var trainingDay = _trainingDaysService.GetTrainingDay(key, trainingDayScenario);
            if (trainingDay == null) // no data found
                return RedirectToAction("View", new { userId = userId, year = year, weekOfYear = weekOfYear, dayOfWeek = dayOfWeek });
            
            var userInfo = _userInfosService.GetUserInfo(new UserInfoKey() { UserId = SessionUserId });
            if (userInfo == null)
                userInfo = new UserInfo();
            return View(TransformTrainingDayToViewModel(trainingDay, userInfo));
        }

        // Edit a training day
        // GET: /User/TrainingJournal/EditTrainingDay
        [HttpPost]
        public IActionResult EditTrainingDay(TrainingDayViewModel viewModel)
        {
            if (ModelState.IsValid)
            {
                if (string.IsNullOrWhiteSpace(viewModel.UserId) || viewModel.Year == 0 || viewModel.WeekOfYear == 0 ||
                    viewModel.DayOfWeek < 0 || viewModel.DayOfWeek > 6 || viewModel.TrainingDayId == 0 || SessionUserId != viewModel.UserId)
                    return View(viewModel);

                //Verify valide week of year
                if (viewModel.WeekOfYear > 0 && viewModel.WeekOfYear <= 52)
                {
                    var trainingDay = ControllerUtils.TransformViewModelToTrainingDay(viewModel);
                    
                    var key = new TrainingDayKey()
                    {
                        UserId = trainingDay.UserId,
                        Year = trainingDay.Year,
                        WeekOfYear = trainingDay.WeekOfYear,
                        DayOfWeek = trainingDay.DayOfWeek,
                        TrainingDayId = trainingDay.TrainingDayId
                    };
                    var trainingDayScenario = new TrainingDayScenario() { ManageExercise = false };
                    var foundTrainingDay = _trainingDaysService.GetTrainingDay(key, trainingDayScenario);
                    if (foundTrainingDay == null) // no data found
                    {
                        ModelState.AddModelError(string.Empty, string.Format(Translation.P0_NOT_EXIST, Translation.TRAINING_DAY));
                        return View(viewModel);
                    }

                    using (var transaction = _dbContext.Database.BeginTransaction())
                    {
                        try
                        {
                            trainingDay = _trainingDaysService.UpdateTrainingDay(trainingDay, trainingDayScenario);
                            transaction.Commit();
                        }
                        catch (Exception exception)
                        {
                            _logger.LogCritical("Unable to edit training day", exception);
                            transaction.Rollback();
                            throw exception;
                        }
                    }
                        
                    if (trainingDay != null)
                    {
                        return RedirectToAction("View", new { userId = trainingDay.UserId, year = trainingDay.Year, weekOfYear = trainingDay.WeekOfYear, dayOfWeekSelected = trainingDay.DayOfWeek });
                    }
                }
            }

            return View(viewModel);
        }

        // Delete a training journals
        // GET: /User/TrainingJournal/DeleteTrainingDay
        [HttpGet]
        public IActionResult DeleteTrainingDay(string userId, int year, int weekOfYear, int dayOfWeek, int trainingDayId, bool confirmation = false)
        {
            if (string.IsNullOrWhiteSpace(userId) || year == 0 || weekOfYear == 0 || dayOfWeek < 0 || dayOfWeek > 6 || trainingDayId == 0 || SessionUserId != userId)
                return RedirectToAction("Index");

            if (confirmation)
            {
                var actionResult = RedirectToAction("View", "TrainingJournal", new { Area = "User", userId = userId, year = year, weekOfYear = weekOfYear, dayOfWeekSelected = dayOfWeek });
                var key = new TrainingDayKey()
                {
                    UserId = userId,
                    Year = year,
                    WeekOfYear = weekOfYear,
                    DayOfWeek = dayOfWeek,
                    TrainingDayId = trainingDayId
                };
                var trainingDayScenario = new TrainingDayScenario() { ManageExercise = true };
                var trainingDay = _trainingDaysService.GetTrainingDay(key, trainingDayScenario);
                if (trainingDay == null)
                    return actionResult;


                using (var transaction = _dbContext.Database.BeginTransaction())
                {
                    try
                    {
                        _trainingDaysService.DeleteTrainingDay(trainingDay);
                        transaction.Commit();
                    }
                    catch (Exception exception)
                    {
                        _logger.LogCritical("Unable to delete training day", exception);
                        transaction.Rollback();
                        throw exception;
                    }
                }
                
                return actionResult;
            }
            else
            {
                string title = Translation.TRAINING_DAY;
                string message = Translation.ARE_YOU_SURE_YOU_WANNA_DELETE_THIS_ELEMENT_PI;
                string returnUrlYes = Url.Action("DeleteTrainingDay", "TrainingJournal", new { Area = "User", userId = userId, year = year, weekOfYear = weekOfYear, dayOfWeek = dayOfWeek, trainingDayId = trainingDayId, confirmation = true });
                string returnUrlNo = Url.Action("View", "TrainingJournal", new { Area = "User", userId = userId, year = year, weekOfYear = weekOfYear, dayOfWeekSelected = dayOfWeek });
                return RedirectToAction("Confirm", "Message", new { Area = "Site", title = title, message = message, returnUrlYes = returnUrlYes, returnUrlNo = returnUrlNo });
            }
        }

        private bool IncorrectHttpData(string userId, int year, int weekOfYear, int dayOfWeek, int trainingDayId, int? trainingExerciseId = null)
        {
            return string.IsNullOrWhiteSpace(userId) || SessionUserId != userId || year == 0 || weekOfYear == 0 ||
                dayOfWeek < 0 || dayOfWeek > 6 || trainingDayId == 0 || (trainingExerciseId != null && !trainingExerciseId.HasValue);
        }

        private IActionResult GetViewActionResult(string userId, int year, int weekOfYear, int dayOfWeek)
        {
            return RedirectToAction("View", "TrainingJournal", new { Area = "User", userId = userId, year = year, weekOfYear = weekOfYear, dayOfWeekSelected = dayOfWeek });
        }

        // Add a training exercises
        // GET: /User/TrainingJournal/AddTrainingExercises
        [HttpGet]
        public IActionResult AddTrainingExercises(string userId, int year, int weekOfYear, int dayOfWeek, int trainingDayId)
        {
            if (IncorrectHttpData(userId, year, weekOfYear, dayOfWeek, trainingDayId))
                return RedirectToAction("Index");

            List<BodyExercise> bodyExerciseList;
            InsertViewBagOnEditTrainingExercise(out bodyExerciseList);

            var viewModel = new TrainingExercisesViewModel();
            CopyViewBagBodyExerciseToViewModel(viewModel, bodyExerciseList);
            return View(viewModel);
        }

        // Add a training exercises
        // POST: /User/TrainingJournal/AddTrainingExercise
        [HttpPost]
        public IActionResult AddTrainingExercises(TrainingExercisesViewModel viewModel, string buttonType)
        {
            if (IncorrectHttpData(viewModel.UserId, viewModel.Year, viewModel.WeekOfYear, viewModel.DayOfWeek, viewModel.TrainingDayId))
                return View(viewModel);

            bool displayMessage = buttonType == "submit";

            int currentMuscularGroupId = 0, currentMuscleId = 0;
            if (viewModel != null)
            {
                currentMuscularGroupId = viewModel.MuscularGroupId;
                currentMuscleId = viewModel.MuscleId;
            }

            List<BodyExercise> bodyExerciseList;
            InsertViewBagOnEditTrainingExercise(out bodyExerciseList, currentMuscularGroupId, currentMuscleId);
            ModelState.Clear();

            if (ModelState.IsValid)
            {
                if(viewModel.BodyExerciseList != null)
                { // filter selected data with existing exercise
                    if (bodyExerciseList == null)
                        viewModel.BodyExerciseList = null;
                    else
                    {
                        var deleteList = new List<SelectBodyExercise>();
                        foreach(var selectBodyExercise in viewModel.BodyExerciseList)
                        {
                            if(bodyExerciseList.FirstOrDefault(c => c.Id == selectBodyExercise.Id) == null)
                            {
                                deleteList.Add(selectBodyExercise);
                            }
                        }

                        foreach (var selectBodyExercise in deleteList)
                        {
                            viewModel.BodyExerciseList.Remove(selectBodyExercise);
                        }
                    }
                }

                if (viewModel.BodyExerciseList == null || viewModel.BodyExerciseList.Count(b => b.Selected == true) == 0)
                {
                    CopyViewBagBodyExerciseToViewModel(viewModel, bodyExerciseList);
                    if (displayMessage)
                        ModelState.AddModelError(string.Empty, string.Format(Translation.THE_P0_FIELD_IS_REQUIRED, Translation.BODY_EXERCISES));
                    return View(viewModel);
                }
                
                var trainingDayKey = new TrainingDayKey() { UserId = viewModel.UserId, Year = viewModel.Year, WeekOfYear = viewModel.WeekOfYear, DayOfWeek = viewModel.DayOfWeek, TrainingDayId = viewModel.TrainingDayId };
                var trainingDayScenario = new TrainingDayScenario() { ManageExercise = true };
                var trainingDay = _trainingDaysService.GetTrainingDay(trainingDayKey, trainingDayScenario);

                if(trainingDay == null)
                {
                    CopyViewBagBodyExerciseToViewModel(viewModel, bodyExerciseList);
                    ModelState.AddModelError(string.Empty, string.Format(Translation.P0_NOT_EXIST, Translation.TRAINING_DAY));
                    return View(viewModel);
                }

                if (trainingDay.TrainingExercises == null)
                    trainingDay.TrainingExercises = new List<TrainingExercise>();

                int bodyExerciseCount = trainingDay.TrainingExercises.Count;
                int maxId = 1;
                if (bodyExerciseCount > 0)
                    maxId = trainingDay.TrainingExercises.Max(t => t.Id) + 1;
                TrainingExercise trainingExercise;
                var bodyExerciseSelectedList = viewModel.BodyExerciseList.Where(b => b.Selected == true);
                foreach (var bodyExercise in bodyExerciseSelectedList)
                {
                    //Only manage add in this page
                    trainingExercise = new TrainingExercise() { UserId = viewModel.UserId, Year = viewModel.Year, WeekOfYear = viewModel.WeekOfYear, DayOfWeek = viewModel.DayOfWeek, TrainingDayId = viewModel.TrainingDayId, Id = maxId, BodyExerciseId = bodyExercise.Id };
                    trainingDay.TrainingExercises.Add(trainingExercise);
                    maxId++;
                }
                if(bodyExerciseCount != trainingDay.TrainingExercises.Count)
                { //data changed
                    using (var transaction = _dbContext.Database.BeginTransaction())
                    {
                        try
                        {
                            _trainingDaysService.UpdateTrainingDay(trainingDay, trainingDayScenario);
                            transaction.Commit();
                        }
                        catch (Exception exception)
                        {
                            _logger.LogCritical("Unable to add training exercises", exception);
                            transaction.Rollback();
                            throw exception;
                        }
                    }
                }

                return GetViewActionResult(viewModel.UserId, viewModel.Year, viewModel.WeekOfYear, viewModel.DayOfWeek);
            }

            CopyViewBagBodyExerciseToViewModel(viewModel, bodyExerciseList);
            return View(viewModel);
        }

        private void CopyViewBagBodyExerciseToViewModel(TrainingExercisesViewModel viewModel, List<BodyExercise> bodyExerciseList)
        {
            viewModel.BodyExerciseList = new List<SelectBodyExercise>();
            if (bodyExerciseList != null)
            {   
                foreach (BodyExercise bodyExercise in bodyExerciseList)
                {
                    viewModel.BodyExerciseList.Add(new SelectBodyExercise() { Id = bodyExercise.Id, Name = bodyExercise.Name });
                }
            }
        }

        private void InsertViewBagOnEditTrainingExercise(out List<BodyExercise> bodyExerciseList, int currentMuscularGroupId = 0, int currentMuscleId = 0, int currentBodyExerciseId = 0)
        {
            bodyExerciseList = null;
            ViewBag.MuscularGroups = ControllerUtils.CreateSelectMuscularGroupItemList(_muscularGroupsService.FindMuscularGroups(), currentMuscularGroupId, true);
            
            if (currentMuscularGroupId == 0)
            { // All exercises
                bodyExerciseList = _bodyExercisesService.FindBodyExercises();
            }
            else
            {
                var muscleCriteria = new MuscleCriteria()
                {
                    MuscularGroupId = new IntegerCriteria() { Equal = currentMuscularGroupId }
                };
                var muscleList = _musclesService.FindMuscles(muscleCriteria);
                ViewBag.Muscles = ControllerUtils.CreateSelectMuscleItemList(muscleList, currentMuscleId, true);

                if (currentMuscleId == 0)
                {
                    if (muscleList != null)
                    {
                        var mucleIdList = new List<int>();
                        foreach (var muscle in muscleList)
                        {
                            mucleIdList.Add(muscle.Id);
                        }
                        var bodyExerciseCriteria = new BodyExerciseCriteria()
                        {
                            MuscleId = new IntegerCriteria() { EqualList = mucleIdList }
                        };
                        bodyExerciseList = _bodyExercisesService.FindBodyExercises(bodyExerciseCriteria);
                    }
                    else //Security
                        bodyExerciseList = _bodyExercisesService.FindBodyExercises();
                }
                else
                {
                    var bodyExerciseCriteria = new BodyExerciseCriteria()
                    {
                        MuscleId = new IntegerCriteria() { Equal = currentMuscleId }
                    };
                    bodyExerciseList = _bodyExercisesService.FindBodyExercises(bodyExerciseCriteria);
                }
            }
        }

        private void OrderTrainingExercices(List<TrainingExercise> trainingExercises, int index, bool upward)
        {
            if (upward && index == 0)
                return;
            if (!upward && index == (trainingExercises.Count -1))
                return;

            var trainingExercise = trainingExercises[index];
            trainingExercises.RemoveAt(index);
            if (upward)
                trainingExercises.Insert(index - 1, trainingExercise);
            else
                trainingExercises.Insert(index + 1, trainingExercise);

            //Parse and change index of exercise
            for (int i = 0; i < trainingExercises.Count; i++)
            {
                index = i + 1;
                trainingExercise = trainingExercises[i];
                trainingExercise.Id = index;

                if (trainingExercise.TrainingExerciseSets != null)
                {
                    foreach (var set in trainingExercise.TrainingExerciseSets)
                    {
                        set.TrainingExerciseId = index;
                    }
                }
            }
        }

        // Add a training exercise
        // GET: /User/TrainingJournal/EditTrainingExercise
        [HttpGet]
        public IActionResult EditTrainingExercise(string userId, int year, int weekOfYear, int dayOfWeek, int trainingDayId,
            int trainingExerciseId, bool upward=false, bool downward=false)
        {
            if (IncorrectHttpData(userId, year, weekOfYear, dayOfWeek, trainingDayId, trainingExerciseId))
                return RedirectToAction("Index");

            var actionResult = GetViewActionResult(userId, year, weekOfYear, dayOfWeek);

            if (upward || downward)
            {
                var findcriteria = new TrainingExerciseCriteria()
                {
                    UserId = new StringCriteria() { Equal = userId },
                    Year = new IntegerCriteria() { Equal = year },
                    WeekOfYear = new IntegerCriteria() { Equal = weekOfYear },
                    DayOfWeek = new IntegerCriteria() { Equal = dayOfWeek },
                    TrainingDayId = new IntegerCriteria() { Equal = trainingDayId }
                };
                var trainingExercises = _trainingExercisesService.FindTrainingExercise(findcriteria);
                if (trainingExercises == null || trainingExercises.Count == 0)
                    return actionResult;

                using (var transaction = _dbContext.Database.BeginTransaction())
                {
                    try
                    {
                        trainingExercises = trainingExercises.OrderBy(t => t.Id).ToList();
                        int indexOfCurrentExercice = trainingExercises.FindIndex(t => t.Id == trainingExerciseId);
                        if (indexOfCurrentExercice == -1)
                            return actionResult;

                        foreach (var trainingExerciseTmp in trainingExercises)
                            _trainingExercisesService.DeleteTrainingExercise(trainingExerciseTmp);

                        OrderTrainingExercices(trainingExercises, indexOfCurrentExercice, upward == true);

                        foreach (var trainingExerciseTmp in trainingExercises)
                            _trainingExercisesService.CreateTrainingExercise(trainingExerciseTmp);

                        transaction.Commit();
                    }
                    catch(Exception exception)
                    {
                        _logger.LogCritical("Unable ordering training exercise", exception);
                        transaction.Rollback();
                        throw exception;
                    }
                }

                return actionResult;
            }


            ViewBag.UserUnit = GetUserUnit(userId);
            
            var key = new TrainingExerciseKey()
            {
                UserId = userId,
                Year = year,
                WeekOfYear = weekOfYear,
                DayOfWeek = dayOfWeek,
                TrainingDayId = trainingDayId,
                Id = trainingExerciseId
            };
            var trainingExercise = _trainingExercisesService.GetTrainingExercise(key);
            if (trainingExercise == null)
                return actionResult;

            var bodyExercise = _bodyExercisesService.GetBodyExercise(new BodyExerciseKey() { Id = trainingExercise.BodyExerciseId });

            var viewModel = new TrainingExerciseViewModel();
            viewModel.UserId = userId;
            viewModel.Year = year;
            viewModel.WeekOfYear = weekOfYear;
            viewModel.DayOfWeek = dayOfWeek;
            viewModel.TrainingDayId = trainingDayId;
            viewModel.TrainingExerciseId = trainingExerciseId;
            viewModel.BodyExerciseId = bodyExercise.Id;
            viewModel.BodyExerciseName = bodyExercise.Name;
            viewModel.BodyExerciseImage = bodyExercise.ImageName;
            viewModel.RestTime = trainingExercise.RestTime;
            viewModel.Unit = (int)GetUserUnit(userId);

            if (trainingExercise.TrainingExerciseSets != null)
            {
                if(trainingExercise.TrainingExerciseSets.Count > 0) //Take unit of first user set if exist
                    viewModel.Unit = (int)trainingExercise.TrainingExerciseSets[0].Unit;

                foreach (var trainingExerciseSet in trainingExercise.TrainingExerciseSets)
                {
                    for(int i=0; i < trainingExerciseSet.NumberOfSets; i++)
                    {
                        viewModel.Reps.Add(trainingExerciseSet.NumberOfReps);
                        viewModel.Weights.Add(trainingExerciseSet.Weight);
                    }
                }
            }

            if(viewModel.Reps == null || viewModel.Reps.Count == 0)
                viewModel.Reps = new List<int?>() { 8 };
            if (viewModel.Weights == null || viewModel.Weights.Count == 0)
                viewModel.Weights = new List<double?>() { 0 };
           
            return View(viewModel);
        }

        // Add a training exercise
        // POST: /User/TrainingJournal/AddTrainingExercise
        [HttpPost]
        public IActionResult EditTrainingExercise(TrainingExerciseViewModel viewModel, string buttonType)
        {
            const int MAX_REPS = 10;
            if(viewModel == null)
                return RedirectToAction("Index");

            ViewBag.UserUnit = GetUserUnit(viewModel.UserId);

            if (viewModel.Reps == null || viewModel.Reps.Count == 0) //Security
                viewModel.Reps = new List<int?>() { 8, 8, 8, 8 };

            if (viewModel.Weights == null)
                viewModel.Weights = new List<double?>();

            while (viewModel.Reps.Count > MAX_REPS)
            {
                viewModel.Reps.RemoveAt(viewModel.Reps.Count - 1);
            }

            while (viewModel.Weights.Count > MAX_REPS)
            {
                viewModel.Weights.RemoveAt(viewModel.Weights.Count - 1);
            }

            while (viewModel.Weights.Count < viewModel.Reps.Count)
            {
                viewModel.Weights.Add(0);
            }

            for (int i = 0; i < viewModel.Reps.Count; i++)
            {
                if(viewModel.Reps[i] == null)
                    viewModel.Reps[i] = 0;
                if (viewModel.Weights[i] == null)
                    viewModel.Weights[i] = 0;
            }

            if ("addRep" == buttonType)
            {
                if (viewModel.Reps.Count < MAX_REPS)
                {
                    int newRepValue = 8;
                    if (viewModel.Reps.Count > 0)
                        newRepValue = viewModel.Reps[viewModel.Reps.Count - 1].Value;
                    viewModel.Reps.Add(newRepValue);

                    double newWeightValue = 8;
                    if (viewModel.Weights.Count > 0)
                        newWeightValue = viewModel.Weights[viewModel.Weights.Count - 1].Value;
                    viewModel.Weights.Add(newWeightValue);
                }
                return View(viewModel);
            }
            else if ("delete" == buttonType)
            {
                if (viewModel.Reps.Count > 1)
                    viewModel.Reps.RemoveAt(viewModel.Reps.Count - 1);
                if (viewModel.Weights.Count > 1)
                    viewModel.Weights.RemoveAt(viewModel.Weights.Count - 1);
            }
            else if ("submit" == buttonType && ModelState.IsValid)
            {
                if (string.IsNullOrWhiteSpace(viewModel.UserId) || SessionUserId != viewModel.UserId || viewModel.Year == 0 || viewModel.WeekOfYear == 0 ||
                    viewModel.DayOfWeek < 0 || viewModel.DayOfWeek > 6 || viewModel.TrainingDayId == 0 || viewModel.TrainingExerciseId == 0 ||
                    viewModel.BodyExerciseId == 0 &&
                    (viewModel.Unit == (int)TUnitType.Imperial || viewModel.Unit == (int)TUnitType.Metric))
                    return View(viewModel);
                
                var key = new TrainingExerciseKey()
                {
                    UserId = viewModel.UserId,
                    Year = viewModel.Year,
                    WeekOfYear = viewModel.WeekOfYear,
                    DayOfWeek = viewModel.DayOfWeek,
                    TrainingDayId = viewModel.TrainingDayId,
                    Id = viewModel.TrainingExerciseId
                };
                var trainingExercise = _trainingExercisesService.GetTrainingExercise(key);
                if (trainingExercise == null)
                {
                    ModelState.AddModelError(string.Empty, string.Format("{0} {1}", Translation.INVALID_INPUT_2P, Translation.TRAINING_EXERCISE));
                    return View(viewModel);
                }

                trainingExercise.BodyExerciseId = viewModel.BodyExerciseId;
                trainingExercise.RestTime = viewModel.RestTime;

                if (viewModel.Reps != null && viewModel.Reps.Count > 0)
                {
                    //Regroup Reps with Set
                    int nbSet = 0, currentRepValue = 0;
                    var tupleSetRepList = new List<Tuple<int, int, double>>();
                    int repValue;
                    double weightValue, currentWeightValue = 0;
                    for (int i=0; i < viewModel.Reps.Count; i++)
                    {
                        repValue = viewModel.Reps[i].Value;
                        weightValue = viewModel.Weights[i].Value;
                        if (repValue == 0)
                            continue;

                        if (weightValue == currentWeightValue && repValue == currentRepValue)
                            nbSet++;
                        else
                        {
                            if (nbSet != 0)
                                tupleSetRepList.Add(new Tuple<int, int, double>(nbSet, currentRepValue, currentWeightValue));
                            currentRepValue = repValue;
                            currentWeightValue = weightValue;
                            nbSet = 1;
                        }
                    }

                    //last data
                    if (nbSet != 0)
                        tupleSetRepList.Add(new Tuple<int, int, double>(nbSet, currentRepValue, currentWeightValue));

                    trainingExercise.TrainingExerciseSets = new List<TrainingExerciseSet>();
                    int id = 1;
                    var unit = Utils.IntToEnum<TUnitType>(viewModel.Unit);
                    foreach (Tuple<int, int, double> tupleSetRep in tupleSetRepList)
                    {
                        trainingExercise.TrainingExerciseSets.Add(new TrainingExerciseSet()
                        {
                            UserId = viewModel.UserId,
                            Year = viewModel.Year,
                            WeekOfYear = viewModel.WeekOfYear,
                            DayOfWeek = viewModel.DayOfWeek,
                            TrainingDayId = viewModel.TrainingDayId,
                            TrainingExerciseId = viewModel.TrainingExerciseId,
                            Id = id,
                            NumberOfSets = tupleSetRep.Item1,
                            NumberOfReps = tupleSetRep.Item2,
                            Weight = tupleSetRep.Item3,
                            Unit = unit
                        });
                        id++;
                    }

                    using (var transaction = _dbContext.Database.BeginTransaction())
                    {
                        try
                        {
                            _trainingExercisesService.UpdateTrainingExercise(trainingExercise, true);
                            transaction.Commit();
                        }
                        catch (Exception exception)
                        {
                            _logger.LogCritical("Unable to modify training exercise", exception);
                            transaction.Rollback();
                            throw exception;
                        }
                    }

                    return RedirectToAction("View", "TrainingJournal", new { Area = "User", userId = viewModel.UserId, year = viewModel.Year, weekOfYear = viewModel.WeekOfYear, dayOfWeekSelected = viewModel.DayOfWeek });

                }
            }
            return View(viewModel);
        }

        private TUnitType GetUserUnit(string userId)
        {
            TUnitType result = TUnitType.Imperial;

            if(userId != null)
            {
                var userInfo = _userInfosService.GetUserInfo(new UserInfoKey() { UserId = userId });
                if(userInfo != null)
                    result = userInfo.Unit;
            }
            return result;
        }

        // Delete a training journals
        // GET: /User/TrainingJournal/DeleteTrainingDay
        [HttpGet]
        public IActionResult DeleteTrainingExercise(string userId, int year, int weekOfYear, int dayOfWeek, int trainingDayId, int trainingExerciseId, bool confirmation = false)
        {
            if (string.IsNullOrWhiteSpace(userId) || year == 0 || weekOfYear == 0 || dayOfWeek < 0 || dayOfWeek > 6 || trainingDayId == 0 || trainingExerciseId == 0 || SessionUserId != userId)
                return RedirectToAction("Index");

            if (confirmation)
            {
                var actionResult = RedirectToAction("View", "TrainingJournal", new { Area = "User", userId = userId, year = year, weekOfYear = weekOfYear, dayOfWeekSelected = dayOfWeek });
                var key = new TrainingExerciseKey()
                {
                    UserId = userId,
                    Year = year,
                    WeekOfYear = weekOfYear,
                    DayOfWeek = dayOfWeek,
                    TrainingDayId = trainingDayId,
                    Id = trainingExerciseId
                };
                var trainingExercise = _trainingExercisesService.GetTrainingExercise(key);
                if (trainingExercise == null)
                    return actionResult;

                using (var transaction = _dbContext.Database.BeginTransaction())
                {
                    try
                    {
                        _trainingExercisesService.DeleteTrainingExercise(trainingExercise);
                        transaction.Commit();
                    }
                    catch (Exception exception)
                    {
                        _logger.LogCritical("Unable to delete training exercise", exception);
                        transaction.Rollback();
                        throw exception;
                    }
                }
                
                return actionResult;
            }
            else
            {
                string title = Translation.TRAINING_DAY;
                string message = Translation.ARE_YOU_SURE_YOU_WANNA_DELETE_THIS_ELEMENT_PI;
                string returnUrlYes = Url.Action("DeleteTrainingExercise", "TrainingJournal", new { Area = "User", userId = userId, year = year, weekOfYear = weekOfYear, dayOfWeek = dayOfWeek, trainingDayId = trainingDayId, trainingExerciseId = trainingExerciseId, confirmation = true });
                string returnUrlNo = Url.Action("View", "TrainingJournal", new { Area = "User", userId = userId, year = year, weekOfYear = weekOfYear, dayOfWeekSelected = dayOfWeek });
                return RedirectToAction("Confirm", "Message", new { Area = "Site", title = title, message = message, returnUrlYes = returnUrlYes, returnUrlNo = returnUrlNo });
            }
        }

        // Create a training day
        // GET: /User/TrainingJournal/CreateTrainingDay
        [HttpGet]
        public IActionResult SwitchTrainingDay(string userId, int year, int weekOfYear, int dayOfWeek, int dayOfWeekSelected)
        {
           if (string.IsNullOrWhiteSpace(userId) || year == 0 || weekOfYear == 0 || dayOfWeek < 0 || dayOfWeek > 6 ||
                dayOfWeekSelected < 0 || dayOfWeekSelected > 6 || SessionUserId != userId)
                return RedirectToAction("Index");

            var actionResult = RedirectToAction("View", "TrainingJournal", new { Area = "User", userId = userId, year = year, weekOfYear = weekOfYear, dayOfWeekSelected = dayOfWeek });

            try
            {
                _trainingDaysService.SwitchDayOnTrainingDay(userId, year, weekOfYear, dayOfWeek, dayOfWeekSelected);
            }
            catch(Exception except)
            {
                _logger.LogError("Unable to switch day of training day", except);
            }
            return actionResult;
        }
    }
}
