using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AutoMapper;
using CoreCodeCamp.Data;
using CoreCodeCamp.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Routing;

namespace CoreCodeCamp.Controllers
{
    [Route("api/camps/{moniker}/[controller]")]
    [ApiController]
    public class TalksController : ControllerBase
    {
        private readonly ICampRepository _CampRepository;
        private readonly IMapper _Mapper;
        private readonly LinkGenerator _LinkGenerator;

        public TalksController(ICampRepository campRepository, IMapper mapper, LinkGenerator linkGenerator)
        {
            _CampRepository = campRepository;
            _Mapper = mapper;
            _LinkGenerator = linkGenerator;
        }

        [HttpGet]
        public async Task<ActionResult<TalkModel[]>> Get(string moniker)
        {
            try
            {
                var talks = await _CampRepository.GetTalksByMonikerAsync(moniker, true);

                return _Mapper.Map<TalkModel[]>(talks);
            }
            catch (Exception)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, "Failed to get target");
            }
        }

        [HttpGet("{id:int}")]
        public async Task<ActionResult<TalkModel>> Get (string moniker, int id)
        {
            try
            {
                var talk = await _CampRepository.GetTalkByMonikerAsync(moniker, id, true);
                if (talk == null) return NotFound("Talk not found");

                return _Mapper.Map<TalkModel>(talk);
            }
            catch (Exception)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, "Failed to get target");
            }
        }

        [HttpPost]
        public async Task<ActionResult<TalkModel>> Post(string moniker, TalkModel model)
        {
            try
            {
                var camp = await _CampRepository.GetCampAsync(moniker);
                if (camp == null) return BadRequest("Camp does not exist");

                var talk = _Mapper.Map<Talk>(model);
                talk.Camp = camp;

                if (model.Speaker == null) return BadRequest("Speaker ID is required");
                var speaker = await _CampRepository.GetSpeakerAsync(model.Speaker.SpeakerId);
                if (speaker == null) return BadRequest("Speaker could not be found");

                talk.Speaker = speaker;
                _CampRepository.Add(talk);

                if (await _CampRepository.SaveChangesAsync())
                {
                    var url = _LinkGenerator.GetPathByAction(HttpContext, "Get", values: new { moniker, id = talk.TalkId });

                    return Created(url, _Mapper.Map<TalkModel>(talk));
                }
                else
                {
                    return BadRequest("Failed to save new talk");
                }
            }
            catch (Exception)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, "Failed to create new talk");
            }
        }

        [HttpPut("{id:int}")]
        public async Task<ActionResult<TalkModel>> Put(string moniker, int id, TalkModel model)
        {
            try
            {
                var talk = await _CampRepository.GetTalkByMonikerAsync(moniker, id, true);
                if (talk == null) return NotFound("Could not find the talk");

                _Mapper.Map(model, talk);

                if (model.Speaker != null)
                {
                    var speaker = await _CampRepository.GetSpeakerAsync(model.Speaker.SpeakerId);
                    if (speaker != null)
                    {
                        talk.Speaker = speaker;
                    }
                }

                if (await _CampRepository.SaveChangesAsync())
                {
                    return _Mapper.Map<TalkModel>(talk);
                }
                else
                {
                    return BadRequest("Failed to update database");
                }
            }
            catch (Exception)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, "Failed to update the talk");
            }
        }

        [HttpDelete("{id:int}")]
        public async Task<IActionResult> Delete(string moniker, int id)
        {
            try
            {
                var talk = await _CampRepository.GetTalkByMonikerAsync(moniker, id);
                if (talk == null) return NotFound("Could not find talk");

                _CampRepository.Delete(talk);
                if (await _CampRepository.SaveChangesAsync())
                {
                    return Ok();
                }
                else
                {
                    return BadRequest("Failed to delete talk");
                }
            }
            catch (Exception)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, "Failed to update the talk");
            }
        }
    }
}