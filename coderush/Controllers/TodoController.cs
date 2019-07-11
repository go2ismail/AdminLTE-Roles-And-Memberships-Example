using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using coderush.Data;
using coderush.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace coderush.Controllers
{
    [Authorize(Roles = Services.App.Pages.Todo.RoleName)]
    public class TodoController : Controller
    {
        private readonly ApplicationDbContext _context;

        //dependency injection through constructor, to directly access services
        public TodoController(ApplicationDbContext context) {
            _context = context;
        }

        //consume db context service, display all todo items
        public IActionResult Index()
        {
            var todos = _context.Todo.OrderByDescending(x => x.CreatedDate).ToList();
            return View(todos);
        }

        //display todo create edit form
        [HttpGet]
        public IActionResult Form(string id)
        {
            //create new
            if (id == null)
            {
                Todo newTodo = new Todo();
                return View(newTodo);
            }

            //edit todo
            Todo todo = new Todo();
            todo = _context.Todo.Where(x => x.TodoId.Equals(id)).FirstOrDefault();

            if (todo == null)
            {
                return NotFound();
            }

            return View(todo);

        }

        //post submitted todo data. if todo.TodoId is null then create new, otherwise edit
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult SubmitForm([Bind("TodoId", "TodoItem", "IsDone")]Todo todo)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    TempData[StaticString.StatusMessage] = "Error: Model state not valid.";
                    return RedirectToAction(nameof(Form), new { id = todo.TodoId ?? "" });
                }

                //create new
                if (todo.TodoId == null)
                {
                    Todo newTodo = new Todo();
                    newTodo.TodoId = Guid.NewGuid().ToString();
                    newTodo.CreatedDate = DateTime.Now;
                    newTodo.TodoItem = todo.TodoItem;
                    newTodo.IsDone = todo.IsDone;
                    _context.Todo.Add(todo);
                    _context.SaveChanges();

                    TempData[StaticString.StatusMessage] = "Create new todo item success.";
                    return RedirectToAction(nameof(Form), new { id = todo.TodoId ?? "" });
                }

                //edit existing
                Todo editTodo = new Todo();
                editTodo = _context.Todo.Where(x => x.TodoId.Equals(todo.TodoId)).FirstOrDefault();
                editTodo.TodoItem = todo.TodoItem;
                editTodo.IsDone = todo.IsDone;
                _context.Update(editTodo);
                _context.SaveChanges();

                TempData[StaticString.StatusMessage] = "Edit existing todo item success.";
                return RedirectToAction(nameof(Form), new { id = todo.TodoId ?? "" });
            }
            catch (Exception ex)
            {

                TempData[StaticString.StatusMessage] = "Error: " + ex.Message;
                return RedirectToAction(nameof(Form), new { id = todo.TodoId ?? "" });
            }
        }

        //display todo item for deletion
        [HttpGet]
        public IActionResult Delete(string id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var todo = _context.Todo.Where(x => x.TodoId.Equals(id)).FirstOrDefault();
            return View(todo);
        }

        //delete submitted todo item if found, otherwise 404
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult SubmitDelete([Bind("TodoId")]Todo todo)
        {
            try
            {
                var deleteTodo = _context.Todo.Where(x => x.TodoId.Equals(todo.TodoId)).FirstOrDefault();
                if (deleteTodo == null)
                {
                    return NotFound();
                }

                _context.Todo.Remove(deleteTodo);
                _context.SaveChanges();

                TempData[StaticString.StatusMessage] = "Delete todo item success.";
                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex)
            {

                TempData[StaticString.StatusMessage] = "Error: " + ex.Message;
                return RedirectToAction(nameof(Delete), new { id = todo.TodoId ?? "" });
            }
        }
    }
}