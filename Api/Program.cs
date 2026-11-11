using System.Reflection.Metadata.Ecma335;

var builder = WebApplication.CreateBuilder(args);
var app = builder.Build();

var tickets = new List<TicketDto>
{
    new(1, "user1", "Lỗi không đăng nhập được", "High", null, TicketStatus.New),
    new(2, "user2", "Cần hướng dẫn thanh toán", "Medium", "support_team_A", TicketStatus.Assigned)
};

app.MapGet("/", () => new
{
    Service = "Support API",
    Status = "Running"
});

app.MapGet("api/tickets", () => Results.Ok(tickets));

app.MapGet("api/tickets/{id:int}", (int id) =>
{
    var ticket = tickets.FirstOrDefault(t => t.Id == id);
    return ticket is not null
        ? Results.Ok(ticket)
        : Results.NotFound(new ApiError("TICKET_NOT_FOUND", "Không tìm thấy yêu cầu hỗ trợ"));
});


app.MapGet("api/users/{userId}/tickets", (String userId) =>
{
    var userTickets = tickets.Where(t => t.SenderId == userId).ToList();
    return userTickets is not null
        ? Results.Ok(userTickets)
        : Results.NotFound(new ApiError("USER_NOT_FOUND", "Không tìm thấy người dùng"));
});

app.MapPost("/api/tickets", (CreateTicketRequest req) =>
{
    if (string.IsNullOrWhiteSpace(req.Content) || string.IsNullOrWhiteSpace(req.SenderId))
        return Results.BadRequest(new ApiError("INVALID_TICKET", "Nội dung và người dùng không được để trống "));

    var newId = tickets.Any() ? tickets.Max(t => t.Id) + 1 : 1;
    var newTicket = new TicketDto(newId, req.SenderId, req.Content, req.Priority, null, TicketStatus.New);
    tickets.Add(newTicket);

    return Results.Created($"api/tickets/{newId}", newTicket);
});

app.MapPatch("/api/tickets/{id:int}/status", (int id, UpdateTicketStatusRequest req) =>
{
    var ticketIndex = tickets.FindIndex(t => t.Id == id);
    if (ticketIndex == -1)
    {
        return Results.NotFound(new ApiError("TICKET_NOT_FOUND", "Không tìm thấy yêu cầu hỗ trợ"));
    }

    var exitingTicket = tickets[ticketIndex];
    var updateTicket = exitingTicket with { Status = req.Status };
    tickets[ticketIndex] = updateTicket;

    return Results.Ok(updateTicket);
});

app.Run();

public enum TicketStatus { New, Assigned, InProgress, Resolved }

public record TicketDto(int Id, string SenderId, string Content, string Priority, string? Assignee, TicketStatus Status);
public record CreateTicketRequest(string SenderId, string Content, string Priority);
public record UpdateTicketStatusRequest(TicketStatus Status);
public record ApiError(string Code, string Message);