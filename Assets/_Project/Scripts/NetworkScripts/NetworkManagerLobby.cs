using System;
using System.Collections.Generic;
using UnityEngine;
using Unity.Services.Core;
using Unity.Services.Authentication;
using System.Threading.Tasks;
using Unity.Netcode;
using Unity.Services.Lobbies;
using Unity.Services.Lobbies.Models;
using System.Net;
using System.Net.Sockets;
using UnityEngine.SceneManagement;
using Unity.Netcode.Transports.UTP;
using STUN.Attributes;
using STUN;

public class NetworkManagerLobby : MonoBehaviour
{
    [SerializeField] private int _maxPlayers = 2;
    private Lobby _hostLobby;
    private Lobby _clientLobby;
    private float _lobbyUpdateTimer;
    public Action OnLobbyUpdate;
    private bool _connected;
    public static async void Authenticate()
    {
        await UnityServices.InitializeAsync();
        AuthenticationService.Instance.SignedIn += () =>
        {
            Debug.Log("Signed in:" + AuthenticationService.Instance.PlayerId);
        };
        await AuthenticationService.Instance.SignInAnonymouslyAsync();
    }

    void Update()
    {
        HandleLobbyPollForUpdates();
    }

    public async Task<string> CreateLobby(DemonData demonData)
    {
        CreateLobbyOptions createLobbyOptions = new CreateLobbyOptions
        {
            IsPrivate = false,
            Player = GetPlayer(demonData)
        };
        Lobby lobby = await LobbyService.Instance.CreateLobbyAsync("darklings", _maxPlayers, createLobbyOptions);
        _hostLobby = lobby;
        NetworkManager.Singleton.StartHost();
        return lobby.LobbyCode;
    }

    public Lobby GetHostLobby()
    {
        return _hostLobby;
    }

    public Lobby GetClientLobby()
    {
        return _clientLobby;
    }

    public async Task<Lobby> JoinLobby(DemonData demonData, string lobbyId)
    {
        JoinLobbyByCodeOptions joinLobbyByCodeOptions = new JoinLobbyByCodeOptions
        {
            Player = GetPlayer(demonData)
        };
        Lobby lobby = await Lobbies.Instance.JoinLobbyByCodeAsync(lobbyId, joinLobbyByCodeOptions);
        _clientLobby = lobby;
        NetworkManager.Singleton.StartClient();
        return lobby;
    }

    public async void UpdateLobbyReady(bool ready, bool isHost)
    {
        string lobbyId;
        string playerId;
        if (isHost)
        {
            lobbyId = _hostLobby.Id;
            playerId = _hostLobby.Players[0].Id;
        }
        else
        {
            lobbyId = _clientLobby.Id;
            playerId = _clientLobby.Players[1].Id;
        }
        UpdateLobbyOptions updateLobbyOptions = new UpdateLobbyOptions();
        await LobbyService.Instance.UpdatePlayerAsync(lobbyId, AuthenticationService.Instance.PlayerId, new UpdatePlayerOptions
        {
            Data = new Dictionary<string, PlayerDataObject>{
                { "Ready", new PlayerDataObject(PlayerDataObject.VisibilityOptions.Public, ready.ToString())}
            }
        });
    }

    public async Task DeleteLobby()
    {
        string id = _hostLobby.Id;
        _hostLobby = null;
        await LobbyService.Instance.DeleteLobbyAsync(id);
    }

    public async Task LeaveLobby()
    {
        await LobbyService.Instance.RemovePlayerAsync(_clientLobby.Id, AuthenticationService.Instance.PlayerId);
    }

    private async void HandleLobbyPollForUpdates()
    {
        if (_hostLobby != null)
        {
            _lobbyUpdateTimer -= Time.unscaledDeltaTime;
            if (_lobbyUpdateTimer < 0)
            {
                _lobbyUpdateTimer = 1.1f;
                try
                {
                    Lobby lobby = await LobbyService.Instance.GetLobbyAsync(_hostLobby.Id);
                    _hostLobby = lobby;
                    OnLobbyUpdate?.Invoke();
                }
                catch (System.Exception)
                {
                }
            }
        }
        else if (_clientLobby != null)
        {
            _lobbyUpdateTimer -= Time.unscaledDeltaTime;
            if (_lobbyUpdateTimer < 0)
            {
                _lobbyUpdateTimer = 1.1f;
                Lobby lobby = await LobbyService.Instance.GetLobbyAsync(_clientLobby.Id);
                _clientLobby = lobby;
                OnLobbyUpdate?.Invoke();
            }
        }
    }

    private Unity.Services.Lobbies.Models.Player GetPlayer(DemonData demonData)
    {
        string address = "";
        string port = "";
        PublicIp(out address, out port);
        return new Unity.Services.Lobbies.Models.Player
        {
            Data = new Dictionary<string, PlayerDataObject>{
                { "DemonName", new PlayerDataObject(PlayerDataObject.VisibilityOptions.Public, demonData.demonName)},
                { "Character", new PlayerDataObject(PlayerDataObject.VisibilityOptions.Public, demonData.character.ToString())},
                { "Assist", new PlayerDataObject(PlayerDataObject.VisibilityOptions.Public, demonData.assist.ToString())},
                { "Color", new PlayerDataObject(PlayerDataObject.VisibilityOptions.Public, demonData.color.ToString())},
                { "Ready", new PlayerDataObject(PlayerDataObject.VisibilityOptions.Public, "False")},
                { "Ip", new PlayerDataObject(PlayerDataObject.VisibilityOptions.Public, address)},
                { "Port", new PlayerDataObject(PlayerDataObject.VisibilityOptions.Public,port)},
                { "PrivateIp", new PlayerDataObject(PlayerDataObject.VisibilityOptions.Public, PrivateIp())},
            }
        };
    }
    private string PrivateIp()
    {
        var host = Dns.GetHostEntry(Dns.GetHostName());
        foreach (var ip in host.AddressList)
        {
            if (ip.AddressFamily == AddressFamily.InterNetwork)
            {
                return ip.ToString();
            }
        }
        return "127.0.0.1";
    }

    private void PublicIp(out string address, out string port)
    {
        if (!STUNUtils.TryParseHostAndPort("stun1.l.google.com:19302", out IPEndPoint stunEndPoint))
            throw new Exception("Failed to resolve STUN server address");

        STUNClient.ReceiveTimeout = 500;
        var queryResult = STUNClient.Query(stunEndPoint, STUNQueryType.ExactNAT, true, NATTypeDetectionRFC.Rfc3489);

        if (queryResult.QueryError != STUNQueryError.Success)
            throw new Exception("Query Error: " + queryResult.QueryError.ToString());

        address = queryResult.PublicEndPoint.Address.ToString();
        port = queryResult.PublicEndPoint.Port.ToString();
    }
}
