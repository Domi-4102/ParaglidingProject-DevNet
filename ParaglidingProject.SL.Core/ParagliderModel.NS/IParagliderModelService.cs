using ParaglidingProject.SL.Core.Paraglider.NS.TransfertObjects;
using ParaglidingProject.SL.Core.ParagliderModel.NS.Helpers;
using ParaglidingProject.SL.Core.ParagliderModel.NS.TransfertObjects;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace ParaglidingProject.SL.Core.ParagliderModel.NS
{
    /// <summary>
    /// ParagliderModel's contract manager
    /// </summary>
  public interface IParagliderModelService
  {
        /// <summary>
        /// An Asynchronous method that returns a ParagliderModelDto
        /// </summary>
        /// <param name="ID">ParagliderModel's ID as an  integer</param>
        /// <returns> 
        ///  The task result contains null by default if source is empty or if no element passes the test specified by predicate.
        /// Otherwise, the task result is a ParagliderModelDto with several properties.
        /// <seealso cref="ParagliderModelDto"/>
        /// </returns>
        Task<ParagliderModelDto> GetParagliderModelAsync(int ID);

        /// <summary>
        /// An Asynchronous method that returns a IReadoOnlyCollection of ParagliderModelDto
        /// </summary>
        /// <param name="options">User options to sort, search, filter pagination</param>
        /// <returns>
        ///  The task result contains an empty list by default if source is empty.
        /// Otherwise, the task result is a Collection of type ParagliderModelDto.
        /// <seealso cref="ParagliderModelDto"/>
        /// </returns>
        Task<IReadOnlyCollection<ParagliderModelDto>> GetAllParagliderModelsAsync(ParagliderModelsSSFP options);

        Task<ParagliderModelPatchDto> GetParagliderModelToPatchAsync(int pParagliderId);

        void CreateParagliderModel(ParagliderModelDto paragliderModelDto);
        Task<bool?> EditParagliderModel(int paragliderId ,ParagliderModelPatchDto paragliderModelPatchDto);
        void DeleteParagliderModel(int id);
  }
}

