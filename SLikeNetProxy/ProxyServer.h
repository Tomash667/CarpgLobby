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
	void Restart();
	void Cleanup();
	void Info(cstring msg) { Notify(MSG_INFO, msg); }
	void Error(cstring msg) { Notify(MSG_ERROR, msg); }
	void Notify(MsgType type, cstring msg);
	Server* FindServer(const SLNet::SystemAddress& adr);
	int SendMsg();
	bool ReadString1(SLNet::BitStream& stream, string& str);

	Callback callback;
	SLNet::RakPeerInterface* peer;
	SLNet::NatPunchthroughServer* punch_server;
	SLNet::BitStream* buf;
	std::thread thread;
	vector<Server*> servers;
	int players, port;
	bool closing;
};
