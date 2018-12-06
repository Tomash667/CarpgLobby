#pragma once

#include "Core.h"
#include "Msg.h"
#include <thread>

namespace SLNet
{
	class RakPeerInterface;
	class NatPunchthroughServer;
	class BitStream;
	struct SystemAddress;
}
struct Server;

class ProxyServer
{
public:
	bool Start(int players, int port, Callback callback);
	bool Init(int players, int port);
	void Run();
	void Shutdown();
	void Cleanup();
	void Info(const char* msg);
	void Error(const char* msg);
	Server* FindServer(const SLNet::SystemAddress& adr);
	int SendMsg();

	Callback callback;
	SLNet::RakPeerInterface* peer;
	SLNet::NatPunchthroughServer* punch_server;
	SLNet::BitStream* buf;
	std::thread thread;
	vector<Server*> servers;
	bool closing;
};
