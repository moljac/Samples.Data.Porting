#include "wave.h"
#include "mylog.h"
#include "stdlib.h"

namespace android {

IMPLEMENT_APA_INTERFACE(APAWave)

APAWave::APAWave(){
}

APAWave::~APAWave(){
}

int APAWave::init(){
    LOGD("APAWave init is called\n");
	return APA_RETURN_SUCCESS;
}

int APAWave::sendCommand(const char* command){

	if(strncmp(command,"PLAY ", 5) == 0){
		char* midi = (char*)command+5;
		char notePart[100];
		char velocityPart[100];
		char string[100];
		strncpy(string, midi, 99);
		char seps[] = " ";
		char *token = strtok(string, seps);

		strncpy(notePart, token, 99);
		token = strtok(NULL, seps);
		strncpy(velocityPart, token, 99);

		int note = atoi(notePart);
		int velocity = atoi(velocityPart);

		mSimpleClient.playnote(note, velocity);
	} else if(strncmp(command,"START", 5) == 0){
		mSimpleClient.startSynth((char*) "/sdcard/Download/collection.sf2", 0, 1);
	}

	return APA_RETURN_SUCCESS;
}

IJackClientInterface* APAWave::getJackClientInterface() {
    return &mSimpleClient;
}

int APAWave::request(const char* what, const long ext1, const long capacity, size_t &len, void*data) {
	return APA_RETURN_SUCCESS;
}

};
