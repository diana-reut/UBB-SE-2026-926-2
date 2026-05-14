using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Threading.Tasks;
using Common.Data.Entity.DTOs;
using Common.Data.Models;
using ERManagementSystem.Proxy;

namespace ERManagementSystem.Proxy.ERRoomProxy;

public class ERRoomProxy : ProxyBase, IERRoomProxy
{
    private const string BaseUri = "api/er-rooms";

    public ERRoomProxy(HttpClient httpClient)
        : base(httpClient)
    {
    }

    public async Task<List<ER_Room>> GetAllAsync()
    {
        return await GetAsync<List<ER_Room>>(BaseUri) ?? new List<ER_Room>();
    }

    public Task<ER_Room?> GetByIdAsync(int id)
    {
        return GetAsync<ER_Room>($"{BaseUri}/{id}");
    }

    public async Task<ER_Room> CreateAsync(ER_Room room)
    {
        return await PostAsync<ER_Room, ER_Room>(BaseUri, room) ?? room;
    }

    public Task UpdateAsync(int id, ER_Room room)
    {
        return PutAsync($"{BaseUri}/{id}", room);
    }

    public Task DeleteAsync(int id)
    {
        return DeleteRequestAsync($"{BaseUri}/{id}");
    }

    public async Task<List<ER_Room>> GetRoomsByStatusAsync(string status)
    {
        return await GetAsync<List<ER_Room>>($"{BaseUri}/status/{status}") ?? new List<ER_Room>();
    }

    public Task<List<ER_Room>> GetAvailableRoomsAsync()
    {
        return GetRoomsByStatusAsync(ER_Room.RoomStatus.Available);
    }

    public Task<List<ER_Room>> GetOccupiedRoomsAsync()
    {
        return GetRoomsByStatusAsync(ER_Room.RoomStatus.Occupied);
    }

    public Task<List<ER_Room>> GetCleaningRoomsAsync()
    {
        return GetRoomsByStatusAsync(ER_Room.RoomStatus.Cleaning);
    }

    public async Task SetCurrentVisitAsync(int roomId, int visitId)
    {
        ER_Room room = await GetByIdAsync(roomId)
            ?? throw new InvalidOperationException($"ER room {roomId} was not found.");

        room.Current_Visit_ID = visitId;
        await UpdateAsync(roomId, room);
    }

    public async Task ClearCurrentVisitAsync(int roomId)
    {
        ER_Room room = await GetByIdAsync(roomId)
            ?? throw new InvalidOperationException($"ER room {roomId} was not found.");

        room.Current_Visit_ID = null;
        await UpdateAsync(roomId, room);
    }

    public Task<ERRoomVisitDetailsDto?> GetVisitDetailsAsync(int roomId)
    {
        return GetAsync<ERRoomVisitDetailsDto>($"{BaseUri}/{roomId}/visit-details");
    }

    public Task MarkRoomAsCleaningAsync(int roomId)
    {
        return PostAsync($"{BaseUri}/{roomId}/mark-cleaning");
    }

    public Task MarkRoomAsAvailableAsync(int roomId)
    {
        return PostAsync($"{BaseUri}/{roomId}/mark-available");
    }
}
