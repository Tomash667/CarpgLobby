#include "ProxyServer.h"
#include <slikenet\peerinterface.h>
#include <slikenet\MessageIdentifiers.h>
#include <slikenet\NatPunchthroughServer.h>
#include <slikenet\sleep.h>
#include <slikenet\BitStream.h>

using namespace SLNet;

enum NetMsg
{
	ID_HOST = 160,
	ID_UPDATE
};

enum HostResult
{
	HOST_OK,
	HOST_BROKEN_DATA,
	HOST_INVALID_DATA
};

struct Server
{
	int id;
	SystemAddress adr;
};

struct DebugInterface : public NatPunchthroughServerDebugInterface
{
	ProxyServer* proxy;
	void OnServerMessage(const char *msg)
	{
		proxy->Notify(MSG_VERBOSE, Format("NAT: %s", msg));
	}
} debug_logging;

bool ProxyServer::Start(int players, int port, Callback callback)
{
	this->callback = callback;
	if(!Init(players, port))
		return false;
	Info("Starting proxy server thread.");
	thread = std::thread(&ProxyServer::Run, this);
	return true;
}

bool ProxyServer::Init(int players, int port)
{
	this->players = players;
	this->port = port;

	buf = new BitStream;
	Info("Initializing proxy server.");
	closing = false;
	peer = RakPeerInterface::GetInstance();
	punch_server = new NatPunchthroughServer;
	peer->AttachPlugin(punch_server);
	peer->SetTimeoutTime(5000, UNASSIGNED_SYSTEM_ADDRESS);
	debug_logging.proxy = this;
	punch_server->SetDebugInterface(&debug_logging);

	SocketDescriptor sd[2];
	sd[0].port = port;
	int sd_count = 1;
	if(peer->GetNumberOfAddresses() > 1)
	{
		strcpy_s(sd[0].hostAddress, peer->GetLocalIP(0));
		sd[1].port = port + 1;
		strcpy_s(sd[1].hostAddress, peer->GetLocalIP(1));
		Info("Using two IP addresses.");
		sd_count = 2;
	}
	StartupResult r = peer->Startup(players, sd, sd_count);
	if(r != 0)
	{
		Error(Format("Failed to initialize proxy server (%d).", r));
		return false;
	}
	peer->SetMaximumIncomingConnections(players);
	return true;
}

void ProxyServer::Run()
{
	Packet* packet;
	while(!closing)
	{
		for(packet = peer->Receive(); packet; peer->DeallocatePacket(packet), packet = peer->Receive())
		{
			BitStream stream(packet->data, packet->length, false);
			Server* server = FindServer(packet->systemAddress);
			byte type;
			stream.Read(type);
			switch(type)
			{
			case ID_NEW_INCOMING_CONNECTION:
				Info(Format("New incoming connection from %s.", packet->systemAddress.ToString()));
				buf->Reset();
				buf->Write(0);
				buf->WriteCasted<byte>(MSG_STAT);
				buf->WriteCasted<byte>(STAT_CONNECT);
				SendMsg();
				break;
			case ID_DISCONNECTION_NOTIFICATION:
			case ID_CONNECTION_LOST:
				if(server)
				{
					buf->Reset();
					buf->Write(0);
					buf->WriteCasted<byte>(MSG_REMOVE_SERVER);
					buf->Write(packet->systemAddress.ToString());
					buf->Write(server->id);
					buf->Write(type == ID_CONNECTION_LOST);
					int r = SendMsg();
					for(auto it = servers.begin(), end = servers.end(); it != end; ++it)
					{
						if(*it == server)
						{
							servers.erase(it);
							break;
						}
					}
					if(r == -1)
						Error(Format("Remove server %d failed from %s.", server->id, packet->systemAddress.ToString()));
				}
				else if(type == ID_CONNECTION_LOST)
					Info(Format("Disconnected from %s.", packet->systemAddress.ToString()));
				else
					Info(Format("Lost connection from %s.", packet->systemAddress.ToString()));
				buf->Reset();
				buf->Write(0);
				buf->WriteCasted<byte>(MSG_STAT);
				buf->WriteCasted<byte>(STAT_DISCONNECT);
				SendMsg();
				break;
			case ID_HOST:
				if(server)
					Error(Format("ID_HOST from existing server %d at %s.", server->id, packet->systemAddress.ToString()));
				else
				{
					string name;
					int players, flags, version;
					byte b[2] = { ID_HOST, HOST_OK };
					if(!ReadString1(stream, name) || !stream.Read(players) || !stream.Read(flags) || !stream.Read(version))
					{
						Error(Format("Broken ID_HOST from %s.", packet->systemAddress.ToString()));
						b[1] = HOST_BROKEN_DATA;
					}
					else
					{
						buf->Reset();
						buf->Write(0);
						buf->WriteCasted<byte>(MSG_CREATE_SERVER);
						buf->Write(name.c_str());
						buf->Write(peer->GetGuidFromSystemAddress(packet->systemAddress).ToString());
						buf->Write(packet->systemAddress.ToString());
						buf->Write(players);
						buf->Write(flags);
						buf->Write(version);
						int id = SendMsg();
						if(id != -1)
						{
							Server* server = new Server;
							server->id = id;
							server->adr = packet->systemAddress;
							servers.push_back(server);
						}
						else
						{
							Error(Format("ID_HOST failed from %s.", packet->systemAddress.ToString()));
							b[1] = HOST_INVALID_DATA;
						}
					}
					peer->Send((cstring)b, 2, MEDIUM_PRIORITY, RELIABLE, 0, packet->systemAddress, false);
				}
				break;
			case ID_UPDATE:
				if(!server)
					Error(Format("ID_UPDATE from non server %s.", packet->systemAddress.ToString()));
				else
				{
					int players;
					if(!stream.Read(players))
						Error(Format("Broken ID_UPDATE %d from %s.", server->id, packet->systemAddress.ToString()));
					else
					{
						buf->Reset();
						buf->Write(0);
						buf->WriteCasted<byte>(MSG_UPDATE_SERVER);
						buf->Write(packet->systemAddress.ToString());
						buf->Write(server->id);
						buf->Write(players);
						if(SendMsg() == -1)
							Error(Format("ID_UPDATE %d failed from %s.", server->id, packet->systemAddress.ToString()));
					}
				}
				break;
			default:
				Info(Format("Unknown message %d from %s.", type, packet->systemAddress.ToString()));
				break;
			}
		}

		RakSleep(30);
	}
}

void ProxyServer::Cleanup()
{
	delete buf;
	peer->DetachPlugin(punch_server);
	delete punch_server;
	peer->Shutdown(100);
	RakPeerInterface::DestroyInstance(peer);
}

void ProxyServer::Shutdown()
{
	closing = true;
	Info("Closing proxy server.");
	thread.join();
	Cleanup();
}

void ProxyServer::Restart()
{
	Info("Restarting proxy server.");
	closing = true;
	thread.join();
	Cleanup();
	Init(players, port);
	Info("Starting proxy server thread.");
	thread = std::thread(&ProxyServer::Run, this);
}

void ProxyServer::Notify(MsgType type, cstring str)
{
	buf->Reset();
	buf->Write(0);
	buf->WriteCasted<byte>(type);
	buf->Write(str);
	SendMsg();
}

Server* ProxyServer::FindServer(const SystemAddress& adr)
{
	for(Server* server : servers)
	{
		if(server->adr == adr)
			return server;
	}
	return nullptr;
}

int ProxyServer::SendMsg()
{
	int len = buf->GetNumberOfBytesUsed() - 4;
	int* data = (int*)buf->GetData();
	memcpy(data, &len, sizeof(int));
	return callback(data);
}

bool ProxyServer::ReadString1(SLNet::BitStream& stream, string& str)
{
	byte len;
	if(!stream.Read(len))
		return false;
	if(len == 0)
		str.clear();
	else
	{
		str.resize(len);
		if(!stream.Read((char*)str.c_str(), len))
			return false;
	}
	return true;
}
