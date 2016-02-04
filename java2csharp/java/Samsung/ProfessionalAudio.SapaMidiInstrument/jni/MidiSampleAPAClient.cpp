/**
 * @date        Jun 06, 2014
 * @copyright   Copyright (c) 2014 by Samsung Electronics Polska Sp. z o. o.
 */

#include "MidiSampleAPAClient.h"

#include "MidiSampleJackClient.h"
#include "log.h"

#include <APACommon.h>

#include <string>


extern "C"
{
IMPLEMENT_APA_INTERFACE(
        ::audiosuite::sapamidisample::MidiSampleAPAClient
        )
}


namespace audiosuite
{
namespace sapamidisample
{

MidiSampleAPAClient::MidiSampleAPAClient() :
    jackClient{new MidiSampleJackClient()}
{
}

MidiSampleAPAClient::~MidiSampleAPAClient()
{
}

int MidiSampleAPAClient::init()
{
    return APA_RETURN_SUCCESS;
}

android::IJackClientInterface*
MidiSampleAPAClient::getJackClientInterface()
{
    return jackClient.get();
}

int MidiSampleAPAClient::request(const char*, const long, const long,
        size_t&, void* const)
{
    return APA_RETURN_SUCCESS;
}

/** When using sendStream in java, sendCommand is called
 *  with following arg: stream:{id}:{bytes}.
 *  Other messages uses cmd:{message}
 */
int MidiSampleAPAClient::sendCommand(const char* message)
{
    LOGD("sendCommand: message=%s", message);
    std::string in(message);
    std::string prefix(in.substr(0, in.find_first_of(':')));

    if (prefix.compare("stream") == 0)
    {
        jackClient->onStream(message);
    }
    else if (prefix.compare("cmd") == 0)
    {
        jackClient->onCommand(message);
    }

    return APA_RETURN_SUCCESS;
}

} /* namespace sapamidisample */
} /* namespace audiosuite */
