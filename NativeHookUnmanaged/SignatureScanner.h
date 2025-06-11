#pragma once
#include "pch.h"
using namespace std;

std::vector<BYTE> GetMemoryBuffer(LPVOID baseAddress, SIZE_T size);

std::vector<int> ParseSignatureString(string signature);

LPCVOID ScanForFirstResult(LPVOID baseAddress, std::vector<BYTE>* buffer, std::vector<int>* signature, string functionName);

std::vector<LPCVOID> ScanForAllResults(LPVOID baseAddress, std::vector<BYTE>* buffer, std::vector<int>* signature, string functionName);

LPCVOID ScanForFirstResult(LPVOID baseAddress, std::vector<BYTE>* buffer, string signature, string functionName);

std::vector<LPCVOID> ScanForAllResults(LPVOID baseAddress, std::vector<BYTE>* buffer, string signature, string functionName);