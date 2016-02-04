/**
 * @date        Jun 06, 2014
 * @copyright   Copyright (c) 2014 by Samsung Electronics Polska Sp. z o. o.
 */

#include "MidiSampleJackClient.h"
#include "Processors.h"
#include "StereoPort.h"
#include "MidiPort.h"
#include "log.h"
#include "Sinus.h"

#include <jack/jack.h>

#include <algorithm>
#include <cassert>
#include <libgen.h>
#include <sstream>


namespace audiosuite
{
namespace sapamidisample
{

MidiSampleJackClient::MidiSampleJackClient() :
    jackClient{
        nullptr,
        [](jack_client_t* jc) {
            if (jc)
                jack_client_close(jc);
        }
    }
{
}

MidiSampleJackClient::~MidiSampleJackClient()
{
}

int MidiSampleJackClient::setUp(int argc, char *argv[])
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

    SLOGD("Registering ports");

    try
    {
        /* creating ports */
        outputPort.reset(new StereoPort(jackClient.get(), "output", true));
        midiInPort.reset(new MidiPort(jackClient.get(), "midi_in", false));
        midiOutPort.reset(new MidiPort(jackClient.get(), "midi_out", true));

        /* sines generator */
        auto sines = std::shared_ptr<AdditiveGenerator>(new AdditiveGenerator(samplerate));

        for (int i = 40; i <= 80; i++)
            sines->addNote(i);

        /* user midi events processor */
        userEventProcessor.reset(new UserMidiProcessor(midiOutPort,
                    sines));
        /* port midi events processor */
        portEventProcessor.reset(new InPortMidiProcessor(midiInPort, midiOutPort, sines));

        /* audio */
        audioProcessor.reset(new AudioProcessor(outputPort, sines));
    }
    catch (const std::runtime_error& e)
    {
        LOGF("%s", e.what());
        return JACK_RETURN_ERROR;
    }
    SLOGD("Setting up processors");

    auto process = [](jack_nframes_t nframes, void *arg) noexcept
    {
        auto* thiz = reinterpret_cast<MidiSampleJackClient*>(arg);
        return thiz->process(nframes) ? 0 : 1;
    };

    if (jack_set_process_callback(
                jackClient.get(),
                process,
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

int MidiSampleJackClient::tearDown()
{
    SLOGT("jackClient tearDown");

    if (activated)
        deactivate();

    jackClient.reset();

    return JACK_RETURN_SUCCESS;
}

int MidiSampleJackClient::activate()
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

int MidiSampleJackClient::deactivate()
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

int MidiSampleJackClient::transport(android::TransportType)
{
    return JACK_RETURN_SUCCESS;
}

int MidiSampleJackClient::sendMidi(char* midi)
{
    LOGD("sendMidi: %#04x", midi[0]);
    return JACK_RETURN_SUCCESS;
}

bool MidiSampleJackClient::process(jack_nframes_t nframes) noexcept
{
    /* output port buffer */
    auto& out = midiOutPort->getBuffer(nframes);
    /* clear last frame data */
    out.clear();
    /* set port offset to 0 */
    out.setTimeOffset(0);
    /* process event from user */
    int timeOffset = userEventProcessor->process(nframes);
    if (timeOffset == -1)
    {
        SLOGD("User event processing error");
        return false;
    }
    /* set offset to last timestamp in the output port   */
    /* error occurs when events are not written in order */
    out.setTimeOffset(timeOffset);
    /* process events from the input */
    timeOffset = portEventProcessor->process(nframes);
    if (timeOffset == -1)
    {
        SLOGD("Port event processing error");
        return false;
    }

    /* generating audio */
    return audioProcessor->process(nframes);
}

void MidiSampleJackClient::onStream(const char* msg)
{
    /* stream:{id}:{hex bytes represented as string} */
    std::string stream(msg);

    auto converter = [](std::string input)
    {
        std::stringstream ss;
        uint32_t bytes = 0x00000000;

        MidiEvent event = {0, 0, 0, 0, 0, 0};

        size_t f = input.find_first_of(':');
        size_t l = input.find_last_of(':');

        std::string id = input.substr(f + 1, l - f - 1);
        std::string data = input.substr(l + 1, input.size());

        ss << std::hex << data;
        ss >> bytes;

        event.param2 = bytes & 0x000000ff;

        bytes = bytes >> 8;

        event.param1 = bytes & 0x000000ff;

        bytes = bytes >> 8;

        event.type = bytes & 0x000000f0;
        event.channel = bytes & 0x0000000f;

        switch (event.type)
        {
            case MidiEvent::NOTE_ON:
            case MidiEvent::NOTE_OFF:
                event.size = 3;
                break;
            default:
                event.size = 1;
        }

        return event;
    };
    userEventProcessor->pushEvent<std::string>(stream, converter);
}

void MidiSampleJackClient::onCommand(const char* cmd)
{
    std::string c(cmd);
    if (c.compare("cmd:shush") == 0)
    {
        audioProcessor->shush();
    }
}

} /* namespace sapamidisample */
} /* namespace audiosuite */
