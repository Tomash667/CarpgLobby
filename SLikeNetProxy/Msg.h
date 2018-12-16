#pragma once

enum MsgType
{
	MSG_VERBOSE,
	MSG_INFO,
	MSG_WARNING,
	MSG_ERROR,
	MSG_CREATE_SERVER,
	MSG_UPDATE_SERVER,
	MSG_REMOVE_SERVER,
	MSG_STAT
};

enum MsgStat
{
	STAT_CONNECT,
	STAT_DISCONNECT
};

typedef int(__stdcall* Callback)(int*);
