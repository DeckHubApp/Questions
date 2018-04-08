using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Slidable.Questions.Data;
using Slidable.Questions.Models;

namespace Slidable.Questions.Controllers
{
    [Route("")]
    public class HomeController : Controller
    {
        private readonly QuestionContext _context;
        private readonly ILogger<HomeController> _logger;
        private readonly RedisPublisher _redis;

        public HomeController(QuestionContext context, ILogger<HomeController> logger, RedisPublisher redis)
        {
            _context = context;
            _logger = logger;
            _redis = redis;
        }

        [HttpGet]
        public string Test()
        {
            return "Hello";
        }

        [HttpGet("{place}/{presenter}/{slug}")]
        public async Task<ActionResult<QuestionsDto>> GetForShow(string place, string presenter, string slug,
            CancellationToken ct)
        {
            var showIdentifier = ShowIdentifier(place, presenter, slug);
            List<Question> questions;
            try
            {
                questions = await _context.Questions
                    .Where(q => q.Show == showIdentifier)
                    .OrderBy(q => q.Time)
                    .Include(q => q.Answers)
                    .ToListAsync(ct)
                    .ConfigureAwait(false);
            }
            catch (Exception ex)
            {
                _logger.LogError(EventIds.DatabaseError, ex, ex.Message);
                throw;
            }

            return new QuestionsDto
            {
                UserIsAuthenticated = User.Identity.IsAuthenticated,
                Questions = questions.Select(QuestionDto.FromQuestion).ToList()
            };
        }

        [HttpGet("{place}/{presenter}/{slug}/{slide:int}")]
        public async Task<ActionResult<List<QuestionDto>>> GetForSlide(string place, string presenter, string slug,
            int slide, CancellationToken ct)
        {
            var showIdentifier = ShowIdentifier(place, presenter, slug);
            List<Question> questions;
            try
            {
                questions = await _context.Questions
                    .Where(q => q.Show == showIdentifier && q.Slide == slide)
                    .OrderBy(q => q.Time)
                    .Include(q => q.Answers)
                    .ToListAsync(ct)
                    .ConfigureAwait(false);
            }
            catch (Exception ex)
            {
                _logger.LogError(EventIds.DatabaseError, ex, ex.Message);
                throw;
            }

            return questions.Select(QuestionDto.FromQuestion).ToList();
        }

        [HttpGet("{uuid}")]
        public async Task<ActionResult<QuestionDto>> Get(string uuid, CancellationToken ct)
        {
            Question question;
            try
            {
                question = await _context.Questions
                    .SingleOrDefaultAsync(q => q.Uuid == uuid, ct);
            }
            catch (Exception ex)
            {
                _logger.LogError(EventIds.DatabaseError, ex, ex.Message);
                throw;
            }

            if (question == null) return NotFound();
            return QuestionDto.FromQuestion(question);
        }

        [Authorize]
        [HttpPost("{place}/{presenter}/{slug}/{slide:int}")]
        public async Task<IActionResult> Ask(string place, string presenter, string slug, int slide,
            [FromBody] QuestionDto dto, CancellationToken ct)
        {
            var showIdentifier = ShowIdentifier(place, presenter, slug);
            var from = User.FindFirstValue(SlidableClaimTypes.Handle);
            if (string.IsNullOrEmpty(from))
            {
                return Forbid();
            }

            var question = new Question
            {
                Uuid = Guid.NewGuid().ToString(),
                Show = showIdentifier,
                Slide = slide,
                Text = dto.Text,
                From = from,
                Time = dto.Time
            };

            try
            {
                _context.Questions.Add(question);
                await _context.SaveChangesAsync(ct);
                _redis.PublishQuestion(question);
            }
            catch (Exception ex)
            {
                _logger.LogError(EventIds.DatabaseError, ex, ex.Message);
                throw;
            }

            return CreatedAtAction("Get", new {uuid = question.Uuid}, QuestionDto.FromQuestion(question));
        }

        [HttpGet("{uuid}/answers")]
        public async Task<IActionResult> Answers(string uuid, CancellationToken ct)
        {
            List<Answer> answers;
            try
            {
                answers = await _context.Answers
                    .Where(a => a.QuestionUuid == uuid)
                    .OrderBy(a => a.Time)
                    .ToListAsync(ct)
                    .ConfigureAwait(false);
            }
            catch (Exception ex)
            {
                _logger.LogError(EventIds.DatabaseError, ex, ex.Message);
                throw;
            }

            return Ok(answers.Select(a => AnswerDto.FromAnswer(uuid, a)));
        }

        [Authorize]
        [HttpPost("{uuid}/answers")]
        public async Task<IActionResult> Answer(string uuid, [FromBody] AnswerDto dto, CancellationToken ct)
        {
            try
            {
                var question = await _context.Questions
                    .SingleOrDefaultAsync(q => q.Uuid == uuid, ct);
                if (question == null) return NotFound();

                var answer = new Answer
                {
                    QuestionId = question.Id,
                    QuestionUuid = uuid,
                    User = dto.User,
                    Text = dto.Text,
                    Time = dto.Time
                };

                _context.Answers.Add(answer);
                await _context.SaveChangesAsync(ct);
            }
            catch (Exception ex)
            {
                _logger.LogError(EventIds.DatabaseError, ex, ex.Message);
                throw;
            }

            return Accepted();
        }

        private static string ShowIdentifier(string place, string presenter, string slug)
            => $"{place}/{presenter}/{slug}";
    }
}