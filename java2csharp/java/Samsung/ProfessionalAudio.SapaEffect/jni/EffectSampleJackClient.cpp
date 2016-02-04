/**
 * @date        Jun 06, 2014
 * @copyright   Copyright (c) 2014 by Samsung Electronics Polska Sp. z o. o.
 */

#include "EffectSampleJackClient.h"
#include "Volume.h"
#include "log.h"

#include <jack/jack.h>

#include <algorithm>
#include <cassert>
#include <libgen.h>


namespace audiosuite
{
namespace sapaeffectsample
{

int EffectSampleJackClient::_process(jack_nframes_t nframes, void* arg)
    noexcept
{
    auto* this_ = reinterpret_cast<EffectSampleJackClient*>(arg);
    return this_->process(nframes) ? 0 : 1;
}

EffectSampleJackClient::EffectSampleJackClient() :
    volume(new Volume),
    jackClient{ nullptr,
        [](jack_client_t* jc) {
            if (jc)
                jack_client_close(jc);
        }
    }
{
}

EffectSampleJackClient::~EffectSampleJackClient()
{
}

int EffectSampleJackClient::setUp(int argc, char *argv[])
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

    SLOGD("Registering output port");
    outputPort[0] = jack_port_register(jackClient.get(), "output1",
                    JACK_DEFAULT_AUDIO_TYPE, JackPortIsOutput, 0);

    inputPort[0] = jack_port_register(jackClient.get(), "input1",
                    JACK_DEFAULT_AUDIO_TYPE, JackPortIsInput, 0);

    outputPort[1] = jack_port_register(jackClient.get(), "output2",
                    JACK_DEFAULT_AUDIO_TYPE, JackPortIsOutput, 0);

    inputPort[1] = jack_port_register(jackClient.get(), "input2",
                    JACK_DEFAULT_AUDIO_TYPE, JackPortIsInput, 0);

    if (!outputPort || !inputPort)
    {
        SLOGF("Failed to register ports");
        return JACK_RETURN_ERROR;
    }

    if (jack_set_process_callback(
                jackClient.get(),
                EffectSampleJackClient::_process,
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

int EffectSampleJackClient::tearDown()
{
    SLOGT("jackClient tearDown");

    if (activated)
        deactivate();

    jackClient.reset();

    return JACK_RETURN_SUCCESS;
}

int EffectSampleJackClient::activate()
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

int EffectSampleJackClient::deactivate()
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

int EffectSampleJackClient::transport(android::TransportType)
{
    return JACK_RETURN_SUCCESS;
}

int EffectSampleJackClient::sendMidi(char*)
{
    return JACK_RETURN_SUCCESS;
}

bool EffectSampleJackClient::process(jack_nframes_t framesCount) noexcept
{
    /* retrieving input port buffer */
    auto* in1 = reinterpret_cast<sample_t *>(
            jack_port_get_buffer(inputPort[0], framesCount)
            );
    auto* in2 = reinterpret_cast<sample_t *>(
            jack_port_get_buffer(inputPort[1], framesCount)
            );
    /* retrieving output port buffer */
    auto* out1 = reinterpret_cast<sample_t *>(
            jack_port_get_buffer(outputPort[0], framesCount)
            );
    auto* out2 = reinterpret_cast<sample_t *>(
            jack_port_get_buffer(outputPort[1], framesCount)
            );

    /* applying volume on the input buffer and storing value in the output buffer */
    std::transform(in1, in1 + framesCount, out1, *volume);
    std::transform(in2, in2 + framesCount, out2, *volume);

    return true;
}

void EffectSampleJackClient::setVolume(float dB)
{
    LOGD("setVolume: dB=%f", dB);
    volume->dB = dB;
}

} /* namespace sapaeffectsample */
} /* namespace audiosuite */
