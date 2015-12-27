﻿using BodyReport.Crud.Module;
using BodyReport.Models;
using Message;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace BodyReport.Manager
{
    public class CityManager : ServiceManager
    {
        CityModule _module = null;

        public CityManager(ApplicationDbContext dbContext) : base(dbContext)
        {
            _module = new CityModule(_dbContext);
        }

        internal City GetCity(CityKey key)
        {
            return _module.Get(key);
        }

        public List<City> FindCities(CriteriaField criteriaField = null)
        {
            return _module.Find(criteriaField);
        }
    }
}