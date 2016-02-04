
#ifndef ANDROID_JACK_SIMPLE_CLIENT_H
#define ANDROID_JACK_SIMPLE_CLIENT_H

#include <jack/jack.h>
#include <stdio.h>
#include <errno.h>
#include <stdlib.h>
#include <string.h>
#include <math.h>
#include <signal.h>
#include <unistd.h>

#include "IJackClientInterface.h"
#include "APACommon.h"

namespace android {

class JackSimpleClient: public IJackClientInterface {
	#define SIZE_SINE_TABLE (2048)
	public:
		JackSimpleClient();
		virtual ~JackSimpleClient();
		int setUp(int argc, char *argv[]);
		int tearDown();
		int activate();
		int deactivate();
		int transport(TransportType type);
		int sendMidi(char* midi);
	private:
		static int processSine (jack_nframes_t nframes, void *arg);

	private:
		jack_port_t * outPort;
		static jack_client_t *jackClient; // jack client object

		// members for sine tuning
		float sineTable[SIZE_SINE_TABLE];
		static int bufferSize;
};

};

#endif // ANDROID_JACK_SIMPLE_CLIENT_H

