using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using API.Data;
using API.DTOs;
using API.Entities;
using API.Interfaces;
using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace API.Controllers
{
    [Authorize]
    public class UsersController : BaseApiController
    {
        private readonly IUserRepository _repo;
        private readonly IMapper _mapper;
        public UsersController(IUserRepository repo, IMapper mapper)
        {
            _mapper = mapper;
            _repo = repo;

        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<MemberDTO>>> GetUsers()
        {
            return Ok(await _repo.GetMembersAsync());

        }

        [HttpGet("{userName}")]
        public async Task<ActionResult<MemberDTO>> GetUserByUserName(string userName)
        {
            return await _repo.GetMemberAsync(userName);
        }

        [HttpPut]
        public async Task<ActionResult> UpdateUser(MemberUpdateDTO memberUpdateDTO)
        {
            var userName = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            AppUser user = await _repo.GetUserByNameAsync(userName);
            _mapper.Map(memberUpdateDTO, user);
            _repo.Update(user);
            if (await _repo.SaveAllAsync())
            {
                return NoContent();
            }
            return BadRequest();
        }
    }
}