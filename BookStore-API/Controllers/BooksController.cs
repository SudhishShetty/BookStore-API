using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AutoMapper;
using BookStore_API.Contracts;
using BookStore_API.Data;
using BookStore_API.DTOs;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace BookStore_API.Controllers
{
    /// <summary>
    /// Endpoint to interact with the Books in the book store's database
    /// </summary>
    [Route("api/[controller]")]
    [ApiController]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public class BooksController : ControllerBase
    {
        private readonly IBookRepository _bookRepository;
        private readonly IMapper _mapper;
        private readonly IAuthorRepository _authorRepository;

        public BooksController(IAuthorRepository authorRepository,IBookRepository bookRepository, IMapper mapper)
        {
            _authorRepository = authorRepository;
            _bookRepository = bookRepository;
            _mapper = mapper;
        }

        /// <summary>
        /// Get All Books
        /// </summary>
        /// <returns>List of all books</returns>
        [HttpGet]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> GetBooks()
        {
            try
            {
                var books = await _bookRepository.FindAll();
                var response = _mapper.Map<IList<BookDTO>>(books);
                return Ok(response);
            }
            catch (Exception)
            {

                return InternalError();
            }
        }

        /// <summary>
        /// Gets a book by ID
        /// </summary>
        /// <param name="Id"></param>
        /// <returns>Book record</returns>
        [HttpGet("{id:int}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> GetBook(int Id)
        {
            try
            {
                var book = await _bookRepository.FindByID(Id);
                if (book == null)
                    return NotFound();
                var response = _mapper.Map<BookDTO>(book);
                return Ok(response);
            }
            catch (Exception)
            {

                return InternalError();
            }
        }

        /// <summary>
        /// Creates a new book record
        /// </summary>
        /// <param name="bookDTO"></param>
        /// <returns>Created book's record</returns>
        [HttpPost]
        [ProducesResponseType(StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> Create([FromBody] BookCreateDTO bookDTO)
        {
            try
            {
                if (bookDTO == null)
                    return BadRequest(ModelState);
                if (!ModelState.IsValid)
                    return BadRequest(ModelState);
                var isExists = await _authorRepository.IsExists(bookDTO.AuthorId);
                if (!isExists)
                    return BadRequest(ModelState);
                var book = _mapper.Map<Book>(bookDTO);
                var isSuccess = await _bookRepository.Create(book);
                if (!isSuccess)
                    return InternalError();
                return Created("Create", new { book });
            }
            catch (Exception)
            {

                return InternalError();
            }
        }

        /// <summary>
        /// Update an book's record
        /// </summary>
        /// <param name="id"></param>
        /// <param name="bookDTO"></param>
        /// <returns>Update the book's details</returns>
        [HttpPut("{id}")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> Update(int id, [FromBody] BookUpdateDTO bookDTO)
        {
            try
            {
                if (id < 1 || bookDTO == null || id != bookDTO.Id)
                    return BadRequest(ModelState);
                if (!ModelState.IsValid)
                    return BadRequest(ModelState);
                var isExists = await _bookRepository.IsExists(id);
                if (!isExists)
                    return NotFound();
                var book = _mapper.Map<Book>(bookDTO);
                var isSuccess = await _bookRepository.Update(book);
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
        /// Deletes a book's record
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        [HttpDelete("{id}")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> Delete(int id)
        {
            try
            {
                if (id < 1)
                    return BadRequest();
                var isExists = await _bookRepository.IsExists(id);
                if (!isExists)
                    return NotFound();
                var book = await _bookRepository.FindByID(id);
                var isSuccess = await _bookRepository.Delete(book);
                if (!isSuccess)
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
