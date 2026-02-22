#include <string>
#include "framework.h"
#include "Structs.h"
class UpdateMovDynSysHook
{
private:
	static LPCVOID Address;
	const static NativeHookConfiguration ConfigValue;
	const static std::string Name;
	const static std::string Signature;
	static void(*ManagedCallback)(int agentIndex, float dt, unsigned int oldFlags, unsigned int newFlags);
	static void(*Original)(LPBYTE, LPVOID, float, LPBYTE, BYTE);
	void static Detour(LPBYTE dynamicsSystemPtr, LPVOID missionPtr, float dt, LPBYTE agentRecPtr, BYTE param);
public:
	static void Create(NativeHookConfiguration currentConfig, LPCVOID nativeDLLAdress, std::vector<BYTE>* buffer, LPCVOID managedCallback);
};

