CFLAGS += -shared -framework CoreFoundation -framework Security

Debug: CFLAGS += -DDEBUG
Debug: ../runtimes/osx/native/libKeychainCredentials.dylib

Release: CFLAGS += -arch x86_64 -arch arm64
Release: ../runtimes/osx/native/libKeychainCredentials.dylib

../runtimes/osx/native:
	mkdir -p $@

../runtimes/osx/native/libKeychainCredentials.dylib: KeychainCredentials.c ../runtimes/osx/native
	$(CC) $< $(CFLAGS) -o $@
