
#include <stdio.h>
#include <IAPAInterface.h>
#include "JackSimpleClient.h"
#include "APACommon.h"

namespace android {

class APAWave : IAPAInterface {

	public:
		APAWave();
		virtual ~APAWave();
		int init();
		int sendCommand(const char* command);
		IJackClientInterface* getJackClientInterface();
		int request(const char* what, const long ext1, const long capacity, size_t &len, void* data);
	private:
		JackSimpleClient mSimpleClient;
};

DECLARE_APA_INTERFACE(APAWave)

};

