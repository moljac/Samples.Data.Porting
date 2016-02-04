#ifndef ANDROID_JACK_SIMPLE_CLIENT_H
#define ANDROID_JACK_SIMPLE_CLIENT_H

#include <jack/jack.h>
#include "IJackClientInterface.h"
#include "APACommon.h"
#include "SynthBase/SynthBase.h"
#include <queue>


typedef struct _midi{
    int note;
    int velocity;
} midi;


namespace android {

class JackSimpleClient : public IJackClientInterface {
	private:
	jack_client_t *client;
	SynthBase  *mpSynth;
	jack_port_t* mpOutPortL;
	jack_port_t* mpOutPortR;
	std::queue<midi>  mNote;

	public:
		JackSimpleClient();
		virtual ~JackSimpleClient();
		int setUp(int argc, char *argv[]);
		int tearDown();
		int activate();
		int deactivate();
		int transport(TransportType type);
		int sendMidi(char* midi);

		void startSynth(char* soundFontPath, int bank, int program);
		void stopSynth();
		void playnote(int note, int velocity);

	private:
		static int process (jack_nframes_t nframes, void *arg);
};

};

#endif // ANDROID_JACK_SIMPLE_CLIENT_H

