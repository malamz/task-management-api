using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
using TaskManagementAPI.Contracts;
using TaskManagementAPI.Interfaces;

namespace TaskManagementAPI.Controllers;

/// <summary>
/// CRUD operations for tasks.  All endpoints require a valid JWT.
/// DELETE is restricted to the Admin role.
/// </summary>
[ApiController]
[Route("api/[controller]")]
[Authorize]                                          // entire controller needs auth
[EnableRateLimiting("GlobalPolicy")]
[Produces("application/json")]
[ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status401Unauthorized)]
public class TasksController : ControllerBase
{
    private readonly ITaskService _taskService;

    public TasksController(ITaskService taskService)
    {
        _taskService = taskService;
    }

    // ── GET /api/tasks ───────────────────────────────────────────────────────

    /// <summary>
    /// List all tasks with optional filtering and pagination.
    /// </summary>
    /// <param name="page">Page number (default 1).</param>
    /// <param name="pageSize">Items per page (default 10, max 50).</param>
    /// <param name="status">Filter by status: Todo | InProgress | Done.</param>
    /// <param name="priority">Filter by priority: Low | Medium | High.</param>
    /// <param name="assignedUserId">Filter by assigned user GUID.</param>
    /// <returns>Paginated list of tasks.</returns>
    [HttpGet]
    [ProducesResponseType(typeof(PaginatedResponse<TaskResponse>), StatusCodes.Status200OK)]
    public async Task<ActionResult<PaginatedResponse<TaskResponse>>> GetTasks(
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 10,
        [FromQuery] string? status = null,
        [FromQuery] string? priority = null,
        [FromQuery] Guid? assignedUserId = null)
    {
        // Clamp page size between 1 and 50
        pageSize = Math.Clamp(pageSize, 1, 50);
        page = Math.Max(page, 1);

        var result = await _taskService.GetTasksAsync(page, pageSize, status, priority, assignedUserId);
        return Ok(result);
    }

    // ── GET /api/tasks/{id} ──────────────────────────────────────────────────

    /// <summary>
    /// Retrieve a single task by its ID.
    /// </summary>
    /// <param name="id">Task GUID.</param>
    /// <returns>The task if found.</returns>
    /// <response code="404">Task not found.</response>
    [HttpGet("{id:guid}")]
    [ProducesResponseType(typeof(TaskResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status404NotFound)]
    public async Task<ActionResult<TaskResponse>> GetTaskById([FromRoute] Guid id)
    {
        var task = await _taskService.GetTaskByIdAsync(id);
        return Ok(task);
    }

    // ── POST /api/tasks ──────────────────────────────────────────────────────

    /// <summary>
    /// Create a new task.
    /// </summary>
    /// <param name="request">Task creation payload.</param>
    /// <returns>The newly created task.</returns>
    /// <response code="201">Task created.</response>
    /// <response code="400">Validation or argument error.</response>
    [HttpPost]
    [ProducesResponseType(typeof(TaskResponse), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<TaskResponse>> CreateTask([FromBody] CreateTaskRequest request)
    {
        var task = await _taskService.CreateTaskAsync(request);
        return CreatedAtAction(nameof(GetTaskById), new { id = task.Id }, task);
    }

    // ── PUT /api/tasks/{id} ──────────────────────────────────────────────────

    /// <summary>
    /// Update an existing task (partial update – only supplied fields change).
    /// </summary>
    /// <param name="id">Task GUID.</param>
    /// <param name="request">Fields to update.</param>
    /// <returns>The updated task.</returns>
    /// <response code="404">Task not found.</response>
    [HttpPut("{id:guid}")]
    [ProducesResponseType(typeof(TaskResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status404NotFound)]
    public async Task<ActionResult<TaskResponse>> UpdateTask(
        [FromRoute] Guid id,
        [FromBody] UpdateTaskRequest request)
    {
        var task = await _taskService.UpdateTaskAsync(id, request);
        return Ok(task);
    }

    // ── DELETE /api/tasks/{id} ───────────────────────────────────────────────

    /// <summary>
    /// Delete a task.  <b>Admin only.</b>
    /// </summary>
    /// <param name="id">Task GUID.</param>
    /// <response code="204">Task deleted.</response>
    /// <response code="403">Not an admin.</response>
    /// <response code="404">Task not found.</response>
    [HttpDelete("{id:guid}")]
    [Authorize(Roles = "Admin")]                     // role-based guard
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status404NotFound)]
    public async Task<ActionResult> DeleteTask([FromRoute] Guid id)
    {
        await _taskService.DeleteTaskAsync(id);
        return NoContent();
    }
}
