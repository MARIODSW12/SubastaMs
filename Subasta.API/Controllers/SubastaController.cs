using System.Text.Json;
using Hangfire;
using Hangfire.MemoryStorage.Database;
using Hangfire.Storage;
using log4net;
using MassTransit;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using RestSharp;
using Subasta.Application.Commands;
using Subasta.Application.DTOs;
using Subasta.Domain.Aggregates;
using Subasta.Domain.Events;
using Subasta.Domain.ValueObjects;
using Subasta.Infrastructure.Dtos;
using Subasta.Infrastructure.Interfaces;
using Subasta.Infrastructure.Queries;
using Subasta.Infrastructure.Services;
using Subasta.Infrastructure.Utils;
using static iText.StyledXmlParser.Jsoup.Select.Evaluator;

namespace Subasta.API.Controllers
{
    [ApiController]
    [Route("api/auction")]
    public class AuctionsController : ControllerBase
    {
        private readonly IMediator _mediator;
        private readonly IPublishEndpoint _publishEndpoint;
        private readonly IRestClient _restClient;
        private readonly ICronJobService cronJobService;


        public AuctionsController(IMediator mediator, IPublishEndpoint publishEndpoint, IRestClient restClient, ICronJobService cronService)
        {
            _mediator = mediator ?? throw new ArgumentNullException(nameof(mediator));
            _publishEndpoint = publishEndpoint ?? throw new ArgumentNullException(nameof(publishEndpoint));
            _restClient = restClient ?? throw new ArgumentNullException(nameof(restClient));
            cronJobService = cronService ?? throw new ArgumentNullException(nameof(cronService));
        }

        #region CreateAuction
        [HttpPost("createAuction")]
        public async Task<IActionResult> CreateAuction([FromBody] CreateAuctionDto auctionDto)
        {

            try
            {
                var APIRequest = new RestRequest(Environment.GetEnvironmentVariable("PRODUCT_MS_URL") + "/GetProductoPorId", Method.Get);
                APIRequest.AddQueryParameter("idProducto", auctionDto.ProductId);
                var APIResponse = await _restClient.ExecuteAsync(APIRequest);
                if (!APIResponse.IsSuccessful)
                {
                    throw new Exception("El producto con ese id no existe.");
                }
                var product = JsonDocument.Parse(APIResponse.Content).Deserialize<ProductInfo>();

                if (product == null)
                {
                    return BadRequest("El producto con ese id no existe.");
                }

                if (product.cantidadProducto < auctionDto.ProductQuantity)
                {
                    return BadRequest("La cantidad del producto no es suficiente para crear la subasta.");
                }
                var auctionId = await _mediator.Send(new CreateAuctionCommand(auctionDto));

                if (auctionId == null)
                {
                    return BadRequest("No se pudo crear la subasta.");
                }
                TimeSpan delay = auctionDto.StartDate.ToUniversalTime() - DateTime.UtcNow;

                var auctionFinish = auctionDto.StartDate.ToUniversalTime().AddMinutes(auctionDto.Duration);
                TimeSpan finishDelay = auctionFinish - DateTime.UtcNow;

                BackgroundJob.Schedule(() => SendStartAuctionCommand(auctionId), delay);

                BackgroundJob.Schedule(() => SendFinishAuctionCommand(auctionId), finishDelay);

                return CreatedAtAction("CreateAuction", new { id = auctionId }, new
                {
                    id = auctionId
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, ex.Message);
            }
        }
        #endregion

        #region ToggleCancelAuction
        [HttpPost("cancelToggleAuction/{id}")]
        public async Task<IActionResult> CancelToggleAuction([FromRoute] string id)
        {
            try
            {
                var auction = await _mediator.Send(new GetAuctionByIdQuery(id));
                if (auction.Status != "pending" && auction.Status != "canceled")
                {
                    return BadRequest("No se puede cancelar una subasta ya iniciada.");
                }
                if (auction.Status == "canceled" && auction.StartDate.CompareTo(DateTime.UtcNow) <= 0)
                {
                    return BadRequest("No se puede iniciar una subasta ya cancelada que paso su fecha de inicio.");
                }
                var updateResponse = _mediator.Send(new ChangeAuctionStatusCommand(
                    new ChangeAuctionStatusDto { AuctionId = id, Status =( auction.Status == "canceled" ? "pending" : "canceled")}));
                return CreatedAtAction("CancelToggleAuction", new { response = (auction.Status == "canceled" ? "pending" : "canceled") }, new
                {
                    response = (auction.Status == "canceled" ? "pending" : "canceled")
                });
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error interno", ex);
                return StatusCode(500, ex.Message);
            }
        }
        #endregion

        #region CompleteAuction
        [HttpPost("completeAuction/{id}")]
        public async Task<IActionResult> CompleteAuction([FromRoute] string id)
        {
            try
            {
                var auction = await _mediator.Send(new GetAuctionByIdQuery(id));
                if (auction.Status != "ended")
                {
                    return BadRequest("No se puede completar una subasta que no este culminada.");
                }
                
                var updateResponse = _mediator.Send(new ChangeAuctionStatusCommand(
                    new ChangeAuctionStatusDto { AuctionId = id, Status = "completed" }));
                var cronId = cronJobService.GetCronJobId(id);
                if (cronId != null && cronId != string.Empty)
                    BackgroundJob.Delete(cronId);
                var APIRequest = new RestRequest(Environment.GetEnvironmentVariable("PRODUCT_MS_URL") + "/GetProductoPorId", Method.Get);
                APIRequest.AddQueryParameter("idProducto", auction.ProductId);
                var APIResponse = await _restClient.ExecuteAsync(APIRequest);
                var product = JsonDocument.Parse(APIResponse.Content).Deserialize<ProductInfo>();
                var APIRequestUpdate = new RestRequest(Environment.GetEnvironmentVariable("PRODUCT_MS_URL") + "/ActualizarProducto/"+ auction.ProductId, Method.Patch);
                APIRequestUpdate.AlwaysMultipartFormData = true;
                APIRequestUpdate.AddParameter("CantidadProducto", product.cantidadProducto - auction.ProductQuantity);
                var APIResponseUpdate = await _restClient.ExecuteAsync(APIRequestUpdate);

                await _mediator.Publish(new NotificationSendEvent(
                [auction.UserId],
                "Subasta completada",
                $"La subasta {auction.Name} ha sido completada ya que se recibio el pago."
                ));
                return CreatedAtAction("CompleteAuction", new { response = "completed" }, new
                {
                    response = "completed"
                });
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error interno: ", ex.Message);
                return StatusCode(500, ex.Message);
            }
        }
        #endregion

        #region UpdateAuction
        [HttpPatch("updateAuction")]
        public async Task<IActionResult> UpdateAuction([FromBody] UpdateAuctionDto auctionDto)
        {

            try
            {
                var auction = await _mediator.Send(new GetAuctionByIdQuery(auctionDto.AuctionId));
                if (auction.Status == "active" || auction.Status == "ended" || auction.Status == "completed")
                {
                    return BadRequest("No se puede actualizar una subasta ya iniciada o  finalizada.");
                }
                var auctionId = await _mediator.Send(new UpdateAuctionCommand(auctionDto));

                if (auctionId == null)
                {
                    return BadRequest("No se pudo actualizar la subasta.");
                }

                if (auctionDto.StartDate != null && auctionDto.StartDate.Value.CompareTo(DateTime.UtcNow) > 0)
                {
                    Console.WriteLine("si");
                    TimeSpan delay = auctionDto.StartDate.Value.ToUniversalTime() - DateTime.UtcNow;

                    var auctionFinish = auctionDto.StartDate.Value.ToUniversalTime()
                        .AddMinutes(auctionDto.Duration == null ? auction.Duration : auctionDto.Duration.Value);
                    TimeSpan finishDelay = auctionFinish - DateTime.UtcNow;

                    BackgroundJob.Schedule(() => SendStartAuctionCommand(auctionId), delay);

                    BackgroundJob.Schedule(() => SendFinishAuctionCommand(auctionId), finishDelay);
                }

                return CreatedAtAction("UpdateAuction", new { id = auctionId }, new
                {
                    id = auctionId
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, ex.Message);
            }
        }
        #endregion

        #region DeleteAuction
        [HttpDelete("deleteAuction/{id}/{userId}")]
        public async Task<IActionResult> DeleteAuction([FromRoute] string id, [FromRoute] string userId)
        {
            try
            {
                var result = await _mediator.Send(new DeleteAuctionCommand(new DeleteAuctionDto{AuctionId = id, UserId = userId }));
                if (result == "No hay subasta con ese id del usuario")
                {
                    return BadRequest("No se puede eliminar una subasta que no es del usuario o no existe.");
                }

                
                return Ok(new
                {
                    response = result
                });
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error interno", ex);
                return StatusCode(500, ex.Message);
            }
        }
        #endregion

        #region GetById
        [HttpGet("getById/{id}")]
        public async Task<IActionResult> GetAuctionById([FromRoute] string id)
        {
            try
            {
                var auction = await _mediator.Send(new GetAuctionByIdQuery(id));
                
                if (auction == null)
                {
                    return NotFound("Subasta no encontrada.");
                }

                var APIRequestProduct = new RestRequest(Environment.GetEnvironmentVariable("PRODUCT_MS_URL") + "/GetProductoPorId", Method.Get);
                APIRequestProduct.AddQueryParameter("idProducto", auction.ProductId);
                var APIResponseProduct = await _restClient.ExecuteAsync(APIRequestProduct);
                ProductInfo? product = null;
                if (APIResponseProduct.IsSuccessful)
                    product = JsonDocument.Parse(APIResponseProduct.Content).Deserialize<ProductInfo>();

                var APIRequestBids = new RestRequest(Environment.GetEnvironmentVariable("BID_MS_URL") + "/getAuctionBids/" + auction.Id, Method.Get);
                var APIResponseBids = await _restClient.ExecuteAsync(APIRequestBids);
                List<GetBidDto> bids = null;
                Console.WriteLine($"APIResponseBids: {APIResponseBids.Content}");
                if (APIResponseBids.IsSuccessful)
                    bids = JsonDocument.Parse(APIResponseBids.Content).Deserialize<List<GetBidDto>>();

                var APIRequestLastBids = new RestRequest(Environment.GetEnvironmentVariable("BID_MS_URL") + "/getAuctionLastBid/" + auction.Id, Method.Get);
                var APIResponseLastBids = await _restClient.ExecuteAsync(APIRequestLastBids);
                Console.WriteLine($"APIResponseLastBids: {APIResponseLastBids.Content}");
                GetBidDto lastBid = null;
                if (APIResponseLastBids.IsSuccessful && APIResponseLastBids.Content != null && APIResponseLastBids.Content != "")
                    lastBid = JsonDocument.Parse(APIResponseLastBids.Content).Deserialize<GetBidDto>();

                var returnAuction = new
                {
                    Id = auction.Id,
                    UserId = auction.UserId,
                    ProductId = auction.ProductId,
                    ProductQuantity = auction.ProductQuantity,
                    Name = auction.Name,
                    Description = auction.Description,
                    Status = auction.Status,
                    BasePrice = auction.BasePrice,
                    Duration = auction.Duration,
                    MinimumIncrease = auction.MinimumIncrease,
                    ReservePrice = auction.ReservePrice,
                    StartDate = auction.StartDate.ToUniversalTime(),
                    ProductName = product == null ? "producto" : product.nombreProducto,
                    ProductImage = product == null ? "" : product.imagenProducto,
                    bids = bids ?? new List<GetBidDto>(),
                    lastBid = lastBid ?? null,
                };

                return Ok(returnAuction);
            }
            catch (Exception ex)
            {
                if (ex.Message == "Subasta no encontrada.")
                    return NotFound("Subasta no encontrada.");
                return StatusCode(500, ex.Message);
            }
        }
        #endregion

        #region GetUserAuctions
        [HttpGet("getUserAuctions/{userId}")]
        public async Task<IActionResult> GetUserAuctions([FromRoute] string userId)
        {
            try
            {
                var auctions = await _mediator.Send(new GetUserAuctionsQuery(userId));
                var responseAuctions = new List<GetAuctionDto>();
                foreach (var auction in auctions)
                {
                    var APIRequest = new RestRequest(Environment.GetEnvironmentVariable("PRODUCT_MS_URL") + "/GetProductoPorId", Method.Get);
                    APIRequest.AddQueryParameter("idProducto", auction.ProductId);
                    var APIResponse = await _restClient.ExecuteAsync(APIRequest);
                    ProductInfo? product = null;
                    if (APIResponse.IsSuccessful)
                        product = JsonDocument.Parse(APIResponse.Content).Deserialize<ProductInfo>();


                    var auctionDto = new GetAuctionDto
                    {
                        Id = auction.Id,
                        UserId = auction.UserId,
                        ProductId = auction.ProductId,
                        ProductQuantity = auction.ProductQuantity,
                        Name = auction.Name,
                        Description = auction.Description,
                        Status = auction.Status,
                        BasePrice = auction.BasePrice,
                        Duration = auction.Duration,
                        MinimumIncrease = auction.MinimumIncrease,
                        ReservePrice = auction.ReservePrice,
                        StartDate = auction.StartDate.ToUniversalTime(),
                        ProductName = product == null ? "producto" : product.nombreProducto,
                        ProductImage = product == null ? "" : product.imagenProducto
                    };
                    responseAuctions.Add(auctionDto);
                }

                return Ok(responseAuctions);
            }
            catch (Exception ex)
            {
                return StatusCode(500, ex.Message);
            }
        }
        #endregion

        #region GetActiveAuctions
        [HttpGet("getActiveAuctions")]
        public async Task<IActionResult> GetActiveAuctions()
        {
            try
            {
                var auctions = await _mediator.Send(new GetAuctionsByStatusQuery("active"));
                var responseAuctions = new List<GetAuctionDto>();
                foreach (var auction in auctions)
                {
                    var APIRequest = new RestRequest(Environment.GetEnvironmentVariable("PRODUCT_MS_URL") + "/GetProductoPorId", Method.Get);
                    APIRequest.AddQueryParameter("idProducto", auction.ProductId);
                    var APIResponse = await _restClient.ExecuteAsync(APIRequest);
                    ProductInfo? product = null;
                    if (APIResponse.IsSuccessful)
                        product = JsonDocument.Parse(APIResponse.Content).Deserialize<ProductInfo>();

                    var auctionDto = new GetAuctionDto
                    {
                        Id = auction.Id,
                        UserId = auction.UserId,
                        ProductId = auction.ProductId,
                        ProductQuantity = auction.ProductQuantity,
                        Name = auction.Name,
                        Description = auction.Description,
                        Status = auction.Status,
                        BasePrice = auction.BasePrice,
                        Duration = auction.Duration,
                        MinimumIncrease = auction.MinimumIncrease,
                        ReservePrice = auction.ReservePrice,
                        StartDate = auction.StartDate.ToUniversalTime(),
                        ProductName = product == null ? "producto" : product.nombreProducto,
                        ProductImage = product == null ? "" : product.imagenProducto
                    };
                    responseAuctions.Add(auctionDto);
                }
                return Ok(responseAuctions);
            }
            catch (Exception ex)
            {
                return StatusCode(500, ex.Message);
            }
        }
        #endregion

        #region GetPendingAuctions
        [HttpGet("getPendingAuctions")]
        public async Task<IActionResult> GetPendingAuctions()
        {
            try
            {
                var auctions = await _mediator.Send(new GetAuctionsByStatusQuery("pending"));
                var responseAuctions = new List<GetAuctionDto>();
                foreach (var auction in auctions)
                {
                    var APIRequest = new RestRequest(Environment.GetEnvironmentVariable("PRODUCT_MS_URL") + "/GetProductoPorId", Method.Get);
                    APIRequest.AddQueryParameter("idProducto", auction.ProductId);
                    var APIResponse = await _restClient.ExecuteAsync(APIRequest);
                    ProductInfo? product = null;
                    if (APIResponse.IsSuccessful)
                        product = JsonDocument.Parse(APIResponse.Content).Deserialize<ProductInfo>();

                    var auctionDto = new GetAuctionDto
                    {
                        Id = auction.Id,
                        UserId = auction.UserId,
                        ProductId = auction.ProductId,
                        ProductQuantity = auction.ProductQuantity,
                        Name = auction.Name,
                        Description = auction.Description,
                        Status = auction.Status,
                        BasePrice = auction.BasePrice,
                        Duration = auction.Duration,
                        MinimumIncrease = auction.MinimumIncrease,
                        ReservePrice = auction.ReservePrice,
                        StartDate = auction.StartDate.ToUniversalTime(),
                        ProductName = product == null ? "producto" : product.nombreProducto,
                        ProductImage = product == null ? "" : product.imagenProducto
                    };
                    responseAuctions.Add(auctionDto);
                }
                return Ok(responseAuctions);
            }
            catch (Exception ex)
            {
                return StatusCode(500, ex.Message);
            }
        }
        #endregion

        #region GetProductAuctions
        [HttpGet("getProductAuctions/{productId}")]
        public async Task<IActionResult> GetPeroductAuctions([FromRoute] string productId)
        {
            try
            {
                var auctions = await _mediator.Send(new GetProductAuctionsQuery(productId));
                var responseAuctions = new List<GetAuctionDto>();
                foreach (var auction in auctions)
                {
                    var APIRequest = new RestRequest(Environment.GetEnvironmentVariable("PRODUCT_MS_URL") + "/GetProductoPorId", Method.Get);
                    APIRequest.AddQueryParameter("idProducto", auction.ProductId);
                    var APIResponse = await _restClient.ExecuteAsync(APIRequest);
                    ProductInfo? product = null;
                    if (APIResponse.IsSuccessful)
                        product = JsonDocument.Parse(APIResponse.Content).Deserialize<ProductInfo>();

                    var auctionDto = new GetAuctionDto
                    {
                        Id = auction.Id,
                        UserId = auction.UserId,
                        ProductId = auction.ProductId,
                        ProductQuantity = auction.ProductQuantity,
                        Name = auction.Name,
                        Description = auction.Description,
                        Status = auction.Status,
                        BasePrice = auction.BasePrice,
                        Duration = auction.Duration,
                        MinimumIncrease = auction.MinimumIncrease,
                        ReservePrice = auction.ReservePrice,
                        StartDate = auction.StartDate.ToUniversalTime(),
                        ProductName = product == null ? "producto" : product.nombreProducto,
                        ProductImage = product == null ? "" : product.imagenProducto
                    };
                    responseAuctions.Add(auctionDto);
                }
                return Ok(responseAuctions);
            }
            catch (Exception ex)
            {
                return StatusCode(500, ex.Message);
            }
        }
        #endregion

        #region GetAuctionsRange
        [HttpGet("getAuctionsRange/{from}/{to}")]
        public async Task<IActionResult> GetAuctionsRange([FromRoute] DateTime from, [FromRoute] DateTime to)
        {
            try
            {
                var auctions = await _mediator.Send(new GetAuctionsInRangeQuery(from, to));
                var responseAuctions = new List<GetAuctionReportDto>();
                foreach (var auction in auctions)
                {
                    var APIRequest = new RestRequest(Environment.GetEnvironmentVariable("PRODUCT_MS_URL") + "/GetProductoPorId", Method.Get);
                    APIRequest.AddQueryParameter("idProducto", auction.ProductId);
                    var APIResponse = await _restClient.ExecuteAsync(APIRequest);
                    ProductInfo? product = null;
                    if (APIResponse.IsSuccessful)
                        product = JsonDocument.Parse(APIResponse.Content).Deserialize<ProductInfo>();


                    var APIRequestLastBid = new RestRequest(Environment.GetEnvironmentVariable("BID_MS_URL") + "/getAuctionLastBid/" + auction.Id, Method.Get);
                    var APIResponseLastBid = await _restClient.ExecuteAsync(APIRequestLastBid);
                    GetBidDto? lastBid;
                    if (!APIResponseLastBid.IsSuccessful || APIResponseLastBid.Content == "")
                    {
                        lastBid = null;
                    }
                    else
                        lastBid = JsonDocument.Parse(APIResponseLastBid.Content).Deserialize<GetBidDto>();


                    var APIRequestParticipants = new RestRequest(Environment.GetEnvironmentVariable("BID_MS_URL") + "/getAuctionPostors/" + auction.Id, Method.Get);
                    var APIResponseParticipants = await _restClient.ExecuteAsync(APIRequestParticipants);
                    if (!APIResponseParticipants.IsSuccessful)
                    {
                        return BadRequest("No se pudo obtener los participantes de la subasta.");
                    }
                    var participants = JsonDocument.Parse(APIResponseParticipants.Content).Deserialize<List<string>>();


                    var auctionDto = new GetAuctionReportDto
                    {
                        Id = auction.Id,
                        Name = auction.Name,
                        StartDate = auction.StartDate.ToUniversalTime(),
                        ProductName = product == null ? "producto" : product.nombreProducto,
                        FinalPrice = lastBid != null ? lastBid.price : 0,
                        Participants = participants,
                    };
                    responseAuctions.Add(auctionDto);
                }
                return Ok(responseAuctions);
            }
            catch (Exception ex)
            {
                return StatusCode(500, ex.Message);
            }
        }
        #endregion

        #region GetUserParticipateAuctions
        [HttpGet("GetUserParticipateAuctions/{userId}")]
        public async Task<IActionResult> GetUserParticipateAuctions([FromRoute] string userId, [FromQuery] DateTime? from, [FromQuery] DateTime? to, [FromQuery] string? status)
        {
            try
            {
                if (from.HasValue && to.HasValue && from.Value > to.Value)
                {
                    return BadRequest("La fecha de inicio no puede ser mayor que la fecha de fin.");
                }
                if ((from.HasValue && !to.HasValue) || (!from.HasValue && to.HasValue))
                {
                    return BadRequest("Debe proporcionar ambas fechas o ninguna para filtrar por fecha.");
                }
                var validStatuses = new List<string> { "deserted", "ended", "completed" };
                if (status != null && !validStatuses.Contains(status.ToLower()))
                {
                    return BadRequest("El estado de la subasta debe ser uno de los siguientes: deserted, ended, completed.");
                }
                var responseAuctions = await fetchUserParticipateAuctionsData(userId, from, to, status);

                return Ok(responseAuctions);
            }
            catch (Exception ex)
            {
                return StatusCode(500, ex.Message);
            }
        }
        #endregion

        #region GetUserWinnedAuctions
        [HttpGet("GetUserWinnedAuctions/{userId}")]
        public async Task<IActionResult> GetUserWinnedAuctions([FromRoute] string userId, [FromQuery] DateTime? from, [FromQuery] DateTime? to, [FromQuery] string? status)
        {
            try
            {
                if (from.HasValue && to.HasValue && from.Value > to.Value)
                {
                    return BadRequest("La fecha de inicio no puede ser mayor que la fecha de fin.");
                }
                if ((from.HasValue && !to.HasValue) || (!from.HasValue && to.HasValue))
                {
                    return BadRequest("Debe proporcionar ambas fechas o ninguna para filtrar por fecha.");
                }
                var validStatuses = new List<string> { "ended", "completed" };
                if (status != null && !validStatuses.Contains(status.ToLower()))
                {
                    return BadRequest("El estado de la subasta debe ser uno de los siguientes: ended, completed.");
                }
                var responseAuctions = await fetchUserWinnedAuctionsData(userId, from, to, status);

                return Ok(responseAuctions);
            }
            catch (Exception ex)
            {
                return StatusCode(500, ex.Message);
            }
        }
        #endregion

        #region GetUserParticipateBids
        [HttpGet("GetUserParticipateBids/{userId}")]
        public async Task<IActionResult> GetUserParticipateBids([FromRoute] string userId, [FromQuery] DateTime? from, [FromQuery] DateTime? to, [FromQuery] string? auctionId)
        {
            try
            {
                if (from.HasValue && to.HasValue && from.Value > to.Value)
                {
                    return BadRequest("La fecha de inicio no puede ser mayor que la fecha de fin.");
                }
                if ((from.HasValue && !to.HasValue) || (!from.HasValue && to.HasValue))
                {
                    return BadRequest("Debe proporcionar ambas fechas o ninguna para filtrar por fecha.");
                }
                var userBidsResponse = await fetchUserParticipateBidsData(userId, from, to, auctionId);

                return Ok(userBidsResponse);
            }
            catch (Exception ex)
            {
                return StatusCode(500, ex.Message);
            }
        }
        #endregion

        #region GetUserParticipateBidsExport
        [HttpGet("GetUserParticipateBidsExport/{userEmail}/{pdf}")]
        public async Task<IActionResult> GetUserParticipateBidsExport([FromRoute] string userEmail, [FromRoute] bool pdf, [FromQuery] DateTime? from, [FromQuery] DateTime? to, [FromQuery] string? auctionId)
        {
            try
            {
                if (from.HasValue && to.HasValue && from.Value > to.Value)
                {
                    return BadRequest("La fecha de inicio no puede ser mayor que la fecha de fin.");
                }
                if ((from.HasValue && !to.HasValue) || (!from.HasValue && to.HasValue))
                {
                    return BadRequest("Debe proporcionar ambas fechas o ninguna para filtrar por fecha.");
                }
                var userInfo = await fetchUserInfoData(userEmail);
                var userBidsResponse = await fetchUserParticipateBidsData(userInfo.Id, from, to, auctionId);

                var Data = new ExportDataDto
                {
                    rowTitles = new List<string> { "Subasta", "Estado Subasta", "Fecha", "Monto Puja" },
                    rows = userBidsResponse.Select(b => new List<string>
                    {
                        b.AuctionName,
                        b.Status,
                        b.Date.ToString("yyyy-MM-dd HH:mm:ss"),
                        b.Price.ToString("F2"),
                    }).ToList(),
                    title = "Pujas que realizó el Usuario"
                };
                if (pdf)
                {
                    var pdfBytes = await _mediator.Send(new PdfGeneratorCommand(userInfo, Data));
                    return File(pdfBytes, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", $"user_participate_bids-{DateTime.Now:yyyy-MM-dd}.pdf");
                }
                else
                {
                    var excelBytes = await _mediator.Send(new ExcelGeneratorCommand(userInfo, Data));
                    return File(excelBytes, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", $"user_participate_bids-{DateTime.Now:yyyy-MM-dd}.xlsx");
                }
            }
            catch (Exception ex)
            {
                return StatusCode(500, ex.Message);
            }
        }
        #endregion

        #region GetUserParticipateAuctionsExport
        [HttpGet("GetUserParticipateAuctionsExport/{userEmail}/{pdf}")]
        public async Task<IActionResult> GetUserParticipateAuctionsExport([FromRoute] string userEmail, [FromRoute] bool pdf, [FromQuery] DateTime? from, [FromQuery] DateTime? to, [FromQuery]string? status)
        {
            try
            {
                if (from.HasValue && to.HasValue && from.Value > to.Value)
                {
                    return BadRequest("La fecha de inicio no puede ser mayor que la fecha de fin.");
                }
                if ((from.HasValue && !to.HasValue) || (!from.HasValue && to.HasValue))
                {
                    return BadRequest("Debe proporcionar ambas fechas o ninguna para filtrar por fecha.");
                }
                var validStatuses = new List<string> { "deserted", "ended", "completed" };
                if (status != null && !validStatuses.Contains(status.ToLower()))
                {
                    return BadRequest("El estado de la subasta debe ser uno de los siguientes: deserted, ended, completed.");
                }
                var userInfo = await fetchUserInfoData(userEmail);
                var userAuctionsResponse = await fetchUserParticipateAuctionsData(userInfo.Id, from, to, status);

                var bidData = new List<List<string>>();
                foreach (var auction in userAuctionsResponse)
                {
                    var bids = auction.Bids.Select(b => new List<string>
                    {
                        auction.Name,
                        b.date.ToString("yyyy-MM-dd HH:mm:ss"),
                        b.price.ToString("F2"),
                        auction.WinnerBid != null && auction.Status != "deserted" && auction.WinnerBid.id == b.id ? "Sí" : "No"
                    }).ToList();
                    bidData.AddRange(bids);
                    Console.WriteLine($"Bids data: {string.Join(", ", bidData.Select(b => string.Join(" | ", b)))}");
                }
                var Data = new ExportDataDto
                {
                    rowTitles = new List<string> { "Subasta", "Fecha Puja", "Monto Puja", "Puja Ganadora"},
                    rows = bidData,
                    title = "Subasta en las que participó el Usuario",
                    groupBy ="Subasta"
                };
                if (pdf)
                {
                    var pdfBytes = await _mediator.Send(new PdfGeneratorCommand(userInfo, Data));
                    return File(pdfBytes, "application/pdf", $"user_participate_auctions-{DateTime.Now:yyyy-MM-dd}.pdf");
                }
                else
                {
                    var excelBytes = await _mediator.Send(new ExcelGeneratorCommand(userInfo, Data));
                    return File(excelBytes, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", $"user_participate_auctions-{DateTime.Now:yyyy-MM-dd}.xlsx");
                }
            }
            catch (Exception ex)
            {
                return StatusCode(500, ex.Message);
            }
        }
        #endregion

        #region GetUserWinnedBidsExport
        [HttpGet("GetUserWinnedBidsExport/{userEmail}/{pdf}")]
        public async Task<IActionResult> GetUserWinnedBidsExport([FromRoute] string userEmail, [FromRoute] bool pdf, [FromQuery] DateTime? from, [FromQuery] DateTime? to, [FromQuery]string? status)
        {
            try
            {
                if (from.HasValue && to.HasValue && from.Value > to.Value)
                {
                    return BadRequest("La fecha de inicio no puede ser mayor que la fecha de fin.");
                }
                if ((from.HasValue && !to.HasValue) || (!from.HasValue && to.HasValue))
                {
                    return BadRequest("Debe proporcionar ambas fechas o ninguna para filtrar por fecha.");
                }
                var validStatuses = new List<string> { "ended", "completed" };
                if (status != null && !validStatuses.Contains(status.ToLower()))
                {
                    return BadRequest("El estado de la subasta debe ser uno de los siguientes: ended, completed.");
                }
                var userInfo = await fetchUserInfoData(userEmail);
                var userBidsResponse = await fetchUserWinnedAuctionsData(userInfo.Id, from, to, status);

                var Data = new ExportDataDto
                {
                    rowTitles = new List<string> { "Subasta", "Fecha", "Producto", "Precio Final" },
                    rows = userBidsResponse.Select(a => new List<string>
                    {
                        a.Name,
                        a.StartDate.ToString("yyyy-MM-dd HH:mm:ss"),
                        a.ProductName,
                        a.WinnerBid.price.ToString("F2")
                    }).ToList(),
                    title = "Subastas ganadas por el Usuario"
                };
                if (pdf)
                {
                    var pdfBytes = await _mediator.Send(new PdfGeneratorCommand(userInfo, Data));
                    return File(pdfBytes, "application/pdf", $"user_winned_auctions-{DateTime.Now:yyyy-MM-dd}.pdf");
                }
                else
                {
                    var excelBytes = await _mediator.Send(new ExcelGeneratorCommand(userInfo, Data));
                    return File(excelBytes, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", $"user_winned_auctions-{DateTime.Now:yyyy-MM-dd}.xlsx");
                }
            }
            catch (Exception ex)
            {
                return StatusCode(500, ex.Message);
            }
        }
        #endregion

        #region CreatePrizeClaim
        [HttpPost("createPrizeClaim")]
        public async Task<IActionResult> CreatePrizeClaim([FromBody] CreatePrizeClaimDto prizeClaimDto)
        {

            try
            {
                var auction = await _mediator.Send(new GetAuctionByIdQuery(prizeClaimDto.AuctionId));
                if (auction == null)
                {
                    return NotFound("Subasta no encontrada.");
                }
                if (auction.Status != "completed")
                {
                    return BadRequest("No se puede reclamar un premio de una subasta que no ha sido completada.");
                }

                var prizeClaim = await _mediator.Send(new GetPrizeClaimByUserAndAuctionQuery(prizeClaimDto.UserId, prizeClaimDto.AuctionId));

                if (prizeClaim != null)
                {
                    return NotFound("Ya hay un reclamo de premio creado para el usuario en la subasta.");
                }

                var APIRequest = new RestRequest(Environment.GetEnvironmentVariable("BID_MS_URL") + "/getAuctionLastBid/" + prizeClaimDto.AuctionId, Method.Get);
                var APIResponse = await _restClient.ExecuteAsync(APIRequest);
                if (!APIResponse.IsSuccessful)
                {
                    return BadRequest("No se pudo obtener la última puja de la subasta.");
                }
                var lastBid = JsonDocument.Parse(APIResponse.Content).Deserialize<GetBidDto>();

                if (lastBid.userId != prizeClaimDto.UserId)
                {
                    return BadRequest("El usuario no es el ganador de la subasta.");
                }

                var prizeClaimId = await _mediator.Send(new CreatePrizeClaimCommand(prizeClaimDto));
                return CreatedAtAction("CreatePrizeClaim", new { id = prizeClaimId }, new
                {
                    id = prizeClaimId
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, ex.Message);
            }
        }
        #endregion

        #region ConfirmPrizeClaim
        [HttpPost("confirmPrizeClaim/{auctionId}/{userId}")]
        public async Task<IActionResult> ConfirmPrizeClaim([FromRoute] string auctionId, [FromRoute] string userId)
        {
            try
            {
                var auction = await _mediator.Send(new GetAuctionByIdQuery(auctionId));
                if (auction.Status != "completed")
                {
                    return BadRequest("No se puede marcar como entregado el premio de una subasta que no este culminada.");
                }

                var prizeClaim = await _mediator.Send(new GetPrizeClaimByUserAndAuctionQuery(userId, auctionId));

                if (prizeClaim == null)
                {
                    return NotFound("No se encontró una reclamación de premio para el usuario y la subasta especificados.");
                }

                var updateResponse = _mediator.Send(new ChangeAuctionStatusCommand(
                    new ChangeAuctionStatusDto { AuctionId = auctionId, Status = "delivered" }));
                
                return CreatedAtAction("ConfirmPrizeClaim", new { response = "delivered" }, new
                {
                    response = "delivered"
                });
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error interno", ex);
                return StatusCode(500, ex.Message);
            }
        }
        #endregion

        #region GetPrizeClaim
        [HttpGet("getPrizeClaim/{auctionId}/{userId}")]
        public async Task<IActionResult> GetPrizeClaimByUserAndAuction([FromRoute] string auctionId, [FromRoute] string userId)
        {
            try
            {
                var prizeClaim = await _mediator.Send(new GetPrizeClaimByUserAndAuctionQuery(userId, auctionId));

                if (prizeClaim == null)
                {
                    return NotFound("No se encontró una reclamación de premio para el usuario y la subasta especificados.");
                }

                return Ok(prizeClaim);
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error interno", ex);
                return StatusCode(500, ex.Message);
            }
        }
        #endregion

        [ApiExplorerSettings(IgnoreApi = true)]
        async public Task SendStartAuctionCommand(string auctionId)
        {
            var auction = await _mediator.Send(new GetAuctionByIdQuery(auctionId));

            if (auction.Status != "pending")
            {
                Console.WriteLine($"No se puede iniciar una subasta cancelada");
                return;
            }
            var APIRequest = new RestRequest(Environment.GetEnvironmentVariable("BID_MS_URL") + "/AuctionInitiated/"+auctionId+"/"+auction.MinimumIncrease+"/"+auction.BasePrice, Method.Post);
            var APIResponse = await _restClient.ExecuteAsync(APIRequest);
            await _mediator.Send(new StartAuctionCommand(auctionId));
            
        }

        [ApiExplorerSettings(IgnoreApi = true)]
        async public Task SendFinishAuctionCommand(string auctionId)
        {
            var auction = await _mediator.Send(new GetAuctionByIdQuery(auctionId));
            if (auction.Status != "active")
            {
                Console.WriteLine($"No se puede finalizar una subasta no iniciada");
                return;
            }
            var APIRequestEnd = new RestRequest(Environment.GetEnvironmentVariable("BID_MS_URL") + "/AuctionEnded/" + auctionId, Method.Post);
            var APIResponseEnd = await _restClient.ExecuteAsync(APIRequestEnd);
            var APIRequest = new RestRequest(Environment.GetEnvironmentVariable("BID_MS_URL") + "/getAuctionLastBid/" + auctionId, Method.Get);
            var status = "";
            var APIResponse = await _restClient.ExecuteAsync(APIRequest);
            GetBidDto bid = null;
            if (!APIResponse.IsSuccessful || APIResponse.Content == null || APIResponse.Content == "")
            {
                status = "deserted";
            } else
            {
                Console.WriteLine($"APIResponse.Content Finish Auction: {APIResponse.Content}");
                bid = JsonDocument.Parse(APIResponse.Content).Deserialize<GetBidDto>();
                if (bid.price < auction.ReservePrice)
                    status = "deserted";
                else
                    status = "ended";
            }

            await _mediator.Send(new ChangeAuctionStatusCommand(new ChangeAuctionStatusDto { AuctionId = auctionId, Status = status }));

            if (status == "ended")
            {
                var APIRequestProduct = new RestRequest(Environment.GetEnvironmentVariable("PRODUCT_MS_URL") + "/GetProductoPorId", Method.Get);
                APIRequestProduct.AddQueryParameter("idProducto", auction.ProductId);
                var APIResponseProduct = await _restClient.ExecuteAsync(APIRequestProduct);
                ProductInfo? product = null;
                if (APIResponseProduct.IsSuccessful)
                    product = JsonDocument.Parse(APIResponseProduct.Content).Deserialize<ProductInfo>();
                var APIRequestUser = new RestRequest(Environment.GetEnvironmentVariable("USER_MS_URL") + "/getuserbyid/" + bid.userId, Method.Get);
                var APIResponseUser = await _restClient.ExecuteAsync(APIRequestUser);
                if (!APIResponseUser.IsSuccessful)
                {
                    throw new Exception("Error al obtener la información del usuario.");
                }
                var user = JsonDocument.Parse(APIResponseUser.Content);
                await _mediator.Publish(new NotificationSendEvent(
                [auction.UserId, bid.userId],
                "Subasta Finalizada",
                $"La subasta {auction.Name} ha sido finalizada, el usuario {user.RootElement.GetProperty("name").GetString() + " " + user.RootElement.GetProperty("lastName")} ha sido el ganador de {auction.ProductQuantity} unidades de {(product == null ? "producto" : product.nombreProducto)}. Se requiere que el ganador realice el pago antes de los proximos {Environment.GetEnvironmentVariable("TO_CANCEL_TIME")} minutos, de lo contrario se cancelara la subasta"
                ));
            }

            var auctionCancelTime = DateTime.UtcNow.AddMinutes((double.Parse(Environment.GetEnvironmentVariable("TO_CANCEL_TIME"))));
            TimeSpan finishDelay = auctionCancelTime - DateTime.UtcNow;

            var cronId = BackgroundJob.Schedule(() => SendCancelAuctionCommand(auctionId), finishDelay);
            cronJobService.AddCronJob(auctionId, cronId);
        }

        [ApiExplorerSettings(IgnoreApi = true)]
        async public Task SendCancelAuctionCommand(string auctionId)
        {
            var auction = await _mediator.Send(new GetAuctionByIdQuery(auctionId));

            if (auction.Status != "ended")
            {
                Console.WriteLine($"No se puede cancelar una subasta no terminada");
                return;
            }

            await _mediator.Send(new ChangeAuctionStatusCommand(new ChangeAuctionStatusDto { AuctionId = auctionId, Status = "deserted" }));
            var APIRequestLastBids = new RestRequest(Environment.GetEnvironmentVariable("BID_MS_URL") + "/getAuctionLastBid/" + auction.Id, Method.Get);
            var APIResponseLastBids = await _restClient.ExecuteAsync(APIRequestLastBids);
            GetBidDto lastBid = null;
            if (APIResponseLastBids.IsSuccessful)
                lastBid = JsonDocument.Parse(APIResponseLastBids.Content).Deserialize<GetBidDto>();
            Console.WriteLine($"Enviando Notificacion a: {lastBid.userId}");
            await _mediator.Publish(new NotificationSendEvent(
                [auction.UserId, lastBid.userId],
                "Subasta cancelada",
                $"La subasta {auction.Name} ha sido cancelada porque no se pago antes del tiempo limite."
            ));
        }

        [ApiExplorerSettings(IgnoreApi = true)]
        async public Task DeleteCronJob(string methodName)
        {
            var monitoringApi = JobStorage.Current.GetMonitoringApi();

            var scheduledJobs = monitoringApi.ScheduledJobs(0, int.MaxValue);
            foreach (var job in scheduledJobs)
            {
                if (job.Value.Job.Method.Name == methodName)
                {
                    BackgroundJob.Delete(job.Key);
                }
            }
        }

        [ApiExplorerSettings(IgnoreApi = true)]
        async public Task<UserInfoDto> fetchUserInfoData(string userEmail)
        {
            var APIRequest = new RestRequest(Environment.GetEnvironmentVariable("USER_MS_URL") + "/getuserbyemail", Method.Get);
            APIRequest.AddParameter("email", userEmail);
            var APIResponse = await _restClient.ExecuteAsync(APIRequest);
            if (!APIResponse.IsSuccessful)
            {
                throw new Exception("Error al obtener la información del usuario.");
            }
            var user = JsonDocument.Parse(APIResponse.Content);

            return new UserInfoDto
            {
                Id = user.RootElement.GetProperty("userId").GetString(),
                Name = user.RootElement.GetProperty("name").GetString() + " " + user.RootElement.GetProperty("lastName"),
                Email = userEmail,
                Address = user.RootElement.GetProperty("address").GetString(),
                Phone = user.RootElement.GetProperty("phone").GetString()
            };
        }

        [ApiExplorerSettings(IgnoreApi = true)]
        async public Task<List<GetUserParticipateBidsDto>> fetchUserParticipateBidsData(string userId, DateTime? from, DateTime? to, string? auctionId)
        {
            var APIRequest = new RestRequest(Environment.GetEnvironmentVariable("BID_MS_URL") + "/GetUserParticipateAuctionBids/" + userId, Method.Get);
            if (from.HasValue)
            {
                APIRequest.AddQueryParameter("from", from.Value.ToString("yyyy-MM-ddTHH:mm:ssZ"));
            }
            if (to.HasValue)
            {
                APIRequest.AddQueryParameter("to", to.Value.ToString("yyyy-MM-ddTHH:mm:ssZ"));
            }
            if (!string.IsNullOrEmpty(auctionId))
            {
                APIRequest.AddQueryParameter("auctionSearchIds", auctionId);
            }
            var APIResponse = await _restClient.ExecuteAsync(APIRequest);
            if (!APIResponse.IsSuccessful)
            {
                Console.WriteLine(APIResponse.Content);
                throw new Exception("Error al obtener las subastas en las que participó el usuario.");
            }
            var bids = JsonDocument.Parse(APIResponse.Content);
            var userBids = bids.RootElement.GetProperty("userBids").Deserialize<List<GetBidDto>>();
            var auctionsIds = userBids
                .Select(b => b.auctionId)
                .Distinct()
                .ToList();
            var auctionIdStatus = new Dictionary<string, AuctionDataDto>();
            foreach (var id in auctionsIds)
            {
                var userAuction = await _mediator.Send(new GetAuctionByIdQuery(id));
                if (userAuction != null)
                {
                    auctionIdStatus[id] = new AuctionDataDto{ Status = userAuction.Status, Name = userAuction.Name};
                }
            }
            var userBidsResponse = new List<GetUserParticipateBidsDto>();
            foreach (var bid in userBids)
            {
                userBidsResponse.Add(new GetUserParticipateBidsDto
                {
                    Id = bid.id,
                    UserId = bid.userId,
                    AuctionId = bid.auctionId,
                    Price = bid.price,
                    Date = bid.date,
                    Status = auctionIdStatus[bid.auctionId].Status,
                    AuctionName = auctionIdStatus[bid.auctionId].Name
                });
            }
            
            return userBidsResponse;
        }

        [ApiExplorerSettings(IgnoreApi = true)]
        async public Task<List<GetUserParticipateAuctionsDto>> fetchUserParticipateAuctionsData(string userId, DateTime? from, DateTime? to, string? status)
        {
            var endedAuctions = await _mediator.Send(new GetAuctionsByStatusesQuery(status != null ? [status] : ["ended", "deserted", "completed"]));
            var auctionIds = endedAuctions
                .Where(a =>
                    (!from.HasValue || a.StartDate >= from.Value) && 
                    (!to.HasValue || a.StartDate <= to.Value))
                .Select(a => a.Id)
                .ToList();
            if (!auctionIds.Any())
            {
                return new List<GetUserParticipateAuctionsDto>();
            }
            var APIRequest = new RestRequest(Environment.GetEnvironmentVariable("BID_MS_URL") + "/GetUserParticipateAuctionBids/" + userId, Method.Get);
            foreach (var auctionId in auctionIds)
            {
                APIRequest.AddQueryParameter("auctionSearchIds", auctionId);
            }
            var APIResponse = await _restClient.ExecuteAsync(APIRequest);
            if (!APIResponse.IsSuccessful)
            {
                throw new Exception("Error al obtener las subastas en las que participó el usuario.");
            }
            var bids = JsonDocument.Parse(APIResponse.Content);
            var auctionsLastBids = bids.RootElement.GetProperty("auctionsLastBid").Deserialize<List<GetBidDto>>();
            var userAuctions = new List<GetAuctionDto>();
            foreach (var auc in auctionsLastBids)
            {
                var userAuction = await _mediator.Send(new GetAuctionByIdQuery(auc.auctionId));
                if (userAuction != null)
                {
                    userAuctions.Add(userAuction);
                }
            }

            var responseAuctions = new List<GetUserParticipateAuctionsDto>();

            foreach (var auction in userAuctions)
            {
                var APIRequestProduct = new RestRequest(Environment.GetEnvironmentVariable("PRODUCT_MS_URL") + "/GetProductoPorId", Method.Get);
                APIRequestProduct.AddQueryParameter("idProducto", auction.ProductId);
                var APIResponseProduct = await _restClient.ExecuteAsync(APIRequestProduct);
                ProductInfo? product = null;
                if (APIResponseProduct.IsSuccessful)
                    product = JsonDocument.Parse(APIResponseProduct.Content).Deserialize<ProductInfo>();
                var lastBid = auctionsLastBids.FirstOrDefault(b => b.auctionId == auction.Id);
                responseAuctions.Add(new GetUserParticipateAuctionsDto
                {
                    Id = auction.Id,
                    UserId = auction.UserId,
                    ProductId = auction.ProductId,
                    ProductName = product == null ? "producto" : product.nombreProducto,
                    ProductQuantity = auction.ProductQuantity,
                    Name = auction.Name,
                    Description = auction.Description,
                    Status = auction.Status,
                    BasePrice = auction.BasePrice,
                    Duration = auction.Duration,
                    MinimumIncrease = auction.MinimumIncrease,
                    ReservePrice = auction.ReservePrice,
                    StartDate = auction.StartDate,
                    WinnerBid = lastBid,
                    Bids = bids.RootElement.GetProperty("userBids").Deserialize<List<GetBidDto>>().Where(b => b.auctionId == auction.Id).ToList()
                });
            }
            return responseAuctions;
        }

        [ApiExplorerSettings(IgnoreApi = true)]
        async public Task<List<GetUserParticipateAuctionsDto>> fetchUserWinnedAuctionsData(string userId, DateTime? from, DateTime? to, string? status)
        {
            var endedAuctions = await _mediator.Send(new GetAuctionsByStatusesQuery(status != null ? [status] : ["ended", "completed"]));
            var auctionIds = endedAuctions
                .Where(a =>
                    (!from.HasValue || a.StartDate >= from.Value) &&
                    (!to.HasValue || a.StartDate <= to.Value))
                .Select(a => a.Id)
                .ToList();
            if (!auctionIds.Any())
            {
                return new List<GetUserParticipateAuctionsDto>();
            }
            var APIRequest = new RestRequest(Environment.GetEnvironmentVariable("BID_MS_URL") + "/GetUserParticipateAuctionBids/" + userId, Method.Get);
            foreach (var auctionId in auctionIds)
            {
                APIRequest.AddQueryParameter("auctionSearchIds", auctionId);
            }
            var APIResponse = await _restClient.ExecuteAsync(APIRequest);
            if (!APIResponse.IsSuccessful)
            {
                throw new Exception("Error al obtener las subastas en las que participó el usuario.");
            }
            var bids = JsonDocument.Parse(APIResponse.Content);
            var auctionsLastBids = bids.RootElement.GetProperty("auctionsLastBid").Deserialize<List<GetBidDto>>();
            var userAuctionsLastBids = auctionsLastBids
                .Where(b => b.userId == userId)
                .ToList();
            var userAuctions = new List<GetAuctionDto>();
            foreach (var auc in userAuctionsLastBids)
            {
                var userAuction = await _mediator.Send(new GetAuctionByIdQuery(auc.auctionId));
                if (userAuction != null)
                {
                    userAuctions.Add(userAuction);
                }
            }

            var responseAuctions = new List<GetUserParticipateAuctionsDto>();

            foreach (var auction in userAuctions)
            {
                var APIRequestProduct = new RestRequest(Environment.GetEnvironmentVariable("PRODUCT_MS_URL") + "/GetProductoPorId", Method.Get);
                APIRequestProduct.AddQueryParameter("idProducto", auction.ProductId);
                var APIResponseProduct = await _restClient.ExecuteAsync(APIRequestProduct);
                ProductInfo? product = null;
                if (APIResponseProduct.IsSuccessful)
                    product = JsonDocument.Parse(APIResponseProduct.Content).Deserialize<ProductInfo>();
                var lastBid = auctionsLastBids.FirstOrDefault(b => b.auctionId == auction.Id);
                responseAuctions.Add(new GetUserParticipateAuctionsDto
                {
                    Id = auction.Id,
                    UserId = auction.UserId,
                    ProductId = auction.ProductId,
                    ProductName = product == null ? "producto" : product.nombreProducto,
                    ProductQuantity = auction.ProductQuantity,
                    Name = auction.Name,
                    Description = auction.Description,
                    Status = auction.Status,
                    BasePrice = auction.BasePrice,
                    Duration = auction.Duration,
                    MinimumIncrease = auction.MinimumIncrease,
                    ReservePrice = auction.ReservePrice,
                    StartDate = auction.StartDate,
                    WinnerBid = lastBid,
                    Bids = []
                });
            }
            return responseAuctions;
        }

    }
}
