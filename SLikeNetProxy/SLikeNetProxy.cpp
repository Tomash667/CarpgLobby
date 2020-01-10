#include "ProxyServer.h"

#define EXPORT extern "C" __declspec(dllexport)

ProxyServer proxy;

EXPORT bool __stdcall InitProxy(int players, int port, Callback callback)
{
	return proxy.Start(players, port, callback);
}

EXPORT void __stdcall RestartProxy()
{
	proxy.Restart();
}

EXPORT void __stdcall ShutdownProxy()
{
	proxy.Shutdown();
}
