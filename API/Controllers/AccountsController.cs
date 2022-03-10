using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using API.Data;
using API.DTOs;
using API.Entities;
using API.Interfaces;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace API.Controllers
{
    public class AccountsController:BaseApiController
    {
        private DataContext _context;
        private ITokenService _tokenService;

        public AccountsController(DataContext context, ITokenService tokenService)
        {
            _context = context;
            _tokenService = tokenService;
        }
        
        [HttpPost("register")]
        public async Task<ActionResult<userDto>> Register(RegisterDto registerObj)
        {
            if(await _userExistsAsync(registerObj.UserName)) return BadRequest("Username already present");
            using var hmac = new HMACSHA512();
            var user = new AppUser{
                UserName = registerObj.UserName.ToLower(),
                PasswordHash = hmac.ComputeHash(Encoding.UTF8.GetBytes(registerObj.Password)),
                PasswordSalt =  hmac.Key
            };
            _context.Users.Add(user);
            await _context.SaveChangesAsync();

            return new userDto{
                UserName = user.UserName,
                Token = _tokenService.CreateToken(user)
            };
        }

        [HttpPost("login")]
        public async Task<ActionResult<userDto>> Login(LoginDto loginDto)
        {
            var user =  await  _context.Users.FirstOrDefaultAsync(x => x.UserName == loginDto.UserName.ToLower());
            if (user == null) return Unauthorized("Inavlid username or password");
            using var hmac = new HMACSHA512(user.PasswordSalt);
            var computedHash =await hmac.ComputeHashAsync
                (new MemoryStream(Encoding.UTF8.GetBytes(loginDto.Password)));
            for (int i =0 ; i < computedHash.Length; i++)
            {
                if(user.PasswordHash[i] != computedHash[i]) return Unauthorized("Inavlid password");
            }
            return new userDto{
                UserName = user.UserName,
                Token = _tokenService.CreateToken(user)
            };
        }

        private async Task<bool> _userExistsAsync(string userName){
            return  await _context.Users.AnyAsync(x => x.UserName.ToLower() == userName.ToLower());
        }
    }
}