#pragma once

/**
 * @date        Jun 06, 2014
 * @copyright   Copyright (c) 2014 by Samsung Electronics Polska Sp. z o. o.
 */

#include <IJackClientInterface.h>

#include "jack_types.h"
#include <atomic>
#include <functional>
#include <memory>
#include <string>

namespace audiosuite
{

class StereoPort;
class MidiPort;

namespace sapamidisample
{

class Sinus;
class AdditiveGenerator;
class UserMidiProcessor;
class InPortMidiProcessor;
class AudioProcessor;

class MidiSampleJackClient : public ::android::IJackClientInterface
{
    /** activation state flag */
    bool activated{false};
    //std::string clientName;
    std::unique_ptr<
            jack_client_t,
            std::function<void(jack_client_t*)>
        > jackClient;
    /** client's ports */
    std::shared_ptr<StereoPort> outputPort;
    std::shared_ptr<MidiPort> midiInPort;
    std::shared_ptr<MidiPort> midiOutPort;
    /** processors */
    /* user midi events */
    std::unique_ptr<UserMidiProcessor> userEventProcessor;
    /* port midi events*/
    std::unique_ptr<InPortMidiProcessor> portEventProcessor;
    /* midi to audio processor */
    std::unique_ptr<AudioProcessor> audioProcessor;
    unsigned int samplerate;
    /** sinewave generator */
public:
    MidiSampleJackClient();
    virtual ~MidiSampleJackClient();

    /** SapaProcessor callbacks */
    virtual int setUp(int argc, char *argv[]);
    virtual int tearDown();
    virtual int activate();
    virtual int deactivate();
    virtual int transport(android::TransportType type);
    virtual int sendMidi(char*);
    virtual bool process(jack_nframes_t framesCount) noexcept;
    virtual void onStream(const char* msg);
    virtual void onCommand(const char* cmd);
};

} /* namespace sapamidisample */
} /* namespace audiosuite */
