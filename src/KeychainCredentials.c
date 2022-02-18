#import <Security/Security.h>

static bool CopyString(CFStringRef string, UniChar *buffer, long *bufferLength)
{
    if (buffer == NULL || bufferLength == NULL)
    {
        return false;
    }
    UInt8 lossByte = 0;
    Boolean byteOrderMark = false;
    long bufferActualLength = *bufferLength;
    CFIndex length = CFStringGetLength(string); // The number (in terms of UTF-16 code pairs) of characters stored in the string.
    CFIndex numberOfCharactersConverted = CFStringGetBytes(string, CFRangeMake(0, length), kCFStringEncodingUTF16, lossByte, byteOrderMark, (UInt8 *)buffer, bufferActualLength * sizeof(UniChar), NULL);
    *bufferLength = length;
    bool success = numberOfCharactersConverted == length;
    if (!success)
    {
        memset(buffer, 0, bufferActualLength);
    }
    return success;
}

static void CFDictionarySetUTF8Value(CFMutableDictionaryRef dictionaryRef, const void *key, const char *value)
{
    CFStringRef valueRef = CFStringCreateWithCString(kCFAllocatorDefault, value, kCFStringEncodingUTF8);
    CFDictionarySetValue(dictionaryRef, key, valueRef);
    CFRelease(valueRef);
}

static CFDictionaryRef CreateQuery(bool getPassword, const char *server, const char *userName, int32_t limit)
{
    CFMutableDictionaryRef query = CFDictionaryCreateMutable(kCFAllocatorDefault, 5, &kCFTypeDictionaryKeyCallBacks, &kCFTypeDictionaryValueCallBacks);
    CFDictionarySetValue(query, kSecReturnData, getPassword ? kCFBooleanTrue : kCFBooleanFalse);
    CFDictionarySetValue(query, kSecReturnAttributes , kCFBooleanTrue);
    CFDictionarySetValue(query, kSecClass, kSecClassInternetPassword);
    CFDictionarySetUTF8Value(query, kSecAttrServer, server);
    if (userName != NULL)
    {
        CFDictionarySetUTF8Value(query, kSecAttrAccount, userName);
    }
    if (limit > 0)
    {
        CFNumberRef limitRef = CFNumberCreate(kCFAllocatorDefault, kCFNumberSInt32Type, &limit);
        CFDictionarySetValue(query, kSecMatchLimit, limitRef);
        CFRelease(limitRef);
    }
    return query;
}

static bool FillPassword(CFDictionaryRef result, UniChar *password, long *passwordLength)
{
    bool bufferTooSmall = false;
    CFTypeRef passwordRef = CFDictionaryGetValue(result, kSecValueData);
    if (passwordRef != NULL && CFGetTypeID(passwordRef) == CFDataGetTypeID())
    {
        CFStringRef passwordStringRef = CFStringCreateWithCString(kCFAllocatorDefault, (const char *)CFDataGetBytePtr(passwordRef), kCFStringEncodingUTF8);
        if (!CopyString(passwordStringRef, password, passwordLength))
        {
            bufferTooSmall = true;
        }
        CFRelease(passwordStringRef);
    }
    return bufferTooSmall;
}

static bool CopyAccountAtIndex(CFDictionaryRef item, UniChar **accounts, long *accountsLength, CFIndex i)
{
    CFTypeRef accountRef = CFDictionaryGetValue(item, kSecAttrAccount);
    if (accountRef != NULL && CFGetTypeID(accountRef) == CFStringGetTypeID())
    {
        long bufferLength = accountsLength[i];
        UniChar *account = (UniChar *)((intptr_t)accounts + bufferLength * i * sizeof(UniChar));
        if (CopyString(accountRef, account, &accountsLength[i]))
        {
            return true;
        }
    }
    return false;
}

OSStatus __cdecl GetAccounts(const char *server, const char *authType, UniChar **accounts, long *accountsLength, int32_t *numberOfAccounts)
{
    if (server == NULL || accounts == NULL || numberOfAccounts == NULL || accountsLength == NULL || *numberOfAccounts <= 0)
    {
        return errSecParam; // Same as paramErr (-50) from <CoreServices/CoreServices.h> (through CarbonCore/MacErrors.h)
    }

    CFDictionaryRef query = CreateQuery(false, server, NULL, *numberOfAccounts);
    *numberOfAccounts = 0;

    CFTypeRef result;
#if DEBUG
    bool logEnabled = strcmp("1", getenv("KEYCHAINCREDENTIALS_ENABLE_DEBUG_LOGS") ?: "0") == 0;
    if (logEnabled)
    {
        CFShow(CFSTR("=== SecItemCopyMatching query ==="));
        CFShow(query);
    }
#endif
    OSStatus status = SecItemCopyMatching(query, &result);
#if DEBUG
    if (logEnabled)
    {
        CFShow(CFSTR("=== SecItemCopyMatching result ==="));
        if (status == errSecSuccess)
        {
            CFShow(result);
        }
        else
        {
            CFStringRef error = SecCopyErrorMessageString(status, NULL);
            if (error != NULL)
            {
                CFShow(error);
                CFRelease(error);
            }
            else
            {
                CFShow(CFSTR("Unknown status code"));
            }
        }
    }
#endif
    CFRelease(query);
    bool bufferTooSmall = false;
    if (status == errSecSuccess && result != NULL)
    {
        CFTypeID resultTypeId = CFGetTypeID(result);
        if (resultTypeId == CFArrayGetTypeID())
        {
            CFIndex count = CFArrayGetCount(result);
            *numberOfAccounts = (int32_t)count;
            for (CFIndex i = 0; i < count; i++)
            {
                CFTypeRef item = CFArrayGetValueAtIndex(result, i);
                if (CFGetTypeID(item) == CFDictionaryGetTypeID())
                {
                    if (!CopyAccountAtIndex(item, accounts, accountsLength, i))
                    {
                        bufferTooSmall = true;
                    }
                }
            }
        }
        else if (resultTypeId == CFDictionaryGetTypeID())
        {
            *numberOfAccounts = 1;
            if (!CopyAccountAtIndex(result, accounts, accountsLength, 0))
            {
                bufferTooSmall = true;
            }
        }
        else
        {
            // return errSecUnimplemented here ?
        }
        CFRelease(result);
    }
    return bufferTooSmall ? -errSecBufferTooSmall : status;
}

/// Get the password for the specified server and user name from the Keychain.
/// @param server The server to search for in the Keychain.
/// @param authType The authentication type. Currently unused.
/// @param userName The user name to search for in the Keychain.
/// @param password A pointer to a UniChar buffer that contains the password for the given `server` and `userName` upon successful execution (i.e. the return value is `errSecSuccess`).
///                 Pass `NULL` if you don't want to retrieve the password but just check for the existence of an item in the Keychain matching the `server` and `userName`,
///                 without triggering a password prompt from the operating system.
/// @param passwordLength A pointer to the length of the `password`. Before execution, the pointer must contain the length of the `password` buffer.
///                       Upon successful execution, contains the actual length of the password found in the keychain.
///                       Note: The length is the number of UniChar, not the number of bytes.
/// @return An OSStatus that describes the result of the underlying `SecItemCopyMatching` function. Returns `errSecParam` if either `server` or `userName` is `NULL`.
/// @discussion If the `password` buffer is not large enough, the call should be retried with a larger buffer using the size provided
///             as output in the `passwordLength` pointer.
OSStatus __cdecl GetPassword(const char *server, const char *authType, const char *userName, UniChar *password, long *passwordLength)
{
    if (server == NULL || userName == NULL)
    {
        return errSecParam; // Same as paramErr (-50) from <CoreServices/CoreServices.h> (through CarbonCore/MacErrors.h)
    }
    bool getPassword = password != NULL && passwordLength != NULL;
    CFDictionaryRef query = CreateQuery(getPassword, server, userName, 0);

    CFTypeRef result;
    OSStatus status = SecItemCopyMatching(query, &result);
    CFRelease(query);
    bool bufferTooSmall = false;
    if (status == errSecSuccess && result != NULL && CFGetTypeID(result) == CFDictionaryGetTypeID())
    {
        bufferTooSmall = FillPassword(result, password, passwordLength);
        CFRelease(result);
    }
    return bufferTooSmall ? -errSecBufferTooSmall : status;
}

OSStatus __cdecl GetErrorMessage(OSStatus statusCode, UniChar *message, long *messageLength)
{
    if (statusCode == errSecSuccess || message == NULL || messageLength == NULL)
    {
        return errSecParam; // Same as paramErr (-50) from <CoreServices/CoreServices.h> (through CarbonCore/MacErrors.h)
    }
    
    bool bufferTooSmall = false;
    CFStringRef errorMessage = SecCopyErrorMessageString(statusCode, NULL);
    if (errorMessage != NULL)
    {
        if (!CopyString(errorMessage, message, messageLength))
        {
            bufferTooSmall = true;
        }
        CFRelease(errorMessage);
    }

    return bufferTooSmall ? -errSecBufferTooSmall : errSecSuccess;
}
