using System.Net;
using System.Net.Http.Json;

var apiBaseUrl = args.Length > 0 ? args[0] : "http:localhost:3000";

using var http = new HttpClient
{
    BaseAddress = new Uri(apiBaseUrl)
};

var tickets = await http.GetFromJsonAsync<List<TicketDto>>("api/tickets");
Console.WriteLine("Danh sách phiếu: ");
foreach(var t in tickets ?? [])
{
    Console.WriteLine($"[{t.Id}] Tu: {t.SenderId} | Nội dung: {t.Content} | Ưu tiên: {t.Priority} | Trạng thái: {t.Status}");
}

var createRequest = new CreateTicketRequest(
    "lvquyen15506",
    "Loi Khong the thanh toan",
    "Hight");
var createRespone = await http.PostAsJsonAsync("/api/tickets", createRequest);

if (createRespone.StatusCode == HttpStatusCode.Created)
{
    var created = await createRespone.Content.ReadFromJsonAsync<TicketDto>();
    Console.WriteLine($"\nĐã tạo yêu cầu mới: [{created?.Id}] {created?.Content}");
} else
{
    var error = await createRespone.Content.ReadFromJsonAsync<ApiError>();
    Console.WriteLine($"\nLoi: {error?.Code} - {error?.Message}");
    return;
}

var detail = await http.GetFromJsonAsync<TicketDto>("api/tickets/1");
Console.WriteLine($"\nChi tiết phiếu yêu cầu 1: do {detail?.SenderId} gửi với nội dung: {detail?.Content}");

//cập nhật status
Console.WriteLine("\nCập nhật trạng thái Ticket 1 sang InProgress:");
var updateRequest = new UpdateTicketStatusRequest(TicketStatus.InProgress);

var patchRespone = await http.PatchAsJsonAsync("api/tickets/1/status", updateRequest);

if (patchRespone.IsSuccessStatusCode)
{
    var updateTicket = await patchRespone.Content.ReadFromJsonAsync<TicketDto>();
    Console.WriteLine($"Cập nhật thành công! Nội dung: {updateTicket?.Content}");
    Console.WriteLine($"Trạng thái mới hiện tại là: {updateTicket?.Status}");
}
else
{
    var patchError =await patchRespone.Content.ReadFromJsonAsync<ApiError>();
    Console.WriteLine($"Lỗi cập nhật (HTTP {patchRespone.StatusCode}: {patchError}");
}

//cố tình post sai

var invalidRespone = await http.PostAsJsonAsync("api/tickets", new CreateTicketRequest("", "", "Low"));
Console.WriteLine($"\nThu gui du lieu sai: {(int)invalidRespone.StatusCode} {invalidRespone.StatusCode}");

var invalidError = await invalidRespone.Content.ReadFromJsonAsync<ApiError>();
Console.WriteLine($"{invalidError?.Code}: {invalidError?.Message}");


public enum TicketStatus { New, Assigned, InProgress, Resolved }

public record TicketDto(int Id, string SenderId, string Content, string Priority, string? Assignee, TicketStatus Status);
public record CreateTicketRequest(string SenderId, string Content, string Priority);
public record UpdateTicketStatusRequest(TicketStatus Status);
public record ApiError(string Code, string Message);