CFLAGS += -shared -framework CoreFoundation -framework Security -mmacosx-version-min=10.13

Debug: CFLAGS += -DDEBUG
Debug: ../runtimes/osx/native/libKeychainCredentials.Native.dylib

Release: CFLAGS += -arch x86_64 -arch arm64
Release: ../runtimes/osx/native/libKeychainCredentials.Native.dylib

../runtimes/osx/native:
	mkdir -p $@

../runtimes/osx/native/libKeychainCredentials.Native.dylib: KeychainCredentials.c ../runtimes/osx/native
	$(CC) $< $(CFLAGS) -o $@
