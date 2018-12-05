#pragma once

enum MsgType
{
	MSG_INFO,
	MSG_ERROR,
	MSG_CREATE_SERVER,
	MSG_UPDATE_SERVER,
	MSG_REMOVE_SERVER
};

struct Msg
{
	MsgType type;
	const char* str;
	int id, players, flags;
};

typedef int(__stdcall* Callback)(Msg*);
