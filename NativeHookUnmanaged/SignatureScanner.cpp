#include "pch.h"
#include "SignatureScanner.h"
using namespace std;

vector<BYTE> GetMemoryBuffer(LPVOID baseAddress, SIZE_T size)
{
    vector<BYTE> buffer(size);
    SIZE_T numOfBytesRead;
    HANDLE curProc = OpenProcess(PROCESS_VM_READ, false, GetCurrentProcessId());
    if (ReadProcessMemory(curProc, baseAddress, buffer.data(), size, &numOfBytesRead) == 0)
    {
        cout << "Failed to get memory buffer";
        return buffer;
    }
    CloseHandle(curProc);
    return buffer;
}

vector<int> ParseSignatureString(string signature)
{
    vector<int> result;
    int offset = 0; 
    int spaceIndex = 0;
    while (spaceIndex != string::npos)
    {
        string thisByteStr = signature.substr(spaceIndex);
        offset = spaceIndex + 1;
        if (thisByteStr == "??") result.push_back(-1);
        else result.push_back(stoi(thisByteStr, 0, 16));
        spaceIndex = signature.find(' ', offset);
    }
    return result;
}

LPCVOID ScanForFirstResult(LPVOID baseAddress, vector<BYTE>* buffer, vector<int>* signature, string functionName)
{
    if (buffer == NULL || signature == NULL)
    {
        cout << "Failed to find address for function " + functionName + ". Null buffer or signature";
        return 0;
    }
    for (int i = 0; i < buffer->size(); i++)
    {
        for (int j = 0; j < signature->size(); j++)
        {
            int element = (*signature)[j];
            if (element != -1 && element != (*buffer)[i + j]) break;
            if (j + 1 == signature->size())
            {
                return (char*)baseAddress + i;
            }
        }
    }
    cout << "Failed to find address for function " + functionName;
    return 0;
}

vector<LPCVOID> ScanForAllResults(LPVOID baseAddress, vector<BYTE>* buffer, vector<int>* signature, string functionName)
{
    vector<LPCVOID> hits;
    if (buffer == NULL || signature == NULL)
    {
        cout << "Failed to find address for function " + functionName + ". Null buffer or signature";
        return hits;
    }
    for (int i = 0; i < buffer->size(); i++)
    {
        for (int j = 0; j < signature->size(); j++)
        {
            int element = (*signature)[j];
            if (element != -1 && element != (*buffer)[i + j]) break;
            if (j + 1 == signature->size())
            {
                hits.push_back((LPCVOID)((char*)baseAddress + i));
            }
        }
    }
    if (hits.empty()) cout << "Failed to find any matches for function " + functionName;
    return hits;
}

LPCVOID ScanForFirstResult(LPVOID baseAddress, vector<BYTE>* buffer, string signature, string functionName)
{
    vector<int> signatureBytes = ParseSignatureString(signature);
    return ScanForFirstResult(baseAddress, buffer, &signatureBytes, functionName);
}

vector<LPCVOID> ScanForAllResults(LPVOID baseAddress, vector<BYTE>* buffer, string signature, string functionName)
{
    vector<int> signatureBytes = ParseSignatureString(signature);
    return ScanForAllResults(baseAddress, buffer, &signatureBytes, functionName);
}