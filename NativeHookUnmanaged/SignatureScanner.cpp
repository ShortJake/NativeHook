#include "pch.h"
#include "SignatureScanner.h"
using namespace std;

//TODO: Improve signature scanning speed. Currently it takes ~2sec
const BYTE WILDCARD_BYTE_MASK = _UI8_MAX;
vector<BYTE> GetMemoryBuffer(LPVOID baseAddress, SIZE_T size)
{
    vector<BYTE> buffer(size);
    SIZE_T numOfBytesRead;
    HANDLE curProc = OpenProcess(PROCESS_VM_READ, false, GetCurrentProcessId());
    if (ReadProcessMemory(curProc, baseAddress, buffer.data(), size, &numOfBytesRead) == 0)
    {
        Helpers::ShowErrorMessage("Failed to get memory buffer");
        return buffer;
    }
    CloseHandle(curProc);
    return buffer;
}

vector<MaskedByte> ParseSignatureString(string signature)
{
    vector<MaskedByte> result;
    int offset = 0; 
    int spaceIndex = 0;
    while (spaceIndex != string::npos && offset != string::npos)
    {
        MaskedByte byte;
        byte.Value = 0;
        byte.Mask = 0;
        // The token is the string in between 2 spaces, which should be represent a byte in the signature
        spaceIndex = signature.find(' ', offset);
        int count = _I64_MAX;
        if (spaceIndex != string::npos) count = spaceIndex - offset;
        string token = signature.substr(offset, count);
        // Set the offset to be the next non-space character
        offset = signature.find_first_not_of(' ', spaceIndex);
        // If this token starts with '[' it represents a masked byte, i.e. one in which some bits are wildcards
        if (token[0] == '[')
        {
            // 8 bits in a byte
            for (int i = 1; i <= 8; i++)
            {
                if (i >= token.length() || token[i] == ']') break;
                // If the current character is anything but a 0/1, treat it as a wildcard
                // token.length - 2 - i because strings have the 0th place at the left but the numbers at the right
                // -2 to account for the brackets
                int digitPlace = pow(2, token.length() - 2 - i);
                if (token[i] == '1') byte.Value += digitPlace;
                else if (token[i] != '0') byte.Mask += digitPlace;
            }
        }
        // Otherwise try changing the token to a base-16 int directly. If it fails, default to a full wildcard byte
        else 
        {
            try
            {
                byte.Value = stoi(token, 0, 16);
            }
            catch (exception ex)
            {
                byte.Mask = WILDCARD_BYTE_MASK;
            }
        }
        result.push_back(byte);
    }
    return result;
}

LPCVOID ScanForFirstResult(LPVOID baseAddress, vector<BYTE>* buffer, vector<MaskedByte>* signature, string functionName)
{
    if (buffer == NULL || signature == NULL)
    {
        Helpers::ShowErrorMessage("Failed to find address for function " + functionName + ". Null buffer or signature");
        return 0;
    }
    for (int i = 0; i < buffer->size(); i++)
    {
        for (int j = 0; j < signature->size(); j++)
        {
            MaskedByte signatureByte = (*signature)[j];
            BYTE currentByte = (*buffer)[i + j];
            // If the mask is empty, check if the current byte matches the signature. If not move on
            if (signatureByte.Mask == 0)
            {
                if (signatureByte.Value != currentByte) break;
            }
            // If the mask isn't empty but isn't a full wildcard byte, set all masked bits of the current byte 
            // to 0 then compare with the signature
            else if (signatureByte.Mask != WILDCARD_BYTE_MASK)
            {
                currentByte &= ~signatureByte.Mask;
                if (signatureByte.Value != currentByte) break;
            }
            // If the mask is a full wildcard byte then ignore this byte
            if (j + 1 == signature->size())
            {
                return (char*)baseAddress + i;
            }
        }
    }
    Helpers::ShowErrorMessage("Failed to find address for function " + functionName);
    return 0;
}

vector<LPCVOID> ScanForAllResults(LPVOID baseAddress, vector<BYTE>* buffer, vector<MaskedByte>* signature, string errorMsgName)
{
    vector<LPCVOID> hits;
    if (buffer == NULL || signature == NULL)
    {
        Helpers::ShowErrorMessage("Failed to find address for function " + errorMsgName + ". Null buffer or signature");
        return hits;
    }
    for (int i = 0; i < buffer->size(); i++)
    {
        for (int j = 0; j < signature->size(); j++)
        {
            MaskedByte signatureByte = (*signature)[j];
            BYTE currentByte = (*buffer)[i + j];
            if (signatureByte.Mask == 0)
            {
                if (signatureByte.Value != currentByte) break;
            }
            else if (signatureByte.Mask != WILDCARD_BYTE_MASK)
            {
                currentByte &= ~signatureByte.Mask;
                if (signatureByte.Value != currentByte) break;
            }
            if (j + 1 == signature->size())
            {
                hits.push_back((LPCVOID)((char*)baseAddress + i));
            }
        }
    }
    if (hits.empty()) Helpers::ShowErrorMessage("Failed to find any matches for function " + errorMsgName);
    return hits;
}

LPCVOID ScanForFirstResult(LPVOID baseAddress, vector<BYTE>* buffer, string signature, string errorMsgName)
{
    vector<MaskedByte> signatureBytes = ParseSignatureString(signature);
    return ScanForFirstResult(baseAddress, buffer, &signatureBytes, errorMsgName);
}

vector<LPCVOID> ScanForAllResults(LPVOID baseAddress, vector<BYTE>* buffer, string signature, string errorMsgName)
{
    vector<MaskedByte> signatureBytes = ParseSignatureString(signature);
    return ScanForAllResults(baseAddress, buffer, &signatureBytes, errorMsgName);
}

extern "C" __declspec(dllexport)
LPCVOID NH_ManagedScanForFirst(LPVOID baseAddress, SIZE_T buffer_size, const char* signature, const char* errorMsgName)
{
    string sig(signature);
    string errorMsg(errorMsgName);
    vector<BYTE> buffer = GetMemoryBuffer(baseAddress, buffer_size);
    LPCVOID result = ScanForFirstResult(baseAddress, &buffer, sig, errorMsg);
    buffer.clear();
    return result;
}
