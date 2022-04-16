using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using API.Data;
using API.DTOs;
using API.Entities;
using API.Extensions;
using API.Interfaces;
using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace API.Controllers
{
    [Authorize]
    public class UsersController : BaseApiController
    {
        private readonly IUserRepository _repo;
        private readonly IMapper _mapper;
        private readonly IPhotoService _photoService;
        public UsersController(IUserRepository repo, IMapper mapper, IPhotoService photoService)
        {
            _photoService = photoService;
            _mapper = mapper;
            _repo = repo;

        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<MemberDTO>>> GetUsers()
        {
            return Ok(await _repo.GetMembersAsync());

        }

        [HttpGet("{userName}", Name = "GetUser")]
        public async Task<ActionResult<MemberDTO>> GetUserByUserName(string userName)
        {
            return await _repo.GetMemberAsync(userName);
        }

        [HttpPut]
        public async Task<ActionResult> UpdateUser(MemberUpdateDTO memberUpdateDTO)
        {
            AppUser user = await _repo.GetUserByNameAsync(User.GetUserName());
            // = _mapper.Map<AppUser>(member);
            _mapper.Map(memberUpdateDTO, user);

            _repo.Update(user);
            if (await _repo.SaveAllAsync())
            {
                return NoContent();
            }
            return BadRequest();
        }

        [HttpPost("add-photo")]
        public async Task<ActionResult<PhotoDTO>> AddPhoto(IFormFile file)
        {
            AppUser user = await _repo.GetUserByNameAsync(User.GetUserName());

            var result = await _photoService.AddPhotoAsync(file);
            if (result.Error != null) return BadRequest(result.Error.Message);

            var photo = new Photo
            {
                Url = result.SecureUrl.AbsoluteUri,
                PublicId = result.PublicId
            };
            if (user.Photos.Count == 0)
            {
                photo.IsMain = true;
            }
            user.Photos.Add(photo);
            if (await _repo.SaveAllAsync())
            {
                return CreatedAtRoute("GetUser", new { userName = user.UserName }, _mapper.Map<PhotoDTO>(photo));
            }
            return BadRequest("Problem adding photo");
        }

        [HttpPut("set-main-photo/{photoid}")]
        public async Task<ActionResult> SetMainPhoto(int photoid)
        {
            var user = await _repo.GetUserByNameAsync(User.GetUserName());
            var photo = user.Photos.FirstOrDefault(photo => photo.Id == photoid);
            if (photo.IsMain)
            {
                return BadRequest("This is already your main photo");
            }

            var currentMain = user.Photos.FirstOrDefault(photo => photo.IsMain);
            if (currentMain != null) currentMain.IsMain = false;
            photo.IsMain = true;
            if (await _repo.SaveAllAsync())
            {
                return NoContent();
            }
            return BadRequest("Failed to set main photo");
        }

        [HttpDelete("delete-photo/{photoId}")]
        public async Task<ActionResult> DeletePhoto(int photoId)
        {
            var user = await _repo.GetUserByNameAsync(User.GetUserName());
            var photo = user.Photos.FirstOrDefault(photo => photo.Id == photoId);
            if (photo == null) return NotFound();
            if (photo.IsMain) return BadRequest("You cannot delete your main photo");
            if(photo.PublicId != null){
                var res = await _photoService.DeletePhotoAsync(photo.PublicId);
                if(res.Error !=null) return BadRequest(res.Error.Message);               
            }

            user.Photos.Remove(photo);
            if(await _repo.SaveAllAsync()) return Ok();

            return BadRequest("Failed to delete the photo");
         }
    }
}