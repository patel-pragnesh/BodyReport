﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace BodyReport.Models
{
    public class UserInfoRow
    {
        /// <summary>
        /// UserId (Key)
        /// </summary>
        public string UserId
        {
            get;
            set;
        }

        /// <summary>
        /// Sex
        /// </summary>
        public int Sex
        {
            get;
            set;
        }

        /// <summary>
        /// Height
        /// </summary>
        public int Height
        {
            get;
            set;
        }

        /// <summary>
        /// Weight
        /// </summary>
        public int Weight
        {
            get;
            set;
        }

        /// <summary>
        /// PostalCode
        /// </summary>
        public string ZipCode
        {
            get;
            set;
        }

        /// <summary>
        /// CityId
        /// </summary>
        public int CityId
        {
            get;
            set;
        }
    }
}
