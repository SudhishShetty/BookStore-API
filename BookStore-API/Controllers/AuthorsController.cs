using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AutoMapper;
using BookStore_API.Contracts;
using BookStore_API.Data;
using BookStore_API.DTOs;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.VisualBasic.CompilerServices;

namespace BookStore_API.Controllers
{
    /// <summary>
    /// Endpoint to interact with the Authors in the book store's database
    /// </summary>
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public class AuthorsController : ControllerBase
    {
        private readonly IAuthorRepository _authorRepository;
        private readonly IMapper _mapper;

        public AuthorsController(IAuthorRepository authorRepository, IMapper mapper)
        {
            _authorRepository = authorRepository;
            _mapper = mapper;
        }

        /// <summary>
        /// Get All Authors
        /// </summary>
        /// <returns>List of Authors</returns>
        [HttpGet]
        [Authorize(Roles = "Customer")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> GetAuthors()
        {
            try
            {
                var authors = await _authorRepository.FindAll();
                var response = _mapper.Map<IList<AuthorDTO>>(authors);
                return Ok(response);
            }
            catch (Exception)
            {

                return InternalError();
            }
        }

        /// <summary>
        /// Get an Author by ID
        /// </summary>
        /// <param name="Id"></param>
        /// <returns>The specific author's record</returns>
        [HttpGet("{id:int}")]
        [Authorize(Roles = "Customer")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> GetAuthor(int Id)
        {
            try
            {
                var author = await _authorRepository.FindByID(Id);
                if(author == null)
                    return NotFound();
                var response = _mapper.Map<AuthorDTO>(author);
                return Ok(response);
            }
            catch (Exception)
            {

               return InternalError();
            }
        }

        /// <summary>
        /// Creates a new author record
        /// </summary>
        /// <param name="authorDTO"></param>
        /// <returns>Created author details</returns>
        [HttpPost]
        [Authorize(Roles = "Administrator")]
        [ProducesResponseType(StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> Create([FromBody] AuthorCreateDTO authorDTO)
        {
            try
            {
                if(authorDTO == null)
                    return BadRequest(ModelState);
                if(!ModelState.IsValid)
                    return BadRequest(ModelState);
                var author = _mapper.Map<Author>(authorDTO);
                var isSuccess = await _authorRepository.Create(author);
                if(!isSuccess)
                    return InternalError();
                return Created("Create", new {author});
            }
            catch (Exception)
            {

                return InternalError();
            }
        }

        /// <summary>
        /// Update an author's record
        /// </summary>
        /// <param name="id"></param>
        /// <param name="authorDTO"></param>
        /// <returns>Update author's details</returns>
        [HttpPut("{id}")]
        [Authorize(Roles = "Administrator")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> Update(int id,[FromBody] AuthorUpdateDTO authorDTO)
        {
            try
            {
                if (id < 1 || authorDTO == null || id !=authorDTO.Id)
                    return BadRequest(ModelState);
                if (!ModelState.IsValid)
                    return BadRequest(ModelState);
                var isExists = await _authorRepository.IsExists(id);
                if (!isExists)
                    return NotFound();
                var author = _mapper.Map<Author>(authorDTO);
                var isSuccess = await _authorRepository.Update(author);
                if (!isSuccess)
                    return InternalError();
                return NoContent();
            }
            catch (Exception)
            {

                return InternalError();
            }
        }

        /// <summary>
        /// Deletes an Author's record
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        [HttpDelete("{id}")]
        [Authorize(Roles = "Administrator")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> Delete(int id)
        {
            try
            {
                if (id < 1)
                    return BadRequest();
                var isExists = await _authorRepository.IsExists(id);
                if (!isExists)
                    return NotFound();
                var author = await _authorRepository.FindByID(id);
                var isSuccess = await _authorRepository.Delete(author);
                if(!isSuccess)
                    return InternalError();
                return NoContent();

            }
            catch (Exception)
            {

                return InternalError();
            }
        }

        private ObjectResult InternalError()
        {
            return StatusCode(500, "Something went wrong. Please contact the administrator");
        }
    }
}
