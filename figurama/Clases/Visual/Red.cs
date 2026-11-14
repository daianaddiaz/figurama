using Godot;

public static class Red
{
    // La conexión existe y está establecida (evita errores si el peer ya se cerró)
    public static bool Conectada(MultiplayerApi api)
    {
        MultiplayerPeer peer = api.MultiplayerPeer;
        if (peer == null || peer is OfflineMultiplayerPeer) return false;
        return peer.GetConnectionStatus() == MultiplayerPeer.ConnectionStatus.Connected;
    }

    public static bool EsServidor(MultiplayerApi api)
    {
        return Conectada(api) && api.IsServer();
    }
}