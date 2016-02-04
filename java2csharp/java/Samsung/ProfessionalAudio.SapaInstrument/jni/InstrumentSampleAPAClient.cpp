/**
 * @date        Jun 06, 2014
 * @copyright   Copyright (c) 2014 by Samsung Electronics Polska Sp. z o. o.
 */

#include "InstrumentSampleAPAClient.h"

#include "InstrumentSampleJackClient.h"
#include "log.h"

#include <APACommon.h>

#include <string>


extern "C"
{
IMPLEMENT_APA_INTERFACE(
        ::audiosuite::sapainstrumentsample::InstrumentSampleAPAClient
        )
}


namespace audiosuite
{
namespace sapainstrumentsample
{

InstrumentSampleAPAClient::InstrumentSampleAPAClient() :
    jackClient{new InstrumentSampleJackClient()}
{
}

InstrumentSampleAPAClient::~InstrumentSampleAPAClient()
{
}

int InstrumentSampleAPAClient::init()
{
    return APA_RETURN_SUCCESS;
}

android::IJackClientInterface*
InstrumentSampleAPAClient::getJackClientInterface()
{
    return jackClient.get();
}

int InstrumentSampleAPAClient::request(const char*, const long, const long,
        size_t&, void* const)
{
    return APA_RETURN_SUCCESS;
}

int InstrumentSampleAPAClient::sendCommand(const char* msg)
{
    LOGD("sendCommand: msg=%s", msg);

    std::string cmd(msg);

    /* check if play command */
    if (cmd == "/play")
        jackClient->play();

    /* check if stop command */
    else if (cmd == "/stop")
        jackClient->stop();

    return APA_RETURN_SUCCESS;
}

} /* namespace sapainstrumentsample */
} /* namespace audiosuite */
