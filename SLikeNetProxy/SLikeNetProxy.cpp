#include <slikenet\peerinterface.h>
#include <slikenet\NatPunchthroughServer.h>
#include <slikenet\sleep.h>
#include <thread>

#define EXPORT extern "C" __declspec(dllexport)

using namespace SLNet;
typedef void (__stdcall* Callback)(const char*);

struct DebugInterface : public NatPunchthroughServerDebugInterface
{
	Callback callback;
	void OnServerMessage(const char *msg)
	{
		callback(msg);
	}
};

RakPeerInterface* peer;
NatPunchthroughServer* server;
DebugInterface logging;
std::thread thread;
bool closing;
void ThreadStart();

EXPORT bool InitProxy(int players, int port, Callback callback)
{
	peer = RakPeerInterface::GetInstance();
	server = new NatPunchthroughServer;
	peer->AttachPlugin(server);
	logging.callback = callback;
	server->SetDebugInterface(&logging);

	SocketDescriptor sd(port, 0);
	sd.socketFamily = AF_INET;
	StartupResult r = peer->Startup(players, &sd, 1);
	if(r != 0)
		return false;
	peer->SetMaximumIncomingConnections(players);

	thread = std::thread(ThreadStart);

	return true;
}

void ThreadStart()
{
	Packet* packet;
	while(!closing)
	{
		for(packet = peer->Receive(); packet; peer->DeallocatePacket(packet), packet = peer->Receive())
		{
		}

		RakSleep(30);
	}

	peer->DetachPlugin(server);
	delete server;
	peer->Shutdown(100);
	RakPeerInterface::DestroyInstance(peer);
}

EXPORT void ShutdownProxy()
{
	closing = true;
	thread.join();
}
