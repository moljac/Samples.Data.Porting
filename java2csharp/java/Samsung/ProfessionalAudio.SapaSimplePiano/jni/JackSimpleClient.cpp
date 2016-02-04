/** @file simple_client.c
 *
 * @brief This simple client demonstrates the basic features of JACK
 * as they would be used by many applications.
 */


#include "mylog.h"
#include "JackSimpleClient.h"

static const char *leftPortName = "Left";
static const char *rightPortName = "Right";

namespace android {

JackSimpleClient::JackSimpleClient(){
	mpSynth = NULL;
}

JackSimpleClient::~JackSimpleClient(){
	stopSynth();
}

void JackSimpleClient::startSynth(char *soundFontPath, int bank, int program) {
    stopSynth();
	mpSynth = new SynthBase(48000, soundFontPath, bank, program);
}

void JackSimpleClient::stopSynth() {

	if( NULL != mpSynth){
        delete mpSynth;
        mpSynth = NULL;
	}
}

void JackSimpleClient::playnote(int note, int velocity) {

	midi data = {note, velocity};
	mNote.push(data);
}

int JackSimpleClient::setUp (int argc, char *argv[]) {

	/* open a client connection to the JACK server */
	client = jack_client_open (argv[0], JackNullOption, NULL, NULL);
	if (client == NULL) {
		LOGE( "jack_client_open() failed\n");
		return APA_RETURN_ERROR;
	}

	jack_set_process_callback(client, process, this);

	mpOutPortL = jack_port_register (client, leftPortName, JACK_DEFAULT_AUDIO_TYPE, JackPortIsOutput, 0);
	if(mpOutPortL == NULL){
		LOGE("mOutPortL register is failed");
		return APA_RETURN_ERROR;
	}

	mpOutPortR = jack_port_register (client, rightPortName, JACK_DEFAULT_AUDIO_TYPE, JackPortIsOutput, 0);
	if(mpOutPortR == NULL){
	    LOGE("mOutPortR register is failed");
	    return APA_RETURN_ERROR;
	}

	return APA_RETURN_SUCCESS;
}

int JackSimpleClient::tearDown(){
    jack_client_close (client);
}

int JackSimpleClient::activate(){
	if (jack_activate(client)) {
		LOGE("fail to activate jack client");
	}

	if (jack_connect(client, jack_port_name(mpOutPortL), "out:playback_1")) {
		LOGE("fail to connect left port");
	    return APA_RETURN_ERROR;
	}

	if (jack_connect(client, jack_port_name(mpOutPortR), "out:playback_2")) {
		LOGE("fail to connect right port");
	    return APA_RETURN_ERROR;
	}

	return APA_RETURN_SUCCESS;
}

int JackSimpleClient::deactivate(){

	if (jack_deactivate(client)){
		LOGE("fail to deactivate jack client");
	}

	return APA_RETURN_SUCCESS;
}

int JackSimpleClient::transport(TransportType type) {
	return APA_RETURN_SUCCESS;
}

int JackSimpleClient::sendMidi(char* midi){
	return APA_RETURN_SUCCESS;
}

int JackSimpleClient::process (jack_nframes_t frames, void *arg){

	if( NULL == arg){
		return 0;
	}

	JackSimpleClient *thiz = (JackSimpleClient*)arg;
	SynthBase* synth = thiz->mpSynth;

	jack_default_audio_sample_t *outL = (jack_default_audio_sample_t*)jack_port_get_buffer (thiz->mpOutPortL, frames);
	jack_default_audio_sample_t *outR = (jack_default_audio_sample_t*)jack_port_get_buffer (thiz->mpOutPortR, frames);

	if( NULL == synth || NULL == outL || NULL == outR){
		return 0;
	}

	while (!thiz->mNote.empty()) {
		midi data = thiz->mNote.front();
		thiz->mNote.pop();
		synth->pushEvents(0, data.note, data.velocity);
	}

	float channelBuffers32[2][frames];
	synth->processing((float *) channelBuffers32, frames);

	memcpy(outL, &channelBuffers32[0][0], sizeof(float) * frames);
	memcpy(outR, &channelBuffers32[1][0], sizeof(float) * frames);

	return 0;
}


};
