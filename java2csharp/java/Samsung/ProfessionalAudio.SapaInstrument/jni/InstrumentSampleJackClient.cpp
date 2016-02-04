/**
 * @date        Jun 06, 2014
 * @copyright   Copyright (c) 2014 by Samsung Electronics Polska Sp. z o. o.
 */

#include "InstrumentSampleJackClient.h"

#include "Sinus.h"
#include "log.h"

#include <jack/jack.h>

#include <algorithm>
#include <cassert>
#include <libgen.h>


namespace audiosuite
{
namespace sapainstrumentsample
{

int InstrumentSampleJackClient::_process(jack_nframes_t nframes, void* arg)
    noexcept
{
    auto* this_ = reinterpret_cast<InstrumentSampleJackClient*>(arg);
    return this_->process(nframes) ? 0 : 1;
}

InstrumentSampleJackClient::InstrumentSampleJackClient() :
    jackClient{
        nullptr,
        [](jack_client_t* jc) {
            if (jc)
                jack_client_close(jc);
        }
    }
{
}

InstrumentSampleJackClient::~InstrumentSampleJackClient()
{
}

int InstrumentSampleJackClient::setUp(int argc, char *argv[])
{
    SLOGT("Entering");

    const char* serverName = nullptr;
    jack_options_t options = JackNullOption;
    jack_status_t status = JackFailure;
/*
    if (argc >= 2)
    {
        clientName = argv[1];

        if (argc >= 3)
        {
            serverName = argv[2];
            options = static_cast<jack_options_t>(options | JackServerName);
        }
    }
    else
    {
        clientName = basename(argv[0]);
    }

    LOGD("Opening Jack client: %s", clientName.c_str());
*/
	LOGD("setUp argc %d", argc);
	for(int i = 0;i< argc; i++){
		LOGD("setup argv %s", argv[i]);
	}
    jackClient.reset(
            jack_client_open(argv[0], options, &status, serverName)
            );

    if (!jackClient)
    {
        LOGF("Opening Jack client failed with status: %d", status);
        return JACK_RETURN_ERROR;
    }

    samplerate = static_cast<unsigned int>(
            jack_get_sample_rate(jackClient.get())
            );
    LOGD("Jack sample rate: %u", samplerate);
    sinus.reset(new Sinus(200.0f, samplerate));

    SLOGD("Registering pair of output ports (stereo)");
    try
    {
        outputPort.reset(new StereoPort(jackClient.get(), "output", true));
    }
    catch (const std::runtime_error& e)
    {
        LOGF("%s", e.what());
        return JACK_RETURN_ERROR;
    }

    if (jack_set_process_callback(
                jackClient.get(),
                InstrumentSampleJackClient::_process,
                this
                )
            != JACK_RETURN_SUCCESS)
    {
        SLOGF("Failed to register Jack process callback");
        return JACK_RETURN_ERROR;
    }

    SLOGT("Jack process callback has been registered");

    return JACK_RETURN_SUCCESS;
}

int InstrumentSampleJackClient::tearDown()
{
    SLOGT("jackClient tearDown");

    if (activated)
        deactivate();

    jackClient.reset();

    return JACK_RETURN_SUCCESS;
}

int InstrumentSampleJackClient::activate()
{
    if (jack_activate(jackClient.get()) != JACK_RETURN_SUCCESS)
    {
        SLOGF("Failed to activate Jack client");
        return JACK_RETURN_ERROR;
    }

    activated = true;
    SLOGT("Jack client: activated");

    return JACK_RETURN_SUCCESS;
}

int InstrumentSampleJackClient::deactivate()
{
    if (jack_deactivate(jackClient.get()) != JACK_RETURN_SUCCESS)
    {
        SLOGF("Failed to deactivate Jack client");
        return JACK_RETURN_ERROR;
    }

    activated = false;
    SLOGT("Jack client: deactivated");

    return JACK_RETURN_SUCCESS;
}

int InstrumentSampleJackClient::transport(android::TransportType)
{
    return JACK_RETURN_SUCCESS;
}

int InstrumentSampleJackClient::sendMidi(char*)
{
    return JACK_RETURN_SUCCESS;
}

bool InstrumentSampleJackClient::processPlayOn(jack_nframes_t framesCount) noexcept
{
    /* retrieving buffers of the output ports */
    auto* outL = outputPort->getLeftPortBuffer(framesCount);
    auto* outR = outputPort->getRightPortBuffer(framesCount);

    /* generate a sinewave and fill the buffer of the left output port */
    std::generate(outL, outL + framesCount, *sinus);
    /* duplicate sinewave to the right output port */
    std::copy(outL, outL + framesCount, outR);

    return true;
}

bool InstrumentSampleJackClient::processPlayOff(jack_nframes_t framesCount) noexcept
{
    /* retrieving buffers of the output ports */
    auto* outL = outputPort->getLeftPortBuffer(framesCount);
    auto* outR = outputPort->getRightPortBuffer(framesCount);

    /* fill the buffers with a silence */
    std::fill(outL, outL + framesCount, 0);
    std::fill(outR, outR + framesCount, 0);

    return true;
}

bool InstrumentSampleJackClient::process(jack_nframes_t framesCount) noexcept
{
    /* check applciation state */
    if (playOn.load())
        return processPlayOn(framesCount);
    else
        return processPlayOff(framesCount);
}

void InstrumentSampleJackClient::play()
{
    SLOGD("play");
    playOn.store(true);
}

void InstrumentSampleJackClient::stop()
{
    SLOGD("stop");
    playOn.store(false);
}

} /* namespace sapainstrumentsample */
} /* namespace audiosuite */
