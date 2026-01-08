#pragma once
#include "pch.h"
using namespace std;

struct MaskedByte {
	BYTE Value;
	BYTE Mask;
};

std::vector<BYTE> GetMemoryBuffer(LPVOID baseAddress, SIZE_T size);

//std::vector<int> ParseSignatureString(string signature);
std::vector<MaskedByte> ParseSignatureString(string signature);

//LPCVOID ScanForFirstResult(LPVOID baseAddress, std::vector<BYTE>* buffer, std::vector<int>* signature, string errorMsgName);
LPCVOID ScanForFirstResult(LPVOID baseAddress, std::vector<BYTE>* buffer, std::vector<MaskedByte>* signature, string errorMsgName);
//std::vector<LPCVOID> ScanForAllResults(LPVOID baseAddress, std::vector<BYTE>* buffer, std::vector<int>* signature, string errorMsgName);
std::vector<LPCVOID> ScanForAllResults(LPVOID baseAddress, std::vector<BYTE>* buffer, std::vector<MaskedByte>* signature, string errorMsgName);

LPCVOID ScanForFirstResult(LPVOID baseAddress, std::vector<BYTE>* buffer, string signature, string errorMsgName);

std::vector<LPCVOID> ScanForAllResults(LPVOID baseAddress, std::vector<BYTE>* buffer, string signature, string errorMsgName);

extern "C" __declspec(dllexport)
LPCVOID NH_ManagedScanForFirst(LPVOID baseAddress, SIZE_T buffer_size, const char* signature, const char* errorMsgName);