#include "ProxyServer.h"
#include <slikenet\peerinterface.h>
#include <slikenet\MessageIdentifiers.h>
#include <slikenet\NatPunchthroughServer.h>
#include <slikenet\sleep.h>
#include <slikenet\BitStream.h>

using namespace SLNet;

enum NetMsg
{
	ID_HOST = ID_USER_PACKET_ENUM,
	ID_UPDATE
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
		proxy->Info(Format("Punchtrough: %s", msg));
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
	Info("Initializing proxy server.");
	closing = false;
	peer = RakPeerInterface::GetInstance();
	punch_server = new NatPunchthroughServer;
	peer->AttachPlugin(punch_server);
	debug_logging.proxy = this;
	punch_server->SetDebugInterface(&debug_logging);

	SocketDescriptor sd(port, 0);
	sd.socketFamily = AF_INET;
	StartupResult r = peer->Startup(players, &sd, 1);
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
				break;
			case ID_DISCONNECTION_NOTIFICATION:
			case ID_CONNECTION_LOST:
				if(server)
				{
					msg.type = MSG_REMOVE_SERVER;
					msg.id = server->id;
					msg.str = nullptr;
					callback(&msg);
					for(auto it = servers.begin(), end = servers.end(); it != end; ++it)
					{
						if(*it == server)
						{
							servers.erase(it);
							break;
						}
					}
				}
				else
					Info(Format("Disconnected at %s.", packet->systemAddress.ToString()));
				break;
			case ID_HOST:
				if(server)
					Error(Format("ID_HOST from existing server %d at %s.", server->id, packet->systemAddress.ToString()));
				else
				{
					string name;
					int players, flags;
					if(!stream.Read(name) || !stream.Read(players) || !stream.Read(flags))
						Error(Format("Broken ID_HOST from %s.", packet->systemAddress.ToString()));
					else
					{
						msg.type = MSG_CREATE_SERVER;
						msg.str = name.c_str();
						msg.players = players;
						msg.flags = flags;
						int id = callback(&msg);
						if(id != -1)
						{
							Server* server = new Server;
							server->id = id;
							server->adr = packet->systemAddress;
							servers.push_back(server);
						}
					}
				}
				break;
			case ID_UPDATE:
				if(!stream.Read(msg.players))
					Error(Format("Broken ID_UPDATE from %s.", packet->systemAddress.ToString()));
				else
				{
					msg.type = MSG_UPDATE_SERVER;
					msg.id = server->id;
					msg.str = nullptr;
					callback(&msg);
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

void ProxyServer::Info(cstring str)
{
	msg.type = MSG_INFO;
	msg.str = str;
	callback(&msg);
}

void ProxyServer::Error(cstring str)
{
	msg.type = MSG_ERROR;
	msg.str = str;
	callback(&msg);
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
