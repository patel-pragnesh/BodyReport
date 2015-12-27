﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Message
{
    public class UserInfoKey
    {
        /// <summary>
        /// UserId (Key)
        /// </summary>
        public string UserId
        {
            get;
            set;
        }
    }

    public class UserInfo : UserInfoKey
    {
        /// <summary>
        /// Sex
        /// </summary>
        public TSexType Sex
        {
            get;
            set;
        }

        /// <summary>
        /// Unit Type
        /// </summary>
        public TUnitType Unit
        {
            get;
            set;
        }

        /// <summary>
        /// Height
        /// </summary>
        public double Height
        {
            get;
            set;
        }

        /// <summary>
        /// Weight
        /// </summary>
        public double Weight
        {
            get;
            set;
        }

        /// <summary>
        /// ZipCode
        /// </summary>
        public string ZipCode
        {
            get;
            set;
        }

        /// <summary>
        /// CountryId
        /// </summary>
        public int CountryId
        {
            get;
            set;
        }
    }
}