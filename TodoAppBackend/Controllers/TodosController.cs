using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;
using TodoAppBackend.Models;

[Authorize]
[ApiController]
[Route("api/[controller]")]
public class TodosController : ControllerBase
{
    private readonly ApplicationDbContext _dbContext;
    private readonly UserManager<IdentityUser> _userManager;

    public TodosController(ApplicationDbContext dbContext, UserManager<IdentityUser> userManager)
    {
        _dbContext = dbContext;
        _userManager = userManager;
    }

    [HttpPost]
    public async Task<IActionResult> CreateTodo([FromBody] TodoDto todoDto)
    {
        var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

        if (userId == null)
        {
            return Unauthorized();
        }

        var todo = new Todo
        {
            Title = todoDto.Title,
            Body = todoDto.Body,
            UserId = userId
        };

        _dbContext.Todos.Add(todo);
        await _dbContext.SaveChangesAsync();

        return Ok(todo);
    }

    [HttpGet]
    public async Task<IActionResult> GetUserTodos(int pageNumber = 1)
    {
        var user = await _userManager.GetUserAsync(User);
        var userId = await _userManager.GetUserIdAsync(user);

        if (userId == null)
        {
            return Unauthorized();
        }

        const int pageSize = 5;
        var todos = await _dbContext.Todos
            .Where(t => t.UserId == userId && t.DeletedAt == null)
            .Skip((pageNumber - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();

        return Ok(todos);
    }


    [HttpPut("{todoId}")]
    [ServiceFilter(typeof(LoggedInUserAttribute))]
    public async Task<IActionResult> UpdateTodo(Guid todoId, [FromBody] TodoDto todoDto)
    {
        var todo = await _dbContext.Todos.FindAsync(todoId);

        if (todo == null)
        {
            return NotFound();
        }

        todo.Title = todoDto.Title;
        todo.Body = todoDto.Body;
        todo.IsCompleted = todoDto.IsCompleted;
        todo.UpdatedAt = DateTime.UtcNow;

        await _dbContext.SaveChangesAsync();
        return Ok(todo);
    }

    [HttpDelete("{todoId}")]
    [ServiceFilter(typeof(LoggedInUserAttribute))]
    public async Task<IActionResult> DeleteTodo(Guid todoId)
    {
        var todo = await _dbContext.Todos.FindAsync(todoId);

        if (todo == null)
        {
            return NotFound();
        }

        todo.DeletedAt = DateTime.UtcNow;

        await _dbContext.SaveChangesAsync();
        return NoContent();
    }
}

public class TodoDto
{
    public string Title { get; set; } = null!;
    public string Body { get; set; } = null!;
    public bool IsCompleted { get; set; } = false;
}
