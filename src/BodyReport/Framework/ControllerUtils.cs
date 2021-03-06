﻿using BodyReport.Areas.User.ViewModels;
using BodyReport.Resources;
using BodyReport.Framework;
using BodyReport.Message;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.AspNetCore.Mvc.Rendering;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using BodyReport.Services;
using BodyReport.Data;
using BodyReport.Manager;
using Microsoft.Extensions.Logging;
using Microsoft.AspNetCore.Http;
using System.Net;
using BodyReport.ServiceLayers.Interfaces;

namespace BodyReport.Framework
{
    public static class ControllerUtils
    {
        private readonly static ILogger _logger = WebAppConfiguration.CreateLogger(typeof(ControllerUtils));

        public static List<SelectListItem> CreateSelectRoleItemList(List<Role> roleList, string currentUserId)
        {
            var result = new List<SelectListItem>();

            foreach (Role role in roleList)
            {
                result.Add(new SelectListItem { Text = role.Name, Value = role.Id, Selected = currentUserId == role.Id });
            }

            return result;
        }

        public static List<SelectListItem> CreateSelectMuscularGroupItemList(List<MuscularGroup> muscularGroupList, int currentId, bool addNotSelectedValue = false)
        {
            var result = new List<SelectListItem>();
            if (addNotSelectedValue)
                result.Add(new SelectListItem { Text = Translation.NOT_SPECIFIED, Value = "0", Selected = currentId == 0 });

            if (muscularGroupList == null)
                return result;

            muscularGroupList = muscularGroupList.OrderBy(m => m.Name).ToList();


            foreach (MuscularGroup muscularGroup in muscularGroupList)
            {
                result.Add(new SelectListItem { Text = muscularGroup.Name, Value = muscularGroup.Id.ToString(), Selected = currentId == muscularGroup.Id });
            }

            return result;
        }

        public static List<SelectListItem> CreateSelectMuscleItemList(List<Muscle> muscleList, int currentId, bool addNotSelectedValue = false)
        {
            var result = new List<SelectListItem>();

            if (addNotSelectedValue)
                result.Add(new SelectListItem { Text = Translation.NOT_SPECIFIED, Value = "0", Selected = currentId == 0 });


            if (muscleList == null)
                return result;

            muscleList = muscleList.OrderBy(m => m.Name).ToList();

            foreach (Muscle muscle in muscleList)
            {
                result.Add(new SelectListItem { Text = muscle.Name, Value = muscle.Id.ToString(), Selected = currentId == muscle.Id });
            }

            return result;
        }

        public static List<SelectListItem> CreateSelectBodyExerciseItemList(List<BodyExercise> bodyExerciseList, int currentId, bool addNotSelectedValue = false)
        {
            var result = new List<SelectListItem>();

            if (bodyExerciseList == null)
                return result;

            bodyExerciseList = bodyExerciseList.OrderBy(m => m.Name).ToList();

            if (addNotSelectedValue)
                result.Add(new SelectListItem { Text = Translation.NOT_SPECIFIED, Value = "0", Selected = currentId == 0 });

            foreach (BodyExercise bodyExercise in bodyExerciseList)
            {
                result.Add(new SelectListItem { Text = bodyExercise.Name, Value = bodyExercise.Id.ToString(), Selected = currentId == bodyExercise.Id });
            }

            return result;
        }

        public static List<SelectListItem> CreateSelectSexItemList(int sexId)
        {
            var result = new List<SelectListItem>();
            result.Add(new SelectListItem { Text = Translation.MAN, Value = ((int)TSexType.MAN).ToString(), Selected = sexId == (int)TSexType.MAN });
            result.Add(new SelectListItem { Text = Translation.WOMAN, Value = ((int)TSexType.WOMAN).ToString(), Selected = sexId == (int)TSexType.WOMAN });
            return result;
        }

        public static List<SelectListItem> CreateSelectUnitItemList(int unitId)
        {
            var result = new List<SelectListItem>();
            result.Add(new SelectListItem { Text = Translation.IMPERIAL, Value = ((int)TUnitType.Imperial).ToString(), Selected = unitId == (int)TUnitType.Imperial });
            result.Add(new SelectListItem { Text = Translation.METRIC, Value = ((int)TUnitType.Metric).ToString(), Selected = unitId == (int)TUnitType.Metric });
            return result;
        }

        public static List<SelectListItem> CreateSelectCountryItemList(List<Country> countryList, int userCountryId)
        {
            var result = new List<SelectListItem>();

            result.Add(new SelectListItem { Text = Translation.NOT_SPECIFIED, Value = "0", Selected = userCountryId == 0 });

            foreach (var country in countryList)
            {
                result.Add(new SelectListItem { Text = country.Name, Value = country.Id.ToString(), Selected = userCountryId == country.Id });
            }

            return result;
        }

        public static List<SelectListItem> CreateSelectTimeZoneItemList(string timeZoneId)
        {
            var result = new List<SelectListItem>();

            foreach (var olsonTimeZoneName in TimeZoneMapper.OlsonTimeZoneNames.Distinct())
            {
                result.Add(new SelectListItem { Text = olsonTimeZoneName, Value = olsonTimeZoneName, Selected = timeZoneId == olsonTimeZoneName });
            }

            return result;
        }

        public static string GetModelStateError(ModelStateDictionary modelState)
        {
            StringBuilder sbError = new StringBuilder();
            foreach (var state in modelState)
            {
                if (state.Value != null)
                {
                    foreach (var error in state.Value.Errors)
                    {
                        if (error != null && !string.IsNullOrWhiteSpace(error.ErrorMessage))
                            sbError.AppendLine(error.ErrorMessage);
                    }
                }
            }
            return sbError.ToString();
        }

        public static TrainingDay TransformViewModelToTrainingDay(TrainingDayViewModel viewModel)
        {
            TrainingDay trainingDay = new TrainingDay();

            trainingDay.UserId = viewModel.UserId;
            trainingDay.Year = viewModel.Year;
            trainingDay.WeekOfYear = viewModel.WeekOfYear;
            trainingDay.DayOfWeek = viewModel.DayOfWeek;
            trainingDay.TrainingDayId = viewModel.TrainingDayId;
            trainingDay.BeginHour = viewModel.BeginHour.ToUniversalTime();
            trainingDay.EndHour = viewModel.EndHour.ToUniversalTime();

            return trainingDay;
        }

        public static void SendEmailToAdmin(ApplicationDbContext dbContext, IUsersService usersService, IEmailSender emailSender, string subject, string message)
        {
            int totalRecords;
            var users = usersService.FindUsers(out totalRecords, null, true);
            if (users == null)
                return;

            foreach (var user in users)
            {
                //All admin user
                if (user != null && !string.IsNullOrWhiteSpace(user.Email) && user.Role != null && user.Role.Id == "2")
                {
                    try
                    {
                        emailSender.SendEmailAsync(user.Email, subject, message);
                    }
                    catch (Exception except)
                    {
                        _logger.LogError(3, except, "can't send email to admin");
                    }
                }
            }
        }

        public static Tuple<string, TFieldSort> GetSortFieldDirection(string sortFieldAndOrder)
        {
            Tuple<string, TFieldSort> result = null;
            if (sortFieldAndOrder != null && sortFieldAndOrder.Contains('|'))
            {
                var splitSort = sortFieldAndOrder.Split('|');
                if (splitSort.Length == 2 && !string.IsNullOrWhiteSpace(splitSort[0]) && !string.IsNullOrWhiteSpace(splitSort[1]))
                {
                    string fieldName = splitSort[0];
                    string sortOrder = splitSort[1];
                    if (sortOrder == "asc")
                        result = new Tuple<string, TFieldSort>(fieldName, TFieldSort.Asc);
                    else if (sortOrder == "desc")
                        result = new Tuple<string, TFieldSort>(fieldName, TFieldSort.Desc);
                    else
                        result = new Tuple<string, TFieldSort>(fieldName, TFieldSort.None);
                }
            }
            return result;
        }
        public static void ManageSortingPossibilities(Dictionary<string, string> sortPossilities, string newSortFieldAndOrder)
        {
            if (sortPossilities == null || newSortFieldAndOrder == null || !newSortFieldAndOrder.Contains('|'))
                return;

            //newSortOrder (format(field|sort direction) Ex : id|asc or id|desc
            var splitSort = newSortFieldAndOrder.Split('|');
            if(splitSort.Length == 2 && !string.IsNullOrWhiteSpace(splitSort[0]) && !string.IsNullOrWhiteSpace(splitSort[1]))
            {
                string fieldName = splitSort[0];
                string sortOrder = splitSort[1];
                if (sortPossilities.ContainsKey(fieldName))
                {
                    if (sortOrder == "asc")
                        sortPossilities[fieldName] = "desc";
                    else
                        sortPossilities[fieldName] = "asc";
                }
            }
        }

        public static Cookie GetIdentityUserCookie(HttpContext httpContext)
        {
            if (httpContext != null && httpContext.Request != null && httpContext.Request.Cookies != null)
            {
                foreach (var cookie in httpContext.Request.Cookies)
                {
                    if (cookie.Key != null && cookie.Key.ToLower() == ".AspNetCore.Identity.Application".ToLower())
                        return new Cookie(cookie.Key, cookie.Value);
                }
            }
            return null;
        }
    }
}
