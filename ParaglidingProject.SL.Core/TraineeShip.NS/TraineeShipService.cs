﻿using Microsoft.EntityFrameworkCore;
using ParaglidingProject.Data;
using ParaglidingProject.SL.Core.Helpers;
using ParaglidingProject.SL.Core.TraineeShip.NS.Helpers;
using ParaglidingProject.SL.Core.TraineeShip.NS.NewFolder1;
using ParaglidingProject.SL.Core.TraineeShip.NS.TransferObjects;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ParaglidingProject.SL.Core.TraineeShip.NS
{
     public class TraineeShipService : ITraineeShipService
    {
        private readonly ParaglidingClubContext _paraContext;
        public TraineeShipService(ParaglidingClubContext paraContext)
        {
            this._paraContext = paraContext;
        }
        public async Task<TraineeshipPatchDto> GetTraineeshipToPatchAsync(int traineeshipId)
        {
            return await _paraContext.Traineeships
                .Where(t => t.ID == traineeshipId)
                .Select(t => new TraineeshipPatchDto
                {
                    TraineeShipStartDate = t.StartDate,
                    TraineeShipEndDate = t.EndDate,
                    TraineeShipPrice = t.Price
                })
                .FirstOrDefaultAsync();
        }
        public async Task<bool?> UpdateTraineeshipAsync(int traineeshipId, TraineeshipPatchDto patchDto)
        {
            var traineeshipToUpdate = await _paraContext.Traineeships
                .FirstOrDefaultAsync(p => p.ID == traineeshipId);

            if (traineeshipToUpdate == null) return null;

            traineeshipToUpdate.StartDate = patchDto.TraineeShipStartDate;
            traineeshipToUpdate.EndDate = patchDto.TraineeShipEndDate;
            traineeshipToUpdate.Price = patchDto.TraineeShipPrice;

            return await _paraContext.SaveChangesAsync() > 0;
        }

        /// <summary>
        /// Get all(collection of traineeships)
        /// </summary>
        /// <param name="options"> options as traineeshipSSFP </param>
        /// <returns></returns>
        public async Task<IReadOnlyCollection<TraineeShipDto>> GetAllTraineeShipAsync(TraineeshipSSFP options)
        {
            var traineeships = _paraContext.Traineeships
                 .AsNoTracking()
                 .SortTraineeshipBy(options.SortBy)
                 .FilterTraineeshipBy(options.FilterBy)
                 .SearchTraineeshipBy(options)
                 .Select(T => new TraineeShipDto
                 {
                     Traineeshipid = T.ID,
                     TraineeShipStartDate=T.StartDate,
                     TraineeShipPrice=T.Price,
                     TraineeShipEndDate=T.EndDate,
                     TraineeshipIsActive=T.IsActive,
                     LicenseId = T.LicenseID,
                     License = T.License
                 });
            options.SetPagingValues(traineeships);

            var pagedQuery = traineeships.Page(options.PageNumber -1, options.PageSize);

            return await pagedQuery.ToListAsync();
        }



        public async Task<TraineeShipDto> GetTraineeShipAsync(int id)
        {
            var traineeship = await _paraContext.Traineeships
              .AsNoTracking()
              .Select(t => new TraineeShipDto
              {
                  Traineeshipid= t.ID,
                  TraineeShipStartDate=t.StartDate,
                  TraineeShipPrice=t.Price,
                  TraineeShipEndDate=t.EndDate,
                  TraineeshipIsActive=t.IsActive,
                  LicenseId = t.LicenseID,
                  License = t.License
              })
              .FirstOrDefaultAsync(p => p.Traineeshipid == id);

            //var pilotDto = pilot.MapPilotDto();

            return traineeship;
        }
        public async Task<IReadOnlyCollection<TraineeShipSortByPilotLicenseDto>> GetAllTraineeShipSortedByPilotLicense(int pilotId)
        {
            int pilotMaxDiffuculty = _paraContext.Pilots.AsNoTracking()
                .Where(p => p.ID == pilotId)
                .Select(p => p.Possessions.Select(l => l.License.Level.DifficultyIndex)
                .Max())
                .FirstOrDefault();
            
            var traneeShipSortedByPilotLicense = _paraContext.Traineeships
                .AsNoTracking()
                .Where(tl => tl.License.Level.DifficultyIndex == (pilotMaxDiffuculty + 1) && tl.StartDate > DateTime.Today.AddDays(1))
                .Select(t => new TraineeShipSortByPilotLicenseDto
                {
                    LicenseId = t.LicenseID,
                    LicenseTitle = t.License.Title,
                    traineeShipId = t.ID
                });
            return await traneeShipSortedByPilotLicense.ToListAsync();
        }

        public async Task<IReadOnlyCollection<TraineeShipDto>> GetTraineeshipsByPilotAsync(int pilotId)
        {
            var traineeships = _paraContext.Traineeships
                  .AsNoTracking()
                  .Include(t => t.TraineeshipPayments)  
                  .Where(t => t.TraineeshipPayments.Any(tp => tp.PilotID == pilotId))
                  .Select(T => new TraineeShipDto
                  {
                      Traineeshipid = T.ID,
                      TraineeShipStartDate = T.StartDate,
                      TraineeShipPrice = T.Price,
                      TraineeShipEndDate = T.EndDate,
                      TraineeshipIsActive = T.IsActive,
                      LicenseId = T.LicenseID
                  });

            return await traineeships.ToListAsync();
        }

        public void CreateTraineeship(TraineeShipDto pTraineeshipDto)
        {
            _paraContext.Traineeships.Add(new Models.Traineeship
            {
                StartDate = pTraineeshipDto.TraineeShipStartDate,
                EndDate = pTraineeshipDto.TraineeShipEndDate,
                LicenseID = pTraineeshipDto.LicenseId,
                Price = pTraineeshipDto.TraineeShipPrice,
                IsActive = true
            });
            _paraContext.SaveChanges();
        }

        public void EditTraineeship(TraineeShipDto pTraineeshipDto)
        {
            var toModifyAsTraineeship = _paraContext.Traineeships.Select(s => s).Where(s => s.ID == pTraineeshipDto.Traineeshipid).FirstOrDefault();

            toModifyAsTraineeship.StartDate = pTraineeshipDto.TraineeShipStartDate;
            toModifyAsTraineeship.EndDate = pTraineeshipDto.TraineeShipEndDate;
            toModifyAsTraineeship.Price = pTraineeshipDto.TraineeShipPrice;
            toModifyAsTraineeship.LicenseID = pTraineeshipDto.LicenseId;

            _paraContext.Traineeships.Update(toModifyAsTraineeship);
            _paraContext.SaveChanges();
        }

        public void DeleteTraineeship(int pTraineeshipId)
        {
            var toDelete = _paraContext.Traineeships.Select(s => s).Where(s => s.ID == pTraineeshipId).FirstOrDefault();

            toDelete.IsActive = false;
            _paraContext.Traineeships.Update(toDelete);
            _paraContext.SaveChanges();
        }
    }
}

