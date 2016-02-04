#pragma once

/**
 * @date        Jun 06, 2014
 * @copyright   Copyright (c) 2014 by Samsung Electronics Polska Sp. z o. o.
 */

#include <IJackClientInterface.h>

#include "jack_types.h"

#include <functional>
#include <memory>
#include <string>
#include <atomic>

namespace audiosuite
{
namespace sapaeffectsample
{

struct Volume;
/**
 * Class implementig jack interface
 */
class EffectSampleJackClient : public ::android::IJackClientInterface
{
    /** activate state flag */
    bool activated{false};
    /** volume functor */
    std::unique_ptr<Volume> volume;
    //std::string clientName;
    std::unique_ptr<
            jack_client_t,
            std::function<void(jack_client_t*)>
        > jackClient;
    /** jack client ports */
    jack_port_t* outputPort[2];
    jack_port_t* inputPort[2];
    unsigned int samplerate;
    /**
     * jack process callback, calls local process method
     * @return result code
     */
    static int _process(jack_nframes_t nframes, void* arg) noexcept;
public:
    EffectSampleJackClient();
    virtual ~EffectSampleJackClient();
    /**
     * Callback after client process is forked
     * @return result code
     */
    virtual int setUp(int argc, char *argv[]);
    /**
     * Callback before client process exit
     * @return result code
     */
    virtual int tearDown();
    /**
     * Called when activate is called on SapaProcessor in application layer
     * @return result code
     */
    virtual int activate();
    /**
     * Called when deactivate is called on SapaProcessor in application layer
     * @return result code
     */
    virtual int deactivate();
    /**
     * Called when setTransport is called on SapaProcessor in application layer
     * @return result code
     */
    virtual int transport(android::TransportType type);
    virtual int sendMidi(char*);
    /**
     * local jack process callback
     */
    virtual bool process(jack_nframes_t framesCount) noexcept;
    /**
     * storing volume value sent from application layer
     */
    void setVolume(float volume);
};

} /* namespace sapaeffectsample */
} /* namespace audiosuite */
