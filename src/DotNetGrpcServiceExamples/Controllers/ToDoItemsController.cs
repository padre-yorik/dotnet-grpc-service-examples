using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using NewVoiceMedia.Claims;
using NewVoiceMedia.DotNetGrpcServiceExamples.DataAccess;
using NewVoiceMedia.DotNetGrpcServiceExamples.Models;

namespace NewVoiceMedia.DotNetGrpcServiceExamples.Controllers
{
    [Route("/todos")]
    [ApiController]
    [ApiVersion("3")]
    public class ToDoItemsController : ControllerBase
    {
        private readonly IToDoItemRepository _repository;

        public ToDoItemsController(IToDoItemRepository toDoItemsRepository)
        {
            _repository = toDoItemsRepository;
        }

        [Description("Get all todo items in an account")]
        [HttpGet]
        public async Task<ActionResult<IEnumerable<ToDoItem>>> Get()
        {
            var accountId = User.GetAccountId();
            var todos = await _repository.GetAllForAccount(accountId);
            return Ok(todos);
        }

        [Description("Get a specific todo item")]
        [HttpGet("{id:guid}")]
        public async Task<ActionResult<ToDoItem>> Get(Guid id)
        {
            var accountId = User.GetAccountId();
            var todo = await _repository.GetById(id.ToString());
            if (todo == null || todo.AccountId != accountId)
            {
                return NotFound();
            }
            return todo;
        }

        [Description("Save a todo item")]
        [HttpPost]
        public async Task<ActionResult> Post(ToDoItem item)
        {
            var account = User.GetAccountId();
            var id = Guid.NewGuid().ToString();
            item.ToDoId = id;
            item.AccountId = account;
            await _repository.Insert(item);
            return CreatedAtAction(nameof(Get), new { id = item.ToDoId }, item);
        }

        [Description("Delete a single todo item")]
        [HttpDelete("{id:guid}")]
        public async Task<ActionResult<ToDoItem>> Delete(Guid id)
        {
            var account = User.GetAccountId();
            var todoItem = await _repository.GetById(id.ToString());
            if (todoItem == null || todoItem.AccountId != account)
            {
                return NotFound();
            }

            await _repository.Delete(id.ToString());
            return NoContent();
        }
    }
}
