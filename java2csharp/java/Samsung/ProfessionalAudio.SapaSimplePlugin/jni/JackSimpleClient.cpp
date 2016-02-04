#include "JackSimpleClient.h"
#include "mylog.h"

namespace android {

jack_client_t *JackSimpleClient::jackClient = NULL;
int JackSimpleClient::bufferSize = 1024;

JackSimpleClient::JackSimpleClient(){
	outPort = NULL;
	bufferSize = 1024;
}

JackSimpleClient::~JackSimpleClient(){
	jackClient = NULL;
}

int JackSimpleClient::processSine (jack_nframes_t frames, void *arg)
{
	JackSimpleClient *thiz = (JackSimpleClient*)arg;
	jack_default_audio_sample_t *out1 = (jack_default_audio_sample_t*)jack_port_get_buffer (thiz->outPort, frames);

	for(unsigned int i=0; i<frames; i++ )
	{
		out1[i] = thiz->sineTable[i%bufferSize];
	}

	return 0;
}

int JackSimpleClient::setUp (int argc, char *argv[])
{
	LOGD("setUp argc %d", argc);
	for(int i = 0;i< argc; i++){
		LOGD("setup argv %s", argv[i]);
	}
  

	// make a alias
	char* name = strrchr(argv[0], '.');
	if(name == NULL){
		name = argv[0];
	}
	jackClient = jack_client_open (name, JackNullOption, NULL, NULL);
	if (jackClient == NULL) {
		return APA_RETURN_ERROR;
	}

	bufferSize = jack_get_buffer_size(jackClient);

	// init the sine table
	for(int i=0; i<bufferSize; i++ )
	{
		sineTable[i] = (float) sin( ((double)i*8/(double)bufferSize) * 2. * 3.14159265 );
	}

	jack_set_process_callback (jackClient, processSine, this);

	outPort = jack_port_register (jackClient, "out",
					  JACK_DEFAULT_AUDIO_TYPE,
					  JackPortIsOutput, 0);
	if(outPort == NULL){
		return APA_RETURN_ERROR;
	}

	return APA_RETURN_SUCCESS;
}

int JackSimpleClient::tearDown(){
	jack_client_close (jackClient);
	return APA_RETURN_SUCCESS;
}

int JackSimpleClient::activate(){
	jack_activate (jackClient);
	LOGD("JackSimpleClient::activate");

	const char **systemInputs = jack_get_ports (jackClient, NULL, NULL,
				JackPortIsPhysical|JackPortIsInput);
	if (systemInputs == NULL) {
		LOGD("system input port is null\n");
		return APA_RETURN_ERROR;
	}

	jack_connect (jackClient, jack_port_name (outPort), systemInputs[0]);

	free (systemInputs);

	return APA_RETURN_SUCCESS;
}

int JackSimpleClient::deactivate(){
	LOGD("JackSimpleClient::deactivate");
	jack_deactivate (jackClient);
	return APA_RETURN_SUCCESS;
}

int JackSimpleClient::transport(TransportType type) {
	return APA_RETURN_SUCCESS;
}

int JackSimpleClient::sendMidi(char* midi){
	return APA_RETURN_SUCCESS;
}

};
