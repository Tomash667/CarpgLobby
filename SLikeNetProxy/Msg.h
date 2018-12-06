#pragma once

enum MsgType
{
	MSG_INFO,
	MSG_ERROR,
	MSG_CREATE_SERVER,
	MSG_UPDATE_SERVER,
	MSG_REMOVE_SERVER
};

typedef int(__stdcall* Callback)(int*);
