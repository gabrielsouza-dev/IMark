using IMark.Shared.Interfaces;
using IMark.Shared.Models.DTO.TimeTrackings;
using IMark.Shared.Models.Requests;
using Microsoft.AspNetCore.Components;
using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Http.Json;
using System.Text;
using System.Text.Json;

namespace IMark.Shared.Services;

public class TimeCheckService
{
    private readonly HttpClient _http;

    public TimeCheckService(HttpClient http)
    {
        _http = http;
    }

    public async Task<List<TimeEntryDTO>> GetAll()
    {
        var response = await _http.GetAsync("api/timecheck");
        var message = await response.Content.ReadAsStringAsync();

        response.EnsureSuccessStatusCode();

        return JsonSerializer.Deserialize<List<TimeEntryDTO>>(message, new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true
        }) ?? new List<TimeEntryDTO>();
    }

    public async Task<TimeEntryDTO> GetById(Guid id)
    {
        var response = await _http.GetAsync($"api/timecheck/{id}");
        var message = await response.Content.ReadAsStringAsync();

        response.EnsureSuccessStatusCode();

        return JsonSerializer.Deserialize<TimeEntryDTO>(message, new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true
        }) ?? new TimeEntryDTO();
    }

    public async Task<List<TimeEntryDTO>> GetAllEntriesNoIncludes()
    {
        var response = await _http.GetAsync("api/timecheck/all-entries-no-includes");
        var message = await response.Content.ReadAsStringAsync();

        response.EnsureSuccessStatusCode();

        return JsonSerializer.Deserialize<List<TimeEntryDTO>>(message, new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true
        }) ?? new List<TimeEntryDTO>();
    }

    public async Task IncludeAsync(TimeCheckRequest request)
    {
        var response = await _http.PostAsJsonAsync("api/timecheck", request);
        response.EnsureSuccessStatusCode();
    }

    public async Task<bool> UpdateTimeEntryAsync(Guid id, TimeEntryDTO dto)
    {
        var response = await _http.PutAsJsonAsync($"api/timecheck/timeentry/{dto.Id}", dto);

        response.EnsureSuccessStatusCode();

        return true;
    }

    public async Task<TimeEntryDTO> GetTodayAsync()
    {
        var localDateNow = DateOnly.FromDateTime(DateTime.UtcNow.ToLocalTime());
        var isoDate = localDateNow.ToString("yyyy-MM-dd");
        var response = await _http.GetAsync($"api/timecheck/get-by-day/{isoDate}");

        response.EnsureSuccessStatusCode();

        var content = await response.Content.ReadAsStringAsync();

        if (string.IsNullOrWhiteSpace(content))
            return new TimeEntryDTO();

        return JsonSerializer.Deserialize<TimeEntryDTO>(content, new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true
        }) ?? new TimeEntryDTO();
    }
}