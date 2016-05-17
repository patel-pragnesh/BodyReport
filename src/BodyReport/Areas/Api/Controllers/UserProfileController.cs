﻿using BodyReport.Models;
using Message.WebApi;
using Microsoft.AspNet.Http;
using Microsoft.AspNet.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Security.Claims;
using BodyReport.Framework;
using System.IO;
using Microsoft.AspNet.Hosting;

namespace BodyReport.Areas.Api.Controllers
{
    [Area("Api")]
    public class UserProfileController : Controller
    {
        /// <summary>
        /// Hosting Environement
        /// </summary>
        IHostingEnvironment _env = null;

        public UserProfileController(IHostingEnvironment env)
        {
            _env = env;
        }

        //
        // POST: /UserProfile/UploadProfileImage
        [HttpPost]
        public IActionResult UploadProfileImage(IFormFile imageFile)
        {
            try
            {
                string userId = User.GetUserId();
                if (string.IsNullOrWhiteSpace(userId))
                    return HttpBadRequest();
                else if (!ImageUtils.CheckUploadedImageIsCorrect(imageFile))
                {
                   return HttpBadRequest();
                }
                ImageUtils.SaveImage(imageFile, Path.Combine(_env.WebRootPath, "images", "userprofil"), userId + ".png");
                string imageRelativeUrl = string.Format("images/userprofil/{0}.png", userId);
                return new HttpOkObjectResult(imageRelativeUrl);
            }
            catch (Exception exception)
            {
                return HttpBadRequest(new WebApiException("Error", exception));
            }
        }
    }
}