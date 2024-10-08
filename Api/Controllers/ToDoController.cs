using System.ComponentModel.DataAnnotations;
using Api.Extensions;
using Api.Responses;
using Application.Commands.Contracts;
using Application.Queries.Contracts;
using Application.Requests;
using Application.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TourmalineCore.AspNetCore.JwtAuthentication.Core.Filters;

namespace Api.Controllers;

/// <summary>
/// Controller with actions to toDos
/// </summary>
[Authorize]
[RequiresPermission(UserClaimsProvider.GuestActionsAllowed)]
[Route("to-dos-api")]
public class ToDoController : Controller
{
    private const long DEFAULT_TENANT_ID = 1L;

    private readonly ToDoService _toDoService;
    private readonly IGetAllToDosQuery _getAllToDosQuery;
    private readonly ICreateToDoCommand _createToDoCommand;
    private readonly IDeleteToDoCommand _deleteToDoCommand;
    private readonly ISoftDeleteToDoCommand _softDeleteToDoCommand;

    /// <summary>
    /// Controller with actions to toDos
    /// </summary>
    public ToDoController(
        ToDoService toDoService,
        IGetAllToDosQuery getAllToDosQuery,
        ICreateToDoCommand createToDoCommand,
        IDeleteToDoCommand deleteToDoCommand,
        ISoftDeleteToDoCommand softDeleteToDoCommand)
    {
        _toDoService = toDoService;
        _getAllToDosQuery = getAllToDosQuery;
        _createToDoCommand = createToDoCommand;
        _deleteToDoCommand = deleteToDoCommand;
        _softDeleteToDoCommand = softDeleteToDoCommand;
    }

    /// <summary>
    /// Get all TodoItems
    /// </summary>
    [HttpGet("to-dos")]
    public async Task<ToDosResponse> GetAllToDosAsync()
    {
        var toDos = await _getAllToDosQuery.GetAllAsync(User.GetTenantId());

        return new ToDosResponse
        {
            ToDos = toDos.Select(x => new ToDo
                        {
                            Id = x.Id,
                            Name = x.Name,
                        }
                    )
                .ToList(),
        };
    }

    /// <summary>
    /// Adds TodoItem.
    /// </summary>
    /// <param name="addToDoRequest"></param>
    [HttpPost("to-dos")]
    public Task<long> AddToDoAsync([FromBody] AddToDoRequest addToDoRequest)
    {
        return _createToDoCommand.CreateAsync(addToDoRequest, User.GetTenantId());
    }

    /// <summary>
    /// Completes TodoItem.
    /// </summary>
    /// <param name="completeToDoRequest"></param>
    [HttpPost("to-dos/complete")]
    public Task CompleteToDo([FromBody] CompleteToDoRequest completeToDoRequest)
    {
        return _toDoService.CompleteToDoAsync(completeToDoRequest.ToDoIds, User.GetTenantId());
    }

    /// <summary>
    /// Deletes specific TodoItem.
    /// </summary>
    /// <param name="toDoId"></param>
    [HttpDelete("to-dos")]
    public Task DeleteToDo([Required] [FromQuery] long toDoId)
    {
        return _deleteToDoCommand.DeleteAsync(toDoId, User.GetTenantId());
    }

    /// <summary>
    /// Soft deletes specific TodoItem (mark as deleted, but not deleting from database)
    /// </summary>
    /// <param name="toDoId"></param>
    [HttpDelete("to-dos/soft-delete")]
    public Task SoftDeleteToDo([FromQuery] long toDoId)
    {
        return _softDeleteToDoCommand.SoftDeleteAsync(toDoId, User.GetTenantId());
    }
}