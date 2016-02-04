#pragma once

/**
 * @date        Jun 06, 2014
 * @copyright   Copyright (c) 2014 by Samsung Electronics Polska Sp. z o. o.
 */

#include <IJackClientInterface.h>

#include "jack_types.h"
#include "StereoPort.h"

#include <atomic>
#include <functional>
#include <memory>
#include <string>

namespace audiosuite
{
namespace sapainstrumentsample
{

class Sinus;

class InstrumentSampleJackClient : public ::android::IJackClientInterface
{
    /** activation state flag */
    bool activated{false};
    /** application state flag play\stop */
    std::atomic_bool playOn{false};
    //std::string clientName;
    std::unique_ptr<
            jack_client_t,
            std::function<void(jack_client_t*)>
        > jackClient;
    /** client's ports */
    std::unique_ptr<StereoPort> outputPort;
    unsigned int samplerate;
    /** sinewave generator */
    std::unique_ptr<Sinus> sinus;

    static int _process(jack_nframes_t nframes, void* arg) noexcept;

    /**
     * processing method when play
     */
    virtual bool processPlayOn(jack_nframes_t framesCount) noexcept;

    /**
     * processing method when stop
     */
    virtual bool processPlayOff(jack_nframes_t framesCount) noexcept;

public:
    InstrumentSampleJackClient();
    virtual ~InstrumentSampleJackClient();

    /** SapaProcessor callbacks */
    virtual int setUp(int argc, char *argv[]);
    virtual int tearDown();
    virtual int activate();
    virtual int deactivate();
    virtual int transport(android::TransportType type);
    virtual int sendMidi(char*);
    virtual bool process(jack_nframes_t framesCount) noexcept;

    /**
     * change state to play
     */

    void play();
    /**
     * change state to stop
     */
    void stop();
};

} /* namespace sapainstrumentsample */
} /* namespace audiosuite */
