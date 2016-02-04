#include <string.h>
#include "wave.h"
#include <stdio.h>
#include "mylog.h"

namespace android {

IMPLEMENT_APA_INTERFACE(APAWave)

APAWave::APAWave(){
}

APAWave::~APAWave(){
}

int APAWave::init(){
	LOGD("wave.so initialized");
	return APA_RETURN_SUCCESS;
}

int APAWave::sendCommand(const char* command){
    LOGD("APAWave send command [%s]\n", command);
	return APA_RETURN_SUCCESS;
}

IJackClientInterface* APAWave::getJackClientInterface(){
    return &mSimpleClient;
}

int APAWave::request(const char* what, const long ext1, const long capacity, size_t &len, void*data)
{
	return APA_RETURN_SUCCESS;
}

};
