using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.JsonPatch;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using Newtonsoft.Json.Schema;
using ParaglidingProject.Models;
using ParaglidingProject.SL.Core.Paraglider.NS;
using ParaglidingProject.SL.Core.ParagliderModel.NS;
using ParaglidingProject.SL.Core.ParagliderModel.NS.Helpers;
using ParaglidingProject.SL.Core.ParagliderModel.NS.TransfertObjects;

namespace ParaglidingProject.API.Controllers
{
    /// <summary>
    /// the controller of paragliderModels
    /// </summary>
    [ApiExplorerSettings(GroupName = "paragliderModels")]
    [Route("api/v1/paragliderModels")]
    [Produces("application/json")]
    [ApiController]
    public class ParagliderModelController : ControllerBase
    {
        private readonly IParagliderModelService _ModelParagliderService;
        private readonly IParagliderService _ParagliderService;

        /// <summary>
        /// ParagliderModel interface constructor.
        /// </summary>
        public ParagliderModelController(IParagliderModelService modelparagliderService, IParagliderService paragliderService)
        {
            _ModelParagliderService = modelparagliderService ?? throw new ArgumentNullException(nameof(modelparagliderService));
            _ParagliderService = paragliderService ?? throw new ArgumentNullException(nameof(paragliderService));
        }

        [HttpPost]
        public async Task<ActionResult<ParagliderModelDto>> CreateParagliderModel([FromBody] ParagliderModelDto pParagliderModelDto)
        {
            _ModelParagliderService.CreateParagliderModel(pParagliderModelDto);
            return Ok();
        }
        [HttpPatch("{pParagliderModelId}")]
        public async Task<ActionResult<ParagliderModelDto>> PatchParagliderModelAsync([FromRoute] int pParagliderModelId,[FromBody] JsonPatchDocument<ParagliderModelPatchDto> pParagliderModelPatchDocument)
        {
            var paragliderModelToPatch = await _ModelParagliderService.GetParagliderModelToPatchAsync(pParagliderModelId);
            if (paragliderModelToPatch == null) return NotFound("^ParagliderModel not found");

            pParagliderModelPatchDocument.ApplyTo(paragliderModelToPatch, ModelState);
            if (!TryValidateModel(paragliderModelToPatch)) return ValidationProblem(ModelState);

            var valuesMakeSense = paragliderModelToPatch.ValidateBusinessLogic();
            if (valuesMakeSense == false) return ValidationProblem("Some problem");

            var patchSuccess = await _ModelParagliderService.EditParagliderModel(pParagliderModelId, paragliderModelToPatch);
            return patchSuccess == true ? NoContent() : StatusCode(StatusCodes.Status503ServiceUnavailable);
        }

        [HttpDelete("{pModelId}")]
        public async Task<ActionResult<ParagliderModelDto>> DeleteParagliderModel([FromRoute] int pModelId)
        {
            _ModelParagliderService.DeleteParagliderModel(pModelId);
            return Ok();
        }

        /// <summary>
        /// get a paraglidermodel by id
        /// </summary>
        /// <param name="modelParagliderId">The unique id of a paraglider</param>
        /// <returns>
        /// an actionresult of type 202 who contain a paragliderDto
        /// an actionresult of type 404 if no paraglider was find
        /// <seealso cref="ParagliderModelDto"/>
        /// </returns>
        /// <remarks></remarks>
        [HttpGet("{paragliderModelId}", Name = "GetParagliderModelAsync")]
        public async Task<ActionResult<ParagliderModelAndParagliders>> GetParagliderModelAsync([FromRoute] int paragliderModelId)
        {
            ParagliderModelAndParagliders paragliderModelAndParagliders = new ParagliderModelAndParagliders();

            var modelParaglider = await _ModelParagliderService.GetParagliderModelAsync(paragliderModelId);
            if (modelParaglider == null) return NotFound("Couldn't find any model of paraglider");
            var paragliders = await _ParagliderService.GetParaglidersByModelParaglider(paragliderModelId);
            if (paragliders == null) return NotFound("There is no paragliders on for this model");

            paragliderModelAndParagliders.ParagliderModelDto = modelParaglider;
            paragliderModelAndParagliders.ParagliderDto = paragliders;

            return Ok(paragliderModelAndParagliders);
        }

        /// <summary>
        /// get all paraglidermodels
        /// </summary>
        /// <param name="options"> User options to sort, search, filter pagination </param>
        /// <returns>
        /// an actionresult of type 202 who contain a list of paragliderModelDto
        /// an actionresult of type 404 if no paraglidermodel was find
        /// <seealso cref="ParagliderModelDto"/>
        /// </returns>
        [HttpGet("", Name = "GetAllModelParaglidersAsync")]
        public async Task<ActionResult<IReadOnlyCollection<ParagliderModelDto>>> GetAllModelParaglidersAsync([FromQuery] ParagliderModelsSSFP options)
        {
            var listModelParaglider = await _ModelParagliderService.GetAllParagliderModelsAsync(options);
            if (listModelParaglider == null) return NotFound("There is no model of paraglider ");
            var previousPageLink = options.HasPrevious ? CreateUriParagliderModel(options, RessourceUriType.PreviousPage) : null;
            var nextPageLink = options.HasNext ? CreateUriParagliderModel(options, RessourceUriType.NextPage) : null;
            var paginationMetadata = new
            {
                options.TotalCount,
                options.PageSize,
                options.PageNumber,
                options.TotalPages,
                previousPageLink,
                nextPageLink
            };

            Response.Headers.Add("X-Pagination", JsonSerializer.Serialize(paginationMetadata));
            return Ok(listModelParaglider);
        }
        

        private string CreateUriParagliderModel(ParagliderModelsSSFP options, RessourceUriType type)
        {
            switch (type)
            {
                case RessourceUriType.PreviousPage:
                    return Url.Link("GetAllModelParaglidersAsync",
                        new
                        {
                            PageNumber = options.PageNumber - 1,
                            options.PageSize,
                            options.FilterBy,
                            options.SearchBy,
                            options.Pilotweight
                        });

                case RessourceUriType.NextPage:
                    return Url.Link("GetAllModelParaglidersAsync",
                        new
                        {
                            PageNumber = options.PageNumber + 1,
                            options.PageSize,
                            options.FilterBy,
                            options.SearchBy,
                            options.Pilotweight
                        });

                default:
                    return Url.Link("GetAllModelParaglidersAsync",
                        new
                        {
                            options.PageNumber,
                            options.PageSize,
                            options.FilterBy,
                            options.SearchBy,
                            options.Pilotweight

                        });
            }
        }

       public enum RessourceUriType
        {
            PreviousPage = 0,
            NextPage = 1
        }
    }
}
