/**
 * @date        Jun 06, 2014
 * @copyright   Copyright (c) 2014 by Samsung Electronics Polska Sp. z o. o.
 */

#include "EffectSampleAPAClient.h"

#include "EffectSampleJackClient.h"

#include "log.h"

#include <cstring>
#include <cstdlib>
#include <APACommon.h>


extern "C"
{
IMPLEMENT_APA_INTERFACE(
        ::audiosuite::sapaeffectsample::EffectSampleAPAClient
        )
}
/* volume command prefix */
static const char* VOLUME_COMMAND = "/volume:";

namespace audiosuite
{
namespace sapaeffectsample
{

EffectSampleAPAClient::EffectSampleAPAClient() :
    jackClient{new EffectSampleJackClient()}
{
}

EffectSampleAPAClient::~EffectSampleAPAClient()
{
}

int EffectSampleAPAClient::init()
{
    return APA_RETURN_SUCCESS;
}

android::IJackClientInterface*
EffectSampleAPAClient::getJackClientInterface()
{
    return jackClient.get();
}

int EffectSampleAPAClient::request(const char*, const long, const long,
        size_t&, void* const)
{
    return APA_RETURN_SUCCESS;
}

int EffectSampleAPAClient::sendCommand(const char* msg)
{
    std::string cmd(msg);
    /* checking if volume command */
    if (cmd.find(VOLUME_COMMAND) != std::string::npos)
    {
        /* retrievieng volume value in dB */
        /* command format is \volume:value */
        float volume = atof(cmd.substr(cmd.find(':') + 1, cmd.size()).c_str());
        /* passing value to the client */
        jackClient->setVolume(volume);
    }
    return APA_RETURN_SUCCESS;
}

} /* namespace sapaeffectsample */
} /* namespace audiosuite */
