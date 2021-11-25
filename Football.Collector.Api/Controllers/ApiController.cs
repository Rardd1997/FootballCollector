using Football.Collector.Api.Extensions;
using Football.Collector.Api.Interfaces;
using Football.Collector.Common.Models;
using Football.Collector.Data.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System;
using System.Net;
using System.Threading.Tasks;

namespace Football.Collector.Api.Controllers
{
    [Authorize]
    [Route("api")]
    [ApiController]
    public class ApiController : ControllerBase
    {
        public const int MaxGamePlayers = 11;
        public const int MinGamePlayers = 4;

        private readonly ITelegramGameRepository gameRepository;
        private readonly ITelegramUserRepository userRepository;
        private readonly ILogger logger;

        public ApiController(ITelegramGameRepository gameRepository, ITelegramUserRepository userRepository, ILogger logger)
        {
            this.gameRepository = gameRepository;
            this.userRepository = userRepository;
            this.logger = logger;
        }

        //Telegram User API
        [HttpPost("telegram_user")]
        public async Task<IActionResult> CreateTelegramUserAsync(CreateTelegramUserRequest request)
        {
            if (request == null)
            {
                return BadRequest();
            }

            if (string.IsNullOrEmpty(request.TelegramId))
            {
                return BadRequest("Telegram Id must be specified");
            }

            if (string.IsNullOrEmpty(request.FirstName))
            {
                return BadRequest("FirstName must be specified");
            }

            try
            {
                var telegramUser = new TelegramUser
                {
                    Id = Guid.NewGuid().ToString("D"),
                    TelegramId = request.TelegramId,
                    Username = request.Username,
                    FirstName = request.FirstName,
                    LastName = request.LastName
                };

                telegramUser = await userRepository.CreateAsync(telegramUser);

                return Ok(telegramUser);
            }
            catch (Exception ex)
            {
                logger.LogError(string.Format("Exception occured while creating a telegram user. Exception: {0}\n{1}", ex.Message, ex.StackTrace));
                return StatusCode((int)HttpStatusCode.InternalServerError);
            }
        }

        [HttpPost("telegram_user/find")]
        public async Task<IActionResult> FindTelegramUserAsync(FindTelegramUserRequest request)
        {
            try
            {
                var user = await userRepository.FindByTelegramIdAsync(request.TelegramId);
                return Ok(user);
            }
            catch (Exception ex)
            {
                logger.LogError(string.Format("Exception occured while finding a telegram user. Exception: {0}\n{1}", ex.Message, ex.StackTrace));
                return StatusCode((int)HttpStatusCode.InternalServerError);
            }
        }

        [HttpPost("telegram_chat_user")]
        public async Task<IActionResult> CreateTelegramChatUserAsync(CreateTelegramChatUserRequest request)
        {
            try
            {
                var telegramChatUser = new TelegramChatUser
                {
                    Id = Guid.NewGuid().ToString("D"),
                    TelegramChatId = request.TelegramChatId,
                    TelegramUserId = request.TelegramUserId,
                    IsAdmin = request.IsAdmin
                };
                telegramChatUser = await userRepository.CreateTelegramChatUserAsync(telegramChatUser);
                return Ok(telegramChatUser);
            }
            catch (Exception ex)
            {
                logger.LogError(string.Format("Exception occured while creating a telegram chat user. Exception: {0}\n{1}", ex.Message, ex.StackTrace));
                return StatusCode((int)HttpStatusCode.InternalServerError);
            }
        }

        [HttpPost("telegram_chat_user/find")]
        public async Task<IActionResult> FindTelegramChatUserAsync(FindTelegramChatUserRequest request)
        {
            try
            {
                var user = await userRepository.FindChatUserAsync(request.TelegramChatId, request.TelegramUserId);
                return Ok(user);
            }
            catch (Exception ex)
            {
                logger.LogError(string.Format("Exception occured while finding a telegram chat user. Exception: {0}\n{1}", ex.Message, ex.StackTrace));
                return StatusCode((int)HttpStatusCode.InternalServerError);
            }
        }


        //Telegram Game API

        [HttpPost("telegram_game")]
        public async Task<IActionResult> CreateTelegramGameAsync(CreateTelegramGameRequest request)
        {
            if (request == null)
            {
                return BadRequest();
            }

            if (string.IsNullOrEmpty(request.Address))
            {
                return BadRequest("Address must be specified");
            }

            if (request.Date < DateTime.UtcNow)
            {
                return BadRequest("Invalid game date");
            }

            try
            {
                var telegramGame = new TelegramGame
                {
                    Id = Guid.NewGuid().ToString("D"),
                    Address = request.Address,
                    Cost = request.Cost,
                    Date = request.Date,
                    DurationInMins = request.DurationInMins,
                    ChatId = request.ChatId,
                    MessageId = request.MessageId,
                    Notes = request.Notes
                };

                telegramGame = await gameRepository.CreateAsync(telegramGame);

                if (!string.IsNullOrEmpty(request.LastGameId))
                {
                    await gameRepository.CopyPlayersAsync(request.LastGameId, telegramGame.Id);
                }

                return Ok(telegramGame);
            }
            catch (Exception ex)
            {
                logger.LogError(string.Format("Exception occured while creating a telegram game. Exception: {0}\n{1}", ex.Message, ex.StackTrace));
                return StatusCode((int)HttpStatusCode.InternalServerError);
            }
        }
        [HttpPut("telegram_game")]
        public async Task<IActionResult> UpdateTelegramGameAsync(UpdateTelegramGameRequest request)
        {
            if (request == null)
            {
                return BadRequest();
            }

            try
            {
                var telegramGame = await gameRepository.FindAsync(request.ChatId, request.MessageId);
                if(telegramGame == null)
                {
                    return NotFound();
                }

                telegramGame.ApplyUpdateRequest(request);
                
                telegramGame = await gameRepository.UpdateAsync(telegramGame);
                return Ok(telegramGame);
            }
            catch (Exception ex)
            {
                logger.LogError(string.Format("Exception occured while updating a telegram game. Exception: {0}\n{1}", ex.Message, ex.StackTrace));
                return StatusCode((int)HttpStatusCode.InternalServerError);
            }
        }

        [HttpPost("telegram_game/last")]
        public async Task<IActionResult> FindLastTelegramGameAsync(FindLastTelegramGameRequest request)
        {
            try
            {
                var lastGame = await gameRepository.FindLastGameAsync(request.TelegramChatId, request.Date);
                return Ok(lastGame);
            }
            catch (Exception ex)
            {
                logger.LogError(string.Format("Exception occured while finding a last telegram game. Exception: {0}\n{1}", ex.Message, ex.StackTrace));
                return StatusCode((int)HttpStatusCode.InternalServerError);
            }
        }

        [HttpPost("telegram_game/find")]
        public async Task<IActionResult> FindTelegramGameAsync(FindTelegramGameRequest request)
        {
            try
            {
                var telegramGame = await gameRepository.FindAsync(request.ChatId, request.MessageId);
                return Ok(telegramGame);
            }
            catch (Exception ex)
            {
                logger.LogError(string.Format("Exception occured while finding a telegram game. Exception: {0}\n{1}", ex.Message, ex.StackTrace));
                return StatusCode((int)HttpStatusCode.InternalServerError);
            }
        }

        [HttpPost("telegram_game/player")]
        public async Task<IActionResult> CreateTelegramGamePlayerAsync(CreateTelegramGamePlayerRequest request)
        {
            if (string.IsNullOrEmpty(request?.TelegramGameId))
            {
                return BadRequest("Game Id must be specified");
            }

            if (string.IsNullOrEmpty(request?.TelegramUserId))
            {
                return BadRequest("User Id must be specified");
            }

            try
            {
                var newGamePlayer = new TelegramGamePlayer
                {
                    Id = Guid.NewGuid().ToString("D"),
                    TelegramGameId = request.TelegramGameId,
                    TelegramUserId = request.TelegramUserId
                };

                newGamePlayer = await gameRepository.CreateTelegramGamePlayerAsync(newGamePlayer);
                return Ok(newGamePlayer);
            }
            catch (Exception ex)
            {
                logger.LogError(string.Format("Exception occured while creating a telegram game player. Exception: {0}\n{1}", ex.Message, ex.StackTrace));
                return StatusCode((int)HttpStatusCode.InternalServerError);
            }
        }

        [HttpDelete("telegram_game/player")]
        public async Task<IActionResult> DeleteTelegramGamePlayerAsync(DeleteTelegramGamePlayerRequest request)
        {
            if (string.IsNullOrEmpty(request?.TelegramGameId))
            {
                return BadRequest("Game Id must be specified");
            }

            if (string.IsNullOrEmpty(request?.TelegramUserId))
            {
                return BadRequest("User Id must be specified");
            }

            try
            {
                await gameRepository.DeleteTelegramGamePlayerAsync(request.TelegramGameId, request.TelegramUserId);
                return NoContent();
            }
            catch (Exception ex)
            {
                logger.LogError(string.Format("Exception occured while deleting a player. Exception: {0}\n{1}", ex.Message, ex.StackTrace));
                return StatusCode((int)HttpStatusCode.InternalServerError);
            }
        }
    }
}
