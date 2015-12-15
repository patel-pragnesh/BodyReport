﻿using BodyReport.Crud.Transformer;
using BodyReport.Manager;
using BodyReport.Models;
using Message;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace BodyReport.Crud.Module
{
    /// <summary>
    /// Crud on Muscle table
    /// </summary>
    public class MuscleModule : Crud
    {
        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="dbContext">database context</param>
        public MuscleModule(ApplicationDbContext dbContext) : base(dbContext)
        {
        }

        /// <summary>
        /// Create data in database
        /// </summary>
        /// <param name="muscle">Data</param>
        /// <returns>insert data</returns>
        public Muscle Create(Muscle muscle)
        {
            if (muscle == null)
                return null;

            if (muscle.Id == 0)
            {
                var sequencerManager = new SequencerManager();
                muscle.Id = sequencerManager.GetNextValue(_dbContext, 1, "muscle");
            }

            if (muscle.Id == 0)
                return null;

            var row = new MuscleRow();
            MuscleTransformer.ToRow(muscle, row);
            _dbContext.Muscles.Add(row);
            _dbContext.SaveChanges();
            
            return MuscleTransformer.ToBean(row);
        }

        /// <summary>
        /// Get data in database
        /// </summary>
        /// <param name="key">Primary Key</param>
        /// <returns>read data</returns>
        public Muscle Get(MuscleKey key)
        {
            if (key == null || key.Id == 0)
                return null;

            var row = _dbContext.Muscles.Where(m => m.Id == key.Id).FirstOrDefault();
            if (row != null)
            {
                return MuscleTransformer.ToBean(row);
            }
            return null;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public List<Muscle> Find()
        {
            List<Muscle> resultList = null;
            var rowList = _dbContext.Muscles;
            if (rowList != null && rowList.Count() > 0)
            {
                resultList = new List<Muscle>();
                foreach (var row in rowList)
                {
                    resultList.Add(MuscleTransformer.ToBean(row));
                }
            }
            return resultList;
        }

        /// <summary>
        /// Update data in database
        /// </summary>
        /// <param name="muscle">data</param>
        /// <returns>updated data</returns>
        public Muscle Update(Muscle muscle)
        {
            if (muscle == null || muscle.Id == 0)
                return null;

            var row = _dbContext.Muscles.Where(m => m.Id == muscle.Id).FirstOrDefault();
            if (row == null)
            { // No data in database
                return Create(muscle);
            }
            else
            { //Modify Data in database
                MuscleTransformer.ToRow(muscle, row);
                _dbContext.SaveChanges();

                return MuscleTransformer.ToBean(row);
            }
        }

        /// <summary>
        /// Delete data in database
        /// </summary>
        /// <param name="key">Primary Key</param>
        public void Delete(MuscleKey key)
        {
            if (key == null || key.Id == 0)
                return;

            var row = _dbContext.Muscles.Where(m => m.Id == key.Id).FirstOrDefault();
            if (row != null)
            {
                _dbContext.Muscles.Remove(row);
                _dbContext.SaveChanges();
            }
        }
    }
}